import { PostApi } from "@/api/postApi";
import type { ApiResponse } from "@/interface/ApiContracts/ApiResponse";
import type { CreatePostDto } from "@/interface/models/post/CreatePostDto";
import type { UpdatePostRequest } from "@/interface/models/post/UpdatePostRequest";


export class PostService {
  private api = new PostApi();

  createPost(dto: CreatePostDto) {
    return this.api.createPost(dto);
  }

  getProfilePosts(userId: string, page: number, pageSize: number) {
    return this.api.getProfilePosts(userId, page, pageSize);
  }

  uploadPostMedia(postId: string, form: FormData) {
    return this.api.uploadPostMedia(postId, form);
  }

   getUserFeedPosts(userId: string, page: number, pageSize: number) {
  return this.api.getUserFeedPosts(userId, page, pageSize);
}
likePost(postId: string) {
  return this.api.likePost(postId);
}

unlikePost(postId: string) {
  return this.api.unlikePost(postId);
}

favoritePost(postId: string) {
  return this.api.favoritePost(postId);
}

unfavoritePost(postId: string) {
  return this.api.unfavoritePost(postId);
}
getLikedPosts(page: number = 1, pageSize: number = 12) {
  return this.api.getLikedPosts(page, pageSize);
}
getFavoritePosts(page: number = 1, pageSize: number = 12) {
  return this.api.getFavoritePosts(page, pageSize);
}
updatePost(postId: string, data: UpdatePostRequest): Promise<ApiResponse<boolean>> {
  return this.api.updatePost(postId, data);
}
deletePost(postId: string): Promise<ApiResponse<boolean>> {
  return this.api.deletePost(postId);
}
}
