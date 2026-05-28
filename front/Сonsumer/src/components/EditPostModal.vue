<template>
  <div v-if="modelValue" class="overlay" @click.self="close">

    <div class="modal">

      <!-- HEADER -->
      <div class="top">
        <h2>Edit post</h2>

        <!-- 3 DOT MENU -->
        <div class="menu-wrapper" v-if="localMedia.length">

          <button class="dots" @click="toggleMenu">
            ⋮
          </button>

          <div v-if="menuOpen" class="dropdown">

            <button @click="removeCurrent">🗑 Delete current</button>

            <button @click="removeAll">🧨 Delete all</button>

            <label class="file">
              ➕ Add media
              <input type="file" multiple hidden @change="addFiles" />
            </label>

          </div>
        </div>
      </div>

      <!-- MEDIA -->
      <div v-if="localMedia.length" class="media">

        <button class="nav left" @click="prev">‹</button>

        <img v-if="current && current.mediaType === 'image'" :src="current.url" />
        <video v-else-if="current" :src="current.url" controls />
        <button class="nav right" @click="next">›</button>

        <div class="counter">
          {{ index + 1 }} / {{ localMedia.length }}
        </div>

      </div>

      <!-- EMPTY STATE -->
      <div v-else class="empty">
        No media yet — add something 👇
      </div>

      <!-- DESCRIPTION -->
      <textarea v-model="localDescription" />

      <!-- ACTIONS -->
      <div class="actions">
        <button class="btn cancel" @click="close">Cancel</button>
        <button class="btn save" @click="save">Save</button>
      </div>

    </div>

  </div>
</template>

<script setup lang="ts">
import { ref, watch, computed } from "vue";
import type { PostFull } from "@/interface/models/post/PostFull";
const props = defineProps<{
  modelValue: boolean;
  post: PostFull | null;
}>();
import { PostService } from "@/service/postService";

const postService = new PostService();

const emit = defineEmits(["update:modelValue", "save"]);

const localDescription = ref("");
const localMedia = ref<import("@/interface/models/post/PostMedia").PostMedia[]>([]);
const index = ref(0);
const menuOpen = ref(false);

watch(
  () => props.post,
  (p) => {
    if (!p) return;

    localDescription.value = p.description;

    localMedia.value = [...p.media];

    index.value = 0;
    menuOpen.value = false;
  },
  { immediate: true }
);

const current = computed(() => {
  return localMedia.value[index.value] ?? null;
});

function close() {
  emit("update:modelValue", false);
}

/* MENU */
function toggleMenu() {
  menuOpen.value = !menuOpen.value;
}

/* NAV */
function next() {
  if (!localMedia.value.length) return;
  index.value = (index.value + 1) % localMedia.value.length;
}

function prev() {
  if (!localMedia.value.length) return;
  index.value =
    index.value <= 0
      ? localMedia.value.length - 1
      : index.value - 1;
}

/* DELETE CURRENT */
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

/* DELETE ALL */
function removeAll() {
  localMedia.value = [];
  index.value = 0;
  menuOpen.value = false;
}

async function addFiles(e: Event) {
  const files = (e.target as HTMLInputElement).files;
  if (!files || !props.post) return;

  menuOpen.value = false;

  for (const file of Array.from(files)) {
    const form = new FormData();

    form.append("file", file);
    form.append(
      "mediaType",
      file.type.startsWith("video") ? "video" : "image"
    );

    const res = await postService.uploadPostMedia(props.post.id, form);

    if (res) {
      localMedia.value.push(res.data);
    }
  }
}

/* SAVE */
function save() {
  if (!props.post) return;

  emit("save", {
    id: props.post.id,
    description: localDescription.value,
    media: localMedia.value.map(m => ({
      id: m.id,
      url: m.url,
      fileKey: m.fileKey,
      bucket: m.bucket,
      mediaType: m.mediaType
    }))
  });

  close();
}
</script>

<style scoped>
.overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, .7);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.modal {
  width: 520px;
  background: #181818;
  border-radius: 18px;
  padding: 16px;
  color: white;
}

/* HEADER */
.top {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.media-actions button,
.add {
  background: #222;
  border: none;
  color: white;
  padding: 6px 10px;
  border-radius: 8px;
  cursor: pointer;
  margin-left: 6px;
}

/* MEDIA */
.media {
  position: relative;
  margin-top: 12px;
}

.media img,
.media video {
  width: 100%;
  height: 260px;
  object-fit: cover;
  border-radius: 12px;
}

.nav {
  position: absolute;
  top: 50%;
  transform: translateY(-50%);
  background: rgba(0, 0, 0, .5);
  border: none;
  color: white;
  width: 34px;
  height: 34px;
  border-radius: 50%;
  cursor: pointer;
}

.nav.left {
  left: 8px;
}

.nav.right {
  right: 8px;
}

.counter {
  position: absolute;
  bottom: 8px;
  right: 10px;
  background: rgba(0, 0, 0, .6);
  padding: 4px 8px;
  border-radius: 8px;
  font-size: 12px;
}

/* TEXTAREA */
textarea {
  width: 100%;
  margin-top: 12px;
  min-height: 100px;
  background: #111;
  border: 1px solid #333;
  color: white;
  padding: 10px;
  border-radius: 10px;
}

/* ACTIONS */
.actions {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
  margin-top: 12px;
}

.btn {
  padding: 8px 12px;
  border-radius: 10px;
  border: none;
  cursor: pointer;
}

.cancel {
  background: #333;
  color: white;
}

.save {
  background: #4163FC;
  color: white;
}

.menu-wrapper {
  position: relative;
}

.dots {
  background: #222;
  border: none;
  color: white;
  width: 34px;
  height: 34px;
  border-radius: 8px;
  cursor: pointer;
}

.dropdown {
  position: absolute;
  right: 0;
  top: 38px;
  background: #1e1e1e;
  border: 1px solid rgba(255, 255, 255, 0.08);
  border-radius: 10px;
  overflow: hidden;
  display: flex;
  flex-direction: column;
  min-width: 180px;
  z-index: 10;
}

.dropdown button,
.file {
  padding: 10px;
  background: transparent;
  border: none;
  color: white;
  text-align: left;
  cursor: pointer;
}

.dropdown button:hover,
.file:hover {
  background: rgba(255, 255, 255, 0.06);
}

.empty {
  margin-top: 12px;
  opacity: 0.6;
  text-align: center;
}
</style>
