import { defineStore } from "pinia";

import { CHAT_MESSAGE_PAGE_SIZE } from "@/constants/chatConstants";
import { useSocialStore } from "@/store/socialStore";
import type { ChatListItemDto } from "@/interface/DTO/chat/ChatListItemDto";
import type { ChatMediaDto } from "@/interface/DTO/chat/ChatMediaDto";
import type { ChatUserDto } from "@/interface/DTO/chat/ChatUserDto";
import type { MessageDto } from "@/interface/DTO/chat/MessageDto";
import {
  ensureChatHubConnection,
  joinChatHubGroup,
  leaveChatHubGroup,
} from "@/composables/useChatHub";
import { ChatService } from "@/service/chatService";
import { getApiData, isApiSuccess } from "@/utils/apiHelpers";
import {
  normalizeChatList,
  normalizeChatListItem,
  normalizeCreateChatResponse,
  normalizeGetMessagesResponse,
  normalizeMessage,
} from "@/utils/chatNormalize";
import {
  resolveAvatarFromGroupMedia,
  resolveChatAvatarPreview,
  type ChatAvatarPreview,
} from "@/utils/chatAvatar";

const service = new ChatService();
const POLL_INTERVAL_MS = 5000;

let pollTimer: ReturnType<typeof setInterval> | null = null;

type MessageThread = {
  items: MessageDto[];
  hasMore: boolean;
  loading: boolean;
  loadingMore: boolean;
};

function findFriend(userId: string) {
  const socialStore = useSocialStore();
  const normalized = userId.toLowerCase();
  return socialStore.friends.find((f) => f.id.toLowerCase() === normalized);
}

function chatTitle(chat: ChatListItemDto): string {
  if (chat.type === "Private") {
    const companion = chat.companion;
    if (companion?.id) {
      const friend = findFriend(companion.id);
      return friend?.login ?? companion.login ?? chat.name ?? "Чат";
    }
    return chat.name ?? "Чат";
  }
  return chat.name ?? "Группа";
}

function chatAvatarPreview(
  chat: ChatListItemDto,
  groupMediaByChat: Record<number, ChatMediaDto[]>
): ChatAvatarPreview {
  if (chat.type === "Private") {
    const companion = chat.companion;
    if (companion?.id) {
      const friend = findFriend(companion.id);
      if (friend?.avatarUrl) {
        return {
          url: friend.avatarUrl,
          isVideo: friend.avatarIsVideo ?? false,
        };
      }
    }
  }

  return resolveChatAvatarPreview(chat, groupMediaByChat[chat.id]);
}

function dedupeMessageMedia(message: MessageDto): MessageDto {
  if (!message.media.length) return message;

  const seen = new Set<string>();
  const media = message.media.filter((item) => {
    const key =
      item.id > 0
        ? `id:${item.id}`
        : `url:${item.url}:${item.originalName}`;
    if (!item.url || seen.has(key)) return false;
    seen.add(key);
    return true;
  });

  return media.length === message.media.length ? message : { ...message, media };
}

function upsertMessage(list: MessageDto[], message: MessageDto): MessageDto[] {
  const normalized = dedupeMessageMedia(message);
  const idx = list.findIndex((m) => m.id === normalized.id);
  if (idx === -1) return [...list, normalized];
  const next = [...list];
  next[idx] = normalized;
  return next;
}

function isGroupChatType(type?: string | null) {
  return (type ?? "").toLowerCase() === "group";
}

function sortMessages(list: MessageDto[]): MessageDto[] {
  return [...list].sort(
    (a, b) => new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime()
  );
}

