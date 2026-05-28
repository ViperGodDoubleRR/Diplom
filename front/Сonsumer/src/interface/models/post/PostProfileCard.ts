export interface PostProfileCard {
  id: string;
  description: string;
  createdAt: string;

  mediaUrl?: string;
  mediaType?: string;

  // NEW
  likesCount: number;
}
