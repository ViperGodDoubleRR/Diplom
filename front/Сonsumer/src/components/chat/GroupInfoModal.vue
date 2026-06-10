<template>
  <Teleport to="body">
    <div v-if="model && !galleryOpen" class="overlay" @click.self="close">
      <div class="panel">
        <button type="button" class="close-btn" @click="close">✕</button>

        <div class="head">
          <button
            type="button"
            class="avatar-btn clickable"
            title="Фото группы"
            @click="openGallery"
          >
            <UserAvatar
              avatar-class="group-avatar"
              :name="liveChat.name ?? 'Группа'"
              :src="groupAvatarPreview.url"
              :is-video="groupAvatarPreview.isVideo"
            />
          </button>
          <div class="head-meta">
            <h3>{{ liveChat.name ?? "Группа" }}</h3>
            <p>
              <template v-if="loadingMembers && !members.length">Загрузка...</template>
              <template v-else>
                {{ members.length }} {{ membersLabel }} ·
                {{ liveChat.isPublic ? "открытая" : "закрытая" }}
              </template>
            </p>
          </div>
        </div>

        <section v-if="isAdmin" class="section admin-section">
          <input v-model="nameDraft" class="input" maxlength="255" placeholder="Название группы" />
          <div class="privacy-row">
            <div class="privacy-copy">
              <span class="privacy-title">Открытая группа</span>
              <span class="privacy-hint">Любой может найти и вступить</span>
            </div>
            <button
              type="button"
              class="privacy-switch"
              :class="{ on: publicDraft }"
              role="switch"
              :aria-checked="publicDraft"
              @click="publicDraft = !publicDraft"
            >
              <span class="privacy-knob" />
            </button>
          </div>
          <button
            type="button"
            class="save-btn"
            :disabled="savingSettings"
            @click="saveSettings"
          >
            {{ savingSettings ? "..." : "Сохранить настройки" }}
          </button>
        </section>

        <section class="section">
          <div class="section-title">Участники</div>
          <div class="members-list">
            <p v-if="loadingMembers" class="hint">Загрузка участников...</p>
            <div v-for="member in members" :key="member.id" class="member-row">
              <UserAvatar
                avatar-class="member-avatar"
                :name="memberDisplayName(member)"
                :src="memberDisplayAvatar(member)"
                :is-video="chatStore.getUserDisplayAvatarIsVideo(member.id, member.avatarIsVideo ?? false)"
              />
              <div class="meta">
                <div class="name">{{ memberDisplayName(member) }}</div>
                <div class="tag">@{{ member.tag || "unknown" }}</div>
              </div>
              <button
                v-if="isAdmin && canRemoveMember(member.id)"
                type="button"
                class="remove-member-btn"
                :disabled="removingMemberId === member.id"
                title="Удалить из группы"
                @click="removeMember(member.id)"
              >
                {{ removingMemberId === member.id ? "..." : "✕" }}
              </button>
            </div>
            <p v-if="!members.length && !loadingMembers" class="hint">Участников нет</p>
          </div>
        </section>

        <section v-if="isAdmin" class="section">
          <div class="section-title">Добавить участника</div>

          <div class="tabs">
            <button
              type="button"
              class="tab"
              :class="{ active: inviteTab === 'all' }"
              @click="inviteTab = 'all'"
            >
              Все
            </button>
            <button
              type="button"
              class="tab"
              :class="{ active: inviteTab === 'friends' }"
              @click="inviteTab = 'friends'"
            >
              Друзья
            </button>
          </div>

          <input
            v-model="query"
            class="input"
            placeholder="Логин или @тег..."
            @input="onSearchInput"
          />

          <p v-if="loadingSearch" class="hint">Поиск...</p>

          <div v-else class="invite-list">
            <button
              v-for="user in inviteCandidates"
              :key="user.id"
              type="button"
              class="invite-row"
              :disabled="invitingId === user.id"
              @click="invite(user.id)"
            >
              <UserAvatar
                avatar-class="member-avatar"
                :name="user.login"
                :src="user.avatarUrl ?? ''"
                :is-video="user.avatarIsVideo"
              />
              <div class="meta">
                <div class="name">{{ user.login }}</div>
                <div class="tag">@{{ user.tag || "unknown" }}</div>
              </div>
              <span class="invite-action">
                {{ invitingId === user.id ? "..." : "+" }}
              </span>
            </button>
            <p v-if="!inviteCandidates.length" class="hint muted">
              {{ query.trim() ? "Никого не найдено" : "Список пуст" }}
            </p>
          </div>
        </section>

        <section v-if="isAdmin" class="section danger-section">
          <div class="section-title">Опасная зона</div>
          <button
            type="button"
            class="danger-btn"
            :disabled="clearing"
            @click="clearGroup"
          >
            {{ clearing ? "..." : "Очистить группу" }}
          </button>
          <button
            type="button"
            class="danger-btn delete"
            :disabled="deleting"
            @click="deleteGroup"
          >
            {{ deleting ? "..." : "Удалить группу" }}
          </button>
        </section>

        <p v-if="errorMessage" class="error">{{ errorMessage }}</p>
        <p v-if="successMessage" class="success">{{ successMessage }}</p>
      </div>
    </div>
  </Teleport>

  <MediaGalleryModal
    v-model="galleryOpen"
    :media="galleryMedia"
    :readonly="!isAdmin"
    :z-index="10100"
    @upload="handleGalleryUpload"
    @replace="handleGalleryReplace"
    @delete="handleGalleryDelete"
    @delete-all="handleGalleryDeleteAll"
    @error="onGalleryError"
  />
