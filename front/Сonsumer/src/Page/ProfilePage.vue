<template>
  <div class="profile-page">

    <div class="bg"></div>

    <section class="profile-card">

      <div class="avatar-block">
        <UserAvatar
          avatar-class="avatar"
          :name="userStore.user?.login"
          :src="avatarPreview.url"
          :is-video="avatarPreview.isVideo"
          @click="openGallery"
        />
        <button class="avatar-upload" type="button" @click="openGallery">Медиа</button>
        <div class="online"></div>
      </div>

      <div class="info-block">

        <h1 class="name">{{ userStore.user?.login }}</h1>
        <p class="tag">@{{ userStore.user?.tag }}</p>

        <p class="bio">{{ userStore.user?.description }}</p>

        <div class="stats">

          <div class="stat" @click="isFriendsOpen = true">
            <span class="num">{{ friends.length }}</span>
            <span class="label">Friends</span>
          </div>

          <div class="stat">
            <span class="num">{{ postsCountLabel }}</span>
            <span class="label">Posts</span>
          </div>

          <div class="stat">
            <span class="num">{{ postStore.totalLikes }}</span>
            <span class="label">Likes</span>
          </div>

        </div>

        <div class="actions">
          <button class="edit-btn" @click="isProfileOpen = true">
            Edit Profile
          </button>

          <button class="create-btn" @click="isCreatePostOpen = true">
            Create Post
          </button>
        </div>

      </div>

    </section>

    <!-- TABS -->
    <div class="tabs">
      <button class="tab" :class="{ active: activeTab === 'posts' }" @click="activeTab = 'posts'">
        Posts
      </button>

      <button class="tab" :class="{ active: activeTab === 'liked' }" @click="activeTab = 'liked'">
        Liked
      </button>

      <button class="tab" :class="{ active: activeTab === 'saved' }" @click="activeTab = 'saved'">
        Saved
      </button>
    </div>

    <ProfilePostGrid
      :items="posts"
      :loading="gridLoading"
      :loading-more="postStore.loadingMore"
      :has-more="gridHasMore"
      :empty-text="emptyText"
      :empty-icon="emptyIcon"
      :show-author="activeTab !== 'posts'"
      @open="openPost"
      @load-more="loadMorePosts"
    />

    <!-- MODALS -->
    <ProfileModal v-model="isProfileOpen" :loading="userStore.updateLoading" :initial-data="{
      name: userStore.user?.login || '',
      tag: userStore.user?.tag || '',
      description: userStore.user?.description || ''
    }" @save="handleProfileSave" />

    <FriendsModal
      v-model="isFriendsOpen"
      :friends="friends"
      :blocked="blocked"
      :all-users="allUsers"
      :page="page"
      :has-more="directoryHasMore"
      :loading="directoryLoading"
      @add="handleAddFriend"
      @block="handleBlockUser"
      @remove-friend="handleRemoveFriend"
      @unblock="handleUnblockUser"
      @search="handleSearch"
      @page="handlePage"
      @rename="openRenameFriend"
    />

    <RenameFriendModal
      v-model="isRenameOpen"
      :initial-login="renameTarget?.login"
      :loading="renameLoading"
      @save="handleRenameFriend"
    />

    <MediaGalleryModal
      v-model="isGalleryOpen"
      :media="mediaList"
      :start-index="galleryIndex"
      @upload="handleUpload"
      @replace="handleReplace"
      @delete="handleDelete"
      @delete-all="handleDeleteAll"
      @error="(msg) => showToast(msg, 'error')"
    />

    <p v-if="toastMessage" :class="['toast', toastType]">{{ toastMessage }}</p>

    <CreatePostModal v-model="isCreatePostOpen" :loading="createLoading" @submit="handleCreatePost" />

  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from "vue";

