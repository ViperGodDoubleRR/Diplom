import type { ChatUserDto } from "./ChatUserDto";
import type { MessageMediaDto } from "./MessageMediaDto";

export interface MessageDto {
  id: number;
  chatId: number;
  user: ChatUserDto;
  text: string;
  replyToMessageId: number | null;
  createdAt: string;
  updatedAt: string | null;
  isEdited: boolean;
  isDeleted: boolean;
  media: MessageMediaDto[];
}
