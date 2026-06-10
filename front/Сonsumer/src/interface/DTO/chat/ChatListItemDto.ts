import type { ChatUserDto } from "./ChatUserDto";
import type { LastMessagePreviewDto } from "./LastMessagePreviewDto";

export interface ChatListItemDto {
  id: number;
  name: string | null;
  type: string;
  isPublic: boolean;
  isMember: boolean;
  myRole: string | null;
  avatarUrl: string | null;
  avatarIsVideo: boolean;
  companion: ChatUserDto | null;
  lastMessage: LastMessagePreviewDto | null;
  createdAt: string;
}
