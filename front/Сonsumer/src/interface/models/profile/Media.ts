import type { MediaType } from "./MediaType";
export interface Media {
  id: number;

  fileKey: string;

  bucket: string;

  mediaType: MediaType;

  contentType: string;

  url?: string;

  isPrimary?: boolean;

  createdAt?: string;
}
