<template>
  <div class="search-panel">
    <input
      v-model="query"
      type="text"
      :placeholder="placeholder"
      @input="onInput"
    />

    <p v-if="loading" class="hint">Поиск...</p>

    <template v-else-if="mode === 'users'">
      <button
        v-for="user in displayUsers"
        :key="user.id"
        type="button"
        class="result-item"
        @click="openUserMenu($event, user)"
      >
        <UserAvatar avatar-class="avatar" :name="user.login" :src="user.avatarUrl ?? ''" />
        <div class="meta">
          <div class="name">{{ user.login }}</div>
          <div class="tag">@{{ user.tag || "unknown" }}</div>
        </div>
        <span v-if="userBadge(user)" class="badge">{{ userBadge(user) }}</span>
      </button>
      <p v-if="!displayUsers.length" class="hint muted">
        {{ query.trim() ? "Никого не найдено" : "Введите логин или тег" }}
      </p>
    </template>

    <template v-else>
      <p v-if="discoverGroups.length" class="discover-hint">
        Открытые группы, в которые можно вступить
      </p>

      <div
        v-for="group in discoverGroups"
        :key="group.id"
        class="result-item group-item"
      >
        <button type="button" class="group-main" @click="onGroupAction(group)">
          <UserAvatar
            avatar-class="avatar"
            :name="group.name ?? 'Группа'"
            :src="chatStore.getChatAvatar(group)"
            :is-video="chatStore.getChatAvatarIsVideo(group)"
          />
          <div class="meta">
            <div class="name">{{ group.name ?? "Группа" }}</div>
            <div class="tag">Открытая группа</div>
          </div>
        </button>

        <button
          type="button"
          class="join-btn"
          :disabled="joiningId === group.id"
          @click="joinGroup(group)"
        >
          {{ joiningId === group.id ? "..." : "Вступить" }}
        </button>
      </div>

      <p v-if="!discoverGroups.length" class="hint muted">
        {{
          query.trim()
            ? "Открытых групп не найдено"
            : "Нет новых открытых групп — создайте свою или измените поиск"
        }}
      </p>
    </template>

    <ChatUserActionsMenu
      v-if="selectedUser"
      v-model="userMenuOpen"
      :user="selectedUser"
      :anchor-x="menuX"
      :anchor-y="menuY"
      @chat-opened="onChatOpened"
    />
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from "vue";

import ChatUserActionsMenu from "@/components/chat/ChatUserActionsMenu.vue";
import UserAvatar from "@/components/ui/UserAvatar.vue";
import { CHAT_SEARCH_DEBOUNCE_MS } from "@/constants/chatConstants";
import type { ChatListItemDto } from "@/interface/DTO/chat/ChatListItemDto";
import { useChatStore } from "@/store/chatStore";
import { useSocialStore } from "@/store/socialStore";
import { useUserStore } from "@/store/userStore";
import { apiErrorMessage, isApiSuccess } from "@/utils/apiHelpers";
import {
  filterSocialUsersByQuery,
  normalizeUserSearchTerm,
  type SocialListUser,
} from "@/utils/socialUser";

const props = defineProps<{
  mode: "users" | "groups";
}>();

const emit = defineEmits<{
  "open-chat": [chatId: number];
}>();

const chatStore = useChatStore();
const socialStore = useSocialStore();
const userStore = useUserStore();

const query = ref("");
const loading = ref(false);
const joiningId = ref<number | null>(null);
let debounceTimer: ReturnType<typeof setTimeout> | null = null;

const userMenuOpen = ref(false);
const selectedUser = ref<SocialListUser | null>(null);
const menuX = ref(0);
const menuY = ref(0);

const friendIds = computed(
  () => new Set(socialStore.friends.map((u) => u.id.toLowerCase()))
);
const blockedIds = computed(
  () => new Set(socialStore.blocked.map((u) => u.id.toLowerCase()))
);

const placeholder = computed(() =>
  props.mode === "users" ? "Логин или тег..." : "Название группы..."
);

function isGroupType(type?: string | null) {
  return (type ?? "").toLowerCase() === "group";
}

const discoverGroups = computed(() =>
  chatStore.groupSearchResults.filter(
    (group) => isGroupType(group.type) && group.isPublic && !group.isMember
  )
);

