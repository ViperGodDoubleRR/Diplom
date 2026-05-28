<template>
  <div class="profile-page">

    <div class="bg"></div>

    <section v-if="viewedUser" class="profile-card">

      <!-- AVATAR -->
      <div class="avatar-block">
        <img class="avatar" :src="avatarUrl" />
        <div class="online"></div>
      </div>

      <!-- INFO -->
      <div class="info-block">

        <h1 class="name">{{ viewedUser.login }}</h1>
        <p class="tag">@{{ viewedUser.tag }}</p>
        <p class="bio">{{ viewedUser.description }}</p>

        <!-- STATS -->
        <div class="stats">
          <div class="stat">
            <span class="num">{{ postStore.totalPosts }}</span>
            <span class="label">Posts</span>
          </div>

          <div class="stat">
            <span class="num">{{ postStore.totalLikes }}</span>
            <span class="label">Likes</span>
          </div>

          <div class="stat">
            <span class="num">{{ socialStore.friends.length }}</span>
            <span class="label">Friends</span>
          </div>
        </div>

        <!-- ACTIONS -->
        <div class="actions">

          <!-- BLOCKED -->
          <template v-if="relation.isBlocked">

            <button class="btn danger" @click="unblockUser">
              Remove Block
            </button>

          </template>

          <!-- FRIEND -->
          <template v-else-if="relation.isFriend">

            <button class="btn danger" @click="removeFriend">
              Remove Friend
            </button>

            <button class="btn edit" @click="editLogin">
              Edit Login
            </button>

          </template>

          <!-- NEUTRAL -->
          <template v-else>

            <button class="btn primary" @click="addFriend">
              Add Friend
            </button>

            <button class="btn danger" @click="blockUser">
              Block
            </button>

          </template>

          <!-- 💬 MESSAGE ALWAYS VISIBLE -->
          <button class="btn primary" @click="openChat">
            Message
          </button>

        </div>

      </div>
    </section>

    <!-- POSTS -->
    <section class="grid">

      <div v-for="post in posts" :key="post.id" class="post" @click="openPost(post)">
        <div class="post-media">
          <img v-if="post.mediaUrl" :src="post.mediaUrl" class="preview" />
        </div>

        <div class="post-info">

          <div class="text">
            {{ post.description }}
          </div>

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

  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from "vue";
import { useRoute } from "vue-router";

import { usePostStore } from "@/store/postStore";
import { useSocialStore } from "@/store/socialStore";
import { UserService } from "@/service/userService";

import type { ViewUser } from "@/interface/models/profile/ViewUser";
import { MediaType } from "@/interface/models/profile/MediaType";
import { useRouter } from "vue-router";
const route = useRoute();

const postStore = usePostStore();
const socialStore = useSocialStore();
const service = new UserService();
const router = useRouter();
const viewedUser = ref<ViewUser | null>(null);

// posts
const posts = computed(() => postStore.profilePosts);

// relation (🔥 главное)
const relation = computed(() => {
  const id = viewedUser.value?.id;

  const isFriend = socialStore.friends.some(f => f.id === id);
  const isBlocked = socialStore.blocked.some(b => b.id === id);

  return {
    isFriend,
    isBlocked,
  };
});
function openPost(post: any) {
  if (!viewedUser.value) return;

  // 🔥 СНАЧАЛА кладём в pinia
  postStore.setCurrentUser({
    id: viewedUser.value.id,
    login: viewedUser.value.login,
    tag: viewedUser.value.tag,
    avatar: avatarUrl.value,
  });

  // 🔥 потом навигация
  router.push({
    name: "feed",
    params: {
      userId: viewedUser.value.id,
      postId: post.id,
    },
  });
}

// avatar
const avatarUrl = computed(() => {
  const media = viewedUser.value?.media ?? [];

  const avatar = media.find(
    (m) => m.mediaType?.toLowerCase() === MediaType.AVATAR
  );

  return avatar?.url || "https://i.pravatar.cc/300";
});

// init
onMounted(async () => {
  const userId = route.params.id as string;

  const res = await service.getUserById(userId);

  if (res.success) {
    viewedUser.value = res.data!;
  }

  await Promise.all([
    socialStore.getFriends(),
    socialStore.getBlocked(),
    postStore.getProfilePosts(userId, 1, 12),
  ]);
});

// actions
async function addFriend() {
  if (!viewedUser.value) return;
  await socialStore.addFriend(viewedUser.value.id);
}

async function removeFriend() {
  if (!viewedUser.value) return;
  await socialStore.removeFriend(viewedUser.value.id);
}

async function blockUser() {
  if (!viewedUser.value) return;
  await socialStore.blockUser(viewedUser.value.id);
}

async function unblockUser() {
  if (!viewedUser.value) return;
  await socialStore.unblockUser(viewedUser.value.id);
}

function editLogin() {
  console.log("edit login");
}

function openChat() {
  console.log("open chat");
}
</script>

<style scoped>
.profile-page {
  min-height: 100vh;
  background: #0F0F0F;
  padding: 40px;
  color: white;
  position: relative;
  overflow-x: hidden;
}

.bg {
  position: absolute;
  top: -200px;
  left: 50%;
  transform: translateX(-50%);
  width: 700px;
  height: 700px;

  background: radial-gradient(circle,
      rgba(65, 99, 252, 0.18),
      transparent 60%);

  filter: blur(80px);
}

.profile-card {
  position: relative;

  display: grid;
  grid-template-columns: 120px 1fr;

  gap: 25px;

  padding: 25px;

  border-radius: 22px;

  max-width: 900px;

  background: rgba(255, 255, 255, 0.04);

  border: 1px solid rgba(255, 255, 255, 0.06);

  backdrop-filter: blur(12px);
}

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

  right: 8px;
  bottom: 8px;

  width: 12px;
  height: 12px;

  border-radius: 50%;

  background: #3CFF78;

  border: 2px solid #0F0F0F;
}

.info-block {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.name {
  font-size: 26px;
  margin: 0;
}

.tag {
  opacity: 0.5;
}

.bio {
  opacity: 0.8;
  max-width: 500px;
}

.stats {
  margin-top: 10px;

  display: grid;
  grid-template-columns: repeat(3, 1fr);

  gap: 10px;
}

.stat {
  display: flex;
  flex-direction: column;
  align-items: center;

  gap: 5px;
}

.num {
  font-size: 18px;
  font-weight: 700;
}

.label {
  opacity: 0.5;
  font-size: 12px;
}

.actions {
  display: flex;
  gap: 10px;

  margin-top: 12px;
}

.btn {
  border: none;
  cursor: pointer;

  padding: 10px 14px;

  border-radius: 12px;

  transition: 0.2s;
}

.btn:hover {
  transform: translateY(-1px);
}

.primary {
  background: #4163FC;
  color: white;
}

.success {
  background: #2ecc71;
  color: white;
}

.danger {
  background: #e74c3c;
  color: white;
}

.edit {
  background: rgba(65, 99, 252, 0.15);

  border: 1px solid rgba(65, 99, 252, 0.4);

  color: #a9b7ff;
}

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

  transition: 0.2s;
}

.post:hover {
  transform: scale(1.02);
}

.post-media {
  height: 180px;

  background: #222;

  overflow: hidden;
}

.preview {
  width: 100%;
  height: 100%;

  object-fit: cover;
}

.post-info {
  padding: 10px;
}

.text {
  font-size: 13px;
}

.post-meta {
  display: flex;
  justify-content: space-between;

  margin-top: 6px;

  font-size: 11px;

  opacity: 0.6;
}

.likes {
  color: #ff4d6d;
}
</style>
