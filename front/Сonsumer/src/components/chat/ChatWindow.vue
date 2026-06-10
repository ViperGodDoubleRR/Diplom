<template>
  <div class="chat-window">
    <header class="header">
      <button
        v-if="isPrivateChat"
        type="button"
        class="header-link"
        @click="goToCompanionProfile"
      >
        <UserAvatar
          avatar-class="header-avatar"
          :name="title"
          :src="avatar"
          :is-video="avatarIsVideo"
        />
        <div class="header-meta">
          <h3>{{ title }}</h3>
          <p v-if="chat?.companion?.tag">@{{ chat.companion.tag }}</p>
        </div>
      </button>

      <button
        v-if="isPrivateChat"
        type="button"
        class="header-menu-btn"
        title="Действия с чатом"
        @click="showPrivateInfo = true"
      >
        ⋯
      </button>

      <button
        v-else-if="isGroupChat && isGroupMember"
        type="button"
        class="header-link"
        :disabled="openingGroupInfo"
        @click="openGroupInfo"
      >
        <UserAvatar
          avatar-class="header-avatar"
          :name="title"
          :src="avatar"
          :is-video="avatarIsVideo"
        />
        <div class="header-meta">
          <h3>{{ title }}</h3>
          <p>{{ chat?.isPublic ? "Открытая группа" : "Группа" }}</p>
        </div>
      </button>

      <template v-else-if="isGroupChat">
        <UserAvatar
          avatar-class="header-avatar"
          :name="title"
          :src="avatar"
          :is-video="avatarIsVideo"
        />
        <div class="header-meta">
          <h3>{{ title }}</h3>
          <p>{{ chat?.isPublic ? "Открытая группа" : "Группа" }}</p>
        </div>
        <button
          type="button"
          class="header-btn primary"
          :disabled="joining"
          @click="joinGroup"
        >
          {{ joining ? "..." : "Вступить" }}
        </button>
      </template>
    </header>

    <div v-if="canJoinGroup" class="guest-panel">
      <p>Открытая группа — вступите, чтобы читать и писать сообщения.</p>
      <button type="button" class="guest-btn" :disabled="joining" @click="joinGroup">
        {{ joining ? "Вступление..." : "Вступить в группу" }}
      </button>
    </div>

    <template v-else>
      <div ref="listRef" class="messages" @scroll.passive="onScroll">
        <p v-if="chatStore.activeLoadingMore" class="load-hint">Загрузка...</p>
        <p v-else-if="chatStore.activeHasMore" class="load-hint link" @click="loadOlder">
          Загрузить ранние сообщения
        </p>

        <p v-if="chatStore.activeLoading && !messages.length" class="load-hint">Загрузка...</p>

        <ChatMessageBubble
          v-for="message in messages"
          :key="message.id"
          :message="message"
          :is-own="isOwn(message)"
          :can-edit="isOwn(message) && !message.isDeleted"
          :can-delete="canDeleteMessage(message)"
          @edit="startEdit"
          @delete="onDelete"
        />
      </div>

      <div v-if="editingMessage" class="edit-bar">
        <div class="edit-bar-main">
          <span>Редактирование</span>
          <button type="button" @click="cancelEdit">✕</button>
        </div>

        <div v-if="editingMedia.length || editingNewFile" class="edit-media">
          <div
            v-for="item in editingMedia"
            :key="item.id"
            class="edit-media-item"
          >
            <img
              v-if="!isVideoMediaType(item.mediaType)"
              :src="item.url"
              alt=""
            />
            <video
              v-else
              :src="item.url"
              muted
              playsinline
              preload="metadata"
            />
            <button type="button" class="remove-media" @click="removeEditingMedia(item.id)">
              ✕
            </button>
          </div>

          <div v-if="editingNewFile" class="edit-media-item new-file">
            <span>{{ editingNewFile.name }}</span>
            <button type="button" class="remove-media" @click="clearEditingNewFile">✕</button>
          </div>
        </div>
      </div>

      <footer class="composer">
        <input
          ref="fileInputRef"
          type="file"
          accept="image/jpeg,image/png,image/webp,image/gif,video/mp4,video/webm,video/quicktime"
          class="hidden-file"
          @change="onFileSelected"
        />

        <button type="button" class="attach-btn" title="Фото или видео до 300 МБ" @click="pickFile">
          📎
        </button>

        <textarea
          v-model="draft"
          rows="1"
          :placeholder="editingMessage ? 'Изменить текст...' : 'Сообщение...'"
          :disabled="chatStore.sending"
          @keydown.enter.exact.prevent="send"
        />

        <button type="button" class="send-btn" :disabled="!canSend" @click="send">
          {{ chatStore.sending ? "..." : editingMessage ? "Сохранить" : "➤" }}
        </button>
      </footer>
    </template>

    <p v-if="errorMessage" class="error">{{ errorMessage }}</p>

    <GroupInfoModal
      v-if="showGroupInfo && chat"
      v-model="showGroupInfo"
      :chat="chat"
      @deleted="onChatDeleted"
    />

    <PrivateChatInfoModal
      v-if="showPrivateInfo && chat"
      v-model="showPrivateInfo"
      :chat="chat"
      @deleted="onChatDeleted"
    />
  </div>
