import { ChatApi } from "@/api/chatApi";

export class ChatService {
  private api = new ChatApi();

  getMyChats() {
    return this.api.getMyChats();
  }

  getChatById(chatId: number) {
    return this.api.getChatById(chatId);
  }

  searchPublicGroups(search: string) {
    return this.api.searchPublicGroups(search);
  }

  createPrivateChat(targetUserId: string) {
    return this.api.createPrivateChat(targetUserId);
  }

  createGroupChat(name: string, isPublic: boolean) {
    return this.api.createGroupChat(name, isPublic);
  }

  inviteGroupMember(chatId: number, targetUserId: string) {
    return this.api.inviteGroupMember(chatId, targetUserId);
  }

  removeGroupMember(chatId: number, targetUserId: string) {
    return this.api.removeGroupMember(chatId, targetUserId);
  }

  deleteChat(chatId: number) {
    return this.api.deleteChat(chatId);
  }

  clearChatMessages(chatId: number) {
    return this.api.clearChatMessages(chatId);
  }

  joinPublicGroup(chatId: number) {
    return this.api.joinPublicGroup(chatId);
  }

  getChatMembers(chatId: number) {
    return this.api.getChatMembers(chatId);
  }

  getChatMedia(chatId: number) {
    return this.api.getChatMedia(chatId);
  }

  uploadChatMedia(chatId: number, file: File, mediaType: string) {
    return this.api.uploadChatMedia(chatId, file, mediaType);
  }

  replaceChatMedia(chatId: number, mediaId: number, file: File, mediaType: string) {
    return this.api.replaceChatMedia(chatId, mediaId, file, mediaType);
  }

  deleteChatMedia(chatId: number, mediaId: number) {
    return this.api.deleteChatMedia(chatId, mediaId);
  }

  deleteAllChatMedia(chatId: number) {
    return this.api.deleteAllChatMedia(chatId);
  }

  updateGroupChat(chatId: number, payload: { name: string; isPublic?: boolean }) {
    return this.api.updateGroupChat(chatId, payload);
  }

  getMessages(chatId: number, beforeMessageId?: number, limit = 50) {
    return this.api.getMessages(chatId, beforeMessageId, limit);
  }

  sendMessage(chatId: number, text: string, replyToMessageId?: number | null) {
    return this.api.sendMessage(chatId, text, replyToMessageId);
  }

  updateMessage(messageId: number, text: string) {
    return this.api.updateMessage(messageId, text);
  }

  deleteMessage(messageId: number) {
    return this.api.deleteMessage(messageId);
  }

  deleteMessageMedia(messageId: number, mediaId: number) {
    return this.api.deleteMessageMedia(messageId, mediaId);
  }

  uploadMessageMedia(messageId: number, file: File, mediaType: string) {
    return this.api.uploadMessageMedia(messageId, file, mediaType);
  }
}