export const useChatStore = defineStore("chat", {
  state: () => ({
    chats: [] as ChatListItemDto[],
    groupSearchResults: [] as ChatListItemDto[],
    activeChatId: null as number | null,
    messagesByChat: {} as Record<number, MessageThread>,
    loadingChats: false,
    searchingGroups: false,
    sending: false,
    hubReady: false,
    hubInitStarted: false,
    joinedChatId: null as number | null,
    groupMembersByChat: {} as Record<number, ChatUserDto[]>,
    groupMediaByChat: {} as Record<number, ChatMediaDto[]>,
    pendingOutgoingMessageIds: [] as number[],
  }),

  getters: {
    activeChat(state): ChatListItemDto | undefined {
      if (state.activeChatId == null) return undefined;
      return state.chats.find((c) => c.id === state.activeChatId);
    },

    activeMessages(state): MessageDto[] {
      if (state.activeChatId == null) return [];
      return state.messagesByChat[state.activeChatId]?.items ?? [];
    },

    activeHasMore(state): boolean {
      if (state.activeChatId == null) return false;
      return state.messagesByChat[state.activeChatId]?.hasMore ?? false;
    },

    activeLoading(state): boolean {
      if (state.activeChatId == null) return false;
      return state.messagesByChat[state.activeChatId]?.loading ?? false;
    },

    activeLoadingMore(state): boolean {
      if (state.activeChatId == null) return false;
      return state.messagesByChat[state.activeChatId]?.loadingMore ?? false;
    },
  },

  actions: {
    ensureThread(chatId: number): MessageThread {
      if (!this.messagesByChat[chatId]) {
        this.messagesByChat[chatId] = {
          items: [],
          hasMore: true,
          loading: false,
          loadingMore: false,
        };
      }
      return this.messagesByChat[chatId]!;
    },

    patchChatInList(chat: ChatListItemDto) {
      const idx = this.chats.findIndex((c) => c.id === chat.id);
      if (idx === -1) {
        this.chats = [chat, ...this.chats];
      } else {
        const next = [...this.chats];
        next[idx] = { ...next[idx], ...chat };
        this.chats = next;
      }

      this.groupSearchResults = this.groupSearchResults.map((group) =>
        group.id === chat.id ? { ...group, ...chat } : group
      );
    },

    updateChatPreview(chatId: number, message: MessageDto) {
      const chat = this.chats.find((c) => c.id === chatId);
      if (!chat) return;

      const previewText = message.isDeleted
        ? "Сообщение удалено"
        : message.text?.trim() ||
          (message.media.length ? "Медиа" : "");

      this.patchChatInList({
        ...chat,
        lastMessage: {
          id: message.id,
          userId: message.user.id,
          userLogin: message.user.login,
          text: previewText,
          createdAt: message.createdAt,
          isDeleted: message.isDeleted,
        },
      });

      this.chats = [...this.chats].sort((a, b) => {
        const aTime = new Date(a.lastMessage?.createdAt ?? a.createdAt).getTime();
        const bTime = new Date(b.lastMessage?.createdAt ?? b.createdAt).getTime();
        return bTime - aTime;
      });
    },

    handleRealtimeMessage(chatId: number, raw: unknown) {
      const message = dedupeMessageMedia(normalizeMessage(raw));

      if (this.pendingOutgoingMessageIds.includes(message.id)) {
        return;
      }

      const thread = this.ensureThread(chatId);
      thread.items = sortMessages(upsertMessage(thread.items, message));
      this.updateChatPreview(chatId, message);
    },

    handleRealtimeDelete(payload: { chatId: number; messageId: number }) {
      const thread = this.messagesByChat[payload.chatId];
      if (!thread) return;

      const msg = thread.items.find((m) => m.id === payload.messageId);
      if (!msg) return;

      const deleted = {
        ...msg,
        isDeleted: true,
        text: "",
        media: [],
      };

      thread.items = upsertMessage(thread.items, deleted);
      this.updateChatPreview(payload.chatId, deleted);
    },

    startMessagePolling() {
      if (pollTimer) return;

      pollTimer = setInterval(() => {
        if (this.activeChatId != null && !this.hubReady) {
          void this.refreshActiveMessages();
        }
      }, POLL_INTERVAL_MS);
    },

    stopMessagePolling() {
      if (!pollTimer) return;
      clearInterval(pollTimer);
      pollTimer = null;
    },

    async refreshActiveMessages() {
      if (this.activeChatId == null) return;

      const chatId = this.activeChatId;
      const thread = this.ensureThread(chatId);
      if (thread.loading) return;

      const res = await service.getMessages(chatId, undefined, CHAT_MESSAGE_PAGE_SIZE);
      const payload = getApiData(res);
      if (!isApiSuccess(res) || !payload) return;

      const batch = normalizeGetMessagesResponse(payload);
      const existing = new Set(thread.items.map((m) => m.id));
      const merged = [
        ...thread.items,
        ...batch.items.filter((m) => !existing.has(m.id)),
      ];

      if (merged.length !== thread.items.length) {
        thread.items = sortMessages(merged);
        const last = thread.items[thread.items.length - 1];
        if (last) this.updateChatPreview(chatId, last);
      }
    },

    initHub() {
      if (this.hubReady || this.hubInitStarted) return;

      this.hubInitStarted = true;
      this.startMessagePolling();

      void ensureChatHubConnection({
        onMessageSent: (payload) => {
          const message = normalizeMessage(payload);
          this.handleRealtimeMessage(message.chatId, payload);
        },
        onMessageUpdated: (payload) => {
          const message = normalizeMessage(payload);
          this.handleRealtimeMessage(message.chatId, payload);
        },
        onMessageDeleted: (payload) => {
          const p = payload as { chatId: number; messageId: number };
          this.handleRealtimeDelete(p);
        },
      })
        .then((conn) => {
          if (conn) {
            this.hubReady = true;
            this.stopMessagePolling();
          }
        })
        .catch(() => {
          /* REST fallback */
        })
        .finally(() => {
          this.hubInitStarted = false;
        });
    },

    async ensureChatInList(chatId: number) {
      if (this.chats.some((c) => c.id === chatId)) return;

      const res = await service.getChatById(chatId);

      const chatData = getApiData(res);
      if (isApiSuccess(res) && chatData) {
        this.patchChatInList(normalizeChatListItem(chatData));
        return;
      }

      this.patchChatInList({
        id: chatId,
        name: "Чат",
        type: "Private",
        isPublic: false,
        isMember: true,
        myRole: "Member",
        avatarUrl: null,
        avatarIsVideo: false,
        companion: null,
        lastMessage: null,
        createdAt: new Date().toISOString(),
      });
    },

    async loadChats() {
      this.loadingChats = true;

      try {
        const res = await service.getMyChats();
        const chatsData = getApiData(res);
        if (isApiSuccess(res) && chatsData) {
          this.chats = normalizeChatList(chatsData);
          void this.prefetchMissingGroupAvatars();
        }
        return res;
      } finally {
        this.loadingChats = false;
      }
    },

    async prefetchMissingGroupAvatars(chatIds?: number[]) {
      const targets = (chatIds ?? this.chats.map((chat) => chat.id)).filter((chatId) => {
        const chat = this.chats.find((item) => item.id === chatId);
        return chat && isGroupChatType(chat.type) && !chat.avatarUrl;
      });

      await Promise.all(
        targets.map((chatId) => this.getChatMedia(chatId).catch(() => undefined))
      );
    },

    async searchPublicGroups(search: string) {
      this.searchingGroups = true;

      try {
        const res = await service.searchPublicGroups(search.trim());
        const groupsData = getApiData(res);
        if (isApiSuccess(res) && groupsData) {
          this.groupSearchResults = normalizeChatList(groupsData);
          void this.prefetchMissingGroupAvatars(
            this.groupSearchResults
              .filter((group) => isGroupChatType(group.type) && !group.avatarUrl)
              .map((group) => group.id)
          );
        } else {
          this.groupSearchResults = [];
        }
        return res;
      } finally {
        this.searchingGroups = false;
      }
    },

    async openChat(chatId: number) {
      this.initHub();

      const fromSearch = this.groupSearchResults.find((c) => c.id === chatId);
      if (fromSearch) {
        this.patchChatInList(fromSearch);
      }

      await this.ensureChatInList(chatId);

      if (this.joinedChatId && this.joinedChatId !== chatId) {
        try {
          await leaveChatHubGroup(this.joinedChatId);
        } catch {
          /* ignore */
        }
      }

      this.activeChatId = chatId;

      try {
        await joinChatHubGroup(chatId);
        this.joinedChatId = chatId;
      } catch {
        /* ignore */
      }

      const active = this.chats.find((c) => c.id === chatId);
      if (active?.isMember === false) return;

      if (isGroupChatType(active?.type)) {
        void this.prefetchGroupInfo(chatId);
      } else if (active?.type === "Private") {
        void this.refreshChat(chatId);
      }

      const thread = this.ensureThread(chatId);
      if (!thread.items.length) {
        await this.loadMessages(chatId, true);
      }
    },

    async loadMessages(chatId: number, reset = false) {
      const thread = this.ensureThread(chatId);
      if (thread.loading) return;

      if (reset) {
        thread.items = [];
        thread.hasMore = true;
      }

      thread.loading = true;

      try {
        const res = await service.getMessages(chatId, undefined, CHAT_MESSAGE_PAGE_SIZE);

        const messagesData = getApiData(res);
        if (isApiSuccess(res) && messagesData) {
          const batch = normalizeGetMessagesResponse(messagesData);
          thread.items = sortMessages(batch.items.map(dedupeMessageMedia));
          thread.hasMore = batch.hasMore;
        }
      } finally {
        thread.loading = false;
      }
    },

    async loadOlderMessages(chatId: number) {
      const thread = this.ensureThread(chatId);
      if (!thread.hasMore || thread.loadingMore || thread.loading) return;

      const first = thread.items[0];
      if (!first) return;

      thread.loadingMore = true;

      try {
        const res = await service.getMessages(
          chatId,
          first.id,
          CHAT_MESSAGE_PAGE_SIZE
        );

        const olderData = getApiData(res);
        if (isApiSuccess(res) && olderData) {
          const batch = normalizeGetMessagesResponse(olderData);
          const existing = new Set(thread.items.map((m) => m.id));

          thread.items = sortMessages([
            ...batch.items
              .map(dedupeMessageMedia)
              .filter((m) => !existing.has(m.id)),
            ...thread.items,
          ]);
          thread.hasMore = batch.hasMore;
        }
      } finally {
        thread.loadingMore = false;
      }
    },

    async startPrivateChat(targetUserId: string) {
      const normalizedId = targetUserId.trim();
      if (!normalizedId) {
        return {
          success: false,
          error: { code: "INVALID_TARGET", message: "Не указан пользователь" },
        };
      }

      const res = await service.createPrivateChat(normalizedId);
      const createdChat = getApiData(res);
      if (!isApiSuccess(res) || !createdChat) return res;

      const chatId = normalizeCreateChatResponse(createdChat);
      if (!chatId) {
        return {
          success: false,
          error: { code: "INVALID_RESPONSE", message: "Сервер не вернул id чата" },
        };
      }

      await this.openChat(chatId);
      void this.loadChats();
      return res;
    },

    async joinGroup(chatId: number) {
      const res = await service.joinPublicGroup(chatId);
      if (!isApiSuccess(res)) return res;

      const chatRes = await service.getChatById(chatId);
      const joinedChat = getApiData(chatRes);
      if (isApiSuccess(chatRes) && joinedChat) {
        this.patchChatInList(normalizeChatListItem(joinedChat));
      }

      this.groupSearchResults = this.groupSearchResults.map((group) =>
        group.id === chatId ? { ...group, isMember: true } : group
      );

      await this.openChat(chatId);
      void this.loadChats();
      return res;
    },

    async inviteGroupMember(chatId: number, targetUserId: string) {
      return service.inviteGroupMember(chatId, targetUserId);
    },

    async createGroup(name: string, isPublic: boolean) {
      const res = await service.createGroupChat(name, isPublic);
      const createdGroup = getApiData(res);
      if (!isApiSuccess(res) || !createdGroup) return res;

      const chatId = normalizeCreateChatResponse(createdGroup);
      if (!chatId) {
        return {
          success: false,
          error: { code: "INVALID_RESPONSE", message: "Сервер не вернул id чата" },
        };
      }

      await this.openChat(chatId);
      void this.loadChats();
      return res;
    },

    async sendMessage(text: string, file?: File | null) {
      if (this.activeChatId == null || this.sending) return null;

      const chatId = this.activeChatId;
      this.sending = true;
      let outgoingMessageId: number | null = null;

      try {
        const res = await service.sendMessage(chatId, text.trim());
        const sentMessage = getApiData(res);

        if (!isApiSuccess(res) || !sentMessage) return res;

        let message = dedupeMessageMedia(normalizeMessage(sentMessage));
        outgoingMessageId = message.id;
        this.pendingOutgoingMessageIds.push(message.id);

        if (file) {
          const mediaType = file.type.startsWith("video") ? "video" : "image";
          const mediaRes = await service.uploadMessageMedia(message.id, file, mediaType);
          const uploadedMessage = getApiData(mediaRes);
          if (isApiSuccess(mediaRes) && uploadedMessage) {
            message = dedupeMessageMedia(normalizeMessage(uploadedMessage));
          }
        }

        const thread = this.ensureThread(chatId);
        thread.items = sortMessages(upsertMessage(thread.items, message));
        this.updateChatPreview(chatId, message);

        return res;
      } finally {
        if (outgoingMessageId != null) {
          this.pendingOutgoingMessageIds = this.pendingOutgoingMessageIds.filter(
            (id) => id !== outgoingMessageId
          );
        }
        this.sending = false;
      }
    },

    async editMessage(
      messageId: number,
      text: string,
      options?: { removeMediaIds?: number[]; newFile?: File | null }
    ) {
      let message: MessageDto | null = null;

      for (const mediaId of options?.removeMediaIds ?? []) {
        const delRes = await service.deleteMessageMedia(messageId, mediaId);
        const deletedMessage = getApiData(delRes);
        if (!isApiSuccess(delRes) || !deletedMessage) return delRes;
        message = dedupeMessageMedia(normalizeMessage(deletedMessage));
      }

      if (options?.newFile) {
        const mediaType = options.newFile.type.startsWith("video") ? "video" : "image";
        const mediaRes = await service.uploadMessageMedia(
          messageId,
          options.newFile,
          mediaType
        );
        const uploadedMessage = getApiData(mediaRes);
        if (!isApiSuccess(mediaRes) || !uploadedMessage) return mediaRes;
        message = dedupeMessageMedia(normalizeMessage(uploadedMessage));
      }

      const res = await service.updateMessage(messageId, text.trim());
      const updatedPayload = getApiData(res);
      if (!isApiSuccess(res) || !updatedPayload) return res;

      const updated = dedupeMessageMedia(normalizeMessage(updatedPayload));
      if (message && message.media.length > updated.media.length) {
        updated.media = message.media;
      }
      message = updated;

      const thread = this.ensureThread(message.chatId);
      thread.items = sortMessages(upsertMessage(thread.items, message));
      this.updateChatPreview(message.chatId, message);

      return { ...res, data: message };
    },

    async deleteMessage(messageId: number) {
      const res = await service.deleteMessage(messageId);
      if (!isApiSuccess(res)) return res;

      if (this.activeChatId != null) {
        const thread = this.ensureThread(this.activeChatId);
        const msg = thread.items.find((m) => m.id === messageId);
        if (msg) {
          const deleted = { ...msg, isDeleted: true, text: "", media: [] };
          thread.items = upsertMessage(thread.items, deleted);
          this.updateChatPreview(this.activeChatId, deleted);
        }
      }

      return res;
    },

    async updateGroup(
      chatId: number,
      payload: { name: string; isPublic?: boolean }
    ) {
      const res = await service.updateGroupChat(chatId, {
        name: payload.name.trim(),
        isPublic: payload.isPublic,
      });
      const updatedGroup = getApiData(res);
      if (isApiSuccess(res) && updatedGroup) {
        this.patchChatInList(normalizeChatListItem(updatedGroup));
      }
      return res;
    },

    async getChatMembers(chatId: number) {
      const res = await service.getChatMembers(chatId);
      const members = getApiData(res);
      if (isApiSuccess(res) && members) {
        this.groupMembersByChat[chatId] = members;
      }
      return res;
    },

    async getChatMedia(chatId: number) {
      const res = await service.getChatMedia(chatId);
      const media = getApiData(res);
      if (isApiSuccess(res) && media) {
        this.groupMediaByChat[chatId] = media;
        this.syncGroupAvatarFromMedia(chatId);
      }
      return res;
    },

    async prefetchGroupInfo(chatId: number) {
      await Promise.all([
        this.refreshChat(chatId),
        this.getChatMembers(chatId),
        this.getChatMedia(chatId),
      ]);
    },

    syncGroupAvatarFromMedia(chatId: number) {
      const media = this.groupMediaByChat[chatId];
      if (!media?.length) return;

      const preview = resolveAvatarFromGroupMedia(media);
      if (!preview.url) return;

      const chat = this.chats.find((c) => c.id === chatId);
      if (!chat) return;

      this.patchChatInList({
        ...chat,
        avatarUrl: preview.url,
        avatarIsVideo: preview.isVideo,
      });
    },

    applyGroupMediaItem(chatId: number, item: ChatMediaDto) {
      const prev = this.groupMediaByChat[chatId] ?? [];
      this.groupMediaByChat[chatId] = [...prev, item];
      this.syncGroupAvatarFromMedia(chatId);
    },

    async uploadGroupMedia(chatId: number, file: File, mediaType: string) {
      const res = await service.uploadChatMedia(chatId, file, mediaType);
      const uploadedMedia = getApiData(res);
      if (isApiSuccess(res) && uploadedMedia) {
        this.applyGroupMediaItem(chatId, uploadedMedia);
        await this.refreshChat(chatId);
      }
      return res;
    },

    async replaceGroupMedia(
      chatId: number,
      mediaId: number,
      file: File,
      mediaType: string
    ) {
      const res = await service.replaceChatMedia(chatId, mediaId, file, mediaType);
      const replacedMedia = getApiData(res);
      if (isApiSuccess(res) && replacedMedia) {
        const list = (this.groupMediaByChat[chatId] ?? []).map((item) =>
          item.id === mediaId ? replacedMedia : item
        );
        this.groupMediaByChat[chatId] = list;
        this.syncGroupAvatarFromMedia(chatId);
        await this.refreshChat(chatId);
      }
      return res;
    },

    async deleteGroupMedia(chatId: number, mediaId: number) {
      const res = await service.deleteChatMedia(chatId, mediaId);
      if (isApiSuccess(res)) {
        await this.getChatMedia(chatId);
        await this.refreshChat(chatId);
      }
      return res;
    },

    async deleteAllGroupMedia(chatId: number) {
      const res = await service.deleteAllChatMedia(chatId);
      if (isApiSuccess(res)) {
        this.groupMediaByChat[chatId] = [];
        const chat = this.chats.find((c) => c.id === chatId);
        if (chat) {
          this.patchChatInList({
            ...chat,
            avatarUrl: null,
            avatarIsVideo: false,
          });
        }
        await this.refreshChat(chatId);
      }
      return res;
    },

    async refreshGroupAvatar(chatId: number) {
      await this.refreshChat(chatId);
    },

    async refreshChat(chatId: number) {
      const res = await service.getChatById(chatId);
      const chatData = getApiData(res);
      if (isApiSuccess(res) && chatData) {
        this.patchChatInList(normalizeChatListItem(chatData));
      }
      return res;
    },

    async removeGroupMember(chatId: number, targetUserId: string) {
      return service.removeGroupMember(chatId, targetUserId);
    },

    async clearChat(chatId: number) {
      const res = await service.clearChatMessages(chatId);
      if (!isApiSuccess(res)) return res;

      const thread = this.ensureThread(chatId);
      thread.items = thread.items.map((msg) => ({
        ...msg,
        isDeleted: true,
        text: "",
        media: [],
      }));

      const last = thread.items[thread.items.length - 1];
      if (last) {
        this.updateChatPreview(chatId, last);
      } else {
        const chat = this.chats.find((c) => c.id === chatId);
        if (chat) {
          this.patchChatInList({
            ...chat,
            lastMessage: null,
          });
        }
      }

      return res;
    },

    async deleteChat(chatId: number) {
      const res = await service.deleteChat(chatId);
      if (!isApiSuccess(res)) return res;

      this.chats = this.chats.filter((c) => c.id !== chatId);
      delete this.messagesByChat[chatId];
      delete this.groupMembersByChat[chatId];
      delete this.groupMediaByChat[chatId];

      if (this.activeChatId === chatId) {
        this.activeChatId = null;
      }

      if (this.joinedChatId === chatId) {
        this.joinedChatId = null;
        try {
          await leaveChatHubGroup(chatId);
        } catch {
          /* ignore */
        }
      }

      return res;
    },

    getChatTitle(chat: ChatListItemDto) {
      return chatTitle(chat);
    },

    getChatAvatarPreview(chat: ChatListItemDto): ChatAvatarPreview {
      return chatAvatarPreview(chat, this.groupMediaByChat);
    },

    getChatAvatar(chat: ChatListItemDto) {
      return this.getChatAvatarPreview(chat).url;
    },

    getChatAvatarIsVideo(chat: ChatListItemDto) {
      return this.getChatAvatarPreview(chat).isVideo;
    },

    getUserDisplayLogin(userId: string, fallback: string) {
      const friend = findFriend(userId);
      return friend?.login ?? fallback;
    },

    getUserDisplayAvatar(userId: string, fallback: string) {
      const friend = findFriend(userId);
      return friend?.avatarUrl ?? fallback;
    },

    getUserDisplayAvatarIsVideo(userId: string, fallback = false) {
      const friend = findFriend(userId);
      if (friend?.avatarIsVideo) return true;
      return fallback;
    },
  },
});