import ProfileModal from "@/components/profile/ProfileModal.vue";
import FriendsModal from "@/components/profile/FriendsModal.vue";
import MediaGalleryModal from "@/components/profile/MediaGalleryModal.vue";
import CreatePostModal from "@/components/profile/CreatePostModal.vue";
import RenameFriendModal from "@/components/profile/RenameFriendModal.vue";
import UserAvatar from "@/components/ui/UserAvatar.vue";
import ProfilePostGrid from "@/components/post/ProfilePostGrid.vue";
import type { PostProfileCard } from "@/interface/models/post/PostProfileCard";
import type { PostReactionCard } from "@/interface/models/post/PostReactionCard";

import { useUserStore } from "@/store/userStore";
import { useMediaStore } from "@/store/usermediaStore";
import { useSocialStore } from "@/store/socialStore";
import { usePostStore, PAGE_SIZE } from "@/store/postStore";
import { useRouter } from "vue-router";
import { MediaType } from "@/interface/models/profile/MediaType";
import type { ProfileForm } from "@/interface/models/profile/ProfileForm";
import { pickProfileMedia, sortProfileMediaChronological } from "@/utils/profileValidation";
import { DIRECTORY_USERS_PAGE_SIZE } from "@/constants/socialConstants";
import { PROFILE_ERROR_MESSAGES, SOCIAL_ERROR_MESSAGES, resolveMessage } from "@/utils/profileMessages";

const userStore = useUserStore();
const mediaStore = useMediaStore();
const socialStore = useSocialStore();
const postStore = usePostStore();

// =====================
// UI STATE
// =====================
const isProfileOpen = ref(false);
const isFriendsOpen = ref(false);
const isGalleryOpen = ref(false);
const isCreatePostOpen = ref(false);
const createLoading = ref(false);

const isRenameOpen = ref(false);
const renameLoading = ref(false);
const renameTarget = ref<{ id: string; login: string } | null>(null);

const toastMessage = ref("");
const toastType = ref<"success" | "error">("success");

const galleryIndex = ref(0);
const page = ref(1);
const directorySearch = ref("");

watch(isFriendsOpen, (open) => {
  if (open) return;

  page.value = 1;
  directorySearch.value = "";
  socialStore.clearDirectoryUsers();
});

function showToast(text: string, type: "success" | "error" = "success") {
  toastMessage.value = text;
  toastType.value = type;
  setTimeout(() => {
    toastMessage.value = "";
  }, 3500);
}

const activeTab = ref<"posts" | "liked" | "saved">("posts");

const emptyText = computed(() => {
  if (activeTab.value === "liked") return "Вы ещё не лайкнули посты";
  if (activeTab.value === "saved") return "Сохранённых постов пока нет";
  return "Создайте первый пост";
});

const emptyIcon = computed(() => {
  if (activeTab.value === "liked") return "❤️";
  if (activeTab.value === "saved") return "⭐";
  return "📷";
});

const router = useRouter();
// =====================
// DATA
// =====================
const friends = computed(() => socialStore.friends);
const blocked = computed(() => socialStore.blocked);
const allUsers = computed(() => socialStore.directoryUsers);
const directoryLoading = computed(() => socialStore.directoryLoading);
const directoryHasMore = computed(() => socialStore.directoryHasMore);

const mediaList = computed(() => sortProfileMediaChronological(userStore.user?.media ?? []));

const avatarPreview = computed(() => pickProfileMedia(userStore.user?.media));
const avatarUrl = computed(() => avatarPreview.value.url);
function openPost(post: PostProfileCard | PostReactionCard) {
  if (activeTab.value === "posts") {
    postStore.setCurrentUser({
      id: userStore.user?.id ?? "",
      login: userStore.user?.login ?? "",
      tag: userStore.user?.tag ?? "",
      avatar: avatarUrl.value ?? "",
    });

    router.push({
      name: "feed",
      params: {
        userId: userStore.user?.id,
        postId: post.id,
      },
    });

    return;
  }

  const reactionPost = post as PostReactionCard;

  postStore.setCurrentUser({
    id: reactionPost.userId,
    login: reactionPost.userLogin,
    tag: reactionPost.userTag,
    avatar: reactionPost.userAvatar ?? "",
  });

  router.push({
    name: "feed",
    params: {
      userId: reactionPost.userId,
      postId: reactionPost.id,
    },
  });
}
// =====================
// POSTS SWITCHER
// =====================
const posts = computed(() => {
  switch (activeTab.value) {
    case "liked":
      return postStore.likedPosts;

    case "saved":
      return postStore.favoritePosts;

    default:
      return postStore.profilePosts;
  }
});

