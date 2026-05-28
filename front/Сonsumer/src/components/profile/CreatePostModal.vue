<template>
  <div v-if="modelValue" class="overlay" @click.self="close">
    <div class="modal">

      <!-- HEADER -->
      <div class="header">
        <h2>Create Post</h2>
        <button class="close" @click="close">✕</button>
      </div>

      <!-- TEXT AREA -->
      <textarea v-model="description" class="textarea" placeholder="Share something with the world..." />

      <!-- UPLOAD ZONE -->
      <label class="upload">
        <input type="file" multiple hidden @change="handleFiles" />
        <div class="upload-box">
          <span>📎 Add photos / videos</span>
          <small>Click or drop files here</small>
        </div>
      </label>

      <!-- FILE LIST -->
      <div v-if="files.length" class="files">
        <div v-for="(file, i) in files" :key="i" class="file">
          <span>📄 {{ file.name }}</span>
          <button @click="removeFile(i)">✕</button>
        </div>
      </div>

      <!-- ACTIONS -->
      <div class="actions">
        <button class="btn ghost" @click="close">
          Cancel
        </button>

        <button class="btn primary" :disabled="loading" @click="submit">
          {{ loading ? "Posting..." : "Create Post" }}
        </button>
      </div>

    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from "vue";

const props = defineProps<{
  modelValue: boolean;
  loading?: boolean;
}>();

const emit = defineEmits<{
  (e: "update:modelValue", value: boolean): void;
  (e: "submit", payload: { description: string; files: File[] }): void;
}>();

const description = ref("");
const files = ref<File[]>([]);

watch(
  () => props.modelValue,
  (val) => {
    if (val) {
      description.value = "";
      files.value = [];
    }
  }
);

function handleFiles(e: Event) {
  const target = e.target as HTMLInputElement;
  if (!target.files) return;
  files.value = Array.from(target.files);
}

function removeFile(index: number) {
  files.value.splice(index, 1);
}

function close() {
  emit("update:modelValue", false);
}

function submit() {
  emit("submit", {
    description: description.value,
    files: files.value,
  });
}
</script>

<style scoped>
/* OVERLAY */
.overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.65);
  backdrop-filter: blur(8px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

/* MODAL */
.modal {
  width: 480px;
  background: rgba(20, 20, 20, 0.95);
  border: 1px solid rgba(255, 255, 255, 0.08);
  border-radius: 18px;
  padding: 18px;
  color: white;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.6);
}

/* HEADER */
.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 10px;
}

.header h2 {
  font-size: 18px;
  margin: 0;
}

.close {
  background: transparent;
  border: none;
  color: white;
  font-size: 18px;
  cursor: pointer;
  opacity: 0.6;
}

.close:hover {
  opacity: 1;
}

/* TEXTAREA */
.textarea {
  width: 100%;
  min-height: 120px;
  resize: none;

  background: rgba(255, 255, 255, 0.04);
  border: 1px solid rgba(255, 255, 255, 0.08);
  border-radius: 12px;

  padding: 12px;
  color: white;
  outline: none;

  margin-bottom: 12px;
}

.textarea:focus {
  border-color: #4163FC;
}

/* UPLOAD */
.upload {
  display: block;
  cursor: pointer;
  margin-bottom: 12px;
}

.upload-box {
  border: 1px dashed rgba(255, 255, 255, 0.2);
  border-radius: 12px;
  padding: 14px;

  display: flex;
  flex-direction: column;
  gap: 4px;

  text-align: center;
  opacity: 0.8;
  transition: 0.2s;
}

.upload-box:hover {
  border-color: #4163FC;
  opacity: 1;
}

/* FILES */
.files {
  display: flex;
  flex-direction: column;
  gap: 8px;
  margin-bottom: 12px;
}

.file {
  display: flex;
  justify-content: space-between;
  align-items: center;

  padding: 8px 10px;
  background: rgba(255, 255, 255, 0.05);
  border-radius: 10px;
  font-size: 12px;
}

.file button {
  background: transparent;
  border: none;
  color: white;
  cursor: pointer;
  opacity: 0.6;
}

.file button:hover {
  opacity: 1;
}

/* ACTIONS */
.actions {
  display: flex;
  justify-content: space-between;
  gap: 10px;
}

.btn {
  flex: 1;
  padding: 10px;
  border-radius: 10px;
  border: none;
  cursor: pointer;
  font-weight: 500;
}

.ghost {
  background: rgba(255, 255, 255, 0.06);
  color: white;
}

.primary {
  background: #4163FC;
  color: white;
}

.primary:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}
</style>
