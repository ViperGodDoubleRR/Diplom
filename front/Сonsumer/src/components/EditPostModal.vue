<template>
  <Teleport to="body">
    <div v-if="modelValue" class="overlay" @click.self="close">
      <div class="modal">
        <button class="close-btn" type="button" @click="close">×</button>

        <div class="top">
          <h2>Редактировать пост</h2>
        </div>

        <textarea
          v-model="localDescription"
          class="description"
          placeholder="Описание поста..."
        />

        <div class="gallery">
          <button
            v-if="hasMultiple"
            class="nav left"
            type="button"
            @click.stop="prev"
          >
            ‹
          </button>

          <div class="media-box">
            <video
              v-if="current && isVideo(current)"
              :key="`video-${current.id}`"
              :src="current.url"
              class="preview"
              controls
              playsinline
            />
            <img
              v-else-if="current?.url"
              :key="`image-${current.id}`"
              :src="current.url"
              class="preview"
              alt=""
            />
            <div v-else class="empty">
              <p>Медиа пока нет</p>
              <div class="empty-actions">
                <button type="button" @click="startUpload('image')">Добавить фото</button>
                <button type="button" @click="startUpload('video')">Добавить видео</button>
              </div>
            </div>
          </div>

          <button
            v-if="hasMultiple"
            class="nav right"
            type="button"
            @click.stop="next"
          >
            ›
          </button>

          <div v-if="hasMultiple" class="thumbnails">
            <button
              v-for="(item, i) in localMedia"
              :key="item.id"
              type="button"
              class="thumb"
              :class="{ active: i === index }"
              @click.stop="goTo(i)"
            >
              <video
                v-if="isVideo(item)"
                :src="item.url"
                muted
                playsinline
                preload="metadata"
              />
              <img v-else :src="item.url" alt="" />
            </button>
          </div>

          <div v-if="hasMultiple" class="counter">
            {{ index + 1 }} / {{ localMedia.length }}
          </div>

          <div v-if="current" class="badge">
            {{ isVideo(current) ? "Видео" : "Фото" }}
          </div>

          <div class="menu-wrapper">
            <button class="menu-btn" type="button" @click.stop="toggleMenu">⋯</button>

            <div v-if="menuOpen" class="menu" @click.stop>
              <button type="button" @click="startUpload('image')">Добавить фото</button>
              <button type="button" @click="startUpload('video')">Добавить видео</button>
              <button type="button" :disabled="!current || uploading" @click="replaceCurrent">
                Заменить текущее
              </button>
              <button type="button" class="danger" :disabled="!current" @click="removeCurrent">
                Удалить текущее
              </button>
              <button type="button" class="danger strong" :disabled="!localMedia.length" @click="removeAll">
                Удалить все
              </button>
            </div>
          </div>
        </div>

        <p v-if="uploadHint" class="hint">{{ uploadHint }}</p>
        <p v-if="errorMessage" class="error">{{ errorMessage }}</p>
        <p v-if="uploading" class="uploading">Загрузка медиа...</p>

        <div class="actions">
          <button class="btn cancel" type="button" :disabled="uploading" @click="close">
            Отмена
          </button>
          <button class="btn save" type="button" :disabled="uploading" @click="save">
            Сохранить
          </button>
        </div>

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
import { computed, ref, watch } from "vue";

import type { PostFull } from "@/interface/models/post/PostFull";
import type { PostMedia } from "@/interface/models/post/PostMedia";
import { PostService } from "@/service/postService";
import { normalizePostMedia } from "@/utils/postNormalize";
import {
  MAX_POST_IMAGE_MB,
  MAX_POST_VIDEO_MB,
  POST_IMAGE_ACCEPT,
  POST_VIDEO_ACCEPT,
  resolvePostMediaType,
  validatePostMediaFile,
} from "@/utils/postMediaValidation";

const props = defineProps<{
  modelValue: boolean;
  post: PostFull | null;
}>();

const emit = defineEmits<{
  "update:modelValue": [value: boolean];
  save: [payload: { id: string; description: string; media: PostMedia[] }];
}>();

const postService = new PostService();

const localDescription = ref("");
const localMedia = ref<PostMedia[]>([]);
const index = ref(0);
const menuOpen = ref(false);
const fileInput = ref<HTMLInputElement | null>(null);
const mode = ref<"add" | "replace">("add");
const pendingType = ref<"image" | "video">("image");
const uploadHint = ref("");
const errorMessage = ref("");
const uploading = ref(false);

