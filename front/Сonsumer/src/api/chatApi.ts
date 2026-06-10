import type { ApiResponse } from "@/interface/ApiContracts/ApiResponse";
import type { ChatListItemDto } from "@/interface/DTO/chat/ChatListItemDto";
import type { ChatMediaDto } from "@/interface/DTO/chat/ChatMediaDto";
import type { ChatUserDto } from "@/interface/DTO/chat/ChatUserDto";
import type { CreateChatResponse } from "@/interface/DTO/chat/CreateChatResponse";
import type { GetMessagesResponse } from "@/interface/DTO/chat/GetMessagesResponse";
import type { MessageDto } from "@/interface/DTO/chat/MessageDto";
import { withApiCatch } from "@/utils/apiHelpers";
import { api } from "./apiUrl";

export class ChatApi {
  getMyChats() {
    return withApiCatch(
      () => api.get<ApiResponse<ChatListItemDto[]>>("/chat/chats").then((r) => r.data),
      "Не удалось загрузить чаты"
    );
  }

  getChatById(chatId: number) {
    return withApiCatch(
      () =>
        api
          .get<ApiResponse<ChatListItemDto>>(`/chat/chats/${chatId}`)
          .then((r) => r.data),
      "Не удалось загрузить чат"
    );
  }

  searchPublicGroups(search: string) {
    return withApiCatch(
      () =>
        api
          .get<ApiResponse<ChatListItemDto[]>>("/chat/search/groups", {
            params: { search },
          })
          .then((r) => r.data),
      "Не удалось найти группы"
    );
  }

  createPrivateChat(targetUserId: string) {
    return withApiCatch(
      () =>
        api
          .post<ApiResponse<CreateChatResponse>>("/chat/chats/private", {
            targetUserId,
          })
          .then((r) => r.data),
      "Не удалось создать приватный чат"
    );
  }

  createGroupChat(name: string, isPublic: boolean) {
    return withApiCatch(
      () =>
        api
          .post<ApiResponse<CreateChatResponse>>("/chat/chats/group", { name, isPublic })
          .then((r) => r.data),
      "Не удалось создать группу"
    );
  }

  inviteGroupMember(chatId: number, targetUserId: string) {
    return withApiCatch(
      () =>
        api
          .post<ApiResponse<boolean>>(`/chat/chats/${chatId}/members`, {
            targetUserId,
          })
          .then((r) => r.data),
      "Не удалось пригласить участника"
    );
  }

  removeGroupMember(chatId: number, targetUserId: string) {
    return withApiCatch(
      () =>
        api
          .delete<ApiResponse<boolean>>(`/chat/chats/${chatId}/members/${targetUserId}`)
          .then((r) => r.data),
      "Не удалось удалить участника"
    );
  }

  deleteChat(chatId: number) {
    return withApiCatch(
      () =>
        api
          .delete<ApiResponse<boolean>>(`/chat/chats/${chatId}`)
          .then((r) => r.data),
      "Не удалось удалить чат"
    );
  }

  clearChatMessages(chatId: number) {
    return withApiCatch(
      () =>
        api
          .delete<ApiResponse<boolean>>(`/chat/chats/${chatId}/messages`)
          .then((r) => r.data),
      "Не удалось очистить чат"
    );
  }

  joinPublicGroup(chatId: number) {
    return withApiCatch(
      () =>
        api
          .post<ApiResponse<CreateChatResponse>>(`/chat/chats/${chatId}/join`)
          .then((r) => r.data),
      "Не удалось вступить в группу"
    );
  }

  getChatMembers(chatId: number) {
    return withApiCatch(
      () =>
        api
          .get<ApiResponse<ChatUserDto[]>>(`/chat/chats/${chatId}/members`)
          .then((r) => r.data),
      "Не удалось загрузить участников"
    );
  }

  getChatMedia(chatId: number) {
    return withApiCatch(
      () =>
        api
          .get<ApiResponse<ChatMediaDto[]>>(`/chat/chats/${chatId}/media`)
          .then((r) => r.data),
      "Не удалось загрузить медиа группы"
    );
  }