const displayUsers = computed(() => {
  const myId = userStore.user?.id?.toLowerCase();
  const map = new Map<string, SocialListUser>();

  for (const user of socialStore.friends) {
    if (!user.id || user.id.toLowerCase() === myId) continue;
    if (blockedIds.value.has(user.id.toLowerCase())) continue;
    map.set(user.id.toLowerCase(), user);
  }

  for (const user of socialStore.users) {
    if (!user.id || user.id.toLowerCase() === myId) continue;
    if (blockedIds.value.has(user.id.toLowerCase())) continue;
    map.set(user.id.toLowerCase(), user);
  }

  const combined = Array.from(map.values());
  const term = query.value.trim();

  if (!term) {
    return combined;
  }

  return filterSocialUsersByQuery(combined, term);
});

onMounted(() => {
  void runSearch("");
});

function userBadge(user: SocialListUser): string | null {
  const id = user.id.toLowerCase();
  if (blockedIds.value.has(id)) return "Заблок.";
  if (friendIds.value.has(id)) return "Друг";
  return null;
}

function onInput() {
  if (debounceTimer) clearTimeout(debounceTimer);

  debounceTimer = setTimeout(() => {
    void runSearch(query.value);
  }, CHAT_SEARCH_DEBOUNCE_MS);
}

async function runSearch(value: string) {
  loading.value = true;

  try {
    if (props.mode === "users") {
      const search = normalizeUserSearchTerm(value);

      await Promise.all([
        socialStore.getFriends(),
        socialStore.getBlocked(),
        socialStore.searchUsers({
          search,
          page: 1,
          pageSize: 50,
        }),
      ]);
    } else {
      await chatStore.searchPublicGroups(value.trim());
    }
  } catch {
    /* errors handled in store/api */
  } finally {
    loading.value = false;
  }
}

function openUserMenu(event: MouseEvent, user: SocialListUser) {
  const rect = (event.currentTarget as HTMLElement).getBoundingClientRect();
  menuX.value = rect.left;
  menuY.value = rect.bottom + 6;
  selectedUser.value = user;
  userMenuOpen.value = true;
}

async function joinGroup(group: ChatListItemDto) {
  joiningId.value = group.id;

  try {
    const res = await chatStore.joinGroup(group.id);

    if (!isApiSuccess(res)) {
      alert(apiErrorMessage(res, "Не удалось вступить в группу"));
      return;
    }

    emit("open-chat", group.id);
  } finally {
    joiningId.value = null;
  }
}

function onGroupAction(group: ChatListItemDto) {
  void joinGroup(group);
}

function onChatOpened(chatId: number) {
  emit("open-chat", chatId);
}
</script>

<style scoped>
.search-panel {
  padding: 0 12px 12px;
  flex: 1;
  overflow-y: auto;
}

input {
  width: 100%;
  box-sizing: border-box;
  padding: 11px 14px;
  border-radius: 12px;
  border: 1px solid rgba(255, 255, 255, 0.08);
  background: rgba(255, 255, 255, 0.04);
  color: white;
  margin-bottom: 10px;
}

.discover-hint {
  margin: 0 0 8px;
  padding: 0 4px;
  color: rgba(255, 255, 255, 0.45);
  font-size: 12px;
}

.result-item {
  width: 100%;
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 10px;
  border: none;
  border-radius: 12px;
  background: transparent;
  cursor: pointer;
  text-align: left;
}

.result-item:hover {
  background: rgba(255, 255, 255, 0.05);
}

.group-item {
  padding: 6px 8px;
  gap: 8px;
}

.group-main {
  flex: 1;
  display: flex;
  align-items: center;
  gap: 12px;
  border: none;
  background: transparent;
  color: inherit;
  cursor: pointer;
  text-align: left;
  padding: 4px;
  min-width: 0;
}

.group-main:hover {
  opacity: 0.92;
}

.join-btn {
  flex-shrink: 0;
  border: none;
  background: rgba(65, 99, 252, 0.22);
  color: #dbe3ff;
  padding: 8px 14px;
  border-radius: 10px;
  font-size: 12px;
  font-weight: 600;
  cursor: pointer;
}

.join-btn:hover:not(:disabled) {
  background: rgba(65, 99, 252, 0.34);
}

.join-btn:disabled {
  opacity: 0.55;
  cursor: default;
}

.result-item :deep(.avatar),
.group-main :deep(.avatar),
.group-main :deep(.user-avatar),
.group-main :deep(.initials) {
  width: 42px;
  height: 42px;
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

.badge {
  margin-left: auto;
  color: rgba(255, 255, 255, 0.45);
  font-size: 11px;
  flex-shrink: 0;
}

.hint {
  text-align: center;
  color: rgba(255, 255, 255, 0.6);
  font-size: 13px;
  padding: 12px;
}

.hint.muted {
  opacity: 0.55;
}
</style>
