<template>
  <div class="profile-page">
    <div class="bg"></div>

    <section v-if="loading" class="state-card">Загрузка профиля...</section>

    <section v-else-if="errorMessage" class="state-card error">
      {{ errorMessage }}
    </section>

    <template v-else-if="viewedUser">
      <section class="profile-card">
        <div class="avatar-block">
          <UserAvatar
            avatar-class="avatar"
            :name="displayName"
            :src="avatarPreview.url"
            :is-video="avatarPreview.isVideo"
            @click="openGallery"
          />
          <button
            v-if="profileMedia.length"
            class="avatar-view"
            type="button"
            @click="openGallery"
          >
            Смотреть
          </button>
        </div>

        <div class="info-block">
          <h1 class="name">{{ displayName }}</h1>
          <p class="tag">@{{ viewedUser.tag || "—" }}</p>
          <p class="bio">{{ viewedUser.description || "Без описания" }}</p>

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
              <span class="num">{{ profileMedia.length }}</span>
              <span class="label">Media</span>
            </div>
          </div>

          <div class="actions">
            <template v-if="relation.isBlocked">
              <button class="btn danger" @click="unblockUser">Remove Block</button>
            </template>

            <template v-else-if="relation.isFriend">
              <button class="btn danger" @click="removeFriend">Remove Friend</button>
              <button class="btn edit" @click="openRename">Изменить имя</button>
            </template>

            <template v-else>
              <button class="btn primary" @click="addFriend">Add Friend</button>
              <button class="btn danger" @click="blockUser">Block</button>
            </template>

            <button class="btn primary" :disabled="chatOpening" @click="openChat">
              {{ chatOpening ? "..." : "Написать" }}
            </button>
          </div>
        </div>
      </section>

      <ProfilePostGrid
        :items="posts"
        :loading="postStore.loading && !posts.length"
        :loading-more="postStore.loadingMore"
        :has-more="postStore.profileHasMore"
        empty-text="У пользователя пока нет постов"
        empty-icon="📷"
        @open="openPost"
        @load-more="loadMorePosts"
      />
    </template>

    <RenameFriendModal
      v-model="isRenameOpen"
      :initial-login="displayName"
      :loading="renameLoading"
      @save="saveFriendNickname"
    />

    <MediaGalleryModal
      v-model="isGalleryOpen"
      :media="profileMedia"
      :start-index="galleryIndex"
      readonly
    />

    <p v-if="toastMessage" :class="['toast', toastType]">{{ toastMessage }}</p>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from "vue";
import { useRoute, useRouter } from "vue-router";

import RenameFriendModal from "@/components/profile/RenameFriendModal.vue";
import MediaGalleryModal from "@/components/profile/MediaGalleryModal.vue";
import UserAvatar from "@/components/ui/UserAvatar.vue";
import ProfilePostGrid from "@/components/post/ProfilePostGrid.vue";
import type { PostProfileCard } from "@/interface/models/post/PostProfileCard";

import { usePostStore, PAGE_SIZE } from "@/store/postStore";
import { useSocialStore } from "@/store/socialStore";
import { UserService } from "@/service/userService";

import type { ViewUser } from "@/interface/models/profile/ViewUser";
import { useChatStore } from "@/store/chatStore";
import { apiErrorMessage, getApiData, isApiSuccess } from "@/utils/apiHelpers";
import { pickProfileMedia, sortProfileMediaChronological } from "@/utils/profileValidation";
import { PROFILE_ERROR_MESSAGES, SOCIAL_ERROR_MESSAGES, resolveMessage } from "@/utils/profileMessages";
import type { Media } from "@/interface/models/profile/Media";

function normalizeViewUser(raw: ViewUser | Record<string, unknown>): ViewUser {
  const item = raw as Record<string, unknown>;
  const mediaRaw = item.media ?? item.Media;

  return {
    id: String(item.id ?? item.Id ?? ""),
    login: String(item.login ?? item.Login ?? "User"),
    tag: (item.tag ?? item.Tag) as string | undefined,
    description: (item.description ?? item.Description) as string | undefined,
    media: Array.isArray(mediaRaw) ? (mediaRaw as Media[]) : [],
  };
}

const route = useRoute();
const router = useRouter();

const postStore = usePostStore();
const socialStore = useSocialStore();
const chatStore = useChatStore();
const service = new UserService();

