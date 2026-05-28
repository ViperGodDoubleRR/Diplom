import type { PostMedia } from "./PostMedia";

export interface UpdatePostRequest {
  description: string;
  media: PostMedia[];
}
