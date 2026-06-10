<template>
  <Teleport to="body">
    <div v-if="model" class="overlay" @click.self="close">
      <div class="modal">
        <h3>Изменить чат</h3>

        <label class="field">
          <span>Название</span>
          <input v-model="nameDraft" maxlength="255" />
        </label>

        <label class="field file-field">
          <span>Фото группы</span>
          <input type="file" accept="image/*" @change="onAvatarSelected" />
        </label>

        <div class="actions">
          <button type="button" class="btn ghost" @click="close">Закрыть</button>
          <button type="button" class="btn primary" :disabled="saving" @click="save">
            {{ saving ? "..." : "Сохранить" }}
          </button>
        </div>

        <p v-if="errorMessage" class="error">{{ errorMessage }}</p>
      </div>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
import { ref, watch } from "vue";

import type { ChatListItemDto } from "@/interface/DTO/chat/ChatListItemDto";
import { useChatStore } from "@/store/chatStore";
import { isApiSuccess } from "@/utils/apiHelpers";

const model = defineModel<boolean>({ required: true });

const props = defineProps<{
  chat: ChatListItemDto;
  isAdmin: boolean;
}>();

const chatStore = useChatStore();
const nameDraft = ref("");
const avatarFile = ref<File | null>(null);
const saving = ref(false);
const errorMessage = ref("");

watch(
  () => props.chat,
  (chat) => {
    nameDraft.value = chat.name ?? "";
  },
  { immediate: true }
);

function close() {
  model.value = false;
}

function onAvatarSelected(event: Event) {
  const input = event.target as HTMLInputElement;
  avatarFile.value = input.files?.[0] ?? null;
}

async function save() {
  if (!props.isAdmin) {
    errorMessage.value = "Только админ может менять группу";
    return;
  }

  errorMessage.value = "";
  saving.value = true;

  try {
    if (nameDraft.value.trim() && nameDraft.value.trim() !== (props.chat.name ?? "")) {
      const res = await chatStore.updateGroupName(props.chat.id, nameDraft.value);
      if (!isApiSuccess(res)) {
        errorMessage.value = res?.error?.message ?? "Не удалось сохранить название";
        return;
      }
    }

    if (avatarFile.value) {
      const res = await chatStore.uploadGroupAvatar(props.chat.id, avatarFile.value);
      if (!isApiSuccess(res)) {
        errorMessage.value = res?.error?.message ?? "Не удалось загрузить фото";
        return;
      }
    }

    close();
  } finally {
    saving.value = false;
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
  width: 440px;
  max-width: 92vw;
  background: #151515;
  border-radius: 18px;
  padding: 22px;
  border: 1px solid rgba(255, 255, 255, 0.08);
}

.modal h3 {
  margin: 0 0 16px;
  color: white;
}

.field {
  display: flex;
  flex-direction: column;
  gap: 6px;
  margin-bottom: 14px;
  color: rgba(255, 255, 255, 0.75);
  font-size: 13px;
}

.field input[type="text"],
.field input:not([type="file"]) {
  padding: 11px 14px;
  border-radius: 12px;
  border: 1px solid rgba(255, 255, 255, 0.08);
  background: rgba(255, 255, 255, 0.04);
  color: white;
}

.actions {
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
</style>