</template>

<script setup lang="ts">
import { computed, ref, watch } from "vue";

import MediaGalleryModal from "@/components/profile/MediaGalleryModal.vue";
import UserAvatar from "@/components/ui/UserAvatar.vue";
import { CHAT_SEARCH_DEBOUNCE_MS } from "@/constants/chatConstants";
import type { ChatListItemDto } from "@/interface/DTO/chat/ChatListItemDto";
import type { ChatMediaDto } from "@/interface/DTO/chat/ChatMediaDto";
import type { ChatUserDto } from "@/interface/DTO/chat/ChatUserDto";
import type { Media } from "@/interface/models/profile/Media";
import { MediaType } from "@/interface/models/profile/MediaType";
import { useChatStore } from "@/store/chatStore";
import { useSocialStore } from "@/store/socialStore";
import { useUserStore } from "@/store/userStore";
import { apiErrorMessage, isApiSuccess } from "@/utils/apiHelpers";
import { chatMediaListToGallery } from "@/utils/chatMedia";
import type { SocialListUser } from "@/utils/socialUser";

const model = defineModel<boolean>({ required: true });

const props = defineProps<{
  chat: ChatListItemDto;
}>();

const emit = defineEmits<{
  deleted: [];
}>();

const chatStore = useChatStore();
const socialStore = useSocialStore();
const userStore = useUserStore();

const members = ref<ChatUserDto[]>([]);
const chatMedia = ref<ChatMediaDto[]>([]);
const nameDraft = ref("");
const publicDraft = ref(false);
const query = ref("");
const inviteTab = ref<"all" | "friends">("all");
const loadingMembers = ref(false);
const loadingSearch = ref(false);
const savingSettings = ref(false);
const invitingId = ref<string | null>(null);
const removingMemberId = ref<string | null>(null);
const clearing = ref(false);
const deleting = ref(false);
const galleryOpen = ref(false);
const errorMessage = ref("");
const successMessage = ref("");
let debounceTimer: ReturnType<typeof setTimeout> | null = null;

const liveChat = computed(() =>
  chatStore.chats.find((c) => c.id === props.chat.id) ?? props.chat
);

const isAdmin = computed(() => liveChat.value.myRole === "Admin");

const groupAvatarPreview = computed(() =>
  chatStore.getChatAvatarPreview(liveChat.value)
);

const membersLabel = computed(() => {
  const count = members.value.length;
  const mod10 = count % 10;
  const mod100 = count % 100;
  if (mod10 === 1 && mod100 !== 11) return "участник";
  if (mod10 >= 2 && mod10 <= 4 && (mod100 < 10 || mod100 >= 20)) return "участника";
  return "участников";
});

const galleryMedia = computed<Media[]>(() => chatMediaListToGallery(chatMedia.value));

const memberIds = computed(
  () => new Set(members.value.map((m) => m.id.toLowerCase()))
);

const inviteCandidates = computed(() => {
  const myId = userStore.user?.id?.toLowerCase();
  const map = new Map<string, SocialListUser>();
  const q = query.value.trim().toLowerCase();

  const pool =
    inviteTab.value === "friends"
      ? socialStore.friends
      : [...socialStore.friends, ...socialStore.users];

  for (const user of pool) {
    if (!user.id || user.id.toLowerCase() === myId) continue;
    if (memberIds.value.has(user.id.toLowerCase())) continue;

    if (q) {
      const login = user.login.toLowerCase();
      const tag = (user.tag ?? "").toLowerCase();
      if (!login.includes(q) && !tag.includes(q) && !`@${tag}`.includes(q)) continue;
    }

    map.set(user.id, user);
  }

  return Array.from(map.values());
});

