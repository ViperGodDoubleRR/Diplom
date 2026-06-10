import type { CommentDto } from "./CommentDto";

export interface GetPostCommentsResponse {
  items: CommentDto[];
  totalCount: number;
  allCommentsCount: number;
  offset: number;
  limit: number;
  hasMore: boolean;
}