const viewedUser = ref<ViewUser | null>(null);
const loading = ref(true);
const errorMessage = ref("");

const isRenameOpen = ref(false);
const renameLoading = ref(false);
const isGalleryOpen = ref(false);
const galleryIndex = ref(0);

const toastMessage = ref("");
const toastType = ref<"success" | "error">("success");
const chatOpening = ref(false);

const posts = computed(() => postStore.profilePosts);

function sameUserId(a?: string, b?: string) {
  return !!a && !!b && a.toLowerCase() === b.toLowerCase();
}

const relation = computed(() => {
  const id = viewedUser.value?.id;
  return {
    isFriend: socialStore.friends.some((f) => sameUserId(f.id, id)),
    isBlocked: socialStore.blocked.some((b) => sameUserId(b.id, id)),
  };
});

const friendEntry = computed(() =>
  socialStore.friends.find((f) => sameUserId(f.id, viewedUser.value?.id))
);

const displayName = computed(() => {
  if (friendEntry.value) return friendEntry.value.login;
  return viewedUser.value?.login ?? "";
});

const profileMedia = computed(() =>
  sortProfileMediaChronological(viewedUser.value?.media ?? [])
);

const avatarPreview = computed(() => pickProfileMedia(viewedUser.value?.media));
const avatarUrl = computed(() => avatarPreview.value.url);

function showToast(text: string, type: "success" | "error" = "success") {
  toastMessage.value = text;
  toastType.value = type;
  setTimeout(() => {
    toastMessage.value = "";
  }, 3500);
}

function openGallery() {
  if (!profileMedia.value.length) return;
  galleryIndex.value = profileMedia.value.length - 1;
  isGalleryOpen.value = true;
}

function openPost(post: PostProfileCard) {
  if (!viewedUser.value) return;

  postStore.setCurrentUser({
    id: viewedUser.value.id,
    login: displayName.value,
    tag: viewedUser.value.tag ?? "",
    avatar: avatarUrl.value ?? "",
  });

  router.push({
    name: "feed",
    params: {
      userId: viewedUser.value.id,
      postId: post.id,
    },
  });
}

function loadMorePosts() {
  if (!viewedUser.value) return;
  void postStore.loadMoreProfilePosts(viewedUser.value.id);
}

async function loadProfile(userId: string) {
  loading.value = true;
  errorMessage.value = "";
  viewedUser.value = null;

  try {
    postStore.resetProfilePosts();

    const userRes = await service.getUserById(userId);

    const userData = getApiData(userRes);

    if (!isApiSuccess(userRes) || !userData) {
      errorMessage.value = resolveMessage(
        PROFILE_ERROR_MESSAGES,
        userRes.error?.code ?? (userRes as { Error?: { code?: string } }).Error?.code,
        apiErrorMessage(userRes, "Профиль не найден")
      );
      return;
    }

    viewedUser.value = normalizeViewUser(userData);

    await Promise.all([
      socialStore.getFriends(),
      socialStore.getBlocked(),
      postStore.getProfilePosts(userId, 1, PAGE_SIZE),
    ]);
  } catch {
    errorMessage.value = "Не удалось загрузить профиль";
  } finally {
    loading.value = false;
  }
}

onMounted(() => {
  void loadProfile(route.params.id as string);
});

watch(
  () => route.params.id,
  (id) => {
    if (typeof id === "string" && id) {
      void loadProfile(id);
    }
  }
);

async function addFriend() {
  if (!viewedUser.value) return;

  const res = await socialStore.addFriend(viewedUser.value.id);

  if (!isApiSuccess(res)) {
    showToast(
      resolveMessage(
        SOCIAL_ERROR_MESSAGES,
        res.error?.code,
        apiErrorMessage(res, "Не удалось добавить в друзья")
      ),
      "error"
    );
    return;
  }

  showToast("Друг добавлен");
}

async function removeFriend() {
  if (!viewedUser.value) return;

  const res = await socialStore.removeFriend(viewedUser.value.id);

  if (!isApiSuccess(res)) {
    showToast(
      resolveMessage(
        SOCIAL_ERROR_MESSAGES,
        res.error?.code,
        apiErrorMessage(res, "Не удалось удалить из друзей")
      ),
      "error"
    );
    return;
  }

  showToast("Удалён из друзей");
}

