<template>
  <Teleport to="body">
    <div v-if="model" class="overlay" @click.self="close">
      <div class="panel">
        <button type="button" class="close-btn" @click="close">✕</button>

        <div class="head">
          <UserAvatar
            avatar-class="chat-avatar"
            :name="chatStore.getChatTitle(chat)"
            :src="chatStore.getChatAvatar(chat)"
            :is-video="chatStore.getChatAvatarIsVideo(chat)"
          />
          <div class="head-meta">
            <h3>{{ chatStore.getChatTitle(chat) }}</h3>
            <p>Приватный чат</p>
          </div>
        </div>

        <section class="section danger-section">
          <div class="section-title">Действия</div>
          <button
            type="button"
            class="danger-btn"
            :disabled="clearing"
            @click="clearChat"
          >
            {{ clearing ? "..." : "Очистить чат" }}
          </button>
          <button
            type="button"
            class="danger-btn delete"
            :disabled="deleting"
            @click="deleteChat"
          >
            {{ deleting ? "..." : "Удалить чат" }}
          </button>
        </section>

        <p v-if="errorMessage" class="error">{{ errorMessage }}</p>
        <p v-if="successMessage" class="success">{{ successMessage }}</p>
      </div>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
import { ref } from "vue";

import UserAvatar from "@/components/ui/UserAvatar.vue";
import type { ChatListItemDto } from "@/interface/DTO/chat/ChatListItemDto";
import { useChatStore } from "@/store/chatStore";
import { apiErrorMessage, isApiSuccess } from "@/utils/apiHelpers";

const model = defineModel<boolean>({ required: true });

const props = defineProps<{
  chat: ChatListItemDto;
}>();

const emit = defineEmits<{
  deleted: [];
}>();

const chatStore = useChatStore();

const clearing = ref(false);
const deleting = ref(false);
const errorMessage = ref("");
const successMessage = ref("");

function close() {
  model.value = false;
  errorMessage.value = "";
  successMessage.value = "";
}

async function clearChat() {
  if (!confirm("Удалить все сообщения в этом чате?")) return;

  clearing.value = true;
  errorMessage.value = "";
  successMessage.value = "";

  try {
    const res = await chatStore.clearChat(props.chat.id);

    if (!isApiSuccess(res)) {
      errorMessage.value = apiErrorMessage(res, "Не удалось очистить чат");
      return;
    }

    successMessage.value = "Чат очищен";
  } finally {
    clearing.value = false;
  }
}

async function deleteChat() {
  if (!confirm("Удалить чат безвозвратно?")) return;

  deleting.value = true;
  errorMessage.value = "";

  try {
    const res = await chatStore.deleteChat(props.chat.id);

    if (!isApiSuccess(res)) {
      errorMessage.value = apiErrorMessage(res, "Не удалось удалить чат");
      return;
    }

    emit("deleted");
    close();
  } finally {
    deleting.value = false;
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

.panel {
  width: 360px;
  max-width: 94vw;
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
  padding: 0 36px 14px 0;
  margin-bottom: 8px;
  border-bottom: 1px solid rgba(255, 255, 255, 0.06);
}

.head :deep(.chat-avatar),
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

.section-title {
  color: rgba(255, 255, 255, 0.55);
  font-size: 12px;
  text-transform: uppercase;
  letter-spacing: 0.04em;
  margin-bottom: 8px;
}

.danger-section {
  display: flex;
  flex-direction: column;
  gap: 8px;
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

.error {
  color: #ff8da8;
  font-size: 13px;
  margin-top: 10px;
}

.success {
  color: #8dffb0;
  font-size: 13px;
  margin-top: 10px;
}
</style>