const fileAccept = computed(() =>
  pendingType.value === "video" ? POST_VIDEO_ACCEPT : POST_IMAGE_ACCEPT
);

const current = computed(() => localMedia.value[index.value] ?? null);
const hasMultiple = computed(() => localMedia.value.length > 1);

watch(
  () => props.post,
  (post) => {
    if (!post) return;

    localDescription.value = post.description;
    localMedia.value = [...post.media];
    index.value = 0;
    menuOpen.value = false;
    errorMessage.value = "";
    uploadHint.value = "";
  },
  { immediate: true }
);

watch(
  () => props.modelValue,
  (open) => {
    if (!open) {
      menuOpen.value = false;
      errorMessage.value = "";
      uploadHint.value = "";
    }
  }
);

function isVideo(item: PostMedia) {
  return item.mediaType?.toLowerCase() === "video";
}

function close() {
  emit("update:modelValue", false);
}

function toggleMenu() {
  menuOpen.value = !menuOpen.value;
}

function goTo(i: number) {
  if (i < 0 || i >= localMedia.value.length) return;
  index.value = i;
}

function next() {
  if (!localMedia.value.length) return;
  index.value = (index.value + 1) % localMedia.value.length;
}

function prev() {
  if (!localMedia.value.length) return;
  index.value =
    index.value <= 0 ? localMedia.value.length - 1 : index.value - 1;
}

function startUpload(type: "image" | "video") {
  mode.value = "add";
  pendingType.value = type;
  uploadHint.value =
    type === "video"
      ? `Видео до ${MAX_POST_VIDEO_MB} МБ`
      : `Фото до ${MAX_POST_IMAGE_MB} МБ`;
  fileInput.value?.click();
  menuOpen.value = false;
}

function replaceCurrent() {
  if (!current.value) return;

  mode.value = "replace";
  pendingType.value = isVideo(current.value) ? "video" : "image";
  uploadHint.value = isVideo(current.value)
    ? `Видео до ${MAX_POST_VIDEO_MB} МБ`
    : `Фото до ${MAX_POST_IMAGE_MB} МБ`;
  fileInput.value?.click();
  menuOpen.value = false;
}

function removeCurrent() {
  localMedia.value.splice(index.value, 1);

  if (!localMedia.value.length) {
    index.value = 0;
    menuOpen.value = false;
    return;
  }

  if (index.value >= localMedia.value.length) {
    index.value = localMedia.value.length - 1;
  }

  menuOpen.value = false;
}

function removeAll() {
  localMedia.value = [];
  index.value = 0;
  menuOpen.value = false;
}

async function uploadFile(file: File, replaceIndex?: number) {
  if (!props.post) return;

  const validationError = validatePostMediaFile(file);
  if (validationError) {
    errorMessage.value = validationError;
    return;
  }

  errorMessage.value = "";
  uploading.value = true;

  try {
    const form = new FormData();
    form.append("file", file);
    form.append("mediaType", resolvePostMediaType(file));

    const res = await postService.uploadPostMedia(props.post.id, form);

    if (!res.success || !res.data) {
      errorMessage.value = res.error?.message ?? "Не удалось загрузить медиа";
      return;
    }

    const uploaded = normalizePostMedia(res.data);

    if (mode.value === "replace" && replaceIndex !== undefined) {
      localMedia.value.splice(replaceIndex, 1, uploaded);
      index.value = Math.min(replaceIndex, localMedia.value.length - 1);
    } else {
      localMedia.value.push(uploaded);
      index.value = localMedia.value.length - 1;
    }

    uploadHint.value = "";
  } catch {
    errorMessage.value = "Ошибка загрузки. Проверьте размер файла и соединение.";
  } finally {
    uploading.value = false;
  }
}

async function onFileChange(e: Event) {
  const file = (e.target as HTMLInputElement).files?.[0];
  if (!file) return;

  const replaceIndex =
    mode.value === "replace" ? index.value : undefined;

  await uploadFile(file, replaceIndex);

  if (fileInput.value) {
    fileInput.value.value = "";
  }
}

function save() {
  if (!props.post || uploading.value) return;

  emit("save", {
    id: props.post.id,
    description: localDescription.value,
    media: localMedia.value.map((m) => ({
      id: m.id,
      url: m.url,
      fileKey: m.fileKey,
      bucket: m.bucket,
      mediaType: m.mediaType,
    })),
  });

  close();
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
  z-index: 10000;
  padding: 16px;
}

