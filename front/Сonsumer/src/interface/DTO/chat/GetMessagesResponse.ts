import type { MessageDto } from "./MessageDto";

export interface GetMessagesResponse {
  items: MessageDto[];
  hasMore: boolean;
}
