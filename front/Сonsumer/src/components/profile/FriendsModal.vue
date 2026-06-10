<template>
  <Teleport to="body">
    <Transition name="modal">
      <div v-if="modelValue" class="overlay" @click.self="close">
      <div class="modal">

        <!-- HEADER -->
        <div class="header">
          <div>
            <h2>Социальная сеть</h2>
            <p>Друзья, блоки и пользователи</p>
          </div>

          <button class="close-btn" @click="close">✕</button>
        </div>

        <!-- SEARCH -->
        <div class="search">
          <input v-model="search" placeholder="Поиск пользователя..." @input="onSearch" />
        </div>

        <!-- TABS -->
        <div class="tabs">
          <button :class="{ active: tab === 'friends' }" @click="setTab('friends')">
            Friends ({{ friends.length }})
          </button>

          <button :class="{ active: tab === 'blocked' }" @click="setTab('blocked')">
            Blocked ({{ blocked.length }})
          </button>

          <button :class="{ active: tab === 'all' }" @click="setTab('all')">
            All users
          </button>
        </div>

        <!-- LIST -->
        <div class="list">

          <!-- FRIENDS -->
          <div v-if="tab === 'friends'">
            <p v-if="!filteredFriends.length" class="empty">
              {{ search.trim() ? "Друзья не найдены" : "Список друзей пуст" }}
            </p>

            <div v-for="u in filteredFriends" :key="u.id" class="row hoverable" @click="openProfile(u.id)">
              <div class="avatar-cell">
                <UserAvatar
                  :size="42"
                  :name="u.login"
                  :src="u.avatarUrl"
                  :is-video="u.avatarIsVideo"
                />
              </div>

              <div class="info">
                <div class="name">{{ u.login }}</div>
                <div class="tag">@{{ u.tag }}</div>
              </div>

              <div class="actions">
                <button class="btn" @click.stop="emit('rename', u.id)">
                  Rename
                </button>

                <button class="btn danger" @click.stop="emit('removeFriend', u.id)">
                  Remove
                </button>

                <button class="btn danger" @click.stop="emit('block', u.id)">
                  Block
                </button>
              </div>
            </div>
          </div>

          <!-- BLOCKED -->
          <div v-else-if="tab === 'blocked'">
            <p v-if="!filteredBlocked.length" class="empty">
              {{ search.trim() ? "Заблокированные не найдены" : "Чёрный список пуст" }}
            </p>

            <div v-for="u in filteredBlocked" :key="u.id" class="row hoverable" @click="openProfile(u.id)">
              <div class="avatar-cell">
                <UserAvatar
                  :size="42"
                  :name="u.login"
                  :src="u.avatarUrl"
                  :is-video="u.avatarIsVideo"
                />
              </div>

              <div class="info">
                <div class="name">{{ u.login }}</div>
                <div class="tag">@{{ u.tag }}</div>
              </div>

              <div class="actions">
                <button class="btn" @click.stop="emit('unblock', u.id)">
                  Unblock
                </button>
              </div>
            </div>
          </div>

          <!-- ALL USERS -->
          <div v-else-if="tab === 'all'">
            <p v-if="loading" class="empty">Поиск...</p>
            <p v-else-if="!allUsers.length" class="empty">
              {{ search.trim() ? "Пользователи не найдены" : "Нет пользователей для показа" }}
            </p>

            <div v-for="u in allUsers" :key="u.id" class="row hoverable" @click="openProfile(u.id)">
              <div class="avatar-cell">
                <UserAvatar
                  :size="42"
                  :name="u.login"
                  :src="u.avatarUrl"
                  :is-video="u.avatarIsVideo"
                />
              </div>

              <div class="info grow">
                <div class="name">{{ u.login }}</div>
                <div class="tag">@{{ u.tag }}</div>
              </div>

              <div class="actions">
                <button class="btn" @click.stop="emit('add', u.id)">
                  Add
                </button>

                <button class="btn danger" @click.stop="emit('block', u.id)">
                  Block
                </button>
              </div>
            </div>

            <!-- PAGINATION -->
            <div class="pagination">
              <button @click="prevPage" :disabled="page === 1">
                Prev
              </button>

              <span>Page {{ page }}</span>

              <button @click="nextPage" :disabled="loading || !hasMore">
                Next
              </button>
            </div>

          </div>

        </div>

      </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup lang="ts">
import { computed, onUnmounted, ref, watch } from "vue";
import UserAvatar from "@/components/ui/UserAvatar.vue";
import { useSocialStore } from "@/store/socialStore";

import {
  filterSocialUsersByQuery,
  type SocialListUser,
} from "@/utils/socialUser";

type Tab = "friends" | "blocked" | "all";
import { useRouter } from "vue-router";
const router = useRouter();
function openProfile(userId: string) {
  router.push(`/profile/${userId}`);
}
const props = defineProps<{
  modelValue: boolean;
  friends: SocialListUser[];
  blocked: SocialListUser[];
  allUsers: SocialListUser[];
  page: number;
  hasMore?: boolean;
  loading?: boolean;
}>();