const gridLoading = computed(() => postStore.loading && !posts.value.length);

const gridHasMore = computed(() => {
  if (activeTab.value === "liked") return postStore.likedHasMore;
  if (activeTab.value === "saved") return postStore.favoriteHasMore;
  return postStore.profileHasMore;
});

const postsCountLabel = computed(() =>
  postStore.profileHasMore
    ? `${postStore.profilePosts.length}+`
    : String(postStore.profilePosts.length)
);

function loadMorePosts() {
  if (!userStore.user?.id) return;

  if (activeTab.value === "posts") {
    void postStore.loadMoreProfilePosts(userStore.user.id);
    return;
  }

  if (activeTab.value === "liked") {
    void postStore.loadMoreLikedPosts();
    return;
  }

  void postStore.loadMoreFavoritePosts();
}
// =====================
// WATCH TAB
// =====================
watch(activeTab, async (tab) => {
  if (!userStore.user?.id) return;

  if (tab === "posts") {
    postStore.resetProfilePosts();
    await postStore.getProfilePosts(userStore.user.id, 1, PAGE_SIZE);
  }

  if (tab === "liked") {
    postStore.resetLikedPosts();
    await postStore.getLikedPosts(1, PAGE_SIZE);
  }

  if (tab === "saved") {
    postStore.resetFavoritePosts();
    await postStore.getFavoritePosts(1, PAGE_SIZE);
  }
});

// =====================
// INIT
// =====================
onMounted(async () => {
  await userStore.getMy();

  await socialStore.getFriends();
  await socialStore.getBlocked();

  if (userStore.user?.id) {
    postStore.resetProfilePosts();
    postStore.resetLikedPosts();
    postStore.resetFavoritePosts();

    await Promise.all([
      postStore.getProfilePosts(userStore.user.id, 1, PAGE_SIZE),
      postStore.getLikedPosts(1, PAGE_SIZE),
      postStore.getFavoritePosts(1, PAGE_SIZE),
    ]);
  }
});

// =====================
// EXISTING LOGIC (НЕ ТРОГАЕМ)
// =====================
function openGallery() {
  galleryIndex.value = mediaList.value.length
    ? mediaList.value.length - 1
    : 0;
  isGalleryOpen.value = true;
}

async function handleProfileSave(data: ProfileForm) {
  const res = await userStore.updateProfile({
    login: data.name,
    tag: data.tag || undefined,
    description: data.description,
  });

  if (!res.success) {
    showToast(
      resolveMessage(PROFILE_ERROR_MESSAGES, res.error?.code, res.error?.message),
      "error"
    );
    return;
  }

  showToast("Профиль обновлён");
  isProfileOpen.value = false;
}

async function handleCreatePost(payload: { description: string; files: File[] }) {
  createLoading.value = true;

  try {
    const res = await postStore.createPost({
      description: payload.description,
    });

    if (!res.success || !res.data) {
      showToast("Не удалось создать пост", "error");
      return;
    }

    const postId = res.data.id;

    if (payload.files.length > 0) {
      await Promise.all(
        payload.files.map(file =>
          postStore.uploadMedia(postId, file)
        )
      );
    }

    if (userStore.user?.id) {
      postStore.resetProfilePosts();
      await postStore.getProfilePosts(userStore.user.id, 1, PAGE_SIZE);
    }

    isCreatePostOpen.value = false;
    showToast("Пост опубликован");
  } finally {
    createLoading.value = false;
  }
}

