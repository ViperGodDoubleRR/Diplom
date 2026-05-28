<template>
  <div class="posts-page">

    <div class="bg"></div>

    <!-- HEADER -->
    <section class="header">
      <div class="left">
        <p class="subtitle">
          {{ userId ? "User posts" : "Explore posts and reactions" }}
        </p>
      </div>
    </section>

    <!-- POSTS -->
    <section class="posts">

      <article v-for="post in posts" :key="post.id" class="post-card" :id="`post-${post.id}`">

        <!-- HEADER -->
        <div class="post-header">

          <div class="user">
            <img class="user-avatar" :src="postStore.currentUser.avatar || avatarFallback" />

            <div>
              <div class="login">
                {{ postStore.currentUser.login || "User" }}
              </div>

              <div class="tag">
                @{{ postStore.currentUser.tag || "unknown" }}
              </div>
            </div>
          </div>

          <div class="right-side">

            <div class="date">
              {{ formatDate(post.createdAt) }}
            </div>

            <!-- EDIT MENU (ВСЕГДА ДОСТУПЕН ДЛЯ СВОИХ ПОСТОВ) -->
            <div v-if="isMyPost(post)" class="post-menu">

              <button class="menu-btn" @click.stop="toggleMenu(post.id)">
                ⋮
              </button>

              <div v-if="openedMenu === post.id" class="menu-dropdown">

                <button class="menu-item" @click.stop="editPost(post)">
                  ✏️ Edit post
                </button>
                <button class="menu-item delete" @click.stop="removePost(post.id)">
                  🗑 Delete post
                </button>
              </div>

            </div>

          </div>

        </div>

        <!-- MEDIA (НЕ БЛОКИРУЕТ РЕДАКТИРОВАНИЕ) -->
        <div v-if="post.media?.length" class="media">

          <!-- LEFT -->
          <button v-if="post.media.length > 1" class="media-nav left" @click.stop="prevMedia(post)">
            ‹
          </button>

          <!-- IMAGE -->
          <img v-if="getCurrentMedia(post)?.mediaType === 'image'" :src="getCurrentMedia(post)?.url"
            class="media-preview" />

          <!-- VIDEO -->
          <video v-else-if="getCurrentMedia(post)?.mediaType === 'video'" :src="getCurrentMedia(post)?.url"
            class="media-preview" controls />

          <!-- RIGHT -->
          <button v-if="post.media.length > 1" class="media-nav right" @click.stop="nextMedia(post)">
            ›
          </button>

          <!-- COUNTER -->
          <div v-if="post.media.length > 1" class="media-counter">
            {{ (mediaIndexes[post.id] ?? 0) + 1 }}
            /
            {{ post.media.length }}
          </div>

        </div>

        <!-- CONTENT -->
        <div class="content">

          <p class="description">
            {{ post.description }}
          </p>

          <div class="actions">

            <button class="action like" :class="{ active: post.isLiked, disabled: isMyPost(post) }"
              :disabled="isMyPost(post)" @click="toggleLike(post)">
              ❤️ {{ post.likesCount }}
            </button>

            <button class="action favorite" :class="{ active: post.isFavorite, disabled: isMyPost(post) }"
              :disabled="isMyPost(post)" @click="toggleFavorite(post)">
              ⭐ {{ post.favoritesCount }}
            </button>

            <button class="action comments">
              💬 {{ post.commentsCount }}
            </button>

          </div>

        </div>

      </article>

    </section>

    <EditPostModal v-model="editModal" :post="editingPost" @save="savePost" />

  </div>
</template>
<script setup lang="ts">
import { computed, onMounted } from "vue";
import { useRoute } from "vue-router";
import { usePostStore } from "@/store/postStore";
import EditPostModal from "@/components/EditPostModal.vue";
import type { PostFull } from "@/interface/models/post/PostFull";
import { ref } from "vue";
import { useUserStore } from "@/store/userStore";

const userStore = useUserStore();
const mediaIndexes = ref<Record<string, number>>({});
const route = useRoute();
const postStore = usePostStore();
const editingPost = ref<PostFull | null>(null);
const originalPost = ref<PostFull | null>(null);
const userId = computed(() => route.params.userId as string | undefined);
const editModal = ref(false);
// posts
const posts = computed(() => postStore.feedPosts);

const avatarFallback = "https://i.pravatar.cc/300";
const openedMenu = ref<string | null>(null);
function editPost(post: PostFull) {
  editingPost.value = JSON.parse(JSON.stringify(post));
  originalPost.value = JSON.parse(JSON.stringify(post));

  editModal.value = true;
  openedMenu.value = null;
}

async function savePost(updated: any) {
  await postStore.updatePost(updated.id, {
    description: updated.description,
    media: updated.media
  });

  editModal.value = false;
  if (userId.value) {
    await postStore.getUserFeedPosts(userId.value, 1, 12);
  }

}
async function removePost(postId: string) {
  const ok = confirm("Delete this post?");

  if (!ok) return;

  try {
    await postStore.deletePost(postId);

    openedMenu.value = null;
  } catch (e) {
    console.error(e);
  }
}
function isMyPost(post: PostFull) {
  return post.userId === userStore.user?.id;
}

function toggleMenu(postId: string) {
  openedMenu.value =
    openedMenu.value === postId
      ? null
      : postId;
}