const emit = defineEmits<{
  (e: "update:modelValue", v: boolean): void;

  (e: "search", payload: { search: string; tab: Tab; page: number }): void;

  (e: "page", value: number): void;

  (e: "add", id: string): void;
  (e: "block", id: string): void;

  (e: "removeFriend", id: string): void;
  (e: "rename", id: string): void;
  (e: "unblock", id: string): void;
}>();

const tab = ref<Tab>("friends");
const search = ref("");
const socialStore = useSocialStore();

let debounceTimer: ReturnType<typeof setTimeout> | null = null;

const filteredFriends = computed(() =>
  filterSocialUsersByQuery(props.friends, search.value)
);

const filteredBlocked = computed(() =>
  filterSocialUsersByQuery(props.blocked, search.value)
);

watch(
  () => props.modelValue,
  async (open) => {
    if (!open) return;

    await Promise.all([socialStore.getFriends(), socialStore.getBlocked()]);

    if (tab.value === "all") {
      emitDirectorySearch(1);
    }
  }
);

onUnmounted(() => {
  if (debounceTimer) clearTimeout(debounceTimer);
});

function close() {
  emit("update:modelValue", false);
}

function emitDirectorySearch(page: number) {
  emit("search", {
    search: search.value,
    tab: "all",
    page,
  });
}

function setTab(t: Tab) {
  tab.value = t;

  if (t === "all") {
    emitDirectorySearch(1);
  }
}

function onSearch() {
  if (tab.value !== "all") return;

  if (debounceTimer) clearTimeout(debounceTimer);

  debounceTimer = setTimeout(() => {
    emitDirectorySearch(1);
  }, 300);
}

function nextPage() {
  if (!props.hasMore || props.loading) return;
  emit("page", props.page + 1);
}

function prevPage() {
  if (props.page > 1) emit("page", props.page - 1);
}
</script>

<style scoped>
.overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.78);
  backdrop-filter: blur(10px);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 10000;
}

.modal {
  width: 560px;
  max-width: calc(100vw - 32px);
  background: #0f0f0f;
  border: 1px solid #222;
  border-radius: 18px;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 12px;
  padding: 14px 16px;
  border-bottom: 1px solid #222;
  flex-shrink: 0;
}

.header h2 {
  color: white;
  margin: 0;
  font-size: 18px;
}

.header p {
  margin: 4px 0 0;
  font-size: 12px;
  color: #777;
}

.close-btn {
  background: transparent;
  border: 1px solid #333;
  color: white;
  border-radius: 8px;
  cursor: pointer;
}

/* SEARCH */
.search {
  padding: 12px;
}

.search input {
  width: 100%;
  padding: 10px;
  border-radius: 10px;
  border: 1px solid #333;
  background: #111;
  color: white;
}

/* TABS */
.tabs {
  display: flex;
  gap: 6px;
  padding: 0 12px 12px;
}

.tabs button {
  flex: 1;
  padding: 8px;
  background: #111;
  border: 1px solid #222;
  color: white;
  border-radius: 8px;
  cursor: pointer;
}

.tabs button.active {
  background: #2d5bff;
}

/* LIST */
.list {
  overflow-y: auto;
  max-height: 420px;
  padding: 0 12px 12px;
}

/* ROW */
.row {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 10px 4px;
  border-bottom: 1px solid #1f1f1f;
}

.avatar-cell {
  flex: 0 0 42px;
  width: 42px;
  height: 42px;
}

.avatar-cell :deep(.avatar-shell) {
  width: 42px;
  height: 42px;
  min-width: 42px;
  min-height: 42px;
}

.info {
  display: flex;
  flex-direction: column;
  flex: 1;
  min-width: 0;
}

.grow {
  flex: 1;
  min-width: 0;
}

.name {
  color: white;
  font-size: 14px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.tag {
  font-size: 11px;
  color: #777;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

/* ACTIONS */
.actions {
  display: flex;
  flex-shrink: 0;
  gap: 4px;
}

.btn {
  padding: 5px 8px;
  font-size: 12px;
  background: #2d5bff;
  border: none;
  color: white;
  border-radius: 8px;
  cursor: pointer;
  white-space: nowrap;
}

.btn.danger {
  background: #ff3b3b;
}

/* HOVER */
.hoverable:hover {
  background: rgba(255, 255, 255, 0.03);
}

/* PAGINATION */
.pagination {
  display: flex;
  justify-content: space-between;
  padding: 12px 0;
  border-top: 1px solid #222;
  margin-top: 10px;
  color: #aaa;
}

.pagination button {
  background: #111;
  border: 1px solid #333;
  color: white;
  padding: 6px 10px;
  border-radius: 8px;
}

.pagination button:disabled {
  opacity: 0.45;
  cursor: not-allowed;
}

.empty {
  margin: 12px 0;
  text-align: center;
  color: #777;
  font-size: 13px;
}
</style>
