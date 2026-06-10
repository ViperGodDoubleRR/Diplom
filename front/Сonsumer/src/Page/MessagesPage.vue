<template>
  <div class="messages-page">
    <aside class="sidebar">
      <header class="sidebar-header">
        <h2>Сообщения</h2>
        <button type="button" class="icon-btn" title="Новая группа" @click="showCreateGroup = true">
          ➕
        </button>
      </header>

      <div class="search-tabs">
        <button
          type="button"
          class="tab"
          :class="{ active: searchMode === null }"
          @click="searchMode = null"
        >
          Чаты
        </button>
        <button
          type="button"
          class="tab"
          :class="{ active: searchMode === 'users' }"
          @click="searchMode = 'users'"
        >
          Люди
        </button>
        <button
          type="button"
          class="tab"
          :class="{ active: searchMode === 'groups' }"
          @click="searchMode = 'groups'"
        >
          Группы
        </button>
      </div>

      <ChatSearchPanel
        v-if="searchMode"
        :mode="searchMode"
        @open-chat="onOpenChatFromSearch"
      />

      <div v-else class="chat-list-wrap">
        <p v-if="chatStore.loadingChats" class="hint">Загрузка...</p>
        <p v-else-if="!chatStore.chats.length" class="hint muted">Чатов пока нет</p>

        <button
          v-for="chat in chatStore.chats"
          :key="chat.id"
          type="button"
          class="chat-item"
          :class="{ active: chat.id === chatStore.activeChatId }"
          @click="selectChat(chat.id)"
        >
          <UserAvatar
            avatar-class="chat-avatar"
            :name="chatStore.getChatTitle(chat)"
            :src="chatStore.getChatAvatar(chat)"
            :is-video="chatStore.getChatAvatarIsVideo(chat)"
          />

          <div class="chat-meta">
            <div class="top-row">
              <span class="title">{{ chatStore.getChatTitle(chat) }}</span>
              <span v-if="chat.lastMessage" class="time">
                {{ formatTime(chat.lastMessage.createdAt) }}
              </span>
            </div>
            <p v-if="chat.lastMessage" class="preview">
              <span v-if="chat.type === 'Group'" class="author">
                {{ chatStore.getUserDisplayLogin(chat.lastMessage.userId, chat.lastMessage.userLogin) }}:
              </span>
              {{ previewText(chat.lastMessage) }}
            </p>
            <p v-else class="preview muted">Нет сообщений</p>
          </div>
        </button>
      </div>
    </aside>

    <p v-if="pageError" class="page-error">{{ pageError }}</p>

    <main class="chat-main">
      <ChatWindow v-if="chatStore.activeChatId != null" />
      <div v-else class="empty-chat">
        <div class="empty-icon">💬</div>
        <h3>Выберите чат</h3>
        <p>Или найдите человека / группу через поиск</p>
      </div>
    </main>

    <Teleport to="body">
      <div v-if="showCreateGroup" class="modal-overlay" @click.self="showCreateGroup = false">
        <div class="modal">
          <h3>Новая группа</h3>
          <input v-model="groupName" placeholder="Название группы" maxlength="255" />
          <label class="checkbox">
            <input v-model="groupPublic" type="checkbox" />
            Открытая группа (видна в поиске)
          </label>
          <div class="modal-actions">
            <button type="button" class="btn ghost" @click="showCreateGroup = false">Отмена</button>
            <button type="button" class="btn primary" :disabled="creatingGroup" @click="createGroup">
              {{ creatingGroup ? "..." : "Создать" }}
            </button>
          </div>
          <p v-if="groupError" class="error">{{ groupError }}</p>
        </div>
      </div>
    </Teleport>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";
import { useRoute, useRouter } from "vue-router";

import ChatSearchPanel from "@/components/chat/ChatSearchPanel.vue";
import ChatWindow from "@/components/chat/ChatWindow.vue";
import UserAvatar from "@/components/ui/UserAvatar.vue";
import { useChatStore } from "@/store/chatStore";
import { useSocialStore } from "@/store/socialStore";
import { apiErrorMessage, isApiSuccess } from "@/utils/apiHelpers";

const chatStore = useChatStore();
const socialStore = useSocialStore();
const route = useRoute();
const router = useRouter();

const searchMode = ref<"users" | "groups" | null>(null);
const showCreateGroup = ref(false);
const groupName = ref("");
const groupPublic = ref(true);
const creatingGroup = ref(false);
const groupError = ref("");

const pageError = ref("");

onMounted(async () => {
  chatStore.initHub();
  void socialStore.getFriends();

  try {
    await chatStore.loadChats();
  } catch {
    pageError.value = "Не удалось загрузить чаты";
  }

  const targetUserId = route.query.userId as string | undefined;
  if (targetUserId) {
    try {
      const res = await chatStore.startPrivateChat(targetUserId);
      if (!isApiSuccess(res)) {
        pageError.value = apiErrorMessage(res, "Не удалось создать приватный чат");
      }
    } catch {
      pageError.value = "Не удалось создать приватный чат";
    }
    void router.replace({ path: "/messages" });
  }

  const chatIdRaw = route.query.chatId as string | undefined;
  if (chatIdRaw) {
    const chatId = Number(chatIdRaw);
    if (Number.isFinite(chatId)) {
      try {
        await chatStore.openChat(chatId);
      } catch {
        pageError.value = "Не удалось открыть чат";
      }
      void router.replace({ path: "/messages" });
    }
  }
});

function formatTime(date: string) {
  return new Date(date).toLocaleString(undefined, {
    day: "2-digit",
    month: "short",
    hour: "2-digit",
    minute: "2-digit",
  });
}

function previewText(last: { text: string; isDeleted: boolean }) {
  if (last.isDeleted) return "Сообщение удалено";
  return last.text || "Медиа";
}

