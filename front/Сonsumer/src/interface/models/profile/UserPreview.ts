export interface UserPreview {
  id: string;
  login: string;
  tag?: string;
  avatarUrl?: string | null;
  avatarIsVideo?: boolean;
}