// SOCIAL + MEDIA
async function handleUpload(payload: { file: File; mediaType: MediaType }) {
  const res = await mediaStore.uploadMedia(payload.file, payload.mediaType);

  if (!res.success) {
    showToast(
      resolveMessage(PROFILE_ERROR_MESSAGES, res.error?.code, res.error?.message),
      "error"
    );
    return;
  }

  await userStore.refreshMedia();
  showToast(
    payload.mediaType === MediaType.VIDEO ? "Видео добавлено" : "Фото добавлено"
  );
}

async function handleReplace(payload: { id: number; file: File; mediaType: MediaType }) {
  const res = await mediaStore.replaceCurrent(payload.id, payload.file, payload.mediaType);

  if (!res.success) {
    showToast(
      resolveMessage(PROFILE_ERROR_MESSAGES, res.error?.code, res.error?.message),
      "error"
    );
    return;
  }

  await userStore.refreshMedia();
  showToast("Медиа заменено");
}

async function handleDelete(id: number) {
  const res = await mediaStore.deleteCurrent(id);

  if (!res.success) {
    showToast(
      resolveMessage(PROFILE_ERROR_MESSAGES, res.error?.code, res.error?.message),
      "error"
    );
    return;
  }

  await userStore.refreshMedia();
  showToast("Медиа удалено");
}

async function handleDeleteAll() {
  const res = await mediaStore.deleteAll();

  if (!res.success) {
    showToast(
      resolveMessage(PROFILE_ERROR_MESSAGES, res.error?.code, res.error?.message),
      "error"
    );
    return;
  }

  await userStore.refreshMedia();
  showToast("Все медиа удалены");
}

async function refreshDirectoryUsers() {
  if (!isFriendsOpen.value) return;

  await socialStore.searchDirectoryUsers({
    search: directorySearch.value,
    page: page.value,
    pageSize: DIRECTORY_USERS_PAGE_SIZE,
  });
}

async function handleAddFriend(id: string) {
  const res = await socialStore.addFriend(id);

  if (!res.success) {
    showToast(resolveMessage(SOCIAL_ERROR_MESSAGES, res.error?.code, res.error?.message), "error");
    return;
  }

  await socialStore.getFriends();
  await refreshDirectoryUsers();
  showToast("Друг добавлен");
}

async function handleRemoveFriend(id: string) {
  const res = await socialStore.removeFriend(id);

  if (!res.success) {
    showToast(resolveMessage(SOCIAL_ERROR_MESSAGES, res.error?.code, res.error?.message), "error");
    return;
  }

  showToast("Удалён из друзей");
}

async function handleBlockUser(id: string) {
  const res = await socialStore.blockUser(id);

  if (!res.success) {
    showToast(resolveMessage(SOCIAL_ERROR_MESSAGES, res.error?.code, res.error?.message), "error");
    return;
  }

  await socialStore.getBlocked();
  await refreshDirectoryUsers();
  showToast("Пользователь заблокирован");
}

async function handleUnblockUser(id: string) {
  const res = await socialStore.unblockUser(id);

  if (!res.success) {
    showToast(resolveMessage(SOCIAL_ERROR_MESSAGES, res.error?.code, res.error?.message), "error");
    return;
  }

  showToast("Пользователь разблокирован");
}

function openRenameFriend(userId: string) {
  const friend = socialStore.friends.find((f) => f.id === userId);
  if (!friend) return;

  renameTarget.value = {
    id: friend.id,
    login: friend.login,
  };
  isRenameOpen.value = true;
}

async function handleRenameFriend(login: string) {
  if (!renameTarget.value) return;

  renameLoading.value = true;

  try {
    const res = await socialStore.renameFriend(renameTarget.value.id, login);

    if (!res.success) {
      showToast(resolveMessage(SOCIAL_ERROR_MESSAGES, res.error?.code, res.error?.message), "error");
      return;
    }

    showToast("Имя друга обновлено");
    isRenameOpen.value = false;
  } finally {
    renameLoading.value = false;
  }
}

