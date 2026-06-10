import { defineStore } from "pinia";

import { PostService } from "@/service/postService";
import { useSocialStore } from "@/store/socialStore";
import { useUserStore } from "@/store/userStore";

import type { CreatePostDto } from "@/interface/models/post/CreatePostDto";
import type { PostFull } from "@/interface/models/post/PostFull";
import type { PostProfileCard } from "@/interface/models/post/PostProfileCard";
import type { PostReactionCard } from "@/interface/models/post/PostReactionCard";
import type { UpdatePostRequest } from "@/interface/models/post/UpdatePostRequest";
import {
  normalizePostFullList,
  normalizePostMedia,
  normalizePostProfileCards,
  normalizePostReactionCards,
} from "@/utils/postNormalize";
import { apiErrorMessage, isApiSuccess } from "@/utils/apiHelpers";

const service = new PostService();

function sameId(a?: string, b?: string): boolean {
  return !!a && !!b && a.toLowerCase() === b.toLowerCase();
}

function findFeedPostIndex(posts: PostFull[], postId: string): number {
  return posts.findIndex((p) => sameId(p.id, postId));
}

function findFeedPost(posts: PostFull[], postId: string): PostFull | undefined {
  const idx = findFeedPostIndex(posts, postId);
  return idx === -1 ? undefined : posts[idx];
}

function patchFeedPost(
  posts: PostFull[],
  postId: string,
  patch: Partial<
    Pick<
      PostFull,
      "isLiked" | "isFavorite" | "likesCount" | "favoritesCount" | "commentsCount"
    >
  >
): PostFull[] {
  const idx = findFeedPostIndex(posts, postId);
  if (idx === -1) return posts;

  const current = posts[idx];
  const next = { ...current, ...patch } as PostFull;
  return [...posts.slice(0, idx), next, ...posts.slice(idx + 1)];
}

function syncReactionCard(
  cards: PostReactionCard[],
  postId: string,
  patch: Partial<Pick<PostReactionCard, "isLiked" | "isFavorite" | "likesCount" | "favoritesCount">>
): PostReactionCard[] {
  const idx = cards.findIndex((p) => sameId(p.id, postId));
  if (idx === -1) return cards;

  const current = cards[idx];
  const next = { ...current, ...patch } as PostReactionCard;
  return [...cards.slice(0, idx), next, ...cards.slice(idx + 1)];
}

function upsertLikedCard(post: PostFull, cards: PostReactionCard[]): PostReactionCard[] {
  const media = post.media[0];
  const card: PostReactionCard = {
    id: post.id,
    description: post.description,
    createdAt: post.createdAt,
    mediaUrl: media?.url,
    mediaType: media?.mediaType,
    likesCount: post.likesCount,
    favoritesCount: post.favoritesCount,
    isLiked: true,
    isFavorite: post.isFavorite,
    userId: post.userId,
    userLogin: post.userLogin ?? "User",
    userTag: post.userTag ?? "",
    userAvatar: post.userAvatar ?? "",
  };

  const idx = cards.findIndex((p) => sameId(p.id, post.id));
  if (idx === -1) return [card, ...cards];

  const next = [...cards];
  next[idx] = { ...next[idx], ...card };
  return next;
}

function upsertFavoriteCard(post: PostFull, cards: PostReactionCard[]): PostReactionCard[] {
  const media = post.media[0];
  const card: PostReactionCard = {
    id: post.id,
    description: post.description,
    createdAt: post.createdAt,
    mediaUrl: media?.url,
    mediaType: media?.mediaType,
    likesCount: post.likesCount,
    favoritesCount: post.favoritesCount,
    isLiked: post.isLiked,
    isFavorite: true,
    userId: post.userId,
    userLogin: post.userLogin ?? "User",
    userTag: post.userTag ?? "",
    userAvatar: post.userAvatar ?? "",
  };

  const idx = cards.findIndex((p) => sameId(p.id, post.id));
  if (idx === -1) return [card, ...cards];

  const next = [...cards];
  next[idx] = { ...next[idx], ...card };
  return next;
}

function removeReactionCard(cards: PostReactionCard[], postId: string): PostReactionCard[] {
  return cards.filter((p) => !sameId(p.id, postId));
}

export const PAGE_SIZE = 20;

export type HomeFeedMode = "friends" | "discover";

