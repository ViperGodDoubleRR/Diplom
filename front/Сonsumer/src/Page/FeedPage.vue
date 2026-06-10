<template>
  <div class="posts-page">
    <div class="bg"></div>

    <section class="header">
      <div class="header-main">
        <h1 class="title">{{ pageTitle }}</h1>
        <p class="subtitle">{{ pageSubtitle }}</p>
      </div>

      <div v-if="isHomeFeed" class="feed-tabs">
        <button
          class="feed-tab"
          :class="{ active: feedMode === 'friends' }"
          type="button"
          @click="switchMode('friends')"
        >
          Друзья
        </button>
        <button
          class="feed-tab"
          :class="{ active: feedMode === 'discover' }"
          type="button"
          @click="switchMode('discover')"
        >
          Рекомендации
        </button>
      </div>
    </section>

    <section v-if="postStore.loading && !posts.length" class="state-card">
      Загрузка ленты...
    </section>

    <section v-else-if="!posts.length" class="state-card empty">
      <span class="empty-icon">✨</span>
      <p>{{ emptyMessage }}</p>
      <button v-if="isHomeFeed" class="link-btn" @click="router.push('/profile')">
        Перейти в профиль
      </button>
    </section>

    <section v-else class="posts">
      <article
        v-for="post in posts"
        :key="post.id"
        :id="`post-${post.id}`"
        class="post-card"
        :class="{ focused: focusedPostId === post.id }"
      >
        <div class="post-header">
          <button class="user" type="button" @click="openAuthorProfile(post)">
            <UserAvatar
              avatar-class="user-avatar"
              :name="getAuthor(post).login"
              :src="getAuthor(post).avatar"
            />

            <div class="user-text">
              <div class="login-row">
                <span class="login">{{ getAuthor(post).login }}</span>
                <span v-if="isHomeFeed && feedMode === 'discover' && post.isFriend" class="friend-badge">Друг</span>
              </div>
              <div class="tag">@{{ getAuthor(post).tag || "unknown" }}</div>
            </div>
          </button>

          <div class="right-side">
            <div class="date">{{ formatDate(post.createdAt) }}</div>

            <div v-if="isMyPost(post)" class="post-menu">
              <button class="menu-btn" type="button" @click.stop="toggleMenu(post.id)">⋮</button>

              <div v-if="openedMenu === post.id" class="menu-dropdown">
                <button class="menu-item" type="button" @click.stop="editPost(post)">
                  ✏️ Редактировать
                </button>
                <button class="menu-item delete" type="button" @click.stop="removePost(post.id)">
                  🗑 Удалить
                </button>
              </div>
            </div>
          </div>
        </div>

        <div v-if="post.media?.length" class="media">
          <button
            v-if="post.media.length > 1"
            class="media-nav left"
            type="button"
            @click.stop="prevMedia(post)"
          >
            ‹
          </button>

          <img
            v-if="getCurrentMedia(post) && !isVideoMediaType(getCurrentMedia(post)?.mediaType)"
            :src="getCurrentMedia(post)?.url"
            class="media-preview"
            alt=""
          />

          <video
            v-else-if="getCurrentMedia(post) && isVideoMediaType(getCurrentMedia(post)?.mediaType)"
            :src="getCurrentMedia(post)?.url"
            class="media-preview"
            controls
            playsinline
            preload="metadata"
          />

          <button
            v-if="post.media.length > 1"
            class="media-nav right"
            type="button"
            @click.stop="nextMedia(post)"
          >
            ›
          </button>

          <div v-if="post.media.length > 1" class="media-counter">
            {{ (mediaIndexes[post.id] ?? 0) + 1 }} / {{ post.media.length }}
          </div>
        </div>

        <div class="content">
          <ExpandableText
            v-if="post.description"
            class="description"
            :text="post.description"
            expand-label="ещё"
          />

          <div class="actions">
            <button
              class="action like"
              :class="{ active: post.isLiked }"
              type="button"
              :aria-pressed="post.isLiked"
              @click.stop="toggleLike(post)"
            >
              ❤️ {{ post.likesCount }}
            </button>

            <button
              class="action favorite"
              :class="{ active: post.isFavorite }"
              type="button"
              :aria-pressed="post.isFavorite"
              @click.stop="toggleFavorite(post)"
            >
              ⭐ {{ post.favoritesCount }}
            </button>

            <button
              class="action comments"
              :class="{
                active:
                  post.commentsCount > 0 ||
                  (isCommentsOpen && selectedPostId === post.id),
              }"
              type="button"
              @click.stop="openComments(post)"
            >
              💬 {{ post.commentsCount }}
            </button>
          </div>
        </div>
      </article>

      <div ref="sentinel" class="scroll-sentinel" aria-hidden="true" />
      <p v-if="postStore.loadingMore" class="loading-more">Загрузка...</p>
      <p v-else-if="!feedHasMore && posts.length" class="loading-more muted">Больше постов нет</p>
    </section>

    <EditPostModal v-model="editModal" :post="editingPost" @save="savePost" />
    <CommentsModal
      v-if="isCommentsOpen && selectedPostId"
      v-model="isCommentsOpen"
      :post-id="selectedPostId"
      @update:model-value="onCommentsOpenChange"
    />
    <p v-if="toastMessage" class="feed-toast">{{ toastMessage }}</p>
  </div>