</template>

<script setup lang="ts">
import { computed, nextTick, ref, watch } from "vue";
import { useRouter } from "vue-router";

import ChatMessageBubble from "@/components/chat/ChatMessageBubble.vue";
import GroupInfoModal from "@/components/chat/GroupInfoModal.vue";
import PrivateChatInfoModal from "@/components/chat/PrivateChatInfoModal.vue";
import UserAvatar from "@/components/ui/UserAvatar.vue";
import { CHAT_MEDIA_MAX_BYTES } from "@/constants/chatConstants";
import type { MessageDto } from "@/interface/DTO/chat/MessageDto";
import { useChatStore } from "@/store/chatStore";
import { useUserStore } from "@/store/userStore";
import { isApiSuccess } from "@/utils/apiHelpers";
import { isVideoMediaType } from "@/utils/postNormalize";

const chatStore = useChatStore();
const userStore = useUserStore();
const router = useRouter();

const draft = ref("");
const selectedFile = ref<File | null>(null);
const editingMessage = ref<MessageDto | null>(null);
const editingRemovedMediaIds = ref<number[]>([]);
const editingNewFile = ref<File | null>(null);
const errorMessage = ref("");
const showGroupInfo = ref(false);
const showPrivateInfo = ref(false);
const openingGroupInfo = ref(false);
const joining = ref(false);
const listRef = ref<HTMLElement | null>(null);
const fileInputRef = ref<HTMLInputElement | null>(null);

const chat = computed(() => chatStore.activeChat);
const messages = computed(() => chatStore.activeMessages);

const title = computed(() => {
  if (chat.value) return chatStore.getChatTitle(chat.value);
  return chatStore.activeChatId != null ? "Чат" : "";
});

const avatarPreview = computed(() =>
  chat.value
    ? chatStore.getChatAvatarPreview(chat.value)
    : { url: "", isVideo: false }
);

const avatar = computed(() => avatarPreview.value.url);
const avatarIsVideo = computed(() => avatarPreview.value.isVideo);

const isPrivateChat = computed(() => chat.value?.type === "Private");
const isGroupChat = computed(() => chat.value?.type === "Group");
const isGroupMember = computed(() => chat.value?.isMember !== false);
const isAdmin = computed(() => chat.value?.myRole === "Admin");
const canJoinGroup = computed(
  () => isGroupChat.value && chat.value?.isPublic && !isGroupMember.value
);

const editingMedia = computed(() => {
  if (!editingMessage.value) return [];
  const removed = new Set(editingRemovedMediaIds.value);
  return editingMessage.value.media.filter((m) => !removed.has(m.id));
});

const canSend = computed(() => {
  if (chatStore.sending) return false;

  if (editingMessage.value) {
    return (
      draft.value.trim().length > 0 ||
      editingMedia.value.length > 0 ||
      !!editingNewFile.value
    );
  }

  return draft.value.trim().length > 0 || !!selectedFile.value;
});

watch(
  () => chatStore.activeChatId,
  async () => {
    draft.value = "";
    selectedFile.value = null;
    cancelEdit();
    await nextTick();
    scrollToBottom();
  }
);

watch(
  () => messages.value.length,
  async () => {
    if (!chatStore.activeLoadingMore) {
      await nextTick();
      scrollToBottom();
    }
  }
);

function isOwn(message: MessageDto) {
  const myId = userStore.user?.id;
  if (!myId) return false;
  return message.user.id.toLowerCase() === myId.toLowerCase();
}

function canDeleteMessage(message: MessageDto) {
  if (message.isDeleted) return false;
  if (isOwn(message)) return true;
  return isGroupChat.value && isAdmin.value;
}

function onChatDeleted() {
  showGroupInfo.value = false;
  showPrivateInfo.value = false;
}

