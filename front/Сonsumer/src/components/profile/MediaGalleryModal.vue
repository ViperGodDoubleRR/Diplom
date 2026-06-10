<template>
  <Teleport to="body">
    <div
      v-if="modelValue"
      class="overlay"
      :style="{ zIndex: overlayZIndex }"
      @click.self="close"
    >
    <div class="modal">
      <button class="close" @click="close">×</button>

      <button v-if="hasMultiple" class="nav left" type="button" @click.stop="prev">‹</button>

      <div class="media-box">
        <video
          v-if="current && isVideo(current)"
          :key="`video-${current.id}`"
          ref="videoRef"
          :src="current.url"
          class="preview-video"
          autoplay
          loop
          muted
          playsinline
          controls
          @click.stop
        />
        <img
          v-else-if="current?.url"
          :key="`image-${current.id}`"
          :src="current.url"
          :alt="currentLabel"
          class="preview-image"
        />
        <div v-else class="empty">
          <p>Медиа пока нет</p>
          <div v-if="!readonly" class="empty-actions">
            <button type="button" @click="startUpload('avatar')">Добавить фото</button>
            <button type="button" @click="startUpload('video')">Добавить видео</button>
          </div>
        </div>
      </div>

      <button v-if="hasMultiple" class="nav right" type="button" @click.stop="next">›</button>

      <div v-if="hasMultiple" class="thumbnails">
        <button
          v-for="(item, i) in media"
          :key="item.id"
          type="button"
          class="thumb"
          :class="{ active: i === index }"
          @click.stop="goTo(i)"
        >
          <video
            v-if="isVideo(item) && item.url"
            :src="item.url"
            muted
            playsinline
            preload="metadata"
          />
          <img v-else-if="item.url" :src="item.url" :alt="item.mediaType" />
        </button>
      </div>

      <div v-if="hasMultiple" class="counter">
        {{ index + 1 }} / {{ media.length }}
      </div>

      <div v-if="current" class="badge">{{ currentLabel }}</div>

      <div v-if="!readonly" class="menu-wrapper">
        <button class="menu-btn" @click.stop="toggleMenu">⋯</button>

        <div v-if="menuOpen" class="menu" @click.stop>
          <button @click="startUpload('avatar')">Добавить фото</button>
          <button @click="startUpload('video')">Добавить видео</button>
          <button @click="replaceCurrent" :disabled="!current">Заменить текущее</button>
          <button class="danger" @click="deleteCurrent" :disabled="!current">
            Удалить текущее
          </button>
          <button class="danger strong" @click="deleteAll">Удалить все</button>
        </div>
      </div>

      <p v-if="uploadHint" class="hint">{{ uploadHint }}</p>

      <input
        ref="fileInput"
        type="file"
        hidden
        :accept="fileAccept"
        @change="onFileChange"
      />
    </div>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
import { computed, ref, watch, onUnmounted } from "vue";
import type { Media } from "@/interface/models/profile/Media";
import { MediaType } from "@/interface/models/profile/MediaType";
import {
  MAX_PROFILE_VIDEO_SECONDS,
  MAX_PROFILE_VIDEO_MB,
  normalizeMediaType,
  validateProfileImage,
  validateProfileVideo,
} from "@/utils/profileValidation";

const props = withDefaults(
  defineProps<{
    modelValue: boolean;
    media: Media[];
    startIndex?: number;
    readonly?: boolean;
    zIndex?: number;
  }>(),
  {
    zIndex: 10050,
  }
);

const overlayZIndex = computed(() => props.zIndex);

const emit = defineEmits<{
  (e: "update:modelValue", v: boolean): void;
  (e: "delete", id: number): void;
  (e: "delete-all"): void;
  (e: "upload", payload: { file: File; mediaType: MediaType }): void;
  (e: "replace", payload: { id: number; file: File; mediaType: MediaType }): void;
  (e: "error", message: string): void;
}>();

const index = ref(props.startIndex ?? 0);
const menuOpen = ref(false);
const fileInput = ref<HTMLInputElement | null>(null);
const videoRef = ref<HTMLVideoElement | null>(null);
const mode = ref<"add" | "replace">("add");
const pendingType = ref<MediaType>(MediaType.AVATAR);
const uploadHint = ref("");

const fileAccept = computed(() =>
  pendingType.value === MediaType.VIDEO
    ? "video/mp4,video/webm,video/quicktime"
    : "image/jpeg,image/png,image/webp"
);

watch(
  () => index.value,
  () => {
    if (videoRef.value) {
      videoRef.value.pause();
      videoRef.value.currentTime = 0;
    }
  }
);

watch(
  () => props.modelValue,
  (v) => {
    if (v) {
      index.value = props.startIndex ?? 0;
      menuOpen.value = false;
      uploadHint.value = "";
      window.addEventListener("keydown", onKeydown);
    } else {
      window.removeEventListener("keydown", onKeydown);
      if (videoRef.value) {
        videoRef.value.pause();
      }
    }
  }
);

watch(
  () => props.media.length,
  (len) => {
    if (len === 0) {
      index.value = 0;
      return;
    }
    if (index.value >= len) index.value = len - 1;
  }
);

const current = computed(() => props.media[index.value] ?? null);
const hasMultiple = computed(() => props.media.length > 1);

const currentLabel = computed(() => {
  const type = normalizeMediaType(current.value?.mediaType);
  if (type === MediaType.VIDEO) return "Видео-аватар";
  if (type === MediaType.AVATAR) return "Фото-аватар";
  return "Медиа";
});

function isVideo(item: Media): boolean {
  return normalizeMediaType(item.mediaType) === MediaType.VIDEO;
}

function close() {
  emit("update:modelValue", false);
}

function goTo(i: number) {
  if (i < 0 || i >= props.media.length) return;
  index.value = i;
}