</template>

<script setup lang="ts">
import { computed, nextTick, onMounted, ref, watch } from "vue";
import { useRoute, useRouter } from "vue-router";

import EditPostModal from "@/components/EditPostModal.vue";
import CommentsModal from "@/components/ui/CommentsModal.vue";
import ExpandableText from "@/components/ui/ExpandableText.vue";
import UserAvatar from "@/components/ui/UserAvatar.vue";
import { useInfiniteScroll } from "@/composables/useInfiniteScroll";

import type { PostFull } from "@/interface/models/post/PostFull";
import type { PostMedia } from "@/interface/models/post/PostMedia";
import { usePostStore, PAGE_SIZE, type HomeFeedMode } from "@/store/postStore";
import { useUserStore } from "@/store/userStore";
import { isVideoMediaType } from "@/utils/postNormalize";

const route = useRoute();
const router = useRouter();
const postStore = usePostStore();
const userStore = useUserStore();

const mediaIndexes = ref<Record<string, number>>({});
const editingPost = ref<PostFull | null>(null);
const editModal = ref(false);
const isCommentsOpen = ref(false);
const selectedPostId = ref<string | null>(null);
const openedMenu = ref<string | null>(null);
const focusedPostId = ref<string | null>(null);
const toastMessage = ref("");
let toastTimer: ReturnType<typeof setTimeout> | null = null;

function showToast(text: string) {
  toastMessage.value = text;
  if (toastTimer) clearTimeout(toastTimer);
  toastTimer = setTimeout(() => {
    toastMessage.value = "";
  }, 3200);
}

const userId = computed(() => route.params.userId as string | undefined);
const routePostId = computed(() => route.params.postId as string | undefined);
const isHomeFeed = computed(() => route.name === "feed-home");
const posts = computed(() => postStore.feedPosts);

const feedHasMore = computed(() =>
  isHomeFeed.value ? postStore.homeFeedHasMore : postStore.feedHasMore
);

const scrollDisabled = computed(
  () => !feedHasMore.value || postStore.loading || postStore.loadingMore
);

const { sentinel } = useInfiniteScroll(() => loadMoreFeed(), {
  disabled: scrollDisabled,
});

const feedMode = computed(() => postStore.homeFeedMode);

const pageTitle = computed(() => {
  if (!isHomeFeed.value) {
    return postStore.currentUser.login || "Посты пользователя";
  }

  return feedMode.value === "friends" ? "Лента друзей" : "Рекомендации";
});

const pageSubtitle = computed(() => {
  if (!isHomeFeed.value) {
    return `@${postStore.currentUser.tag || "unknown"}`;
  }

  return feedMode.value === "friends"
    ? "Только посты людей из вашего списка друзей"
    : "Новые авторы; если среди них друг — будет метка «Друг»";
});

const emptyMessage = computed(() => {
  if (!isHomeFeed.value) return "У пользователя пока нет постов";

  return feedMode.value === "friends"
    ? "Добавьте друзей, чтобы видеть их посты"
    : "Пока нет рекомендаций — загляните позже";
});