  uploadChatMedia(chatId: number, file: File, mediaType: string) {
    const form = new FormData();
    form.append("file", file);
    form.append("mediaType", mediaType);

    return withApiCatch(
      () =>
        api
          .post<ApiResponse<ChatMediaDto>>(`/chat/chats/${chatId}/media`, form, {
            headers: { "Content-Type": "multipart/form-data" },
            timeout: 20 * 60 * 1000,
            maxContentLength: 35 * 1024 * 1024,
            maxBodyLength: 35 * 1024 * 1024,
          })
          .then((r) => r.data),
      "Не удалось загрузить медиа"
    );
  }

  replaceChatMedia(chatId: number, mediaId: number, file: File, mediaType: string) {
    const form = new FormData();
    form.append("file", file);
    form.append("mediaType", mediaType);

    return withApiCatch(
      () =>
        api
          .put<ApiResponse<ChatMediaDto>>(`/chat/chats/${chatId}/media/${mediaId}`, form, {
            headers: { "Content-Type": "multipart/form-data" },
            timeout: 20 * 60 * 1000,
            maxContentLength: 35 * 1024 * 1024,
            maxBodyLength: 35 * 1024 * 1024,
          })
          .then((r) => r.data),
      "Не удалось заменить медиа"
    );
  }

  deleteChatMedia(chatId: number, mediaId: number) {
    return withApiCatch(
      () =>
        api
          .delete<ApiResponse<boolean>>(`/chat/chats/${chatId}/media/${mediaId}`)
          .then((r) => r.data),
      "Не удалось удалить медиа"
    );
  }

  deleteAllChatMedia(chatId: number) {
    return withApiCatch(
      () =>
        api
          .delete<ApiResponse<boolean>>(`/chat/chats/${chatId}/media`)
          .then((r) => r.data),
      "Не удалось удалить медиа"
    );
  }

  updateGroupChat(chatId: number, payload: { name: string; isPublic?: boolean }) {
    return withApiCatch(
      () =>
        api
          .patch<ApiResponse<ChatListItemDto>>(`/chat/chats/${chatId}`, payload)
          .then((r) => r.data),
      "Не удалось обновить группу"
    );
  }

  getMessages(chatId: number, beforeMessageId?: number, limit = 50) {
    return withApiCatch(
      () =>
        api
          .get<ApiResponse<GetMessagesResponse>>(`/chat/chats/${chatId}/messages`, {
            params: {
              beforeMessageId: beforeMessageId ?? undefined,
              limit,
            },
          })
          .then((r) => r.data),
      "Не удалось загрузить сообщения"
    );
  }

  sendMessage(chatId: number, text: string, replyToMessageId?: number | null) {
    return withApiCatch(
      () =>
        api
          .post<ApiResponse<MessageDto>>(`/chat/chats/${chatId}/messages`, {
            text,
            replyToMessageId: replyToMessageId ?? null,
          })
          .then((r) => r.data),
      "Не удалось отправить сообщение"
    );
  }

  updateMessage(messageId: number, text: string) {
    return withApiCatch(
      () =>
        api
          .put<ApiResponse<MessageDto>>(`/chat/messages/${messageId}`, { text })
          .then((r) => r.data),
      "Не удалось изменить сообщение"
    );
  }

  deleteMessage(messageId: number) {
    return withApiCatch(
      () =>
        api
          .delete<ApiResponse<boolean>>(`/chat/messages/${messageId}`)
          .then((r) => r.data),
      "Не удалось удалить сообщение"
    );
  }

  deleteMessageMedia(messageId: number, mediaId: number) {
    return withApiCatch(
      () =>
        api
          .delete<ApiResponse<MessageDto>>(
            `/chat/messages/${messageId}/media/${mediaId}`
          )
          .then((r) => r.data),
      "Не удалось удалить медиа"
    );
  }

  uploadMessageMedia(messageId: number, file: File, mediaType: string) {
    const form = new FormData();
    form.append("file", file);
    form.append("mediaType", mediaType);

    return withApiCatch(
      () =>
        api
          .post<ApiResponse<MessageDto>>(`/chat/messages/${messageId}/media`, form, {
            headers: { "Content-Type": "multipart/form-data" },
            timeout: 20 * 60 * 1000,
            maxContentLength: 310 * 1024 * 1024,
            maxBodyLength: 310 * 1024 * 1024,
          })
          .then((r) => r.data),
      "Не удалось загрузить медиа"
    );
  }
}
