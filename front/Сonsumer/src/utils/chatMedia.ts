import type { ChatMediaDto } from "@/interface/DTO/chat/ChatMediaDto";
import type { Media } from "@/interface/models/profile/Media";
import { MediaType } from "@/interface/models/profile/MediaType";

export function chatMediaToGallery(item: ChatMediaDto): Media {
  return {
    id: item.id,
    fileKey: "",
    bucket: "",
    mediaType:
      item.mediaType.toLowerCase() === "video" ? MediaType.VIDEO : MediaType.AVATAR,
    contentType: "",
    url: item.url,
    createdAt: item.createdAt,
  };
}

export function chatMediaListToGallery(items: ChatMediaDto[]): Media[] {
  return items.map(chatMediaToGallery);
}
