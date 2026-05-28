import type { PostMedia } from "./PostMedia";

export interface PostFull {
  id: string;
  userId: string;

  description: string;
  createdAt: string;



  media: PostMedia[];

  likesCount: number;
  favoritesCount: number;
  commentsCount: number;

  isLiked: boolean;
  isFavorite: boolean;
}
