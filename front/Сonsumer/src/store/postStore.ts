import { defineStore } from "pinia";
import { PostService } from "@/service/postService";

import type { CreatePostDto } from "@/interface/models/post/CreatePostDto";
import type { PostProfileCard } from "@/interface/models/post/PostProfileCard";
import type { PostFull } from "@/interface/models/post/PostFull";
import type { PostReactionCard } from "@/interface/models/post/PostReactionCard";
import type { UpdatePostRequest } from "@/interface/models/post/UpdatePostRequest";

const service = new PostService();

export const usePostStore = defineStore("post", {
  state: () => ({
  profilePosts: [] as PostProfileCard[],
  feedPosts: [] as PostFull[],
  likedPosts: [] as PostReactionCard[],
  favoritePosts: [] as PostReactionCard[],
  post: null as PostFull | null,
   currentUser: {
    id: "",
    login: "",
    tag: "",
    avatar: "",
  },
  totalPosts: 0,
  totalLikes: 0,

  loading: false,
  createLoading: false,
}),

  actions: {

    // =====================
    // CREATE POST (STEP 1)
    // =====================
    async createPost(dto: CreatePostDto) {
      this.createLoading = true;

      try {
        const res = await service.createPost(dto);
        return res;
      } finally {
        this.createLoading = false;
      }
    },
    setCurrentUser(payload: {
  id: string;
  login: string;
  tag: string;
  avatar: string;
}) {
  this.currentUser = payload;
},
async uploadMedia(postId: string, file: File) {
  const form = new FormData();

  form.append("file", file);
  form.append("mediaType", "image");

  return await service.uploadPostMedia(postId, form);
},

   async getProfilePosts(userId: string, page = 1, pageSize = 12) {
  this.loading = true;

  try {
    const res = await service.getProfilePosts(userId, page, pageSize);

    if (res.success && res.data) {
      this.profilePosts = res.data;


      this.totalPosts = res.data.length;

      this.totalLikes = res.data.reduce((sum, post) => {
        return sum + (post.likesCount ?? 0);
      }, 0);
    }

    return res;
  } finally {
    this.loading = false;
  }
},
 async getUserFeedPosts(userId: string, page = 1, pageSize = 12) {
  this.loading = true;

  try {
    const res = await service.getUserFeedPosts(userId, page, pageSize);

    if (res.success && res.data) {
      this.feedPosts = res.data;
    }

    return res;
  } finally {
    this.loading = false;
  }
  },
  async toggleLike(postId: string) {
  const post = this.feedPosts.find(p => p.id === postId);
  if (!post) return;

  const prevLiked = post.isLiked;
  const prevCount = post.likesCount;

  // optimistic update
  post.isLiked = !prevLiked;
  post.likesCount += post.isLiked ? 1 : -1;

  try {
    if (post.isLiked) {
      await service.likePost(postId);
    } else {
      await service.unlikePost(postId);
    }
  } catch (e) {
    // rollback (ВАЖНО: через prev значения)
    post.isLiked = prevLiked;
    post.likesCount = prevCount;
    console.log(e);
  }
},

async toggleFavorite(postId: string) {
  const post = this.feedPosts.find(p => p.id === postId);
  if (!post) return;

  const prev = post.isFavorite;
  const prevCount = post.favoritesCount;

  // optimistic update
  post.isFavorite = !prev;
  post.favoritesCount += post.isFavorite ? 1 : -1;

  try {
    if (post.isFavorite) {
      await service.favoritePost(postId);
    } else {
      await service.unfavoritePost(postId);
    }
  } catch (e) {
    // rollback
    post.isFavorite = prev;
    post.favoritesCount = prevCount;
    console.log(e);
  }
},
async getLikedPosts(page = 1, pageSize = 12) {
  this.loading = true;

  try {
    const res = await service.getLikedPosts(page, pageSize);

    if (res.success && res.data) {
      this.likedPosts = res.data;
    }

    return res;
  } finally {
    this.loading = false;
  }
},
async getFavoritePosts(page = 1, pageSize = 12) {
  this.loading = true;

  try {
    const res = await service.getFavoritePosts(page, pageSize);

    if (res.success && res.data) {
      this.favoritePosts = res.data;
    }

    return res;
  } finally {
    this.loading = false;
  }
},
async updatePost(id: string, post: UpdatePostRequest) {
  const res = await service.updatePost(id, post);

  if (!res.success) return res;

  const target = this.feedPosts.find(p => p.id === id);

  if (target) {
    target.description = post.description;
    target.media = post.media;
  }

  await this.getUserFeedPosts(this.currentUser.id, 1, 12);

  return res;
},
async deletePost(postId: string) {
  const res = await service.deletePost(postId);

  if (!res.success) {
    return res;
  }

  this.feedPosts = this.feedPosts.filter(
    p => p.id !== postId
  );
  this.totalPosts = Math.max(0, this.totalPosts - 1);

  return res;
}
  }
});
