<template>
  <Teleport to="body">
    <div v-if="open" class="backdrop" @click.self="close">
      <div class="menu" :style="menuStyle" @click.stop>
        <div class="user-head">
          <UserAvatar avatar-class="avatar" :name="user.login" :src="user.avatarUrl ?? ''" />
          <div>
            <div class="name">{{ user.login }}</div>
            <div class="tag">@{{ user.tag || "unknown" }}</div>
          </div>
        </div>

        <button type="button" class="menu-item" @click="viewProfile">Просмотреть профиль</button>
        <button type="button" class="menu-item primary" :disabled="busy" @click="startChat">
          {{ busy ? "Создание..." : "Написать сообщение" }}
        </button>

        <p v-if="errorMessage" class="error">{{ errorMessage }}</p>
      </div>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
import { computed, ref } from "vue";
import { useRouter } from "vue-router";

import UserAvatar from "@/components/ui/UserAvatar.vue";
import type { SocialListUser } from "@/utils/socialUser";
import { useChatStore } from "@/store/chatStore";
import { useUserStore } from "@/store/userStore";
import { apiErrorMessage, isApiSuccess } from "@/utils/apiHelpers";

const open = defineModel<boolean>({ required: true });

const props = defineProps<{
  user: SocialListUser;
  anchorX?: number;
  anchorY?: number;
}>();

const emit = defineEmits<{
  "chat-opened": [chatId: number];
}>();

const router = useRouter();
const chatStore = useChatStore();
const userStore = useUserStore();

const busy = ref(false);
const errorMessage = ref("");

const menuStyle = computed(() => {
  const x = props.anchorX ?? window.innerWidth / 2;
  const y = props.anchorY ?? window.innerHeight / 2;
  return {
    left: `${Math.min(x, window.innerWidth - 280)}px`,
    top: `${Math.min(y, window.innerHeight - 220)}px`,
  };
});

function close() {
  open.value = false;
  errorMessage.value = "";
}

function viewProfile() {
  if (props.user.id === userStore.user?.id) {
    void router.push({ name: "profile" });
  } else {
    void router.push({ name: "profile-view", params: { id: props.user.id } });
  }
  close();
}

async function startChat() {
  if (props.user.id === userStore.user?.id) {
    errorMessage.value = "Нельзя написать самому себе";
    return;
  }

  busy.value = true;
  errorMessage.value = "";

  try {
    const res = await chatStore.startPrivateChat(props.user.id);

    if (!isApiSuccess(res)) {
      errorMessage.value = apiErrorMessage(res, "Не удалось создать чат");
      return;
    }

    if (chatStore.activeChatId != null) {
      emit("chat-opened", chatStore.activeChatId);
    }

    close();
  } catch {
    errorMessage.value = "Не удалось создать чат";
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

.user-head {
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
  margin-bottom: 6px;
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