watch(
  model,
  (open) => {
    if (!open) return;
    query.value = "";
    errorMessage.value = "";
    successMessage.value = "";
    applyCachedState();
    void bootstrap();
  },
  { immediate: true }
);

function applyCachedState() {
  syncDraftsFromStore();
  members.value = [...(chatStore.groupMembersByChat[props.chat.id] ?? [])];
  chatMedia.value = [...(chatStore.groupMediaByChat[props.chat.id] ?? [])];
}

async function bootstrap() {
  loadingMembers.value = !members.value.length;

  try {
    await Promise.all([
      chatStore.prefetchGroupInfo(props.chat.id),
      runSearch(""),
      socialStore.getFriends(),
    ]);
    applyCachedState();
  } finally {
    loadingMembers.value = false;
  }
}

function syncDraftsFromStore() {
  const fresh = liveChat.value;
  nameDraft.value = fresh.name ?? "";
  publicDraft.value = fresh.isPublic;
}

function close() {
  model.value = false;
}

function memberDisplayName(member: ChatUserDto) {
  return chatStore.getUserDisplayLogin(member.id, member.login);
}

function memberDisplayAvatar(member: ChatUserDto) {
  return chatStore.getUserDisplayAvatar(member.id, member.avatar);
}

async function loadMembers() {
  loadingMembers.value = true;
  try {
    const res = await chatStore.getChatMembers(props.chat.id);
    if (isApiSuccess(res) && res.data) {
      members.value = res.data;
    }
  } finally {
    loadingMembers.value = false;
  }
}

async function loadMedia() {
  const res = await chatStore.getChatMedia(props.chat.id);
  if (isApiSuccess(res) && res.data) {
    chatMedia.value = res.data;
  }
  await chatStore.refreshChat(props.chat.id);
}

function onSearchInput() {
  if (debounceTimer) clearTimeout(debounceTimer);
  debounceTimer = setTimeout(() => {
    void runSearch(query.value);
  }, CHAT_SEARCH_DEBOUNCE_MS);
}

async function runSearch(value: string) {
  loadingSearch.value = true;
  try {
    await socialStore.searchUsers({
      search: value.trim(),
      page: 1,
      pageSize: 100,
    });
  } finally {
    loadingSearch.value = false;
  }
}

function canRemoveMember(memberId: string) {
  const myId = userStore.user?.id?.toLowerCase();
  return !!myId && memberId.toLowerCase() !== myId;
}

async function removeMember(memberId: string) {
  if (!confirm("Удалить участника из группы?")) return;

  removingMemberId.value = memberId;
  errorMessage.value = "";
  successMessage.value = "";

  try {
    const res = await chatStore.removeGroupMember(props.chat.id, memberId);

    if (!isApiSuccess(res)) {
      errorMessage.value = apiErrorMessage(res, "Не удалось удалить участника");
      return;
    }

    await loadMembers();
    successMessage.value = "Участник удалён";
  } finally {
    removingMemberId.value = null;
  }
}

async function clearGroup() {
  if (!confirm("Удалить все сообщения в группе?")) return;

  clearing.value = true;
  errorMessage.value = "";
  successMessage.value = "";

  try {
    const res = await chatStore.clearChat(props.chat.id);

    if (!isApiSuccess(res)) {
      errorMessage.value = apiErrorMessage(res, "Не удалось очистить группу");
      return;
    }

    successMessage.value = "Сообщения удалены";
  } finally {
    clearing.value = false;
  }
}

async function deleteGroup() {
  if (!confirm("Удалить группу безвозвратно?")) return;

  deleting.value = true;
  errorMessage.value = "";

  try {
    const res = await chatStore.deleteChat(props.chat.id);

    if (!isApiSuccess(res)) {
      errorMessage.value = apiErrorMessage(res, "Не удалось удалить группу");
      return;
    }

    emit("deleted");
    close();
  } finally {
    deleting.value = false;
  }
}

async function saveSettings() {
  if (!isAdmin.value) return;

  savingSettings.value = true;
  errorMessage.value = "";

  try {
    const res = await chatStore.updateGroup(props.chat.id, {
      name: nameDraft.value,
      isPublic: publicDraft.value,
    });

    if (!isApiSuccess(res)) {
      errorMessage.value = apiErrorMessage(res, "Не удалось сохранить");
      return;
    }

    syncDraftsFromStore();
    successMessage.value = "Настройки сохранены";
  } finally {
    savingSettings.value = false;
  }
}