function getAuthor(post: PostFull) {
  if (post.userLogin) {
    return {
      login: post.userLogin,
      tag: post.userTag ?? "",
      avatar: post.userAvatar ?? "",
    };
  }

  if (post.userId === postStore.currentUser.id) {
    return {
      login: postStore.currentUser.login || "User",
      tag: postStore.currentUser.tag || "",
      avatar: postStore.currentUser.avatar || "",
    };
  }

  return {
    login: "User",
    tag: "",
    avatar: "",
  };
}

function isMyPost(post: PostFull) {
  const myId = userStore.user?.id;
  return !!myId && post.userId.toLowerCase() === myId.toLowerCase();
}

function formatDate(date: string) {
  return new Date(date).toLocaleDateString();
}

function getCurrentMedia(post: PostFull) {
  const index = mediaIndexes.value[post.id] ?? 0;
  return post.media[index];
}

function nextMedia(post: PostFull) {
  const current = mediaIndexes.value[post.id] ?? 0;
  mediaIndexes.value[post.id] =
    current >= post.media.length - 1 ? 0 : current + 1;
}

function prevMedia(post: PostFull) {
  const current = mediaIndexes.value[post.id] ?? 0;
  mediaIndexes.value[post.id] =
    current <= 0 ? post.media.length - 1 : current - 1;
}

function toggleMenu(postId: string) {
  openedMenu.value = openedMenu.value === postId ? null : postId;
}

function editPost(post: PostFull) {
  editingPost.value = JSON.parse(JSON.stringify(post));
  editModal.value = true;
  openedMenu.value = null;
}

function openComments(post: PostFull) {
  if (!post?.id) return;

  selectedPostId.value = post.id;
  isCommentsOpen.value = true;
}

function onCommentsOpenChange(open: boolean) {
  if (!open) {
    selectedPostId.value = null;
  }
}

async function savePost(updated: {
  id: string;
  description: string;
  media: PostMedia[];
}) {
  await postStore.updatePost(updated.id, {
    description: updated.description,
    media: updated.media,
  });

  editModal.value = false;
  await loadFeed(true);
}

async function removePost(postId: string) {
  if (!confirm("Удалить этот пост?")) return;

  try {
    await postStore.deletePost(postId);
    openedMenu.value = null;
  } catch (e) {
    console.error(e);
  }
}

async function toggleLike(post: PostFull) {
  try {
    await postStore.toggleLike(post.id);
  } catch (e) {
    showToast(e instanceof Error ? e.message : "Не удалось поставить лайк");
    console.error(e);
  }
}

async function toggleFavorite(post: PostFull) {
  try {
    await postStore.toggleFavorite(post.id);
  } catch (e) {
    showToast(e instanceof Error ? e.message : "Не удалось добавить в избранное");
    console.error(e);
  }
}

function openAuthorProfile(post: PostFull) {
  const authorId = post.userId;
  if (!authorId) return;

  if (authorId === userStore.user?.id) {
    router.push({ name: "profile" });
    return;
  }

  router.push({ name: "profile-view", params: { id: authorId } });
}

async function loadFeed(reset = false) {
  if (userId.value) {
    if (reset) {
      postStore.resetUserFeed();
      await postStore.getUserFeedPosts(userId.value, 1, PAGE_SIZE);
    }
    return;
  }

  await postStore.getHomeFeed(reset, feedMode.value);
}

async function switchMode(mode: HomeFeedMode) {
  if (feedMode.value === mode) return;
  await postStore.getHomeFeed(true, mode);
}

function loadMoreFeed() {
  if (userId.value) {
    void postStore.loadMoreUserFeedPosts(userId.value);
    return;
  }

  void postStore.loadMoreHomeFeed();
}

async function scrollToPost(postId?: string) {
  if (!postId) return;

  focusedPostId.value = postId;

  await nextTick();

  const el = document.getElementById(`post-${postId}`);
  el?.scrollIntoView({ behavior: "smooth", block: "center" });

  window.setTimeout(() => {
    if (focusedPostId.value === postId) {
      focusedPostId.value = null;
    }
  }, 2500);
}

