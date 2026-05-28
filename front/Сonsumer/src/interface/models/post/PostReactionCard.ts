export interface PostReactionCard {
  id: string;
  description: string;
  createdAt: string;

  mediaUrl?: string;
  mediaType?: string;

  likesCount: number;
  favoritesCount: number;

  isLiked: boolean;
  isFavorite: boolean;

  // user info
  userLogin: string;
  userTag: string;
  userAvatar: string;
  userId: string;
}