async function invite(userId: string) {
  invitingId.value = userId;
  errorMessage.value = "";
  successMessage.value = "";

  try {
    const res = await chatStore.inviteGroupMember(props.chat.id, userId);

    if (!isApiSuccess(res)) {
      errorMessage.value = apiErrorMessage(res, "Не удалось пригласить");
      return;
    }

    await loadMembers();
    const user = socialStore.users.find((u) => u.id === userId)
      ?? socialStore.friends.find((u) => u.id === userId);
    successMessage.value = `${user?.login ?? "Пользователь"} добавлен`;
  } finally {
    invitingId.value = null;
  }
}

function openGallery() {
  galleryOpen.value = true;
}

async function handleGalleryUpload(payload: { file: File; mediaType: MediaType }) {
  const type = payload.mediaType === MediaType.VIDEO ? "video" : "avatar";
  const res = await chatStore.uploadGroupMedia(props.chat.id, payload.file, type);

  if (!isApiSuccess(res)) {
    errorMessage.value = apiErrorMessage(res, "Не удалось загрузить");
    return;
  }

  await loadMedia();
}

async function handleGalleryReplace(payload: {
  id: number;
  file: File;
  mediaType: MediaType;
}) {
  const type = payload.mediaType === MediaType.VIDEO ? "video" : "avatar";
  const res = await chatStore.replaceGroupMedia(
    props.chat.id,
    payload.id,
    payload.file,
    type
  );

  if (!isApiSuccess(res)) {
    errorMessage.value = apiErrorMessage(res, "Не удалось заменить");
    return;
  }

  await loadMedia();
}

async function handleGalleryDelete(id: number) {
  const res = await chatStore.deleteGroupMedia(props.chat.id, id);

  if (!isApiSuccess(res)) {
    errorMessage.value = apiErrorMessage(res, "Не удалось удалить");
    return;
  }

  await loadMedia();
}

async function handleGalleryDeleteAll() {
  const res = await chatStore.deleteAllGroupMedia(props.chat.id);

  if (!isApiSuccess(res)) {
    errorMessage.value = apiErrorMessage(res, "Не удалось удалить");
    return;
  }

  await loadMedia();
}

function onGalleryError(message: string) {
  errorMessage.value = message;
}
</script>

<style scoped>
.overlay {
  position: fixed;
  inset: 0;
  z-index: 10001;
  background: rgba(0, 0, 0, 0.75);
  display: flex;
  align-items: center;
  justify-content: center;
}

.panel {
  width: 420px;
  max-width: 94vw;
  max-height: 88vh;
  overflow-y: auto;
  background: #151515;
  border-radius: 18px;
  padding: 18px;
  border: 1px solid rgba(255, 255, 255, 0.08);
  position: relative;
}

.close-btn {
  position: absolute;
  top: 12px;
  right: 12px;
  border: none;
  background: rgba(255, 255, 255, 0.06);
  color: white;
  width: 32px;
  height: 32px;
  border-radius: 10px;
  cursor: pointer;
}

.head {
  display: flex;
  gap: 12px;
  align-items: center;
  width: 100%;
  padding: 0 36px 14px 0;
  margin-bottom: 8px;
  border-bottom: 1px solid rgba(255, 255, 255, 0.06);
}

.avatar-btn {
  border: none;
  background: transparent;
  padding: 0;
  border-radius: 50%;
  flex-shrink: 0;
}

.avatar-btn.clickable {
  cursor: pointer;
}

.avatar-btn.clickable:hover {
  box-shadow: 0 0 0 3px rgba(65, 99, 252, 0.25);
}

.remove-member-btn {
  border: none;
  background: rgba(255, 141, 168, 0.12);
  color: #ff8da8;
  width: 30px;
  height: 30px;
  border-radius: 8px;
  cursor: pointer;
  flex-shrink: 0;
}

.remove-member-btn:disabled {
  opacity: 0.5;
  cursor: default;
}

.danger-section {
  display: flex;
  flex-direction: column;
  gap: 8px;
  padding-top: 8px;
  border-top: 1px solid rgba(255, 255, 255, 0.06);
}

.danger-btn {
  width: 100%;
  border: 1px solid rgba(255, 141, 168, 0.25);
  background: rgba(255, 141, 168, 0.08);
  color: #ff8da8;
  padding: 10px 12px;
  border-radius: 12px;
  cursor: pointer;
  text-align: left;
}

