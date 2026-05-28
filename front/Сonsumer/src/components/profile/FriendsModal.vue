<template>
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
            <div v-for="u in friends" :key="u.id" class="row hoverable" @click="openProfile(u.id)">
              <img :src="u.avatarUrl || fallback" />

              <div class="info">
                <div class="name">{{ u.login }}</div>
                <div class="tag">@{{ u.tag }}</div>
              </div>

              <div class="actions">
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
            <div v-for="u in blocked" :key="u.id" class="row hoverable" @click="openProfile(u.id)">
              <img :src="u.avatarUrl || fallback" />

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

            <div v-for="u in allUsers" :key="u.id" class="row hoverable" @click="openProfile(u.id)">
              <img :src="u.avatarUrl || fallback" />

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

              <button @click="nextPage">
                Next
              </button>
            </div>

          </div>

        </div>

      </div>
    </div>
  </Transition>
</template>

<script setup lang="ts">
import { ref } from "vue";

import type { Friend } from "@/interface/models/profile/Friend";
import type { BlackList } from "@/interface/models/profile/BlackList";
import type { UserPreview } from "@/interface/models/profile/UserPreview";

type Tab = "friends" | "blocked" | "all";
import { useRouter } from "vue-router";
const router = useRouter();
function openProfile(userId: string) {
  router.push(`/profile/${userId}`);
}
const props = defineProps<{
  modelValue: boolean;
  friends: Friend[];
  blocked: BlackList[];
  allUsers: UserPreview[];
  page: number;
}>();

const emit = defineEmits<{
  (e: "update:modelValue", v: boolean): void;

  (e: "search", payload: { search: string; tab: Tab; page: number }): void;

  (e: "page", value: number): void;

  (e: "add", id: string): void;
  (e: "block", id: string): void;

  (e: "removeFriend", id: string): void;
  (e: "unblock", id: string): void;
}>();

const tab = ref<Tab>("friends");
const search = ref("");

const fallback = "https://i.pravatar.cc/150";

function close() {
  emit("update:modelValue", false);
}

function setTab(t: Tab) {
  tab.value = t;

  emit("search", {
    search: search.value,
    tab: t,
    page: 1,
  });
}

function onSearch() {
  emit("search", {
    search: search.value,
    tab: tab.value,
    page: 1,
  });
}

function nextPage() {
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
  background: rgba(0, 0, 0, .7);
  backdrop-filter: blur(8px);
  display: flex;
  justify-content: center;
  align-items: center;
}

.modal {
  width: 560px;
  max-height: 85vh;
  background: #0f0f0f;
  border: 1px solid #222;
  border-radius: 18px;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

/* HEADER */
.header {
  display: flex;
  justify-content: space-between;
  padding: 14px 16px;
  border-bottom: 1px solid #222;
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
  flex: 1;
  padding: 0 12px;
}

/* ROW */
.row {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 10px;
  border-bottom: 1px solid #1f1f1f;
}

.row img {
  width: 42px;
  height: 42px;
  border-radius: 50%;
}

.info {
  display: flex;
  flex-direction: column;
}

.grow {
  flex: 1;
}

.name {
  color: white;
}

.tag {
  font-size: 12px;
  color: #777;
}

/* ACTIONS */
.actions {
  display: flex;
  gap: 6px;
}

.btn {
  padding: 6px 10px;
  background: #2d5bff;
  border: none;
  color: white;
  border-radius: 8px;
  cursor: pointer;
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
</style>
