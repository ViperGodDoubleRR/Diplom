<template>
  <Teleport to="body">
    <Transition name="modal">
      <div v-if="modelValue" class="overlay" @click.self="close">
      <div class="modal">
        <div class="header">
          <div>
            <h2>Имя для друга</h2>
            <p>Только вы видите это имя в своём списке друзей</p>
          </div>
          <button class="close-btn" :disabled="loading" @click="close">✕</button>
        </div>

        <div class="field">
          <label>Отображаемое имя</label>
          <input
            v-model="nickname"
            type="text"
            maxlength="32"
            placeholder="Как показывать друга"
            @keyup.enter="save"
          />
          <p v-if="error" class="error">{{ error }}</p>
        </div>

        <div class="actions">
          <button class="cancel" :disabled="loading" @click="close">Отмена</button>
          <button class="save" :disabled="loading" @click="save">
            {{ loading ? "Сохранение..." : "Сохранить" }}
          </button>
        </div>
      </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup lang="ts">
import { ref, watch } from "vue";
import { validateFriendNickname } from "@/utils/profileValidation";

const props = defineProps<{
  modelValue: boolean;
  initialLogin?: string;
  loading?: boolean;
}>();

const emit = defineEmits<{
  (e: "update:modelValue", v: boolean): void;
  (e: "save", login: string): void;
}>();

const nickname = ref("");
const error = ref("");

watch(
  () => props.modelValue,
  (open) => {
    if (!open) return;
    nickname.value = props.initialLogin ?? "";
    error.value = "";
  }
);

function close() {
  if (props.loading) return;
  emit("update:modelValue", false);
}

function save() {
  const validationError = validateFriendNickname(nickname.value);
  if (validationError) {
    error.value = validationError;
    return;
  }

  error.value = "";
  emit("save", nickname.value.trim());
}
</script>

<style scoped>
.overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.78);
  backdrop-filter: blur(10px);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 20px;
  z-index: 10000;
}

.modal {
  width: 100%;
  max-width: 420px;
  background: #0f0f0f;
  border: 1px solid #262626;
  border-radius: 20px;
  padding: 20px;
}

.header {
  display: flex;
  justify-content: space-between;
  gap: 12px;
  margin-bottom: 16px;
}

.header h2 {
  margin: 0;
  color: white;
  font-size: 18px;
}

.header p {
  margin: 4px 0 0;
  color: #888;
  font-size: 13px;
}

.close-btn {
  background: transparent;
  border: 1px solid #333;
  color: #aaa;
  border-radius: 10px;
  padding: 6px 10px;
  cursor: pointer;
}

.field label {
  display: block;
  color: #aaa;
  font-size: 12px;
  margin-bottom: 6px;
}

.field input {
  width: 100%;
  box-sizing: border-box;
  background: #141414;
  border: 1px solid #262626;
  border-radius: 12px;
  padding: 10px 12px;
  color: white;
}

.error {
  color: #ff8a8a;
  font-size: 13px;
  margin-top: 8px;
}

.hint {
  color: #666;
  font-size: 12px;
  margin-top: 8px;
}

.actions {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
  margin-top: 18px;
}

.cancel,
.save {
  border-radius: 12px;
  padding: 10px 14px;
  cursor: pointer;
}

.cancel {
  background: transparent;
  border: 1px solid #333;
  color: #aaa;
}

.save {
  background: #4163fc;
  border: none;
  color: white;
}

.modal-enter-active,
.modal-leave-active {
  transition: all 0.2s ease;
}

.modal-enter-from,
.modal-leave-to {
  opacity: 0;
  transform: translateY(12px);
}
</style>