async function handleSearch(payload: {
  search: string;
  tab: "friends" | "blocked" | "all";
  page: number;
}) {
  if (payload.tab !== "all") return;

  page.value = payload.page;
  directorySearch.value = payload.search;

  await socialStore.searchDirectoryUsers({
    search: payload.search,
    page: payload.page,
    pageSize: DIRECTORY_USERS_PAGE_SIZE,
  });
}

async function handlePage(value: number) {
  page.value = value;

  await socialStore.searchDirectoryUsers({
    search: directorySearch.value,
    page: value,
    pageSize: DIRECTORY_USERS_PAGE_SIZE,
  });

  if (value > 1 && socialStore.directoryUsers.length === 0) {
    page.value = value - 1;
    socialStore.directoryHasMore = false;

    await socialStore.searchDirectoryUsers({
      search: directorySearch.value,
      page: page.value,
      pageSize: DIRECTORY_USERS_PAGE_SIZE,
    });
  }
}
</script>

<style scoped>
/* PAGE */
.profile-page {
  width: 100%;
  max-width: 960px;
  margin: 0 auto;
  padding-bottom: 48px;
  color: white;
  position: relative;
}

/* BACKGROUND GLOW */
.bg {
  position: absolute;
  top: -120px;
  left: 50%;
  transform: translateX(-50%);
  width: min(640px, 90vw);
  height: 640px;
  background: radial-gradient(circle, rgba(65, 99, 252, 0.16), transparent 65%);
  filter: blur(80px);
  pointer-events: none;
  z-index: 0;
}

/* PROFILE CARD */
.profile-card {
  position: relative;
  z-index: 1;
  display: grid;
  grid-template-columns: 132px 1fr;
  gap: 28px;

  padding: 28px;
  border-radius: 24px;

  background: rgba(255, 255, 255, 0.04);
  border: 1px solid rgba(255, 255, 255, 0.08);
  backdrop-filter: blur(14px);
  box-shadow: 0 12px 40px rgba(0, 0, 0, 0.25);
}

/* AVATAR */
.avatar-block {
  position: relative;
  width: 132px;
  height: 132px;
  flex-shrink: 0;
}

.avatar-block :deep(.avatar),
.avatar-block :deep(.user-avatar),
.avatar-block :deep(.initials) {
  width: 100%;
  height: 100%;
  cursor: pointer;
  border: 3px solid rgba(65, 99, 252, 0.55);
  box-shadow: 0 8px 24px rgba(65, 99, 252, 0.2);
  transition: transform 0.2s ease, box-shadow 0.2s ease;
}

.avatar-block:hover :deep(.avatar),
.avatar-block:hover :deep(.user-avatar),
.avatar-block:hover :deep(.initials) {
  transform: scale(1.03);
  box-shadow: 0 10px 28px rgba(65, 99, 252, 0.35);
}

.avatar-block :deep(.initials) {
  font-size: 2rem;
}

.online {
  position: absolute;
  bottom: 10px;
  right: 10px;
  width: 14px;
  height: 14px;
  background: #3cff78;
  border-radius: 50%;
  border: 2px solid #0f0f0f;
  box-shadow: 0 0 8px rgba(60, 255, 120, 0.5);
}