.modal {
  position: relative;
  width: min(720px, 96vw);
  max-height: 92vh;
  overflow-y: auto;
  background: #141414;
  border: 1px solid rgba(255, 255, 255, 0.08);
  border-radius: 18px;
  padding: 18px;
  color: white;
}

.close-btn {
  position: absolute;
  top: 12px;
  right: 12px;
  border: none;
  background: transparent;
  color: white;
  font-size: 28px;
  cursor: pointer;
  opacity: 0.7;
  z-index: 2;
}

.top h2 {
  margin: 0 0 12px;
  font-size: 18px;
}

.description {
  width: 100%;
  box-sizing: border-box;
  min-height: 96px;
  resize: vertical;
  background: rgba(255, 255, 255, 0.04);
  border: 1px solid rgba(255, 255, 255, 0.08);
  border-radius: 12px;
  padding: 12px;
  color: white;
  margin-bottom: 14px;
}

.gallery {
  position: relative;
  min-height: 280px;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-bottom: 12px;
}

.media-box {
  width: 100%;
  min-height: 260px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.preview {
  width: 100%;
  max-height: 360px;
  object-fit: contain;
  border-radius: 12px;
  background: #000;
}

.empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 14px;
  opacity: 0.85;
  padding: 24px;
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
  z-index: 2;
  width: 38px;
  height: 38px;
  border: none;
  border-radius: 50%;
  background: rgba(0, 0, 0, 0.55);
  color: white;
  cursor: pointer;
  font-size: 22px;
}

.nav.left {
  left: 8px;
}

.nav.right {
  right: 8px;
}

.thumbnails {
  position: absolute;
  bottom: 8px;
  left: 50%;
  transform: translateX(-50%);
  display: flex;
  gap: 8px;
  padding: 8px 12px;
  background: rgba(0, 0, 0, 0.55);
  border-radius: 14px;
  max-width: calc(100% - 24px);
  overflow-x: auto;
  z-index: 2;
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
}

.counter {
  position: absolute;
  top: 8px;
  left: 8px;
  z-index: 2;
  background: rgba(0, 0, 0, 0.55);
  padding: 4px 8px;
  border-radius: 8px;
  font-size: 12px;
}

.badge {
  position: absolute;
  top: 8px;
  right: 48px;
  z-index: 2;
  background: rgba(0, 0, 0, 0.55);
  padding: 6px 10px;
  border-radius: 999px;
  font-size: 12px;
}

.menu-wrapper {
  position: absolute;
  top: 8px;
  right: 8px;
  z-index: 3;
}

.menu-btn {
  width: 36px;
  height: 36px;
  border: none;
  border-radius: 10px;
  background: rgba(0, 0, 0, 0.55);
  color: white;
  cursor: pointer;
  font-size: 20px;
}

.menu {
  position: absolute;
  right: 0;
  top: 42px;
  min-width: 210px;
  background: #111;
  border: 1px solid #333;
  border-radius: 12px;
  overflow: hidden;
  display: flex;
  flex-direction: column;
}

.menu button {
  border: none;
  background: transparent;
  color: white;
  padding: 10px 12px;
  text-align: left;
  cursor: pointer;
}

.menu button:hover:not(:disabled) {
  background: rgba(255, 255, 255, 0.06);
}

.menu button:disabled {
  opacity: 0.45;
  cursor: not-allowed;
}

.menu .danger {
  color: #ff4d4d;
}

.menu .strong {
  font-weight: 700;
  border-top: 1px solid rgba(255, 255, 255, 0.1);
}

.hint,
.error,
.uploading {
  font-size: 12px;
  margin: 0 0 8px;
}

.hint {
  color: #aaa;
}

.error {
  color: #ff8a8a;
}

.uploading {
  color: #9eb0ff;
}

.actions {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
  margin-top: 8px;
}

.btn {
  padding: 10px 16px;
  border-radius: 10px;
  border: none;
  cursor: pointer;
  font-weight: 600;
}

.btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.cancel {
  background: #333;
  color: white;
}

.save {
  background: #4163fc;
  color: white;
}

@media (max-width: 640px) {
  .modal {
    padding: 14px;
  }

  .preview {
    max-height: 240px;
  }
}
</style>
