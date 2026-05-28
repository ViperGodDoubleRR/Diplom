import type { Media } from "./Media";

export interface ViewUser {
  id: string;

  login: string;
  tag: string;
  description?: string;

  media?: Media[];
}
