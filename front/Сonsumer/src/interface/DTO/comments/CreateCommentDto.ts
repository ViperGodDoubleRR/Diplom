export interface CreateCommentDto {
  postId: string;

  parentId?: string | null;

  text: string;
}