.danger-btn.delete {
  border-color: rgba(255, 90, 120, 0.35);
  background: rgba(255, 90, 120, 0.12);
}

.danger-btn:disabled {
  opacity: 0.5;
  cursor: default;
}

.head :deep(.group-avatar),
.head :deep(.user-avatar),
.head :deep(.initials) {
  width: 56px;
  height: 56px;
}

.head-meta h3 {
  margin: 0;
  color: white;
  font-size: 18px;
}

.head-meta p {
  margin: 4px 0 0;
  color: rgba(255, 255, 255, 0.45);
  font-size: 12px;
}

.section {
  margin-bottom: 16px;
}

.section-title {
  color: rgba(255, 255, 255, 0.55);
  font-size: 12px;
  text-transform: uppercase;
  letter-spacing: 0.04em;
  margin-bottom: 8px;
}

.admin-section {
  display: flex;
  flex-direction: column;
  gap: 10px;
  padding-bottom: 14px;
  border-bottom: 1px solid rgba(255, 255, 255, 0.06);
}

.input {
  width: 100%;
  box-sizing: border-box;
  padding: 10px 12px;
  border-radius: 12px;
  border: 1px solid rgba(255, 255, 255, 0.08);
  background: rgba(255, 255, 255, 0.04);
  color: white;
}

.privacy-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  padding: 12px 14px;
  border-radius: 14px;
  background: rgba(255, 255, 255, 0.04);
  border: 1px solid rgba(255, 255, 255, 0.06);
}

.privacy-copy {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.privacy-title {
  color: white;
  font-size: 14px;
  font-weight: 600;
}

.privacy-hint {
  color: rgba(255, 255, 255, 0.42);
  font-size: 12px;
}

.privacy-switch {
  width: 48px;
  height: 28px;
  border: none;
  border-radius: 999px;
  background: rgba(255, 255, 255, 0.14);
  padding: 3px;
  cursor: pointer;
  transition: background 0.2s ease;
  flex-shrink: 0;
}

.privacy-switch.on {
  background: #4163fc;
}

.privacy-knob {
  display: block;
  width: 22px;
  height: 22px;
  border-radius: 50%;
  background: white;
  transform: translateX(0);
  transition: transform 0.2s ease;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.25);
}

.privacy-switch.on .privacy-knob {
  transform: translateX(20px);
}

.save-btn {
  align-self: flex-start;
  border: none;
  background: rgba(65, 99, 252, 0.25);
  color: #dbe3ff;
  padding: 8px 14px;
  border-radius: 10px;
  cursor: pointer;
}

.members-list,
.invite-list {
  max-height: 200px;
  overflow-y: auto;
}

.member-row,
.invite-row {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 8px 4px;
  width: 100%;
  border: none;
  background: transparent;
  text-align: left;
  cursor: default;
}

.invite-row {
  cursor: pointer;
  border-radius: 10px;
}

.invite-row:hover:not(:disabled) {
  background: rgba(255, 255, 255, 0.05);
}

.invite-row:disabled {
  opacity: 0.6;
}

.member-row :deep(.member-avatar),
.invite-row :deep(.member-avatar),
.member-row :deep(.user-avatar),
.invite-row :deep(.user-avatar) {
  width: 38px;
  height: 38px;
}

.meta {
  flex: 1;
  min-width: 0;
}

.name {
  color: white;
  font-weight: 600;
  font-size: 14px;
}

.tag {
  color: rgba(255, 255, 255, 0.45);
  font-size: 12px;
}

.invite-action {
  color: #7c9bff;
  font-size: 18px;
  line-height: 1;
}

.tabs {
  display: flex;
  gap: 6px;
  margin-bottom: 8px;
}

.tab {
  border: none;
  background: rgba(255, 255, 255, 0.05);
  color: rgba(255, 255, 255, 0.7);
  padding: 6px 12px;
  border-radius: 999px;
  cursor: pointer;
  font-size: 12px;
}

.tab.active {
  background: rgba(65, 99, 252, 0.25);
  color: #dbe3ff;
}

.hint {
  text-align: center;
  color: rgba(255, 255, 255, 0.5);
  font-size: 13px;
  padding: 12px 0;
}

.hint.muted {
  opacity: 0.7;
}

.error {
  color: #ff8da8;
  font-size: 13px;
}

.success {
  color: #8dffb0;
  font-size: 13px;
}
</style>
