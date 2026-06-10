<template>
  <Teleport to="body">
    <div v-if="open" class="backdrop" @click.self="close">
      <div class="menu" :style="menuStyle" @click.stop>
        <div class="group-head">
          <UserAvatar
            avatar-class="avatar"
            :name="group.name ?? 'Группа'"
            :src="chatStore.getChatAvatar(group)"
            :is-video="chatStore.getChatAvatarIsVideo(group)"
          />
          <div>
            <div class="name">{{ group.name ?? "Группа" }}</div>
            <div class="tag">Открытая группа</div>
          </div>
        </div>

        <button
          v-if="group.isMember"
          type="button"
          class="menu-item primary"
          :disabled="busy"
          @click="openChat"
        >
          Открыть чат
        </button>
        <button
          v-else
          type="button"
          class="menu-item primary"
          :disabled="busy"
          @click="join"
        >
          {{ busy ? "..." : "Вступить в группу" }}
        </button>

        <p v-if="errorMessage" class="error">{{ errorMessage }}</p>
      </div>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
import { computed, ref } from "vue";

import UserAvatar from "@/components/ui/UserAvatar.vue";
import type { ChatListItemDto } from "@/interface/DTO/chat/ChatListItemDto";
import { useChatStore } from "@/store/chatStore";
import { apiErrorMessage, isApiSuccess } from "@/utils/apiHelpers";

const open = defineModel<boolean>({ required: true });

const props = defineProps<{
  group: ChatListItemDto;
  anchorX?: number;
  anchorY?: number;
}>();

const emit = defineEmits<{
  "chat-opened": [chatId: number];
}>();

const chatStore = useChatStore();
const busy = ref(false);
const errorMessage = ref("");

const menuStyle = computed(() => {
  const x = props.anchorX ?? window.innerWidth / 2;
  const y = props.anchorY ?? window.innerHeight / 2;
  return {
    left: `${Math.min(x, window.innerWidth - 280)}px`,
    top: `${Math.min(y, window.innerHeight - 180)}px`,
  };
});

function close() {
  open.value = false;
  errorMessage.value = "";
}

async function openChat() {
  busy.value = true;
  errorMessage.value = "";

  try {
    await chatStore.openChat(props.group.id);
    emit("chat-opened", props.group.id);
    close();
  } catch {
    errorMessage.value = "Не удалось открыть чат";
  } finally {
    busy.value = false;
  }
}

async function join() {
  busy.value = true;
  errorMessage.value = "";

  try {
    const res = await chatStore.joinGroup(props.group.id);

    if (!isApiSuccess(res)) {
      errorMessage.value = apiErrorMessage(res, "Не удалось вступить");
      return;
    }

    if (chatStore.activeChatId != null) {
      emit("chat-opened", chatStore.activeChatId);
    }

    close();
  } finally {
    busy.value = false;
  }
}
</script>

<style scoped>
.backdrop {
  position: fixed;
  inset: 0;
  z-index: 10002;
}

.menu {
  position: fixed;
  width: 260px;
  background: #171717;
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 16px;
  padding: 12px;
  box-shadow: 0 16px 40px rgba(0, 0, 0, 0.45);
}

.group-head {
  display: flex;
  gap: 10px;
  align-items: center;
  padding-bottom: 10px;
  margin-bottom: 8px;
  border-bottom: 1px solid rgba(255, 255, 255, 0.08);
}

.menu :deep(.avatar),
.menu :deep(.user-avatar),
.menu :deep(.initials) {
  width: 44px;
  height: 44px;
}

.name {
  color: white;
  font-weight: 700;
}

.tag {
  color: rgba(255, 255, 255, 0.45);
  font-size: 12px;
}

.menu-item {
  width: 100%;
  border: none;
  background: rgba(255, 255, 255, 0.05);
  color: white;
  border-radius: 10px;
  padding: 10px 12px;
  text-align: left;
  cursor: pointer;
}

.menu-item.primary {
  background: rgba(65, 99, 252, 0.25);
  color: #dbe3ff;
}

.menu-item:disabled {
  opacity: 0.5;
  cursor: default;
}

.error {
  margin: 6px 0 0;
  color: #ff8da8;
  font-size: 12px;
}
</style>
