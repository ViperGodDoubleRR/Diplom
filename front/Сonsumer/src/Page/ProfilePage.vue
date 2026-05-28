<template>
  <div class="profile-page">

    <div class="bg"></div>

    <section class="profile-card">

      <div class="avatar-block">
        <img class="avatar" :src="avatarUrl" @click="openGallery" />
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
            <span class="num">{{ postStore.totalPosts }}</span>
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

    <!-- GRID -->
    <section class="grid">
      <div v-for="post in posts" :key="post.id" class="post" @click="openPost(post)">
        <div class="post-media">
          <img v-if="post.mediaUrl" :src="post.mediaUrl" class="preview" />
        </div>

        <div class="post-info">
          <div class="text">{{ post.description }}</div>

          <div class="post-meta">
            <span class="date">
              {{ new Date(post.createdAt).toLocaleDateString() }}
            </span>

            <span class="likes">
              ❤️ {{ post.likesCount }}
            </span>
          </div>
        </div>
      </div>
    </section>

    <!-- MODALS -->
    <ProfileModal v-model="isProfileOpen" :loading="false" :initial-data="{
      name: userStore.user?.login || '',
      tag: userStore.user?.tag || '',
      description: userStore.user?.description || ''
    }" @save="handleProfileSave" />

    <FriendsModal v-model="isFriendsOpen" :friends="friends" :blocked="blocked" :allUsers="allUsers" :page="page"
      @add="handleAddFriend" @block="handleBlockUser" @removeFriend="handleRemoveFriend" @unblock="handleUnblockUser"
      @search="handleSearch" @page="handlePage" />

    <MediaGalleryModal v-model="isGalleryOpen" :media="mediaList" :start-index="galleryIndex" @upload="handleUpload"
      @replace="handleReplace" @delete="handleDelete" @delete-all="handleDeleteAll" />

    <CreatePostModal v-model="isCreatePostOpen" :loading="createLoading" @submit="handleCreatePost" />

  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from "vue";

import ProfileModal from "@/components/profile/ProfileModal.vue";
import FriendsModal from "@/components/profile/FriendsModal.vue";
import MediaGalleryModal from "@/components/profile/MediaGalleryModal.vue";
import CreatePostModal from "@/components/profile/CreatePostModal.vue";

import { useUserStore } from "@/store/userStore";
import { useMediaStore } from "@/store/usermediaStore";
import { useSocialStore } from "@/store/socialStore";
import { usePostStore } from "@/store/postStore";
import { useRouter } from "vue-router";
import { MediaType } from "@/interface/models/profile/MediaType";
import type { ProfileForm } from "@/interface/models/profile/ProfileForm";

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

const galleryIndex = ref(0);
const page = ref(1);

const activeTab = ref<"posts" | "liked" | "saved">("posts");


const router = useRouter();
// =====================
// DATA
// =====================
const friends = computed(() => socialStore.friends);
const blocked = computed(() => socialStore.blocked);
const allUsers = computed(() => socialStore.users);

const mediaList = computed(() => userStore.user?.media ?? []);

