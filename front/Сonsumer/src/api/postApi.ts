import { api } from "@/api/apiUrl";
import type { ApiResponse } from "@/interface/ApiContracts/ApiResponse";
import type { CreatePostDto } from "@/interface/models/post/CreatePostDto";
import type { PostFull } from "@/interface/models/post/PostFull";
import type { PostProfileCard } from "@/interface/models/post/PostProfileCard";
import type { PostReactionCard } from "@/interface/models/post/PostReactionCard";
import type { UpdatePostRequest } from "@/interface/models/post/UpdatePostRequest";

export class PostApi {
  async createPost(dto: CreatePostDto): Promise<ApiResponse<{ id: string }>> {
    const res = await api.post<ApiResponse<{ id: string }>>(
      "/post/createpost",
      dto
    );

    return res.data;
  }
  async getProfilePosts(
    userId: string,
    page: number,
    pageSize: number
  ): Promise<ApiResponse<PostProfileCard[]>> {
    const res = await api.get<ApiResponse<PostProfileCard[]>>(
      `/post/user/${userId}/cards?page=${page}&pageSize=${pageSize}`
    );

    return res.data;
  }

 async uploadPostMedia(postId: string, form: FormData) {
  const res = await api.post<ApiResponse<any>>(
    `/post/${postId}/media`,
    form,
    {
      headers: {
        "Content-Type": "multipart/form-data",
      },
    }
  );

  return res.data;
}
async getUserFeedPosts(userId: string, page: number, pageSize: number) {
  const res = await api.get<ApiResponse<PostFull[]>>(
    `/post/user/${userId}/posts?page=${page}&pageSize=${pageSize}`
  );

  return res.data;
}
async likePost(postId: string) {
  const res = await api.post<ApiResponse<boolean>>(
    `/post/${postId}/like`
  );

  return res.data;
}

async unlikePost(postId: string) {
  const res = await api.delete<ApiResponse<boolean>>(
    `/post/${postId}/like`
  );

  return res.data;
}

async favoritePost(postId: string) {
  const res = await api.post<ApiResponse<boolean>>(
    `/post/${postId}/favorite`
  );

  return res.data;
}

async unfavoritePost(postId: string) {
  const res = await api.delete<ApiResponse<boolean>>(
    `/post/${postId}/favorite`
  );

  return res.data;
}
async getLikedPosts(page = 1, pageSize = 12) {
  const res = await api.get<ApiResponse<PostReactionCard[]>>(
    `/post/liked?page=${page}&pageSize=${pageSize}`
  );
  return res.data;
}

async getFavoritePosts(page = 1, pageSize = 12) {
  const res = await api.get<ApiResponse<PostReactionCard[]>>(
    `/post/favorites?page=${page}&pageSize=${pageSize}`
  );
  return res.data;
}
async updatePost(postId: string, data: UpdatePostRequest): Promise<ApiResponse<boolean>> {
  const res = await api.put<ApiResponse<boolean>>(`/post/${postId}`, data);
  return res.data;
}
async deletePost(postId: string): Promise<ApiResponse<boolean>> {
  const res = await api.delete<ApiResponse<boolean>>(
    `/post/${postId}`
  );

  return res.data;
}
}
