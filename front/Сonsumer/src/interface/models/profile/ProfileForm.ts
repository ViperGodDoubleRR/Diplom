export interface ProfileForm {
  name: string;
  tag: string;
  description: string;

  avatar?: File | null;

  avatarUrl?: string | null;
}
