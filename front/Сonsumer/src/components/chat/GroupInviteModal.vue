<template>
  <Teleport to="body">
    <div v-if="model" class="overlay" @click.self="close">
      <div class="modal">
        <h3>Пригласить в группу</h3>

        <input
          v-model="query"
          type="text"
          placeholder="Логин или тег..."
          @input="onInput"
        />

        <p v-if="loading" class="hint">Поиск...</p>

        <div v-else class="user-list">
          <button
            v-for="user in displayUsers"
            :key="user.id"
            type="button"
            class="user-row"
            :disabled="invitingId === user.id"
            @click="invite(user.id)"
          >
            <UserAvatar
              avatar-class="avatar"
              :name="user.login"
              :src="user.avatarUrl ?? ''"
            />
            <div class="meta">
              <div class="name">{{ user.login }}</div>
              <div class="tag">@{{ user.tag || "unknown" }}</div>
            </div>
            <span class="action">
              {{ invitingId === user.id ? "..." : "Пригласить" }}
            </span>
          </button>

          <p v-if="!displayUsers.length" class="hint muted">
            {{ query.trim() ? "Никого не найдено" : "Введите имя для поиска" }}
          </p>
        </div>

        <div class="actions">
          <button type="button" class="btn ghost" @click="close">Закрыть</button>
        </div>

        <p v-if="errorMessage" class="error">{{ errorMessage }}</p>
        <p v-if="successMessage" class="success">{{ successMessage }}</p>
      </div>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
import { computed, ref, watch } from "vue";

import UserAvatar from "@/components/ui/UserAvatar.vue";
import { CHAT_SEARCH_DEBOUNCE_MS } from "@/constants/chatConstants";
import { useChatStore } from "@/store/chatStore";
import { useSocialStore } from "@/store/socialStore";
import { useUserStore } from "@/store/userStore";
import { apiErrorMessage, isApiSuccess } from "@/utils/apiHelpers";
const model = defineModel<boolean>({ required: true });

const props = defineProps<{
  chatId: number;
}>();

const chatStore = useChatStore();
const socialStore = useSocialStore();
const userStore = useUserStore();

const query = ref("");
const loading = ref(false);
const invitingId = ref<string | null>(null);
const errorMessage = ref("");
const successMessage = ref("");
let debounceTimer: ReturnType<typeof setTimeout> | null = null;

const displayUsers = computed(() => {
  const myId = userStore.user?.id?.toLowerCase();
  return socialStore.users.filter(
    (user) => user.id && user.id.toLowerCase() !== myId
  );
});

watch(model, (open) => {
  if (!open) return;
  query.value = "";
  errorMessage.value = "";
  successMessage.value = "";
  void runSearch("");
});

function close() {
  model.value = false;
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
    await socialStore.searchUsers({
      search: value.trim(),
      page: 1,
      pageSize: 50,
    });
  } finally {
    loading.value = false;
  }
}

async function invite(userId: string) {
  errorMessage.value = "";
  successMessage.value = "";
  invitingId.value = userId;

  try {
    const res = await chatStore.inviteGroupMember(props.chatId, userId);

    if (!isApiSuccess(res)) {
      errorMessage.value = apiErrorMessage(res, "Не удалось пригласить");
      return;
    }

    const user = socialStore.users.find((u) => u.id === userId);
    successMessage.value = `${user?.login ?? "Пользователь"} приглашён`;
  } finally {
    invitingId.value = null;
  }
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

.modal {
  width: 460px;
  max-width: 92vw;
  max-height: 80vh;
  display: flex;
  flex-direction: column;
  background: #151515;
  border-radius: 18px;
  padding: 22px;
  border: 1px solid rgba(255, 255, 255, 0.08);
}

.modal h3 {
  margin: 0 0 14px;
  color: white;
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

.user-list {
  flex: 1;
  overflow-y: auto;
  min-height: 180px;
  max-height: 360px;
}

.user-row {
  width: 100%;
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 10px;
  border: none;
  border-radius: 12px;
  background: transparent;
  cursor: pointer;
  text-align: left;
}

.user-row:hover:not(:disabled) {
  background: rgba(255, 255, 255, 0.05);
}

.user-row:disabled {
  opacity: 0.6;
  cursor: default;
}

.user-row :deep(.avatar),
.user-row :deep(.user-avatar),
.user-row :deep(.initials) {
  width: 40px;
  height: 40px;
}

.meta {
  flex: 1;
  min-width: 0;
}

.name {
  color: white;
  font-weight: 600;
}

.tag {
  color: rgba(255, 255, 255, 0.45);
  font-size: 12px;
}

.action {
  color: #7c9bff;
  font-size: 12px;
}

.actions {
  display: flex;
  justify-content: flex-end;
  margin-top: 12px;
}

.btn {
  border: none;
  border-radius: 10px;
  padding: 10px 16px;
  cursor: pointer;
}

.btn.ghost {
  background: rgba(255, 255, 255, 0.06);
  color: white;
}

.hint {
  text-align: center;
  color: rgba(255, 255, 255, 0.55);
  font-size: 13px;
  padding: 16px 0;
}

.hint.muted {
  opacity: 0.7;
}

.error {
  margin-top: 8px;
  color: #ff8da8;
  font-size: 13px;
}

.success {
  margin-top: 8px;
  color: #8dffb0;
  font-size: 13px;
}
</style>
