import type { CommentDto } from "@/interface/DTO/comments/CommentDto";
import { CommentService } from "@/service/commentService";
import { defineStore } from "pinia";
import { useUserStore } from "@/store/userStore";
const service = new CommentService();
export const useCommentStore = defineStore("comment", {
  state: () => ({
    comments: [] as CommentDto[],
    loading: false
  }),
  actions: {
    async loadPostComments(postId: string) {
      this.loading = true;

      try {
        const res =
          await service.getPostComments(postId);

        if (res.success && res.data) {
          this.comments = res.data;
        }

        return res;
      }
      finally {
        this.loading = false;
      }
    },

    async createComment(postId: string, text: string, parentId?: string) {
  const res = await service.createComment({
    postId,
    text,
    parentId
  });

  if (res.success && res.data) {
    const userStore = useUserStore();

    const newComment: CommentDto = {
  id: crypto.randomUUID(),
  postId,
  parentId: parentId ?? null,
  text,
  createdAt: new Date().toISOString(),
  updatedAt: null,
  isDeleted: false,

  media: [],

  reactions: {
    likes: 0,
    dislikes: 0,
    loves: 0,
    angry: 0,
    myReaction: null
  },

  repliesCount: 0,

  user: {
    id: userStore.user?.id ?? "",
    login: userStore.user?.login ?? "",
    tag: userStore.user?.tag ?? "",
    avatar: userStore.user?.media?.[0]?.url ?? ""
  }
};

    this.comments.unshift(newComment);
  }

  return res;
},

    async deleteComment(commentId: string) {
      const res =
        await service.deleteComment(commentId);

      if (res.success) {
        this.comments =
          this.comments.filter(
            x => x.id !== commentId
          );
      }

      return res;
    }
  }
});