async function blockUser() {
  if (!viewedUser.value) return;

  const res = await socialStore.blockUser(viewedUser.value.id);

  if (!isApiSuccess(res)) {
    showToast(
      resolveMessage(
        SOCIAL_ERROR_MESSAGES,
        res.error?.code,
        apiErrorMessage(res, "Не удалось заблокировать")
      ),
      "error"
    );
    return;
  }

  showToast("Пользователь заблокирован");
}

async function unblockUser() {
  if (!viewedUser.value) return;

  const res = await socialStore.unblockUser(viewedUser.value.id);

  if (!isApiSuccess(res)) {
    showToast(
      resolveMessage(
        SOCIAL_ERROR_MESSAGES,
        res.error?.code,
        apiErrorMessage(res, "Не удалось разблокировать")
      ),
      "error"
    );
    return;
  }

  showToast("Пользователь разблокирован");
}

function openRename() {
  if (!relation.value.isFriend) return;
  isRenameOpen.value = true;
}

async function saveFriendNickname(login: string) {
  if (!viewedUser.value) return;

  renameLoading.value = true;

  try {
    const res = await socialStore.renameFriend(viewedUser.value.id, login);

    if (!isApiSuccess(res)) {
      showToast(
        resolveMessage(
          SOCIAL_ERROR_MESSAGES,
          res.error?.code,
          apiErrorMessage(res, "Не удалось изменить имя")
        ),
        "error"
      );
      return;
    }

    showToast("Имя друга обновлено");
    isRenameOpen.value = false;
  } finally {
    renameLoading.value = false;
  }
}

async function openChat() {
  const userId = viewedUser.value?.id ?? (route.params.id as string);
  if (!userId || chatOpening.value) return;

  chatOpening.value = true;

  try {
    const res = await chatStore.startPrivateChat(userId);

    if (!isApiSuccess(res)) {
      showToast(apiErrorMessage(res, "Не удалось создать чат"), "error");
      return;
    }

    void router.push({
      name: "messages",
      query:
        chatStore.activeChatId != null
          ? { chatId: String(chatStore.activeChatId) }
          : undefined,
    });
  } finally {
    chatOpening.value = false;
  }
}
</script>

<style scoped>
.profile-page {
  width: 100%;
  max-width: 960px;
  margin: 0 auto;
  padding-bottom: 48px;
  color: white;
  position: relative;
}

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

.state-card {
  position: relative;
  z-index: 1;
  padding: 24px;
  border-radius: 18px;
  background: rgba(255, 255, 255, 0.04);
  border: 1px solid rgba(255, 255, 255, 0.06);
}

.state-card.error {
  color: #ff9b9b;
  border-color: rgba(255, 120, 120, 0.25);
}

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

.avatar-view {
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
  transition: transform 0.2s ease;
}

.avatar-view:hover {
  transform: translateX(-50%) translateY(-1px);
}

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
  opacity: 0.78;
  line-height: 1.55;
  max-width: 520px;
  margin: 4px 0 0;
  font-size: 14px;
}

.stats {
  margin-top: 14px;
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 10px;
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
}

.num {
  font-size: 20px;
  font-weight: 700;
}

.label {
  opacity: 0.5;
  font-size: 11px;
  letter-spacing: 0.4px;
  text-transform: uppercase;
}

.actions {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
  margin-top: 16px;
}

.btn {
  border: none;
  cursor: pointer;
  padding: 10px 16px;
  border-radius: 12px;
  font-size: 14px;
  font-weight: 600;
  transition: transform 0.2s ease, opacity 0.2s ease;
}

.btn:hover {
  transform: translateY(-1px);
}

.primary {
  background: linear-gradient(135deg, #4163fc, #5b7cff);
  color: white;
  box-shadow: 0 4px 16px rgba(65, 99, 252, 0.28);
}

.danger {
  background: rgba(231, 76, 60, 0.15);
  border: 1px solid rgba(231, 76, 60, 0.45);
  color: #ff8a7a;
}

.danger:hover {
  background: rgba(231, 76, 60, 0.28);
  color: white;
}

.edit {
  background: rgba(255, 255, 255, 0.04);
  border: 1px solid rgba(255, 255, 255, 0.12);
  color: rgba(255, 255, 255, 0.9);
}

.edit:hover {
  border-color: rgba(65, 99, 252, 0.35);
  background: rgba(65, 99, 252, 0.1);
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
}
</style>