async function openGroupInfo() {
  const chatId = chat.value?.id;
  if (chatId == null || openingGroupInfo.value) return;

  openingGroupInfo.value = true;
  errorMessage.value = "";

  try {
    await chatStore.prefetchGroupInfo(chatId);
    showGroupInfo.value = true;
  } catch {
    errorMessage.value = "Не удалось загрузить данные группы";
  } finally {
    openingGroupInfo.value = false;
  }
}

function goToCompanionProfile() {
  const companionId = chat.value?.companion?.id;
  if (!companionId) return;

  if (companionId === userStore.user?.id) {
    void router.push({ name: "profile" });
    return;
  }

  void router.push({ name: "profile-view", params: { id: companionId } });
}

async function joinGroup() {
  if (chatStore.activeChatId == null) return;

  joining.value = true;
  errorMessage.value = "";

  try {
    const res = await chatStore.joinGroup(chatStore.activeChatId);
    if (!isApiSuccess(res)) {
      errorMessage.value = res?.error?.message ?? "Не удалось вступить";
    }
  } finally {
    joining.value = false;
  }
}

function scrollToBottom() {
  const el = listRef.value;
  if (!el) return;
  el.scrollTop = el.scrollHeight;
}

function onScroll() {
  const el = listRef.value;
  if (!el || chatStore.activeChatId == null) return;

  if (el.scrollTop <= 80 && chatStore.activeHasMore && !chatStore.activeLoadingMore) {
    void loadOlder();
  }
}

async function loadOlder() {
  if (chatStore.activeChatId == null) return;
  const el = listRef.value;
  const prevHeight = el?.scrollHeight ?? 0;

  await chatStore.loadOlderMessages(chatStore.activeChatId);

  await nextTick();
  if (el) {
    el.scrollTop = el.scrollHeight - prevHeight;
  }
}

function pickFile() {
  fileInputRef.value?.click();
}

function onFileSelected(event: Event) {
  errorMessage.value = "";
  const input = event.target as HTMLInputElement;
  const file = input.files?.[0];
  if (!file) return;

  if (file.size > CHAT_MEDIA_MAX_BYTES) {
    errorMessage.value = "Файл не больше 300 МБ";
    input.value = "";
    return;
  }

  if (editingMessage.value) {
    editingNewFile.value = file;
  } else {
    selectedFile.value = file;
  }
}

function startEdit(message: MessageDto) {
  editingMessage.value = message;
  editingRemovedMediaIds.value = [];
  editingNewFile.value = null;
  draft.value = message.text;
  selectedFile.value = null;
}

function cancelEdit() {
  editingMessage.value = null;
  editingRemovedMediaIds.value = [];
  editingNewFile.value = null;
  draft.value = "";
  if (fileInputRef.value) fileInputRef.value.value = "";
}

function removeEditingMedia(mediaId: number) {
  if (!editingRemovedMediaIds.value.includes(mediaId)) {
    editingRemovedMediaIds.value = [...editingRemovedMediaIds.value, mediaId];
  }
}

function clearEditingNewFile() {
  editingNewFile.value = null;
  if (fileInputRef.value) fileInputRef.value.value = "";
}

async function send() {
  errorMessage.value = "";

  if (editingMessage.value) {
    const res = await chatStore.editMessage(
      editingMessage.value.id,
      draft.value,
      {
        removeMediaIds: editingRemovedMediaIds.value,
        newFile: editingNewFile.value,
      }
    );
    if (!isApiSuccess(res)) {
      errorMessage.value = res?.error?.message ?? "Не удалось изменить";
      return;
    }
    cancelEdit();
    return;
  }

  if (!draft.value.trim() && !selectedFile.value) return;

  const file = selectedFile.value;
  const res = await chatStore.sendMessage(draft.value, file);

  if (!isApiSuccess(res)) {
    errorMessage.value = res?.error?.message ?? "Не удалось отправить";
    return;
  }

  draft.value = "";
  selectedFile.value = null;
  if (fileInputRef.value) fileInputRef.value.value = "";
}

async function onDelete(messageId: number) {
  if (!confirm("Удалить сообщение?")) return;

  const res = await chatStore.deleteMessage(messageId);
  if (!isApiSuccess(res)) {
    errorMessage.value = res?.error?.message ?? "Не удалось удалить";
  }
}
</script>

<style scoped>
.chat-window {
  display: flex;
  flex-direction: column;
  height: 100%;
}

.header {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 14px 18px;
  border-bottom: 1px solid rgba(255, 255, 255, 0.06);
}

.header-link {
  display: flex;
  align-items: center;
  gap: 12px;
  flex: 1;
  min-width: 0;
  border: none;
  background: transparent;
  padding: 0;
  cursor: pointer;
  text-align: left;
  border-radius: 12px;
  transition: background 0.15s ease;
}

