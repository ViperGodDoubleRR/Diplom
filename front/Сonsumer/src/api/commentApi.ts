import type { ApiResponse } from "@/interface/ApiContracts/ApiResponse";
import type { CommentDto } from "@/interface/DTO/comments/CommentDto";
import type { CommentMediaDto } from "@/interface/DTO/comments/CommentMediaDto";
import type { CreateCommentDto } from "@/interface/DTO/comments/CreateCommentDto";
import type { CreateCommentResponse } from "@/interface/DTO/comments/CreateCommentResponse";
import type { GetPostCommentsResponse } from "@/interface/DTO/comments/GetPostCommentsResponse";
import { api } from "./apiUrl";

export class CommentApi {
  async getPostComments(postId: string, offset = 0, limit = 50) {
    const res = await api.get<ApiResponse<GetPostCommentsResponse>>(
      `/comment/post/${postId}`,
      { params: { offset, limit } }
    );

    return res.data;
  }

  async getCommentReplies(parentCommentId: string, offset = 0, limit = 3) {
    const res = await api.get<ApiResponse<GetPostCommentsResponse>>(
      `/comment/${parentCommentId}/replies`,
      { params: { offset, limit } }
    );

    return res.data;
  }

  async createComment(dto: CreateCommentDto) {
    const res = await api.post<ApiResponse<CreateCommentResponse>>(
      "/comment",
      dto
    );

    return res.data;
  }

  async deleteComment(commentId: string) {
    const res = await api.delete<ApiResponse<boolean>>(
      `/comment/${commentId}`
    );

    return res.data;
  }

  async react(commentId: string, type: number) {
    const res = await api.post<ApiResponse<boolean>>(
      `/comment/${commentId}/reaction`,
      { type }
    );

    return res.data;
  }

  async uploadCommentMedia(commentId: string, file: File, mediaType: string) {
    const form = new FormData();
    form.append("file", file);
    form.append("mediaType", mediaType);

    const res = await api.post<ApiResponse<CommentMediaDto>>(
      `/comment/${commentId}/media`,
      form,
      {
        headers: { "Content-Type": "multipart/form-data" },
        timeout: 15 * 60 * 1000,
        maxContentLength: 35 * 1024 * 1024,
        maxBodyLength: 35 * 1024 * 1024,
      }
    );

    return res.data;
  }
}