function onKeydown(e: KeyboardEvent) {
  if (!props.modelValue || !hasMultiple.value) return;

  if (e.key === "ArrowRight") {
    e.preventDefault();
    next();
  } else if (e.key === "ArrowLeft") {
    e.preventDefault();
    prev();
  } else if (e.key === "Escape") {
    close();
  }
}

onUnmounted(() => {
  window.removeEventListener("keydown", onKeydown);
});

function next() {
  if (!props.media.length) return;
  index.value = (index.value + 1) % props.media.length;
}

function prev() {
  if (!props.media.length) return;
  index.value = (index.value - 1 + props.media.length) % props.media.length;
}

function toggleMenu() {
  menuOpen.value = !menuOpen.value;
}

function startUpload(type: "avatar" | "video") {
  mode.value = "add";
  pendingType.value = type === "avatar" ? MediaType.AVATAR : MediaType.VIDEO;
  uploadHint.value =
    type === "video"
      ? `Видео до ${MAX_PROFILE_VIDEO_SECONDS} сек и ${MAX_PROFILE_VIDEO_MB} МБ`
      : "Фото до 5 МБ";
  fileInput.value?.click();
  menuOpen.value = false;
}

function replaceCurrent() {
  if (!current.value) return;

  mode.value = "replace";
  pendingType.value = isVideo(current.value) ? MediaType.VIDEO : MediaType.AVATAR;
  uploadHint.value = isVideo(current.value)
    ? `Видео до ${MAX_PROFILE_VIDEO_SECONDS} сек и ${MAX_PROFILE_VIDEO_MB} МБ`
    : "Фото до 5 МБ";
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

async function onFileChange(e: Event) {
  const file = (e.target as HTMLInputElement).files?.[0];
  if (!file) return;

  const validationError =
    pendingType.value === MediaType.VIDEO
      ? await validateProfileVideo(file)
      : validateProfileImage(file);

  if (validationError) {
    emit("error", validationError);
    if (fileInput.value) fileInput.value.value = "";
    return;
  }

  if (mode.value === "add") {
    emit("upload", { file, mediaType: pendingType.value });
  } else if (current.value) {
    emit("replace", {
      id: current.value.id,
      file,
      mediaType: pendingType.value,
    });
  }

  uploadHint.value = "";
  if (fileInput.value) fileInput.value.value = "";
  menuOpen.value = false;
}
</script>

<style scoped>
.overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.88);
  backdrop-filter: blur(12px);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 16px;
}

.modal {
  position: relative;
  width: min(900px, 95vw);
  height: min(700px, 85vh);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 0 56px 72px;
  box-sizing: border-box;
}

.media-box {
  position: relative;
  z-index: 1;
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
  pointer-events: none;
}

.preview-image,
.preview-video {
  max-width: 100%;
  max-height: 100%;
  border-radius: 12px;
  object-fit: contain;
  pointer-events: auto;
}

.preview-video {
  background: #000;
}

.empty {
  color: #888;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 14px;
  pointer-events: auto;
}

.empty-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
  justify-content: center;
}

.empty-actions button {
  border: none;
  border-radius: 10px;
  padding: 10px 14px;
  cursor: pointer;
  background: linear-gradient(135deg, #4163fc, #5b7cff);
  color: white;
  font-weight: 600;
  font-size: 13px;
}

.nav {
  position: absolute;
  top: 50%;
  transform: translateY(-50%);
  z-index: 20;
  font-size: 40px;
  color: white;
  background: rgba(0, 0, 0, 0.55);
  border: none;
  border-radius: 12px;
  cursor: pointer;
  padding: 4px 14px;
  line-height: 1;
}

.left {
  left: 8px;
}

.right {
  right: 8px;
}

.thumbnails {
  position: absolute;
  bottom: 8px;
  left: 50%;
  transform: translateX(-50%);
  z-index: 20;
  display: flex;
  gap: 8px;
  padding: 8px 12px;
  background: rgba(0, 0, 0, 0.55);
  border-radius: 14px;
  max-width: calc(100% - 32px);
  overflow-x: auto;
}

.thumb {
  flex: 0 0 auto;
  width: 52px;
  height: 52px;
  padding: 0;
  border: 2px solid transparent;
  border-radius: 10px;
  overflow: hidden;
  cursor: pointer;
  background: #222;
}

.thumb.active {
  border-color: #4163fc;
}

.thumb img,
.thumb video {
  width: 100%;
  height: 100%;
  object-fit: cover;
  display: block;
  pointer-events: none;
}

.counter {
  position: absolute;
  bottom: 72px;
  left: 50%;
  transform: translateX(-50%);
  z-index: 20;
  color: white;
}

.badge {
  position: absolute;
  top: 12px;
  left: 12px;
  z-index: 20;
  background: rgba(0, 0, 0, 0.55);
  color: white;
  padding: 6px 10px;
  border-radius: 999px;
  font-size: 12px;
}

.hint {
  position: absolute;
  bottom: -52px;
  left: 0;
  right: 0;
  text-align: center;
  color: #aaa;
  font-size: 12px;
}

.close {
  position: absolute;
  top: 0;
  right: 0;
  z-index: 20;
  font-size: 30px;
  color: white;
  background: none;
  border: none;
  cursor: pointer;
}

.menu-wrapper {
  position: absolute;
  top: 10px;
  right: 10px;
  z-index: 20;
}

.menu-btn {
  background: rgba(0, 0, 0, 0.5);
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
  min-width: 210px;
  overflow: hidden;
  z-index: 2;
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

.strong {
  font-weight: 700;
  border-top: 1px solid rgba(255, 255, 255, 0.1);
}

@media (max-width: 640px) {
  .nav {
    font-size: 28px;
  }
}
</style>
