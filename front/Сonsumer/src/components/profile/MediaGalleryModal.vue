<template>
  <div v-if="modelValue" class="overlay" @click.self="close">

    <div class="modal">

      <button class="close" @click="close">×</button>

      <button class="nav left" @click="prev">‹</button>

      <div class="image-box">
        <img v-if="current" :src="current.url" />
      </div>

      <button class="nav right" @click="next">›</button>

      <div class="counter">
        {{ index + 1 }} / {{ media.length }}
      </div>

      <!-- MENU -->
      <div class="menu-wrapper">
        <button class="menu-btn" @click.stop="toggleMenu">
          ⋯
        </button>

        <div v-if="menuOpen" class="menu" @click.stop>

          <button @click="addNew">
            Добавить
          </button>

          <button @click="replaceCurrent" :disabled="!current">
            Заменить текущую
          </button>

          <button class="danger" @click="deleteCurrent" :disabled="!current">
            Удалить текущую
          </button>

          <button class="danger strong" @click="deleteAll">
            Удалить все
          </button>

        </div>
      </div>

      <input ref="fileInput" type="file" hidden accept="image/*" @change="onFileChange" />

    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from "vue";
import type { Media } from "@/interface/models/profile/Media";

const props = defineProps<{
  modelValue: boolean;
  media: Media[];
  startIndex?: number;
}>();

const emit = defineEmits<{
  (e: "update:modelValue", v: boolean): void;
  (e: "delete", id: number): void;
  (e: "delete-all"): void;
  (e: "upload", file: File): void;
  (e: "replace", payload: { id: number; file: File }): void;
}>();

const index = ref(props.startIndex ?? 0);
const menuOpen = ref(false);
const fileInput = ref<HTMLInputElement | null>(null);
const mode = ref<"add" | "replace">("add");

watch(
  () => props.modelValue,
  (v) => {
    if (v) index.value = props.startIndex ?? 0;
  }
);

const current = computed(() => props.media[index.value]);

function close() {
  emit("update:modelValue", false);
}

function next() {
  if (!props.media.length) return;
  index.value = (index.value + 1) % props.media.length;
}

function prev() {
  if (!props.media.length) return;
  index.value = (index.value - 1 + props.media.length) % props.media.length;
}

// MENU
function toggleMenu() {
  menuOpen.value = !menuOpen.value;
}

function addNew() {
  mode.value = "add";
  fileInput.value?.click();
  menuOpen.value = false;
}

function replaceCurrent() {
  if (!current.value) return;

  mode.value = "replace";
  fileInput.value?.click();
  menuOpen.value = false;
}

function deleteCurrent() {
  if (!current.value) return;

  emit("delete", current.value.id);
  menuOpen.value = false;
}

function deleteAll() {
  emit("delete-all");
  menuOpen.value = false;
}

// FILE
function onFileChange(e: Event) {
  const file = (e.target as HTMLInputElement).files?.[0];
  if (!file) return;

  if (mode.value === "add") {
    emit("upload", file);
  }

  if (mode.value === "replace" && current.value) {
    emit("replace", {
      id: current.value.id,
      file,
    });
  }

  if (fileInput.value) fileInput.value.value = "";
  menuOpen.value = false;
}
</script>

<style scoped>
.overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, .85);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 999;
}

.modal {
  position: relative;
  width: min(900px, 95vw);
  height: min(700px, 85vh);
  display: flex;
  align-items: center;
  justify-content: center;
}

/* IMAGE FIXED */
.image-box {
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
}

.strong {
  font-weight: 700;
  border-top: 1px solid rgba(255, 255, 255, 0.1);
}

.image-box img {
  max-width: 100%;
  max-height: 100%;
  object-fit: contain;
  border-radius: 12px;
}

/* NAV */
.nav {
  position: absolute;
  top: 50%;
  transform: translateY(-50%);
  font-size: 40px;
  color: white;
  background: none;
  border: none;
  cursor: pointer;
}

.left {
  left: -60px;
}

.right {
  right: -60px;
}

/* COUNTER */
.counter {
  position: absolute;
  bottom: -30px;
  color: white;
}

/* CLOSE */
.close {
  position: absolute;
  top: -40px;
  right: 0;
  font-size: 30px;
  color: white;
  background: none;
  border: none;
  cursor: pointer;
}

/* MENU */
.menu-wrapper {
  position: absolute;
  top: 10px;
  right: 10px;
}

.menu-btn {
  background: rgba(0, 0, 0, .5);
  border: none;
  color: white;
  font-size: 22px;
  padding: 6px 10px;
  border-radius: 8px;
  cursor: pointer;
}

.menu {
  position: absolute;
  right: 0;
  top: 35px;
  background: #111;
  border: 1px solid #333;
  border-radius: 12px;
  display: flex;
  flex-direction: column;
  min-width: 180px;
  overflow: hidden;
}

.menu button {
  background: transparent;
  border: none;
  color: white;
  padding: 10px;
  text-align: left;
  cursor: pointer;
}

.menu button:hover {
  background: rgba(255, 255, 255, 0.05);
}

.menu .danger {
  color: #ff4d4d;
}
</style>
