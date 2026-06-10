import { defineStore } from "pinia";

import {
  COMMENT_REACTION_DISLIKE,
  COMMENT_REACTION_LIKE,
  COMMENT_REPLY_BATCH,
  COMMENT_ROOT_INITIAL,
  COMMENT_ROOT_LAZY,
  COMMENT_ROOT_SECOND,
} from "@/constants/commentConstants";
import type { CommentDto } from "@/interface/DTO/comments/CommentDto";
import type { CommentMediaDto } from "@/interface/DTO/comments/CommentMediaDto";
import { CommentService } from "@/service/commentService";
import { usePostStore } from "@/store/postStore";
import { useUserStore } from "@/store/userStore";
import { getApiData, isApiSuccess } from "@/utils/apiHelpers";
import {
  normalizeComment,
  normalizeCommentMedia,
  normalizeGetPostCommentsResponse,
} from "@/utils/commentNormalize";

const service = new CommentService();

export type CommentThreadState = {
  items: CommentDto[];
  offset: number;
  totalCount: number;
  allCommentsCount: number;
  hasMore: boolean;
  loadPhase: "initial" | "second" | "lazy";
  loading: boolean;
  loadingMore: boolean;
  opened: boolean;
};

function emptyThread(): CommentThreadState {
  return {
    items: [],
    offset: 0,
    totalCount: 0,
    allCommentsCount: 0,
    hasMore: false,
    loadPhase: "initial",
    loading: false,
    loadingMore: false,
    opened: false,
  };
}

function sameId(a?: string, b?: string) {
  return !!a && !!b && a.toLowerCase() === b.toLowerCase();
}

function readCommentId(payload: unknown): string {
  if (!payload || typeof payload !== "object") return "";

  const raw = payload as Record<string, unknown>;
  const id = raw.id ?? raw.Id;
  return id != null ? String(id) : "";
}

function buildUserComment(
  postId: string,
  commentId: string,
  text: string,
  parentId: string | null,
  media: CommentMediaDto[]
): CommentDto {
  const userStore = useUserStore();
  const avatar =
    userStore.user?.media?.find((m) => m.mediaType?.toLowerCase() === "image")
      ?.url ??
    userStore.user?.media?.[0]?.url ??
    "";

  return normalizeComment({
    id: commentId,
    postId,
    parentId,
    text,
    createdAt: new Date().toISOString(),
    updatedAt: null,
    isDeleted: false,
    media,
    reactions: {
      likes: 0,
      dislikes: 0,
      loves: 0,
      angry: 0,
      myReaction: null,
    },
    repliesCount: 0,
    user: {
      id: userStore.user?.id ?? "",
      login: userStore.user?.login ?? "You",
      tag: userStore.user?.tag ?? "",
      avatar,
    },
  });
}

function patchReaction(
  comment: CommentDto,
  type: number,
  active: boolean
): CommentDto {
  const prev = comment.reactions.myReaction;
  const next = { ...comment.reactions };

  if (prev === COMMENT_REACTION_LIKE) next.likes = Math.max(0, next.likes - 1);
  if (prev === COMMENT_REACTION_DISLIKE) {
    next.dislikes = Math.max(0, next.dislikes - 1);
  }

  if (active) {
    if (type === COMMENT_REACTION_LIKE) next.likes += 1;
    if (type === COMMENT_REACTION_DISLIKE) next.dislikes += 1;
    next.myReaction = type;
  } else {
    next.myReaction = null;
  }

  return { ...comment, reactions: next };
}