onMounted(async () => {
  if (userId.value) {
    await postStore.getUserFeedPosts(userId.value, 1, 12);
  }
});
function getCurrentMedia(post: PostFull) {
  const index = mediaIndexes.value[post.id] ?? 0;
  return post.media[index];
}
function formatDate(date: string) {
  return new Date(date).toLocaleDateString();
}
function nextMedia(post: PostFull) {
  const current = mediaIndexes.value[post.id] ?? 0;

  mediaIndexes.value[post.id] =
    current >= post.media.length - 1
      ? 0
      : current + 1;
}
function prevMedia(post: PostFull) {
  const current = mediaIndexes.value[post.id] ?? 0;

  mediaIndexes.value[post.id] =
    current <= 0
      ? post.media.length - 1
      : current - 1;
}
// ==========================
// LIKE
// ==========================
async function toggleLike(post: PostFull) {
  try {
    await postStore.toggleLike(post.id);
  } catch { }
}

// ==========================
// FAVORITE
// ==========================
async function toggleFavorite(post: PostFull) {
  try {
    await postStore.toggleFavorite(post.id);
  } catch (e) {
    console.log(e);
  }
}
</script>

<style scoped>
.posts-page {
  min-height: 100vh;
  background: #0F0F0F;
  padding: 40px;
  color: white;
  position: relative;
  overflow-x: hidden;
}

/* BACKGROUND GLOW */
.bg {
  position: absolute;
  top: -200px;
  left: 50%;
  transform: translateX(-50%);
  width: 900px;
  height: 900px;
  background: radial-gradient(circle, rgba(65, 99, 252, .18), transparent 60%);
  filter: blur(110px);
  pointer-events: none;
}

/* HEADER */
.header {
  display: flex;
  justify-content: space-between;
  margin-bottom: 30px;
  align-items: flex-end;
}

.title {
  font-size: 30px;
  margin: 0;
}

.subtitle {
  opacity: .5;
  font-size: 13px;
  margin-top: 6px;
}

/* POSTS CONTAINER */
.posts {
  max-width: 760px;
  margin: 0 auto;
  display: flex;
  flex-direction: column;
  gap: 18px;
}

/* CARD */
.post-card {
  border-radius: 18px;
  background: rgba(255, 255, 255, .04);
  border: 1px solid rgba(255, 255, 255, .06);
  padding: 16px;
  transition: .2s ease;
  overflow: hidden;
  /* 🔥 ВАЖНО — режет всё что вылезает */
}

.menu-item.delete {
  color: #ff5c5c;
}

.menu-item.delete:hover {
  background: rgba(255, 92, 92, 0.12);
}

.post-card:hover {
  transform: translateY(-2px);
}

.post-card.focused {
  border: 1px solid #4163FC;
  box-shadow: 0 0 0 2px rgba(65, 99, 252, .25);
}

/* HEADER INSIDE CARD */
.post-header {
  display: flex;
  justify-content: space-between;
  margin-bottom: 12px;
  align-items: center;
}

.user {
  display: flex;
  gap: 10px;
  align-items: center;
}

.user-avatar {
  width: 44px;
  height: 44px;
  border-radius: 50%;
  object-fit: cover;
  /* 🔥 фикс кривых аватарок */
  border: 2px solid rgba(65, 99, 252, .4);
}

.login {
  font-weight: 600;
  font-size: 14px;
}

.tag {
  opacity: .5;
  font-size: 12px;
}

.date {
  opacity: .4;
  font-size: 12px;
}

/* MEDIA FIX 🔥 */
.media-nav {
  position: absolute;
  top: 50%;
  transform: translateY(-50%);

  width: 38px;
  height: 38px;

  border: none;
  border-radius: 50%;

  background: rgba(0, 0, 0, .45);
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

  background: rgba(0, 0, 0, .6);

  font-size: 12px;
}

/* КЛЮЧЕВОЙ FIX КАРТИНОК */
.media img {
  width: 100%;
  height: 320px;
  /* фикс высоты */
  object-fit: cover;
  /* НЕ вылезает, красиво кропается */
  display: block;
  border-radius: 14px;
}

/* CONTENT */
.content {
  margin-top: 12px;
}

.description {
  font-size: 14px;
  line-height: 1.5;
  opacity: .92;
  margin: 0;
}

/* ACTIONS */
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
  background: rgba(255, 255, 255, .04);
  color: white;

  font-size: 13px;

  transition: .2s;
  cursor: pointer;
}

.action:hover {
  transform: translateY(-1px);
}

/* STATES */
.action.like.active {
  color: #ff4d6d;
  background: rgba(255, 77, 109, .12);
}

.action.favorite.active {
  color: #ffc400;
  background: rgba(255, 196, 0, .12);
}

.media {
  position: relative;
}

.post-menu {
  position: absolute;
  top: 10px;
  right: 10px;
  z-index: 50;
}

.action.disabled {
  opacity: 0.4;
  cursor: not-allowed;
  pointer-events: none;
  filter: grayscale(1);
}

.menu-btn {
  width: 36px;
  height: 36px;

  border: none;
  border-radius: 10px;

  background: rgba(0, 0, 0, .6);
  color: white;

  cursor: pointer;
  font-size: 18px;
}

.menu-dropdown {
  position: absolute;

  top: 42px;
  right: 0;

  min-width: 160px;

  border-radius: 12px;

  overflow: hidden;

  background: #181818;
  border: 1px solid rgba(255, 255, 255, .08);
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
  background: rgba(255, 255, 255, .06);
}
</style>
