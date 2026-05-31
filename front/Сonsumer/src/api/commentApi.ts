import type { ApiResponse } from "@/interface/ApiContracts/ApiResponse";
import type { CommentDto } from "@/interface/DTO/comments/CommentDto";
import type { CreateCommentDto } from "@/interface/DTO/comments/CreateCommentDto";
import { api } from "./apiUrl";

export class CommentApi {

  async getPostComments(postId: string) {
    const res = await api.get<ApiResponse<CommentDto[]>>(
      `/comments/post/${postId}`
    );

    return res.data;
  }

  async createComment(dto: CreateCommentDto): Promise<ApiResponse<boolean>> {
  const res = await api.post<ApiResponse<boolean>>(
    "/comment",
    dto
  );

  return res.data;
}

  async deleteComment(commentId: string) {
    const res = await api.delete<ApiResponse<boolean>>(
      `/comments/${commentId}`
    );

    return res.data;
  }

  async react(commentId: string, type: number) {
    const res = await api.post<ApiResponse<boolean>>(
      `/comments/${commentId}/reaction`,
      { type }
    );

    return res.data;
  }
}