export const useCommentStore = defineStore("comment", {
  state: () => ({
    activePostId: "",
    rootByPost: {} as Record<string, CommentThreadState>,
    repliesByParent: {} as Record<string, CommentThreadState>,
    replyTargetId: null as string | null,
    sending: false,
  }),

  getters: {
    currentRoot(state): CommentThreadState {
      if (!state.activePostId) return emptyThread();
      return state.rootByPost[state.activePostId] ?? emptyThread();
    },

    rootComments(): CommentDto[] {
      return this.currentRoot.items;
    },

    rootHasMore(): boolean {
      return this.currentRoot.hasMore;
    },

    rootLoading(): boolean {
      return this.currentRoot.loading;
    },

    rootLoadingMore(): boolean {
      return this.currentRoot.loadingMore;
    },

    rootTotalCount(): number {
      return this.currentRoot.totalCount;
    },

    allCommentsCount(): number {
      const root = this.currentRoot;
      if (root.allCommentsCount > 0) return root.allCommentsCount;

      const feedPost = usePostStore().feedPosts.find((p) =>
        sameId(p.id, this.activePostId)
      );
      if (feedPost && feedPost.commentsCount > 0) return feedPost.commentsCount;

      return root.totalCount;
    },
  },

  actions: {
    ensureRoot(postId: string): CommentThreadState {
      if (!this.rootByPost[postId]) {
        this.rootByPost[postId] = emptyThread();
      }
      return this.rootByPost[postId];
    },

    ensureReplies(parentId: string): CommentThreadState {
      if (!this.repliesByParent[parentId]) {
        this.repliesByParent[parentId] = emptyThread();
      }
      return this.repliesByParent[parentId];
    },

    getReplies(parentId: string): CommentDto[] {
      return this.repliesByParent[parentId]?.items ?? [];
    },

    repliesOpened(parentId: string): boolean {
      return this.repliesByParent[parentId]?.opened ?? false;
    },

    repliesHasMore(parentId: string): boolean {
      return this.repliesByParent[parentId]?.hasMore ?? false;
    },

    repliesLoadingMore(parentId: string): boolean {
      return this.repliesByParent[parentId]?.loadingMore ?? false;
    },

    repliesLoading(parentId: string): boolean {
      return this.repliesByParent[parentId]?.loading ?? false;
    },

    resetActivePost() {
      this.activePostId = "";
      this.replyTargetId = null;
      this.sending = false;
    },

    clearPostCache(postId: string) {
      delete this.rootByPost[postId];
      for (const parentId of Object.keys(this.repliesByParent)) {
        const reply = this.repliesByParent[parentId]?.items[0];
        if (reply?.postId === postId) {
          delete this.repliesByParent[parentId];
        }
      }
    },

    async loadPostComments(postId: string, force = false): Promise<boolean> {
      this.activePostId = postId;
      const thread = this.ensureRoot(postId);

      const feedPost = usePostStore().feedPosts.find((p) => sameId(p.id, postId));
      if (feedPost && thread.allCommentsCount === 0) {
        thread.allCommentsCount = feedPost.commentsCount;
      }

      if (!force && thread.items.length && !thread.loading) {
        return true;
      }

      thread.loading = true;
      thread.loadPhase = "initial";
      thread.offset = 0;

      try {
        const res = await service.getPostComments(postId, 0, COMMENT_ROOT_INITIAL);

        if (isApiSuccess(res) && res.data) {
          const batch = normalizeGetPostCommentsResponse(res.data);
          thread.items = batch.items;
          thread.offset = batch.items.length;
          thread.totalCount = batch.totalCount;
          thread.hasMore = batch.hasMore;
          thread.loadPhase = batch.hasMore ? "second" : "lazy";
          this.syncModalCommentsCount(postId, batch.allCommentsCount);
          return true;
        }

        return false;
      } catch {
        return false;
      } finally {
        thread.loading = false;
      }
    },

    async loadMoreRootComments() {
      const postId = this.activePostId;
      if (!postId) return;

      const thread = this.ensureRoot(postId);
      if (!thread.hasMore || thread.loadingMore || thread.loading) return;

      const limit =
        thread.loadPhase === "second" ? COMMENT_ROOT_SECOND : COMMENT_ROOT_LAZY;

      thread.loadingMore = true;

      try {
        const res = await service.getPostComments(postId, thread.offset, limit);

        if (isApiSuccess(res) && res.data) {
          const batch = normalizeGetPostCommentsResponse(res.data);
          const existing = new Set(thread.items.map((c) => c.id));

          thread.items = [
            ...thread.items,
            ...batch.items.filter((c) => !existing.has(c.id)),
          ];
          thread.offset += batch.items.length;
          thread.totalCount = batch.totalCount;
          thread.hasMore = batch.hasMore;

          if (thread.loadPhase === "second") {
            thread.loadPhase = "lazy";
          }
        }
      } finally {
        thread.loadingMore = false;
      }
    },

    findRootComment(commentId: string): CommentDto | undefined {
      if (!this.activePostId) return undefined;
      const root = this.rootByPost[this.activePostId];
      return root?.items.find((c) => sameId(c.id, commentId));
    },

    syncReplyHasMore(parentId: string) {
      const thread = this.ensureReplies(parentId);
      const parent = this.findRootComment(parentId);
      const total = Math.max(thread.totalCount, parent?.repliesCount ?? 0);
      thread.totalCount = total;
      thread.hasMore = thread.offset < total;
    },

    syncModalCommentsCount(postId: string, count: number) {
      const root = this.ensureRoot(postId);
      root.allCommentsCount = Math.max(0, count);
    },

    bumpAllCommentsCount(postId: string, delta: number) {
      const root = this.ensureRoot(postId);
      const feedPost = usePostStore().feedPosts.find((p) => sameId(p.id, postId));
      const base = root.allCommentsCount || feedPost?.commentsCount || root.totalCount;
      root.allCommentsCount = Math.max(0, base + delta);
      usePostStore().changePostCommentsCount(postId, delta);
    },

    async fetchRepliesBatch(parentId: string, append: boolean) {
      const thread = this.ensureReplies(parentId);
      const loadingMore = append;

      if (loadingMore) {
        if (thread.loadingMore || thread.loading) return;
        thread.loadingMore = true;
      } else {
        if (thread.loading) return;
        thread.loading = true;
      }

      try {
        const res = await service.getCommentReplies(
          parentId,
          thread.offset,
          COMMENT_REPLY_BATCH
        );

        if (isApiSuccess(res) && res.data) {
          const batch = normalizeGetPostCommentsResponse(res.data);

          if (append) {
            const existing = new Set(thread.items.map((c) => c.id));
            thread.items = [
              ...thread.items,
              ...batch.items.filter((c) => !existing.has(c.id)),
            ];
          } else {
            thread.items = batch.items;
          }

          thread.offset += batch.items.length;
          thread.totalCount = batch.totalCount;
          thread.hasMore = batch.hasMore;
        }
      } finally {
        if (loadingMore) {
          thread.loadingMore = false;
        } else {
          thread.loading = false;
        }
      }
    },

    async reloadRepliesWindow(parentId: string, limit: number) {
      const thread = this.ensureReplies(parentId);
      const safeLimit = Math.max(limit, COMMENT_REPLY_BATCH);

      thread.loading = true;

      try {
        const res = await service.getCommentReplies(parentId, 0, safeLimit);

        if (isApiSuccess(res) && res.data) {
          const batch = normalizeGetPostCommentsResponse(res.data);
          thread.items = batch.items;
          thread.offset = batch.items.length;
          thread.totalCount = batch.totalCount;
          thread.hasMore = batch.hasMore;
        }
      } finally {
        thread.loading = false;
      }
    },

    closeReplies(parentId: string) {
      const thread = this.repliesByParent[parentId];
      if (!thread) return;
      thread.opened = false;
    },

    async openReplies(parentId: string) {
      const thread = this.ensureReplies(parentId);
      thread.opened = true;
      this.syncReplyHasMore(parentId);

      if (thread.loading || thread.loadingMore) return;

      if (thread.offset === 0 && thread.items.length === 0) {
        await this.fetchRepliesBatch(parentId, false);
      }
    },

    async loadMoreReplies(parentId: string) {
      const thread = this.ensureReplies(parentId);
      this.syncReplyHasMore(parentId);

      if (!thread.hasMore || thread.loadingMore || thread.loading) return;

      await this.fetchRepliesBatch(parentId, true);
    },

    setReplyTarget(commentId: string | null) {
      this.replyTargetId = commentId;
    },

    async createComment(
      postId: string,
      text: string,
      file?: File | null,
      parentId?: string | null
    ) {
      this.sending = true;

      try {
        const res = await service.createComment({
          postId,
          text,
          parentId: parentId ?? null,
        });

        const createdPayload = getApiData(res);
        const createdId = createdPayload
          ? readCommentId(createdPayload)
          : "";

        if (!isApiSuccess(res) || !createdId) {
          return res;
        }

        const commentId = createdId;
        let media: CommentMediaDto[] = [];

        if (file) {
          const mediaType = file.type.startsWith("video") ? "video" : "image";
          const mediaRes = await service.uploadCommentMedia(
            commentId,
            file,
            mediaType
          );

          const mediaPayload = getApiData(mediaRes);
          if (isApiSuccess(mediaRes) && mediaPayload) {
            media = [normalizeCommentMedia(mediaPayload)];
          }
        }

        const created = buildUserComment(
          postId,
          commentId,
          text,
          parentId ?? null,
          media
        );

        if (parentId) {
          const replyThread = this.ensureReplies(parentId);
          replyThread.opened = true;

          const root = this.ensureRoot(postId);
          const parent = root.items.find((c) => sameId(c.id, parentId));
          if (parent) {
            parent.repliesCount += 1;
            replyThread.totalCount = parent.repliesCount;
          }

          this.bumpAllCommentsCount(postId, 1);

          const reloadLimit = Math.max(
            replyThread.items.length + 1,
            COMMENT_REPLY_BATCH
          );
          await this.reloadRepliesWindow(parentId, reloadLimit);
        } else {
          const root = this.ensureRoot(postId);
          root.items.unshift(created);
          root.offset += 1;
          root.totalCount += 1;
          this.bumpAllCommentsCount(postId, 1);
        }

        this.replyTargetId = null;
        return res;
      } finally {
        this.sending = false;
      }
    },

    async deleteComment(commentId: string, parentId?: string | null) {
      const res = await service.deleteComment(commentId);
      if (!isApiSuccess(res)) return res;

      const postId = this.activePostId;

      if (parentId) {
        const thread = this.ensureReplies(parentId);
        thread.items = thread.items.filter((c) => !sameId(c.id, commentId));
        thread.offset = Math.max(0, thread.offset - 1);
        thread.totalCount = Math.max(0, thread.totalCount - 1);
        thread.hasMore = thread.offset < thread.totalCount;

        if (postId) {
          const root = this.ensureRoot(postId);
          const parent = root.items.find((c) => sameId(c.id, parentId));
          if (parent) {
            parent.repliesCount = Math.max(0, parent.repliesCount - 1);
          }
          this.bumpAllCommentsCount(postId, -1);
        }
      } else if (postId) {
        const root = this.ensureRoot(postId);
        root.items = root.items.filter((c) => !sameId(c.id, commentId));
        root.offset = Math.max(0, root.offset - 1);
        root.totalCount = Math.max(0, root.totalCount - 1);
        this.bumpAllCommentsCount(postId, -1);
      }

      delete this.repliesByParent[commentId];
      if (this.replyTargetId && sameId(this.replyTargetId, commentId)) {
        this.replyTargetId = null;
      }

      return res;
    },

    async toggleReaction(commentId: string, type: number, parentId?: string | null) {
      const updateList = (items: CommentDto[]) => {
        const idx = items.findIndex((c) => sameId(c.id, commentId));
        if (idx === -1) return items;

        const current = items[idx]!;
        const isSame = current.reactions.myReaction === type;
        const next = [...items];
        next[idx] = patchReaction(current, type, !isSame);
        return next;
      };

      const postId = this.activePostId;
      if (parentId) {
        const thread = this.ensureReplies(parentId);
        thread.items = updateList(thread.items);
      } else if (postId) {
        const root = this.ensureRoot(postId);
        root.items = updateList(root.items);
      }

      const res = await service.react(commentId, type);

      if (!isApiSuccess(res)) {
        await this.loadPostComments(postId, true);
        if (parentId) {
          const thread = this.ensureReplies(parentId);
          thread.items = [];
          thread.offset = 0;
          await this.openReplies(parentId);
        }
      }

      return res;
    },
  },
});