async function selectChat(chatId: number) {
  try {
    await chatStore.openChat(chatId);
  } catch {
    pageError.value = "Не удалось открыть чат";
  }
}

async function onOpenChatFromSearch(chatId: number) {
  searchMode.value = null;
  try {
    await chatStore.openChat(chatId);
  } catch {
    pageError.value = "Не удалось открыть чат";
  }
}

async function createGroup() {
  groupError.value = "";
  if (groupName.value.trim().length < 2) {
    groupError.value = "Минимум 2 символа";
    return;
  }

  creatingGroup.value = true;

  try {
    const res = await chatStore.createGroup(groupName.value, groupPublic.value);
    if (!isApiSuccess(res)) {
      groupError.value = apiErrorMessage(res, "Не удалось создать группу");
      return;
    }
    showCreateGroup.value = false;
    groupName.value = "";
  } finally {
    creatingGroup.value = false;
  }
}
</script>

<style scoped>
.messages-page {
  position: relative;
  display: flex;
  height: calc(100vh - 48px);
  max-height: calc(100vh - 48px);
  margin: -24px -32px;
  background: #0f0f0f;
  border-radius: 16px;
  overflow: hidden;
  border: 1px solid rgba(255, 255, 255, 0.06);
}

.sidebar {
  width: 360px;
  min-width: 300px;
  display: flex;
  flex-direction: column;
  border-right: 1px solid rgba(255, 255, 255, 0.06);
  background: rgba(255, 255, 255, 0.02);
}

.sidebar-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 18px 16px 10px;
}

.sidebar-header h2 {
  margin: 0;
  font-size: 22px;
  color: white;
}

.icon-btn {
  border: none;
  background: rgba(255, 255, 255, 0.06);
  color: white;
  width: 36px;
  height: 36px;
  border-radius: 10px;
  cursor: pointer;
}

.search-tabs {
  display: flex;
  gap: 6px;
  padding: 0 12px 12px;
}

.tab {
  flex: 1;
  border: none;
  background: rgba(255, 255, 255, 0.04);
  color: rgba(255, 255, 255, 0.7);
  padding: 8px 10px;
  border-radius: 10px;
  cursor: pointer;
  font-size: 13px;
}

.tab.active {
  background: rgba(65, 99, 252, 0.25);
  color: #dbe3ff;
}

.chat-list-wrap {
  flex: 1;
  overflow-y: auto;
  padding: 0 8px 12px;
}

.chat-item {
  width: 100%;
  display: flex;
  gap: 12px;
  align-items: flex-start;
  padding: 12px;
  border: none;
  border-radius: 14px;
  background: transparent;
  cursor: pointer;
  text-align: left;
}

.chat-item:hover,
.chat-item.active {
  background: rgba(65, 99, 252, 0.12);
}

.chat-item :deep(.chat-avatar),
.chat-item :deep(.user-avatar),
.chat-item :deep(.initials) {
  width: 48px;
  height: 48px;
}

.chat-meta {
  flex: 1;
  min-width: 0;
}

.top-row {
  display: flex;
  justify-content: space-between;
  gap: 8px;
  margin-bottom: 4px;
}

.title {
  color: white;
  font-weight: 700;
  font-size: 15px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.time {
  color: rgba(255, 255, 255, 0.45);
  font-size: 11px;
  flex-shrink: 0;
}

.preview {
  margin: 0;
  color: rgba(255, 255, 255, 0.65);
  font-size: 13px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.preview .author {
  color: rgba(255, 255, 255, 0.45);
}

.hint {
  padding: 16px;
  color: rgba(255, 255, 255, 0.7);
  text-align: center;
}

.hint.muted,
.preview.muted {
  opacity: 0.55;
}

.chat-main {
  flex: 1;
  min-width: 0;
  display: flex;
  flex-direction: column;
}

.empty-chat {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  color: rgba(255, 255, 255, 0.75);
}

.empty-icon {
  font-size: 48px;
  margin-bottom: 12px;
}

.modal-overlay {
  position: fixed;
  inset: 0;
  z-index: 10000;
  background: rgba(0, 0, 0, 0.75);
  display: flex;
  align-items: center;
  justify-content: center;
}

.modal {
  width: 420px;
  max-width: 92vw;
  background: #151515;
  border-radius: 18px;
  padding: 22px;
  border: 1px solid rgba(255, 255, 255, 0.08);
}

.modal h3 {
  margin: 0 0 14px;
  color: white;
}

.modal input[type="text"],
.modal input:not([type="checkbox"]) {
  width: 100%;
  box-sizing: border-box;
  padding: 12px 14px;
  border-radius: 12px;
  border: 1px solid rgba(255, 255, 255, 0.08);
  background: rgba(255, 255, 255, 0.04);
  color: white;
  margin-bottom: 12px;
}

.checkbox {
  display: flex;
  align-items: center;
  gap: 8px;
  color: rgba(255, 255, 255, 0.8);
  font-size: 14px;
  margin-bottom: 16px;
}

.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
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

.btn.primary {
  background: #4163fc;
  color: white;
}

.error {
  margin-top: 10px;
  color: #ff8da8;
  font-size: 13px;
}

.page-error {
  position: absolute;
  top: 12px;
  left: 50%;
  transform: translateX(-50%);
  z-index: 5;
  margin: 0;
  padding: 8px 14px;
  border-radius: 10px;
  background: rgba(255, 80, 120, 0.15);
  color: #ff8da8;
  font-size: 13px;
}

@media (max-width: 900px) {
  .messages-page {
    flex-direction: column;
    height: auto;
    max-height: none;
  }

  .sidebar {
    width: 100%;
    max-height: 45vh;
  }
}
</style>
