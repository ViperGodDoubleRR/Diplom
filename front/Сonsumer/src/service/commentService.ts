import { CommentApi } from "@/api/commentApi";
import type { CreateCommentDto } from "@/interface/DTO/comments/CreateCommentDto";

export class CommentService {
  private api = new CommentApi();

  getPostComments(postId: string) {
    return this.api.getPostComments(postId);
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
}