type FeedAuthor = {
  id: string;
  login: string;
  tag?: string;
  avatarUrl?: string | null;
  isFriend: boolean;
};

export const usePostStore = defineStore("post", {
  state: () => ({
    profilePosts: [] as PostProfileCard[],
    profilePage: 1,
    profileHasMore: true,
    profileUserId: "",

    feedPosts: [] as PostFull[],
    feedPage: 1,
    feedHasMore: true,
    feedUserId: "",

    homeAuthors: [] as FeedAuthor[],
    homeAuthorCursors: {} as Record<string, number>,
    homeAuthorExhausted: {} as Record<string, boolean>,
    homeFeedHasMore: true,
    homeFeedMode: "friends" as HomeFeedMode,

    likedPosts: [] as PostReactionCard[],
    likedPage: 1,
    likedHasMore: true,

    favoritePosts: [] as PostReactionCard[],
    favoritePage: 1,
    favoriteHasMore: true,

    post: null as PostFull | null,
    currentUser: {
      id: "",
      login: "",
      tag: "",
      avatar: "",
    },
    totalPosts: 0,
    totalLikes: 0,
    loading: false,
    loadingMore: false,
    createLoading: false,
  }),

  actions: {
    async createPost(dto: CreatePostDto) {
      this.createLoading = true;

      try {
        return await service.createPost(dto);
      } finally {
        this.createLoading = false;
      }
    },

    setCurrentUser(payload: {
      id: string;
      login: string;
      tag: string;
      avatar?: string | null;
    }) {
      this.currentUser = {
        id: payload.id,
        login: payload.login,
        tag: payload.tag,
        avatar: payload.avatar ?? "",
      };
    },

    async uploadMedia(postId: string, file: File) {
      const form = new FormData();
      form.append("file", file);
      form.append(
        "mediaType",
        file.type.startsWith("video") ? "video" : "image"
      );

      const res = await service.uploadPostMedia(postId, form);

      if (res.success && res.data) {
        return {
          ...res,
          data: normalizePostMedia(res.data),
        };
      }

      return res;
    },

    resetProfilePosts() {
      this.profilePosts = [];
      this.profilePage = 1;
      this.profileHasMore = true;
    },

    async getProfilePosts(
      userId: string,
      page = 1,
      pageSize = PAGE_SIZE,
      append = false
    ) {
      if (append) {
        if (!this.profileHasMore || this.loadingMore) return;
        this.loadingMore = true;
      } else {
        this.loading = true;
        this.profileUserId = userId;
        this.profilePage = 1;
        this.profileHasMore = true;
      }

      try {
        const res = await service.getProfilePosts(userId, page, pageSize);

        if (res.success && res.data) {
          const batch = normalizePostProfileCards(res.data);

          if (append) {
            const existing = new Set(this.profilePosts.map((p) => p.id));
            this.profilePosts = [
              ...this.profilePosts,
              ...batch.filter((p) => !existing.has(p.id)),
            ];
          } else {
            this.profilePosts = batch;
          }

          this.profilePage = page;
          this.profileHasMore = batch.length >= pageSize;
          this.totalPosts = this.profilePosts.length;
          this.totalLikes = this.profilePosts.reduce(
            (sum, post) => sum + (post.likesCount ?? 0),
            0
          );
        }

        return res;
      } finally {
        this.loading = false;
        this.loadingMore = false;
      }
    },

    loadMoreProfilePosts(userId: string) {
      if (!this.profileHasMore || this.loadingMore) return;
      return this.getProfilePosts(userId, this.profilePage + 1, PAGE_SIZE, true);
    },

    resetUserFeed() {
      this.feedPosts = [];
      this.feedPage = 1;
      this.feedHasMore = true;
    },

    async getUserFeedPosts(
      userId: string,
      page = 1,
      pageSize = PAGE_SIZE,
      append = false
    ) {
      if (append) {
        if (!this.feedHasMore || this.loadingMore) return;
        this.loadingMore = true;
      } else {
        this.loading = true;
        this.feedUserId = userId;
        this.feedPage = 1;
        this.feedHasMore = true;
      }

      try {
        const res = await service.getUserFeedPosts(userId, page, pageSize);

        if (res.success && res.data) {
          const batch = normalizePostFullList(res.data);

          if (append) {
            const existing = new Set(this.feedPosts.map((p) => p.id));
            this.feedPosts = [
              ...this.feedPosts,
              ...batch.filter((p) => !existing.has(p.id)),
            ];
          } else {
            this.feedPosts = batch;
          }

          this.feedPage = page;
          this.feedHasMore = batch.length >= pageSize;
        }

        return res;
      } finally {
        this.loading = false;
        this.loadingMore = false;
      }
    },

    loadMoreUserFeedPosts(userId: string) {
      if (!this.feedHasMore || this.loadingMore) return;
      return this.getUserFeedPosts(userId, this.feedPage + 1, PAGE_SIZE, true);
    },

    resetHomeFeed() {
      this.feedPosts = [];
      this.homeAuthors = [];
      this.homeAuthorCursors = {};
      this.homeAuthorExhausted = {};
      this.homeFeedHasMore = true;
    },

    async prepareHomeAuthors(mode: HomeFeedMode) {
      const socialStore = useSocialStore();
      const userStore = useUserStore();
      const myId = userStore.user?.id;

      if (!myId) {
        this.homeAuthors = [];
        return;
      }

      if (!socialStore.friends.length) {
        await socialStore.getFriends();
      }

      if (!socialStore.blocked.length) {
        await socialStore.getBlocked();
      }

      const friendIds = new Set(socialStore.friends.map((f) => f.id));
      const blockedIds = new Set(socialStore.blocked.map((b) => b.id));

      if (mode === "friends") {
        const friends = socialStore.friends.map((friend) => ({
          id: friend.id,
          login: friend.login,
          tag: friend.tag,
          avatarUrl: friend.avatarUrl,
          isFriend: true,
        }));

        const me = userStore.user;
        if (me?.id) {
          const avatar =
            me.media?.find((m) => m.mediaType?.toLowerCase() === "image")?.url ??
            me.media?.[0]?.url ??
            null;

          this.homeAuthors = [
            {
              id: me.id,
              login: me.login,
              tag: me.tag,
              avatarUrl: avatar,
              isFriend: false,
            },
            ...friends.filter((friend) => !sameId(friend.id, me.id)),
          ];
        } else {
          this.homeAuthors = friends;
        }

        return;
      }

      await socialStore.searchUsers({
        search: "",
        page: 1,
        pageSize: 40,
      });

      this.homeAuthors = socialStore.users
        .filter((user) => user.id !== myId && !blockedIds.has(user.id))
        .slice(0, 30)
        .map((user) => ({
          id: user.id,
          login: user.login,
          tag: user.tag,
          avatarUrl: user.avatarUrl,
          isFriend: friendIds.has(user.id),
        }));
    },

    async getHomeFeed(reset = true, mode?: HomeFeedMode) {
      if (mode) {
        this.homeFeedMode = mode;
      }

      if (reset) {
        this.loading = true;
        this.resetHomeFeed();
        await this.prepareHomeAuthors(this.homeFeedMode);
      } else if (!this.homeFeedHasMore || this.loadingMore) {
        return this.feedPosts;
      } else {
        this.loadingMore = true;
      }

      try {
        if (!this.homeAuthors.length) {
          this.homeFeedHasMore = false;
          this.feedPosts = [];
          return [];
        }

        const batch = await this.collectHomeFeedBatch(PAGE_SIZE);

        if (reset) {
          this.feedPosts = batch;
        } else {
          const existing = new Set(this.feedPosts.map((p) => p.id));
          this.feedPosts = [
            ...this.feedPosts,
            ...batch.filter((p) => !existing.has(p.id)),
          ];
        }

        const activeAuthors = this.homeAuthors.filter(
          (author) => !this.homeAuthorExhausted[author.id]
        );
        this.homeFeedHasMore =
          batch.length >= PAGE_SIZE || activeAuthors.length > 0;

        return this.feedPosts;
      } finally {
        this.loading = false;
        this.loadingMore = false;
      }
    },

    loadMoreHomeFeed() {
      return this.getHomeFeed(false);
    },

    async collectHomeFeedBatch(limit: number) {
      const collected: PostFull[] = [];
      const seen = new Set(this.feedPosts.map((p) => p.id));
      let rounds = 0;
      const maxRounds = this.homeAuthors.length * 4;

      while (collected.length < limit && rounds < maxRounds) {
        const author = this.homeAuthors[rounds % this.homeAuthors.length];
        rounds++;

        if (!author || this.homeAuthorExhausted[author.id]) continue;

        const page = this.homeAuthorCursors[author.id] ?? 1;
        const res = await service.getUserFeedPosts(author.id, page, 5);

        const posts =
          res.success && res.data ? normalizePostFullList(res.data) : [];

        if (!posts.length) {
          this.homeAuthorExhausted[author.id] = true;
          continue;
        }

        this.homeAuthorCursors[author.id] = page + 1;

        for (const post of posts) {
          if (seen.has(post.id)) continue;

          seen.add(post.id);
          collected.push({
            ...post,
            userLogin: author.login,
            userTag: author.tag ?? "",
            userAvatar: author.avatarUrl ?? null,
            isFriend: author.isFriend,
          });

          if (collected.length >= limit) break;
        }

        if (posts.length < 5) {
          this.homeAuthorExhausted[author.id] = true;
        }
      }

      return collected.sort(
        (a, b) =>
          new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
      );
    },

    async toggleLike(postId: string) {
      const post = findFeedPost(this.feedPosts, postId);
      if (!post) {
        throw new Error("Пост не найден в ленте");
      }

      const prevLiked = post.isLiked;
      const prevCount = post.likesCount;
      const nextLiked = !prevLiked;
      const nextCount = Math.max(0, prevCount + (nextLiked ? 1 : -1));

      this.feedPosts = patchFeedPost(this.feedPosts, postId, {
        isLiked: nextLiked,
        likesCount: nextCount,
      });
      this.likedPosts = syncReactionCard(this.likedPosts, postId, {
        isLiked: nextLiked,
        likesCount: nextCount,
      });
      this.favoritePosts = syncReactionCard(this.favoritePosts, postId, {
        isLiked: nextLiked,
        likesCount: nextCount,
      });

      const updatedPost = findFeedPost(this.feedPosts, postId)!;

      try {
        const res = nextLiked
          ? await service.likePost(postId)
          : await service.unlikePost(postId);

        if (!isApiSuccess(res)) {
          throw new Error(apiErrorMessage(res, "Не удалось поставить лайк"));
        }

        if (nextLiked) {
          this.likedPosts = upsertLikedCard(updatedPost, this.likedPosts);
        } else {
          this.likedPosts = removeReactionCard(this.likedPosts, postId);
        }
      } catch (e) {
        this.feedPosts = patchFeedPost(this.feedPosts, postId, {
          isLiked: prevLiked,
          likesCount: prevCount,
        });
        this.likedPosts = syncReactionCard(this.likedPosts, postId, {
          isLiked: prevLiked,
          likesCount: prevCount,
        });
        this.favoritePosts = syncReactionCard(this.favoritePosts, postId, {
          isLiked: prevLiked,
          likesCount: prevCount,
        });
        console.error(e);
        throw e;
      }
    },

    async toggleFavorite(postId: string) {
      const post = findFeedPost(this.feedPosts, postId);
      if (!post) {
        throw new Error("Пост не найден в ленте");
      }

      const prev = post.isFavorite;
      const prevCount = post.favoritesCount;
      const nextFavorite = !prev;
      const nextCount = Math.max(0, prevCount + (nextFavorite ? 1 : -1));

      this.feedPosts = patchFeedPost(this.feedPosts, postId, {
        isFavorite: nextFavorite,
        favoritesCount: nextCount,
      });
      this.likedPosts = syncReactionCard(this.likedPosts, postId, {
        isFavorite: nextFavorite,
        favoritesCount: nextCount,
      });
      this.favoritePosts = syncReactionCard(this.favoritePosts, postId, {
        isFavorite: nextFavorite,
        favoritesCount: nextCount,
      });

      const updatedPost = findFeedPost(this.feedPosts, postId)!;

      try {
        const res = nextFavorite
          ? await service.favoritePost(postId)
          : await service.unfavoritePost(postId);

        if (!isApiSuccess(res)) {
          throw new Error(apiErrorMessage(res, "Не удалось добавить в избранное"));
        }

        if (nextFavorite) {
          this.favoritePosts = upsertFavoriteCard(updatedPost, this.favoritePosts);
        } else {
          this.favoritePosts = removeReactionCard(this.favoritePosts, postId);
        }
      } catch (e) {
        this.feedPosts = patchFeedPost(this.feedPosts, postId, {
          isFavorite: prev,
          favoritesCount: prevCount,
        });
        this.likedPosts = syncReactionCard(this.likedPosts, postId, {
          isFavorite: prev,
          favoritesCount: prevCount,
        });
        this.favoritePosts = syncReactionCard(this.favoritePosts, postId, {
          isFavorite: prev,
          favoritesCount: prevCount,
        });
        console.error(e);
        throw e;
      }
    },

    resetLikedPosts() {
      this.likedPosts = [];
      this.likedPage = 1;
      this.likedHasMore = true;
    },

    async getLikedPosts(page = 1, pageSize = PAGE_SIZE, append = false) {
      if (append) {
        if (!this.likedHasMore || this.loadingMore) return;
        this.loadingMore = true;
      } else {
        this.loading = true;
        this.likedPage = 1;
        this.likedHasMore = true;
      }

      try {
        const res = await service.getLikedPosts(page, pageSize);

        if (res.success && res.data) {
          const batch = normalizePostReactionCards(res.data);

          if (append) {
            const existing = new Set(this.likedPosts.map((p) => p.id));
            this.likedPosts = [
              ...this.likedPosts,
              ...batch.filter((p) => !existing.has(p.id)),
            ];
          } else {
            this.likedPosts = batch;
          }

          this.likedPage = page;
          this.likedHasMore = batch.length >= pageSize;
        }

        return res;
      } finally {
        this.loading = false;
        this.loadingMore = false;
      }
    },

    loadMoreLikedPosts() {
      if (!this.likedHasMore || this.loadingMore) return;
      return this.getLikedPosts(this.likedPage + 1, PAGE_SIZE, true);
    },

    resetFavoritePosts() {
      this.favoritePosts = [];
      this.favoritePage = 1;
      this.favoriteHasMore = true;
    },

    async getFavoritePosts(page = 1, pageSize = PAGE_SIZE, append = false) {
      if (append) {
        if (!this.favoriteHasMore || this.loadingMore) return;
        this.loadingMore = true;
      } else {
        this.loading = true;
        this.favoritePage = 1;
        this.favoriteHasMore = true;
      }

      try {
        const res = await service.getFavoritePosts(page, pageSize);

        if (res.success && res.data) {
          const batch = normalizePostReactionCards(res.data);

          if (append) {
            const existing = new Set(this.favoritePosts.map((p) => p.id));
            this.favoritePosts = [
              ...this.favoritePosts,
              ...batch.filter((p) => !existing.has(p.id)),
            ];
          } else {
            this.favoritePosts = batch;
          }

          this.favoritePage = page;
          this.favoriteHasMore = batch.length >= pageSize;
        }

        return res;
      } finally {
        this.loading = false;
        this.loadingMore = false;
      }
    },

    loadMoreFavoritePosts() {
      if (!this.favoriteHasMore || this.loadingMore) return;
      return this.getFavoritePosts(this.favoritePage + 1, PAGE_SIZE, true);
    },

    async updatePost(id: string, post: UpdatePostRequest) {
      const res = await service.updatePost(id, post);

      if (!res.success) return res;

      const target = this.feedPosts.find((p) => p.id === id);

      if (target) {
        target.description = post.description;
        target.media = post.media;
      }

      if (this.feedUserId) {
        await this.getUserFeedPosts(this.feedUserId, 1, PAGE_SIZE);
      }

      return res;
    },

    async deletePost(postId: string) {
      const res = await service.deletePost(postId);

      if (!res.success) {
        return res;
      }

      this.feedPosts = this.feedPosts.filter((p) => p.id !== postId);
      this.profilePosts = this.profilePosts.filter((p) => p.id !== postId);
      this.totalPosts = Math.max(0, this.totalPosts - 1);

      return res;
    },

    setPostCommentsCount(postId: string, count: number) {
      const safeCount = Math.max(0, count);
      const idx = findFeedPostIndex(this.feedPosts, postId);
      if (idx === -1) return;

      this.feedPosts = patchFeedPost(this.feedPosts, postId, {
        commentsCount: safeCount,
      });
    },

    changePostCommentsCount(postId: string, delta: number) {
      const post = findFeedPost(this.feedPosts, postId);
      if (!post) return;

      this.setPostCommentsCount(
        postId,
        Math.max(0, post.commentsCount + delta)
      );
    },
  },
});