.header-link:hover:not(:disabled) {
  background: rgba(255, 255, 255, 0.04);
}

.header-link:disabled {
  opacity: 0.6;
  cursor: wait;
}

.header :deep(.header-avatar),
.header :deep(.user-avatar),
.header :deep(.initials) {
  width: 44px;
  height: 44px;
}

.header-meta {
  flex: 1;
  min-width: 0;
}

.header-meta h3 {
  margin: 0;
  color: white;
  font-size: 17px;
}

.header-meta p {
  margin: 2px 0 0;
  color: rgba(255, 255, 255, 0.45);
  font-size: 12px;
}

.header-btn {
  margin-left: auto;
  border: none;
  background: rgba(65, 99, 252, 0.25);
  color: #dbe3ff;
  padding: 8px 14px;
  border-radius: 10px;
  cursor: pointer;
  font-size: 13px;
  white-space: nowrap;
  flex-shrink: 0;
}

.header-btn:hover:not(:disabled) {
  background: rgba(65, 99, 252, 0.32);
}

.header-btn:disabled {
  opacity: 0.5;
  cursor: default;
}

.header-menu-btn {
  margin-left: auto;
  border: none;
  background: rgba(255, 255, 255, 0.06);
  color: white;
  width: 36px;
  height: 36px;
  border-radius: 10px;
  cursor: pointer;
  font-size: 18px;
  line-height: 1;
  flex-shrink: 0;
}

.header-menu-btn:hover {
  background: rgba(255, 255, 255, 0.1);
}

.guest-panel {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 16px;
  padding: 24px;
  text-align: center;
  color: rgba(255, 255, 255, 0.65);
}

.guest-btn {
  border: none;
  background: #4163fc;
  color: white;
  padding: 12px 20px;
  border-radius: 12px;
  cursor: pointer;
  font-size: 14px;
}

.guest-btn:disabled {
  opacity: 0.5;
  cursor: default;
}

.messages {
  flex: 1;
  overflow-y: auto;
  padding: 16px;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.load-hint {
  text-align: center;
  font-size: 12px;
  color: rgba(255, 255, 255, 0.45);
  margin: 4px 0 10px;
}

.load-hint.link {
  color: #7c9bff;
  cursor: pointer;
}

.edit-bar {
  padding: 8px 16px 12px;
  background: rgba(65, 99, 252, 0.12);
  color: #dbe3ff;
  font-size: 13px;
}

.edit-bar-main {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.edit-bar-main button {
  border: none;
  background: transparent;
  color: white;
  cursor: pointer;
}

.edit-media {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  margin-top: 10px;
}

.edit-media-item {
  position: relative;
  width: 72px;
  height: 72px;
  border-radius: 10px;
  overflow: hidden;
  background: rgba(0, 0, 0, 0.25);
}

.edit-media-item img,
.edit-media-item video {
  width: 100%;
  height: 100%;
  object-fit: cover;
  display: block;
}

.edit-media-item.new-file {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 6px;
  font-size: 10px;
  word-break: break-all;
}

.remove-media {
  position: absolute;
  top: 4px;
  right: 4px;
  width: 22px;
  height: 22px;
  border: none;
  border-radius: 50%;
  background: rgba(0, 0, 0, 0.65);
  color: white;
  cursor: pointer;
  font-size: 12px;
  line-height: 1;
}

.composer {
  display: flex;
  gap: 8px;
  align-items: flex-end;
  padding: 12px 16px 16px;
  border-top: 1px solid rgba(255, 255, 255, 0.06);
}

.hidden-file {
  display: none;
}

.attach-btn,
.send-btn {
  border: none;
  background: rgba(255, 255, 255, 0.06);
  color: white;
  width: 40px;
  height: 40px;
  border-radius: 12px;
  cursor: pointer;
  flex-shrink: 0;
}

.send-btn {
  background: #4163fc;
  min-width: 40px;
  width: auto;
  padding: 0 14px;
}

.send-btn:disabled {
  opacity: 0.45;
  cursor: default;
}

textarea {
  flex: 1;
  min-height: 40px;
  max-height: 120px;
  resize: none;
  border: 1px solid rgba(255, 255, 255, 0.08);
  border-radius: 14px;
  background: rgba(255, 255, 255, 0.04);
  color: white;
  padding: 10px 14px;
  font: inherit;
}

.error {
  padding: 0 16px 12px;
  color: #ff8da8;
  font-size: 13px;
}
</style>
