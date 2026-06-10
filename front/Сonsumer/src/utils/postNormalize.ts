import type { PostFull } from "@/interface/models/post/PostFull";
import type { PostMedia } from "@/interface/models/post/PostMedia";
import type { PostProfileCard } from "@/interface/models/post/PostProfileCard";
import type { PostReactionCard } from "@/interface/models/post/PostReactionCard";

type Raw = Record<string, unknown>;

function readString(raw: Raw, ...keys: string[]): string | undefined {
  for (const key of keys) {
    const value = raw[key];
    if (typeof value === "string") {
      return value;
    }
  }

  return undefined;
}

function readNumber(raw: Raw, ...keys: string[]): number {
  for (const key of keys) {
    const value = raw[key];
    if (typeof value === "number" && Number.isFinite(value)) {
      return value;
    }
  }

  return 0;
}

function readBoolean(raw: Raw, ...keys: string[]): boolean {
  for (const key of keys) {
    const value = raw[key];
    if (typeof value === "boolean") {
      return value;
    }
  }

  return false;
}

function readId(raw: Raw, ...keys: string[]): string {
  for (const key of keys) {
    const value = raw[key];
    if (value != null) {
      return String(value);
    }
  }

  return "";
}

function normalizeMedia(raw: unknown): PostMedia {
  const item = (raw ?? {}) as Raw;

  return {
    id: readId(item, "id", "Id"),
    url: readString(item, "url", "Url") ?? "",
    fileKey: readString(item, "fileKey", "FileKey") ?? "",
    bucket: readString(item, "bucket", "Bucket") ?? "",
    mediaType: (readString(item, "mediaType", "MediaType") ?? "image").toLowerCase(),
  };
}

export function normalizePostProfileCard(raw: unknown): PostProfileCard {
  const item = (raw ?? {}) as Raw;

  return {
    id: readId(item, "id", "Id"),
    description: readString(item, "description", "Description") ?? "",
    createdAt: readString(item, "createdAt", "CreatedAt") ?? new Date().toISOString(),
    mediaUrl: readString(item, "mediaUrl", "MediaUrl"),
    mediaType: readString(item, "mediaType", "MediaType"),
    likesCount: readNumber(item, "likesCount", "LikesCount"),
  };
}

export function normalizePostReactionCard(raw: unknown): PostReactionCard {
  const item = (raw ?? {}) as Raw;

  return {
    id: readId(item, "id", "Id"),
    description: readString(item, "description", "Description") ?? "",
    createdAt: readString(item, "createdAt", "CreatedAt") ?? new Date().toISOString(),
    mediaUrl: readString(item, "mediaUrl", "MediaUrl"),
    mediaType: readString(item, "mediaType", "MediaType"),
    likesCount: readNumber(item, "likesCount", "LikesCount"),
    favoritesCount: readNumber(item, "favoritesCount", "FavoritesCount"),
    isLiked: readBoolean(item, "isLiked", "IsLiked"),
    isFavorite: readBoolean(item, "isFavorite", "IsFavorite"),
    userId: readId(item, "userId", "UserId"),
    userLogin: readString(item, "userLogin", "UserLogin") ?? "User",
    userTag: readString(item, "userTag", "UserTag") ?? "",
    userAvatar: readString(item, "userAvatar", "UserAvatar") ?? "",
  };
}

export function normalizePostFull(raw: unknown): PostFull {
  const item = (raw ?? {}) as Raw;
  const mediaRaw = item.media ?? item.Media;
  const media = Array.isArray(mediaRaw) ? mediaRaw.map(normalizeMedia) : [];

  return {
    id: readId(item, "id", "Id"),
    userId: readId(item, "userId", "UserId"),
    description: readString(item, "description", "Description") ?? "",
    createdAt: readString(item, "createdAt", "CreatedAt") ?? new Date().toISOString(),
    media,
    likesCount: readNumber(item, "likesCount", "LikesCount"),
    favoritesCount: readNumber(item, "favoritesCount", "FavoritesCount"),
    commentsCount: readNumber(item, "commentsCount", "CommentsCount"),
    isLiked: readBoolean(item, "isLiked", "IsLiked"),
    isFavorite: readBoolean(item, "isFavorite", "IsFavorite"),
    userLogin: readString(item, "userLogin", "UserLogin"),
    userTag: readString(item, "userTag", "UserTag"),
    userAvatar: readString(item, "userAvatar", "UserAvatar"),
    isFriend: readBoolean(item, "isFriend", "IsFriend"),
  };
}

export function normalizePostProfileCards(raw: unknown): PostProfileCard[] {
  if (!Array.isArray(raw)) return [];
  return raw.map(normalizePostProfileCard);
}

export function normalizePostReactionCards(raw: unknown): PostReactionCard[] {
  if (!Array.isArray(raw)) return [];
  return raw.map(normalizePostReactionCard);
}

export function normalizePostFullList(raw: unknown): PostFull[] {
  if (!Array.isArray(raw)) return [];
  return raw.map(normalizePostFull);
}

export function normalizePostMedia(raw: unknown): PostMedia {
  return normalizeMedia(raw);
}

export function isVideoMediaType(mediaType?: string): boolean {
  return mediaType?.toLowerCase() === "video";
}
