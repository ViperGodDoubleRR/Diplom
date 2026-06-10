import type { ChatListItemDto } from "@/interface/DTO/chat/ChatListItemDto";
import type { ChatUserDto } from "@/interface/DTO/chat/ChatUserDto";
import type { GetMessagesResponse } from "@/interface/DTO/chat/GetMessagesResponse";
import type { LastMessagePreviewDto } from "@/interface/DTO/chat/LastMessagePreviewDto";
import type { MessageDto } from "@/interface/DTO/chat/MessageDto";
import type { MessageMediaDto } from "@/interface/DTO/chat/MessageMediaDto";

type Raw = Record<string, unknown>;

function readString(raw: Raw, ...keys: string[]): string | undefined {
  for (const key of keys) {
    const value = raw[key];
    if (typeof value === "string") return value;
  }
  return undefined;
}

function readNumber(raw: Raw, ...keys: string[]): number {
  for (const key of keys) {
    const value = raw[key];
    if (typeof value === "number" && Number.isFinite(value)) return value;
  }
  return 0;
}

function readBoolean(raw: Raw, ...keys: string[]): boolean {
  for (const key of keys) {
    const value = raw[key];
    if (typeof value === "boolean") return value;
  }
  return false;
}

function readId(raw: Raw, ...keys: string[]): string {
  for (const key of keys) {
    const value = raw[key];
    if (value != null) return String(value);
  }
  return "";
}

function normalizeUser(raw: unknown): ChatUserDto {
  const item = (raw ?? {}) as Raw;
  return {
    id: readId(item, "id", "Id"),
    login: readString(item, "login", "Login") ?? "User",
    tag: readString(item, "tag", "Tag") ?? "",
    avatar: readString(item, "avatar", "Avatar") ?? "",
    avatarIsVideo: readBoolean(item, "avatarIsVideo", "AvatarIsVideo"),
  };
}

function normalizeLastMessage(raw: unknown): LastMessagePreviewDto | null {
  if (!raw || typeof raw !== "object") return null;
  const item = raw as Raw;
  return {
    id: readNumber(item, "id", "Id"),
    userId: readId(item, "userId", "UserId"),
    userLogin: readString(item, "userLogin", "UserLogin") ?? "",
    text: readString(item, "text", "Text") ?? "",
    createdAt: readString(item, "createdAt", "CreatedAt") ?? new Date().toISOString(),
    isDeleted: readBoolean(item, "isDeleted", "IsDeleted"),
  };
}

function normalizeMedia(raw: unknown): MessageMediaDto {
  const item = (raw ?? {}) as Raw;
  return {
    id: readNumber(item, "id", "Id"),
    url: readString(item, "url", "Url") ?? "",
    mediaType: (readString(item, "mediaType", "MediaType") ?? "image").toLowerCase(),
    originalName: readString(item, "originalName", "OriginalName") ?? "",
    size: readNumber(item, "size", "Size"),
  };
}

export function normalizeMessage(raw: unknown): MessageDto {
  const item = (raw ?? {}) as Raw;
  const mediaRaw = item.media ?? item.Media;
  const media = Array.isArray(mediaRaw) ? mediaRaw.map(normalizeMedia) : [];

  return {
    id: readNumber(item, "id", "Id"),
    chatId: readNumber(item, "chatId", "ChatId"),
    user: normalizeUser(item.user ?? item.User),
    text: readString(item, "text", "Text") ?? "",
    replyToMessageId: readNumber(item, "replyToMessageId", "ReplyToMessageId") || null,
    createdAt: readString(item, "createdAt", "CreatedAt") ?? new Date().toISOString(),
    updatedAt: readString(item, "updatedAt", "UpdatedAt") ?? null,
    isEdited: readBoolean(item, "isEdited", "IsEdited"),
    isDeleted: readBoolean(item, "isDeleted", "IsDeleted"),
    media,
  };
}

export function normalizeMessages(raw: unknown): MessageDto[] {
  if (!Array.isArray(raw)) return [];
  return raw.map(normalizeMessage);
}

export function normalizeChatListItem(raw: unknown): ChatListItemDto {
  const item = (raw ?? {}) as Raw;
  const companionRaw = item.companion ?? item.Companion;

  return {
    id: readNumber(item, "id", "Id"),
    name: readString(item, "name", "Name") ?? null,
    type: readString(item, "type", "Type") ?? "Private",
    isPublic: readBoolean(item, "isPublic", "IsPublic"),
    isMember: readBoolean(item, "isMember", "IsMember"),
    myRole: readString(item, "myRole", "MyRole") ?? null,
    avatarUrl: readString(item, "avatarUrl", "AvatarUrl") ?? null,
    avatarIsVideo: readBoolean(item, "avatarIsVideo", "AvatarIsVideo"),
    companion: companionRaw ? normalizeUser(companionRaw) : null,
    lastMessage: normalizeLastMessage(item.lastMessage ?? item.LastMessage),
    createdAt: readString(item, "createdAt", "CreatedAt") ?? new Date().toISOString(),
  };
}

export function normalizeChatList(raw: unknown): ChatListItemDto[] {
  if (!Array.isArray(raw)) return [];
  return raw.map(normalizeChatListItem);
}

export function normalizeGetMessagesResponse(raw: unknown): GetMessagesResponse {
  const item = (raw ?? {}) as Raw;
  const itemsRaw = item.items ?? item.Items;

  return {
    items: normalizeMessages(itemsRaw),
    hasMore: readBoolean(item, "hasMore", "HasMore"),
  };
}

export function normalizeCreateChatResponse(raw: unknown): number {
  const item = (raw ?? {}) as Raw;
  return readNumber(item, "chatId", "ChatId");
}
