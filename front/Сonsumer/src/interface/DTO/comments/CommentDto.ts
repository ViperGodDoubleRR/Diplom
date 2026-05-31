import type { CommentMediaDto } from "./CommentMediaDto";
import type { CommentReactionSummaryDto } from "./CommentReactionSummaryDto";
import type { CommentUserDto } from "./CommentUserDto";

export interface CommentDto {
  id: string;
  postId: string;

  parentId: string | null;

  text: string;

  createdAt: string;
  updatedAt?: string | null;

  isDeleted: boolean;

  user: CommentUserDto;

  media: CommentMediaDto[];

  reactions: CommentReactionSummaryDto;

  repliesCount: number;
}
