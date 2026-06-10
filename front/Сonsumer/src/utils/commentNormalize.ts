import type { CommentDto } from "@/interface/DTO/comments/CommentDto";
import type { CommentMediaDto } from "@/interface/DTO/comments/CommentMediaDto";
import type { CommentReactionSummaryDto } from "@/interface/DTO/comments/CommentReactionSummaryDto";
import type { CommentUserDto } from "@/interface/DTO/comments/CommentUserDto";
import type { GetPostCommentsResponse } from "@/interface/DTO/comments/GetPostCommentsResponse";

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

function normalizeUser(raw: unknown): CommentUserDto {
  const item = (raw ?? {}) as Raw;
  return {
    id: readId(item, "id", "Id"),
    login: readString(item, "login", "Login") ?? "User",
    tag: readString(item, "tag", "Tag") ?? "",
    avatar: readString(item, "avatar", "Avatar") ?? "",
  };
}

function normalizeReactions(raw: unknown): CommentReactionSummaryDto {
  const item = (raw ?? {}) as Raw;
  const myReaction = item.myReaction ?? item.MyReaction;

  return {
    likes: readNumber(item, "likes", "Likes"),
    dislikes: readNumber(item, "dislikes", "Dislikes"),
    loves: readNumber(item, "loves", "Loves"),
    angry: readNumber(item, "angry", "Angry"),
    myReaction:
      typeof myReaction === "number" && Number.isFinite(myReaction)
        ? myReaction
        : null,
  };
}

export function normalizeCommentMedia(raw: unknown): CommentMediaDto {
  const item = (raw ?? {}) as Raw;
  return {
    id: readNumber(item, "id", "Id"),
    url: readString(item, "url", "Url") ?? "",
    mediaType: (readString(item, "mediaType", "MediaType") ?? "image").toLowerCase(),
    originalName: readString(item, "originalName", "OriginalName") ?? "",
  };
}

export function normalizeComment(raw: unknown): CommentDto {
  const item = (raw ?? {}) as Raw;
  const mediaRaw = item.media ?? item.Media;
  const media = Array.isArray(mediaRaw) ? mediaRaw.map(normalizeCommentMedia) : [];

  return {
    id: readId(item, "id", "Id"),
    postId: readId(item, "postId", "PostId"),
    parentId: readId(item, "parentId", "ParentId") || null,
    text: readString(item, "text", "Text") ?? "",
    createdAt: readString(item, "createdAt", "CreatedAt") ?? new Date().toISOString(),
    updatedAt: readString(item, "updatedAt", "UpdatedAt") ?? null,
    isDeleted: readBoolean(item, "isDeleted", "IsDeleted"),
    user: normalizeUser(item.user ?? item.User),
    media,
    reactions: normalizeReactions(item.reactions ?? item.Reactions),
    repliesCount: readNumber(item, "repliesCount", "RepliesCount"),
  };
}

export function normalizeComments(raw: unknown): CommentDto[] {
  if (!Array.isArray(raw)) return [];
  return raw.map(normalizeComment);
}

export function normalizeGetPostCommentsResponse(raw: unknown): GetPostCommentsResponse {
  const item = (raw ?? {}) as Raw;
  const itemsRaw = item.items ?? item.Items;

  return {
    items: normalizeComments(itemsRaw),
    totalCount: readNumber(item, "totalCount", "TotalCount"),
    allCommentsCount: readNumber(
      item,
      "allCommentsCount",
      "AllCommentsCount",
      "totalCount",
      "TotalCount"
    ),
    offset: readNumber(item, "offset", "Offset"),
    limit: readNumber(item, "limit", "Limit"),
    hasMore: readBoolean(item, "hasMore", "HasMore"),
  };
}