onMounted(async () => {
  await loadFeed(true);
  await scrollToPost(routePostId.value);
});

watch(
  () => [route.name, route.params.userId, route.params.postId],
  async () => {
    await loadFeed(true);
    await scrollToPost(routePostId.value);
  }
);
</script>

<style scoped>
.posts-page {
  width: 100%;
  max-width: 820px;
  margin: 0 auto;
  padding-bottom: 48px;
  color: white;
  position: relative;
}

.bg {
  position: absolute;
  top: -160px;
  left: 50%;
  transform: translateX(-50%);
  width: min(900px, 95vw);
  height: 900px;
  background: radial-gradient(circle, rgba(65, 99, 252, 0.18), transparent 60%);
  filter: blur(110px);
  pointer-events: none;
}

.header {
  position: relative;
  z-index: 1;
  display: flex;
  justify-content: space-between;
  align-items: flex-end;
  gap: 16px;
  margin-bottom: 24px;
  flex-wrap: wrap;
}

.feed-tabs {
  display: inline-flex;
  gap: 4px;
  padding: 4px;
  border-radius: 14px;
  background: rgba(255, 255, 255, 0.04);
  border: 1px solid rgba(255, 255, 255, 0.06);
}

.feed-tab {
  border: none;
  background: transparent;
  color: rgba(255, 255, 255, 0.55);
  cursor: pointer;
  padding: 8px 16px;
  border-radius: 10px;
  font-size: 14px;
  font-weight: 500;
  transition: 0.2s ease;
}

.feed-tab.active {
  color: white;
  background: rgba(65, 99, 252, 0.22);
  box-shadow: inset 0 0 0 1px rgba(65, 99, 252, 0.35);
}

.title {
  margin: 0;
  font-size: clamp(24px, 4vw, 30px);
  font-weight: 700;
}

.subtitle {
  margin: 6px 0 0;
  opacity: 0.55;
  font-size: 13px;
  line-height: 1.5;
}

.state-card {
  position: relative;
  z-index: 1;
  padding: 36px 20px;
  text-align: center;
  border-radius: 18px;
  background: rgba(255, 255, 255, 0.04);
  border: 1px solid rgba(255, 255, 255, 0.06);
  opacity: 0.8;
}

.state-card.empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 10px;
}

.empty-icon {
  font-size: 30px;
}

.link-btn {
  margin-top: 4px;
  border: none;
  background: rgba(65, 99, 252, 0.18);
  color: white;
  padding: 8px 14px;
  border-radius: 10px;
  cursor: pointer;
}

.posts {
  position: relative;
  z-index: 1;
  display: flex;
  flex-direction: column;
  gap: 18px;
}

.post-card {
  border-radius: 18px;
  background: rgba(255, 255, 255, 0.04);
  border: 1px solid rgba(255, 255, 255, 0.06);
  padding: 16px;
  transition: 0.2s ease;
  overflow: hidden;
}

.post-card.focused {
  border-color: rgba(65, 99, 252, 0.55);
  box-shadow: 0 0 0 2px rgba(65, 99, 252, 0.2);
}

.post-header {
  display: flex;
  justify-content: space-between;
  gap: 12px;
  margin-bottom: 12px;
  align-items: flex-start;
}

.user {
  display: flex;
  gap: 10px;
  align-items: center;
  border: none;
  background: transparent;
  color: inherit;
  cursor: pointer;
  padding: 0;
  text-align: left;
}

.user :deep(.user-avatar),
.user :deep(.initials) {
  width: 44px;
  height: 44px;
  flex-shrink: 0;
  border: 2px solid rgba(65, 99, 252, 0.4);
}

.user-text {
  min-width: 0;
}

.login-row {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}

.login {
  font-weight: 600;
  font-size: 14px;
}

.friend-badge {
  font-size: 10px;
  font-weight: 700;
  letter-spacing: 0.3px;
  text-transform: uppercase;
  padding: 3px 8px;
  border-radius: 999px;
  background: rgba(65, 99, 252, 0.18);
  color: #9eb0ff;
  border: 1px solid rgba(65, 99, 252, 0.35);
}

.tag {
  opacity: 0.5;
  font-size: 12px;
}

