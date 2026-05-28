<template>
  <Transition name="modal">
    <div v-if="modelValue" class="overlay" @click.self="close">
      <div class="modal">

        <!-- HEADER -->
        <div class="header">
          <div>
            <h2>Редактирование профиля</h2>
            <p>Измени информацию о себе</p>
          </div>

          <button class="close-btn" :disabled="loading" @click="close">
            ✕
          </button>
        </div>

        <!-- FORM -->
        <div class="form">

          <div class="field">
            <label>Имя</label>
            <input v-model="form.name" type="text" maxlength="32" />
          </div>

          <div class="field">
            <label>Тэг</label>
            <div class="tag-input">
              <span>@</span>
              <input v-model="form.tag" type="text" maxlength="32" />
            </div>
          </div>

          <div class="field">
            <label>Описание</label>
            <textarea v-model="form.description" rows="5" maxlength="250" />
          </div>

        </div>

        <!-- ACTIONS -->
        <div class="actions">
          <button class="cancel" :disabled="loading" @click="close">
            Отмена
          </button>

          <button class="save" :disabled="loading || !isValid" @click="save">
            {{ loading ? "Сохранение..." : "Сохранить" }}
          </button>
        </div>

      </div>
    </div>
  </Transition>
</template>

<script setup lang="ts">
import { reactive, computed, watch } from "vue";
import type { ProfileForm } from "@/interface/models/profile/ProfileForm";

const props = defineProps<{
  modelValue: boolean;
  initialData?: ProfileForm;
  loading?: boolean;
}>();

const emit = defineEmits<{
  (e: "update:modelValue", v: boolean): void;
  (e: "save", v: ProfileForm): void;
}>();

const form = reactive<ProfileForm>({
  name: "",
  tag: "",
  description: "",
  avatar: null,
  avatarUrl: null,
});

watch(
  () => props.initialData,
  (v) => {
    if (!v) return;

    form.name = v.name;
    form.tag = v.tag;
    form.description = v.description;

    form.avatar = null;
    form.avatarUrl = null;
  },
  { immediate: true }
);

const isValid = computed(() =>
  form.name.trim().length >= 2 &&
  form.tag.trim().length >= 2
);

function close() {
  if (props.loading) return;
  emit("update:modelValue", false);
}

function save() {
  emit("save", {
    ...form,
    name: form.name.trim(),
    tag: form.tag.trim(),
    description: form.description.trim(),
  });
}
</script>
<style scoped>
.menu-btn {
  position: absolute;
  top: 5px;
  right: 5px;
  background: rgba(0, 0, 0, 0.5);
  border: none;
  color: white;
  border-radius: 8px;
  padding: 4px 8px;
  cursor: pointer;
}

.menu {
  position: absolute;
  top: 35px;
  right: 0;
  background: #141414;
  border: 1px solid #2a2a2a;
  border-radius: 12px;
  padding: 6px;
  display: flex;
  flex-direction: column;
  gap: 6px;
  min-width: 180px;
  z-index: 10;
}

.menu button {
  background: transparent;
  border: none;
  color: white;
  text-align: left;
  padding: 6px 10px;
  cursor: pointer;
  border-radius: 8px;
}

.menu button:hover {
  background: rgba(255, 255, 255, 0.05);
}

.menu .danger {
  color: #ff4d4d;
}

/* OVERLAY */
.overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.6);
  backdrop-filter: blur(6px);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 20px;
  z-index: 50;
}

/* MODAL */
.modal {
  width: 100%;
  max-width: 600px;
  background: #0f0f0f;
  border: 1px solid #262626;
  border-radius: 24px;
  padding: 24px;
  box-shadow: 0 20px 80px rgba(0, 0, 0, 0.6);
}

/* HEADER */
.header {
  display: flex;
  justify-content: space-between;
  align-items: start;
  margin-bottom: 20px;
}

.header h2 {
  color: white;
  font-size: 20px;
  margin: 0;
}

.header p {
  color: #888;
  font-size: 13px;
  margin-top: 4px;
}

.close-btn {
  background: transparent;
  border: 1px solid #333;
  color: #aaa;
  border-radius: 10px;
  padding: 6px 10px;
  cursor: pointer;
}

/* AVATAR */
.avatar-section {
  display: flex;
  flex-direction: column;
  align-items: center;
  margin-bottom: 20px;
}

.avatar-wrapper {
  position: relative;
  width: 110px;
  height: 110px;
}

.avatar-wrapper img {
  width: 100%;
  height: 100%;
  border-radius: 50%;
  object-fit: cover;
  border: 3px solid #2a2a2a;
}

.avatar-edit {
  position: absolute;
  inset: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  background: rgba(0, 0, 0, 0.6);
  color: white;
  font-size: 12px;
  opacity: 0;
  transition: 0.2s;
  border-radius: 50%;
  cursor: pointer;
}

.avatar-wrapper:hover .avatar-edit {
  opacity: 1;
}

.hint {
  font-size: 12px;
  color: #666;
  margin-top: 8px;
}

/* FORM */
.form {
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.field label {
  font-size: 12px;
  color: #aaa;
  margin-bottom: 6px;
  display: block;
}

.field input,
.field textarea {
  width: 100%;
  background: #141414;
  border: 1px solid #262626;
  border-radius: 14px;
  padding: 10px 12px;
  color: white;
  outline: none;
}

.tag-input {
  position: relative;
}

.tag-input span {
  position: absolute;
  left: 10px;
  top: 50%;
  transform: translateY(-50%);
  color: #666;
}

.tag-input input {
  padding-left: 24px;
}

/* ACTIONS */
.actions {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
  margin-top: 20px;
}

.cancel {
  background: transparent;
  border: 1px solid #333;
  color: #aaa;
  padding: 10px 14px;
  border-radius: 12px;
  cursor: pointer;
}

.save {
  background: #4163fc;
  border: none;
  color: white;
  padding: 10px 16px;
  border-radius: 12px;
  cursor: pointer;
}

/* ANIMATION */
.modal-enter-active,
.modal-leave-active {
  transition: all 0.25s ease;
}

.modal-enter-from,
.modal-leave-to {
  opacity: 0;
  transform: translateY(20px) scale(0.96);
}
</style>