.avatar-upload {
  position: absolute;
  left: 50%;
  bottom: -8px;
  transform: translateX(-50%);
  background: linear-gradient(135deg, #4163fc, #5b7cff);
  border: none;
  color: white;
  font-size: 11px;
  font-weight: 600;
  padding: 5px 10px;
  border-radius: 999px;
  cursor: pointer;
  white-space: nowrap;
  box-shadow: 0 4px 14px rgba(65, 99, 252, 0.35);
  transition: transform 0.2s ease, box-shadow 0.2s ease;
}

.avatar-upload:hover {
  transform: translateX(-50%) translateY(-1px);
  box-shadow: 0 6px 18px rgba(65, 99, 252, 0.45);
}

/* INFO */
.info-block {
  display: flex;
  flex-direction: column;
  gap: 8px;
  min-width: 0;
}

.name {
  font-size: clamp(22px, 3vw, 28px);
  margin: 0;
  font-weight: 700;
  letter-spacing: -0.02em;
}

.tag {
  opacity: 0.55;
  font-size: 14px;
  margin: 0;
}

.bio {
  font-size: 14px;
  line-height: 1.55;
  opacity: 0.78;
  max-width: 520px;
  margin: 4px 0 0;
}

/* STATS */
.stats {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 10px;
  margin-top: 14px;
}

.stat {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 4px;
  padding: 12px 8px;
  border-radius: 14px;
  background: rgba(255, 255, 255, 0.03);
  border: 1px solid rgba(255, 255, 255, 0.06);
  transition: background 0.2s ease, border-color 0.2s ease;
}

.stat:first-child {
  cursor: pointer;
}

.stat:first-child:hover {
  background: rgba(65, 99, 252, 0.1);
  border-color: rgba(65, 99, 252, 0.25);
}

.num {
  font-size: 20px;
  font-weight: 700;
  line-height: 1;
}

.label {
  font-size: 11px;
  opacity: 0.5;
  letter-spacing: 0.4px;
  text-transform: uppercase;
}

/* ACTIONS */
.actions {
  margin-top: 16px;
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
  align-items: center;
}

.edit-btn,
.create-btn {
  border: none;
  padding: 10px 16px;
  border-radius: 12px;
  font-size: 14px;
  font-weight: 600;
  cursor: pointer;
  transition: transform 0.2s ease, opacity 0.2s ease, background 0.2s ease;
}

.edit-btn {
  background: linear-gradient(135deg, #4163fc, #5b7cff);
  color: white;
  box-shadow: 0 4px 16px rgba(65, 99, 252, 0.3);
}

.edit-btn:hover {
  transform: translateY(-1px);
  opacity: 0.95;
}

.create-btn {
  background: rgba(255, 255, 255, 0.04);
  border: 1px solid rgba(255, 255, 255, 0.12);
  color: rgba(255, 255, 255, 0.9);
}

.create-btn:hover {
  background: rgba(65, 99, 252, 0.12);
  border-color: rgba(65, 99, 252, 0.35);
  color: white;
  transform: translateY(-1px);
}

/* TABS */
.tabs {
  position: relative;
  z-index: 1;
  margin-top: 28px;
  display: inline-flex;
  gap: 4px;
  padding: 4px;
  border-radius: 14px;
  background: rgba(255, 255, 255, 0.04);
  border: 1px solid rgba(255, 255, 255, 0.06);
}

.tab {
  background: transparent;
  border: none;
  color: rgba(255, 255, 255, 0.55);
  cursor: pointer;
  padding: 8px 18px;
  border-radius: 10px;
  font-size: 14px;
  font-weight: 500;
  transition: color 0.2s ease, background 0.2s ease;
}

.tab:hover {
  color: rgba(255, 255, 255, 0.85);
}

.tab.active {
  color: white;
  background: rgba(65, 99, 252, 0.22);
  box-shadow: inset 0 0 0 1px rgba(65, 99, 252, 0.35);
}

.toast {
  position: fixed;
  bottom: 24px;
  right: 24px;
  padding: 12px 16px;
  border-radius: 12px;
  z-index: 1000;
  backdrop-filter: blur(8px);
}

.toast.success {
  background: rgba(60, 255, 120, 0.15);
  border: 1px solid rgba(60, 255, 120, 0.4);
}

.toast.error {
  background: rgba(255, 80, 80, 0.15);
  border: 1px solid rgba(255, 80, 80, 0.4);
}

@media (max-width: 640px) {
  .profile-card {
    grid-template-columns: 1fr;
    justify-items: center;
    text-align: center;
    padding: 22px 18px;
  }

  .info-block {
    align-items: center;
  }

  .bio {
    text-align: center;
  }

  .actions {
    justify-content: center;
  }

  .tabs {
    width: 100%;
    justify-content: center;
  }
}
</style>
