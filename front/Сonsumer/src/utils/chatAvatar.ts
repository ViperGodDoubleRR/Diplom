import type { ChatListItemDto } from "@/interface/DTO/chat/ChatListItemDto";
import type { ChatMediaDto } from "@/interface/DTO/chat/ChatMediaDto";
import type { ChatUserDto } from "@/interface/DTO/chat/ChatUserDto";
import { isVideoMediaType } from "@/utils/postNormalize";

export type ChatAvatarPreview = {
  url: string;
  isVideo: boolean;
};

const EMPTY_PREVIEW: ChatAvatarPreview = { url: "", isVideo: false };

export function resolveAvatarFromGroupMedia(media: ChatMediaDto[]): ChatAvatarPreview {
  if (!media.length) return EMPTY_PREVIEW;

  const sorted = [...media].sort(
    (a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
  );

  const latest =
    sorted.find((item) => {
      const type = item.mediaType.toLowerCase();
      return type === "avatar" || type === "image" || type === "video";
    }) ?? sorted[0];

  return {
    url: latest.url,
    isVideo: isVideoMediaType(latest.mediaType),
  };
}

export function resolveUserAvatar(user: {
  avatar?: string | null;
  avatarUrl?: string | null;
  avatarIsVideo?: boolean;
}): ChatAvatarPreview {
  const url = (user.avatarUrl ?? user.avatar ?? "").trim();
  if (!url) return EMPTY_PREVIEW;

  return {
    url,
    isVideo: user.avatarIsVideo ?? false,
  };
}

export function resolveChatAvatarPreview(
  chat: ChatListItemDto,
  groupMedia?: ChatMediaDto[]
): ChatAvatarPreview {
  if (chat.type === "Group") {
    if (chat.avatarUrl) {
      return {
        url: chat.avatarUrl,
        isVideo: chat.avatarIsVideo ?? false,
      };
    }

    if (groupMedia?.length) {
      return resolveAvatarFromGroupMedia(groupMedia);
    }

    return EMPTY_PREVIEW;
  }

  if (chat.companion) {
    return resolveUserAvatar({
      avatar: chat.companion.avatar,
      avatarIsVideo: chat.companion.avatarIsVideo,
    });
  }

  return EMPTY_PREVIEW;
}
