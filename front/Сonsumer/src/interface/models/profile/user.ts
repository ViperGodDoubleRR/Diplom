
import type { Media } from "./Media";

export interface User {
  id: string;
  login: string;
  email: string;
  tag?: string;
  description?: string;

  accessToken: string;
  refreshToken: string;

  media: Media[];
}
