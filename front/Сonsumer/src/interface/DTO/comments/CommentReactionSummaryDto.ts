export interface CommentReactionSummaryDto {
  likes: number;

  dislikes: number;

  loves: number;

  angry: number;

  myReaction?: number | null;
}