const avatarUrl = computed(() => {
  const avatar = mediaList.value.find(m => m.mediaType === MediaType.AVATAR);
  return avatar?.url || "https://i.pravatar.cc/300";
});
function openPost(post: import("@/interface/models/post/PostReaction").PostReaction | import("@/interface/models/post/PostProfileCard").PostProfileCard) {

  // МОИ ПОСТЫ
  if (activeTab.value === "posts") {

    postStore.setCurrentUser({
      id: userStore.user?.id ?? "",
      login: userStore.user?.login ?? "",
      tag: userStore.user?.tag ?? "",
      avatar: avatarUrl.value,
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

  // LIKED + SAVED
  const reactionPost = post as import("@/interface/models/post/PostReactionCard").PostReactionCard;

  postStore.setCurrentUser({
    id: reactionPost.userId,
    login: reactionPost.userLogin,
    tag: reactionPost.userTag,
    avatar: reactionPost.userAvatar,
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

// =====================
// LOAD FAVORITES
// =====================
async function loadFavorites() {
  await postStore.getFavoritePosts(1, 12);
}
async function loadLiked() {
  await postStore.getLikedPosts(1, 12);
}
// =====================
// WATCH TAB
// =====================
watch(activeTab, async (tab) => {
  if (!userStore.user?.id) return;

  if (tab === "posts") {
    await postStore.getProfilePosts(userStore.user.id, 1, 12);
  }

  if (tab === "liked") {
    await loadLiked();
  }

  if (tab === "saved") {
    await loadFavorites();
  }
});

// =====================
// INIT
// =====================
onMounted(async () => {
  await userStore.getMy();

  await socialStore.getFriends();
  await socialStore.getBlocked();

  await socialStore.searchUsers({
    search: "",
    page: 1,
    pageSize: 20,
  });

  if (userStore.user?.id) {
    await Promise.all([
      postStore.getProfilePosts(userStore.user.id, 1, 12),
      loadLiked(),
      loadFavorites(),
    ]);
  }
});

// =====================
// EXISTING LOGIC (НЕ ТРОГАЕМ)
// =====================
function openGallery() {
  galleryIndex.value = 0;
  isGalleryOpen.value = true;
}

async function handleProfileSave(data: ProfileForm) {
  await userStore.updateProfile({
    login: data.name,
    tag: data.tag,
    description: data.description,
  });

  isProfileOpen.value = false;
}

async function handleCreatePost(payload: { description: string; files: File[] }) {
  createLoading.value = true;

  try {
    const res = await postStore.createPost({
      description: payload.description,
    });

    if (!res.success || !res.data) return;

    const postId = res.data.id;

    if (payload.files.length > 0) {
      await Promise.all(
        payload.files.map(file =>
          postStore.uploadMedia(postId, file)
        )
      );
    }

    if (userStore.user?.id) {
      await postStore.getProfilePosts(userStore.user.id, 1, 12);
    }

    isCreatePostOpen.value = false;
  } finally {
    createLoading.value = false;
  }
}

// SOCIAL + MEDIA (НЕ ТРОГАЕМ)
async function handleUpload(file: File) {
  await mediaStore.uploadMedia(file, MediaType.AVATAR);
  await userStore.getMy();
}

async function handleReplace(payload: any) {
  await mediaStore.replaceCurrent(payload.id, payload.file, MediaType.AVATAR);
  await userStore.getMy();
}

async function handleDelete(id: number) {
  await mediaStore.deleteCurrent(id);
  await userStore.getMy();
}

async function handleDeleteAll() {
  await mediaStore.deleteAll();
  await userStore.getMy();
}

async function handleAddFriend(id: string) {
  await socialStore.addFriend(id);
  await socialStore.getFriends();
}

async function handleRemoveFriend(id: string) {
  socialStore.friends = socialStore.friends.filter(f => f.id !== id);
}

async function handleBlockUser(id: string) {
  await socialStore.blockUser(id);
  socialStore.friends = socialStore.friends.filter(f => f.id !== id);
  await socialStore.getBlocked();
}

async function handleUnblockUser(id: string) {
  await socialStore.unblockUser(id);
  socialStore.blocked = socialStore.blocked.filter(b => b.id !== id);
}

async function handleSearch(payload: any) {
  page.value = payload.page;

  await socialStore.searchUsers({
    search: payload.search,
    page: payload.page,
    pageSize: 20,
  });
}

function handlePage(value: number) {
  page.value = value;

  socialStore.searchUsers({
    search: "",
    page: value,
    pageSize: 20,
  });
}
</script>

<style scoped>
/* PAGE */
.profile-page {
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
  width: 700px;
  height: 700px;
  background: radial-gradient(circle, rgba(65, 99, 252, 0.18), transparent 60%);
  filter: blur(80px);
}

/* PROFILE CARD */
.profile-card {
  position: relative;
  display: grid;
  grid-template-columns: 120px 1fr;
  gap: 25px;

  padding: 25px;
  border-radius: 22px;

  background: rgba(255, 255, 255, 0.04);
  border: 1px solid rgba(255, 255, 255, 0.06);
  backdrop-filter: blur(12px);

  max-width: 900px;
}

/* AVATAR */
.avatar-block {
  position: relative;
  width: 120px;
  height: 120px;
}

.avatar {
  width: 100%;
  height: 100%;
  border-radius: 50%;
  object-fit: cover;
  border: 3px solid rgba(65, 99, 252, 0.6);
}

.online {
  position: absolute;
  bottom: 8px;
  right: 8px;
  width: 12px;
  height: 12px;
  background: #3CFF78;
  border-radius: 50%;
  border: 2px solid #0F0F0F;
}

/* INFO */
.info-block {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

/* NAME */
.name {
  font-size: 26px;
  margin: 0;
}

.tag {
  opacity: 0.6;
  font-size: 14px;
}

/* BIO */
.bio {
  font-size: 14px;
  opacity: 0.8;
  max-width: 500px;
}

/* STATS */
.stats {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 10px;

  margin-top: 10px;
}

.stat {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 4px;

  padding: 10px 0;
}

.num {
  font-size: 18px;
  font-weight: 600;
  line-height: 1;
}

.label {
  font-size: 12px;
  opacity: 0.5;
  letter-spacing: 0.3px;
}

/* ACTIONS */
.actions {
  margin-top: 12px;
  display: flex;
  gap: 12px;
  align-items: center;
}

.edit-btn {
  background: #4163FC;
  border: none;
  padding: 10px 14px;
  border-radius: 12px;
  color: white;
  cursor: pointer;
  transition: 0.2s;
}

.edit-btn:hover {
  opacity: 0.9;
  transform: translateY(-1px);
}

/* TABS */
.tabs {
  margin-top: 20px;
  display: flex;
  gap: 15px;
}

.tab {
  background: transparent;
  border: none;
  color: white;
  opacity: 0.5;
  cursor: pointer;
  padding: 6px 10px;
}

.tab.active {
  opacity: 1;
  border-bottom: 2px solid #4163FC;
}

/* POSTS GRID */
.grid {
  margin-top: 25px;
  max-width: 900px;

  column-count: 3;
  column-gap: 15px;
}

.post {
  break-inside: avoid;
  margin-bottom: 15px;

  border-radius: 15px;
  overflow: hidden;

  background: rgba(255, 255, 255, 0.03);
  cursor: pointer;

  transition: 0.2s;
}

.post:hover {
  transform: scale(1.03);
}

.post-media {
  height: 180px;
  background: #222;
  overflow: hidden;
  display: flex;
  align-items: center;
  justify-content: center;
}

.post-meta {
  display: flex;
  justify-content: space-between;
  font-size: 11px;
  opacity: 0.6;
  margin-top: 4px;
}

.likes {
  color: #ff4d6d;
}

.preview {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.text {
  font-size: 13px;
  opacity: 0.9;
}

.date {
  font-size: 11px;
  opacity: 0.5;
  margin-top: 4px;
}

.post-info {
  padding: 10px;
}

.post-title {
  font-size: 14px;
}

.post-meta {
  font-size: 12px;
  opacity: 0.5;
}

.create-btn {
  background: rgba(65, 99, 252, 0.15);
  border: 1px solid rgba(65, 99, 252, 0.4);
  padding: 10px 14px;
  border-radius: 12px;
  color: #a9b7ff;
  cursor: pointer;
  transition: 0.2s;

  backdrop-filter: blur(8px);
}

.create-btn:hover {
  background: rgba(65, 99, 252, 0.25);
  border-color: rgba(65, 99, 252, 0.7);
  color: white;
  transform: translateY(-1px);
}

.create-btn:active {
  transform: translateY(0px);
}
</style>
