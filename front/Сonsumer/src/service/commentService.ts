import { CommentApi } from "@/api/commentApi";
import type { CreateCommentDto } from "@/interface/DTO/comments/CreateCommentDto";

export class CommentService {
  private api = new CommentApi();

  getPostComments(postId: string, offset = 0, limit = 50) {
    return this.api.getPostComments(postId, offset, limit);
  }

  getCommentReplies(parentCommentId: string, offset = 0, limit = 3) {
    return this.api.getCommentReplies(parentCommentId, offset, limit);
  }

  createComment(dto: CreateCommentDto) {
    return this.api.createComment(dto);
  }

  deleteComment(commentId: string) {
    return this.api.deleteComment(commentId);
  }

  react(commentId: string, type: number) {
    return this.api.react(commentId, type);
  }

  uploadCommentMedia(commentId: string, file: File, mediaType: string) {
    return this.api.uploadCommentMedia(commentId, file, mediaType);
  }
}