.right-side {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 8px;
  position: relative;
}

.date {
  opacity: 0.4;
  font-size: 12px;
  white-space: nowrap;
}

.media {
  position: relative;
  border-radius: 14px;
  overflow: hidden;
}

.media-preview {
  width: 100%;
  height: clamp(220px, 42vw, 360px);
  object-fit: cover;
  display: block;
  background: #1a1a1a;
}

.media-nav {
  position: absolute;
  top: 50%;
  transform: translateY(-50%);
  width: 38px;
  height: 38px;
  border: none;
  border-radius: 50%;
  background: rgba(0, 0, 0, 0.45);
  color: white;
  cursor: pointer;
  z-index: 10;
}

.media-nav.left {
  left: 10px;
}

.media-nav.right {
  right: 10px;
}

.media-counter {
  position: absolute;
  right: 10px;
  bottom: 10px;
  padding: 4px 8px;
  border-radius: 8px;
  background: rgba(0, 0, 0, 0.6);
  font-size: 12px;
}

.content {
  margin-top: 12px;
  position: relative;
  z-index: 2;
}

.description {
  font-size: 14px;
  line-height: 1.55;
  opacity: 0.92;
  margin: 0 0 0;
  color: rgba(255, 255, 255, 0.92);
}

.actions {
  display: flex;
  gap: 10px;
  margin-top: 14px;
  flex-wrap: wrap;
}

.action {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 8px 12px;
  border-radius: 10px;
  border: none;
  background: rgba(255, 255, 255, 0.04);
  color: white;
  font-size: 13px;
  cursor: pointer;
  transition: 0.2s;
}

.action:hover {
  transform: translateY(-1px);
}

.action.like.active {
  color: #ff4d6d;
  background: rgba(255, 77, 109, 0.18);
  box-shadow: inset 0 0 0 1px rgba(255, 77, 109, 0.45);
}

.action.favorite.active {
  color: #ffc400;
  background: rgba(255, 196, 0, 0.18);
  box-shadow: inset 0 0 0 1px rgba(255, 196, 0, 0.45);
}

.action.comments.active {
  color: #9eb0ff;
  background: rgba(65, 99, 252, 0.2);
  box-shadow: inset 0 0 0 1px rgba(65, 99, 252, 0.45);
}

.feed-toast {
  position: fixed;
  left: 50%;
  bottom: 28px;
  transform: translateX(-50%);
  z-index: 100;
  padding: 12px 18px;
  border-radius: 12px;
  background: rgba(220, 53, 69, 0.95);
  color: white;
  font-size: 14px;
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.35);
  max-width: min(92vw, 420px);
  text-align: center;
}

.post-menu {
  position: relative;
}

.menu-btn {
  width: 34px;
  height: 34px;
  border: none;
  border-radius: 10px;
  background: rgba(255, 255, 255, 0.06);
  color: white;
  cursor: pointer;
  font-size: 18px;
}

.menu-dropdown {
  position: absolute;
  top: 40px;
  right: 0;
  min-width: 170px;
  border-radius: 12px;
  overflow: hidden;
  background: #181818;
  border: 1px solid rgba(255, 255, 255, 0.08);
  z-index: 20;
}

.menu-item {
  width: 100%;
  border: none;
  background: transparent;
  color: white;
  text-align: left;
  padding: 12px;
  cursor: pointer;
}

.menu-item:hover {
  background: rgba(255, 255, 255, 0.06);
}

.menu-item.delete {
  color: #ff5c5c;
}

.scroll-sentinel {
  width: 100%;
  height: 1px;
}

.loading-more {
  text-align: center;
  padding: 8px 0 4px;
  font-size: 13px;
  opacity: 0.65;
}

.loading-more.muted {
  opacity: 0.45;
}

@media (max-width: 640px) {
  .header {
    flex-direction: column;
    align-items: stretch;
  }

  .feed-tabs {
    width: 100%;
    justify-content: center;
  }

  .feed-tab {
    flex: 1;
    text-align: center;
  }

  .post-card {
    padding: 14px;
  }

  .media-preview {
    height: clamp(200px, 56vw, 280px);
  }
}
</style>
