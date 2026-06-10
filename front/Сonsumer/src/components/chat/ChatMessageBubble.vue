<template>
  <div class="bubble-row" :class="{ own: isOwn }">
    <UserAvatar
      v-if="!isOwn"
      avatar-class="msg-avatar"
      :name="authorLogin"
      :src="authorAvatar"
      :is-video="authorAvatarIsVideo"
    />

    <div
      class="bubble"
      :class="{ own: isOwn, deleted: message.isDeleted }"
      @contextmenu.prevent="onContextMenu"
    >
      <div v-if="!isOwn" class="author">{{ authorLogin }}</div>

      <p v-if="message.isDeleted" class="text deleted-text">Сообщение удалено</p>
      <p v-else-if="message.text?.trim()" class="text">{{ message.text }}</p>

      <div v-if="!message.isDeleted && message.media.length" class="media-list">
        <img
          v-for="(item, index) in imageMedia"
          :key="`img-${item.id || index}-${item.url}`"
          :src="item.url"
          class="media"
          alt=""
        />
        <video
          v-for="(item, index) in videoMedia"
          :key="`video-${item.id || index}-${item.url}`"
          :src="item.url"
          class="media"
          controls
          playsinline
          preload="metadata"
        />
      </div>

      <div class="meta">
        <span class="time">{{ formatTime(message.createdAt) }}</span>
        <span v-if="message.isEdited && !message.isDeleted" class="edited">изменено</span>
      </div>
    </div>

    <Teleport to="body">
      <div
        v-if="menuOpen"
        class="ctx-backdrop"
        @click="closeMenu"
        @contextmenu.prevent="closeMenu"
      >
        <div class="ctx-menu" :style="menuStyle" @click.stop>
          <button v-if="canEdit" type="button" class="ctx-item" @click="handleEdit">
            Изменить
          </button>
          <button v-if="canDelete" type="button" class="ctx-item danger" @click="handleDelete">
            Удалить
          </button>
        </div>
      </div>
    </Teleport>
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from "vue";

import UserAvatar from "@/components/ui/UserAvatar.vue";
import type { MessageDto } from "@/interface/DTO/chat/MessageDto";
import { useChatStore } from "@/store/chatStore";
import { isVideoMediaType } from "@/utils/postNormalize";

const props = defineProps<{
  message: MessageDto;
  isOwn: boolean;
  canEdit?: boolean;
  canDelete?: boolean;
}>();

const emit = defineEmits<{
  edit: [message: MessageDto];
  delete: [messageId: number];
}>();

const chatStore = useChatStore();

const menuOpen = ref(false);
const menuX = ref(0);
const menuY = ref(0);

const authorLogin = computed(() =>
  chatStore.getUserDisplayLogin(props.message.user.id, props.message.user.login)
);

const authorAvatar = computed(() =>
  chatStore.getUserDisplayAvatar(props.message.user.id, props.message.user.avatar)
);

const authorAvatarIsVideo = computed(() =>
  chatStore.getUserDisplayAvatarIsVideo(
    props.message.user.id,
    props.message.user.avatarIsVideo ?? false
  )
);

const menuStyle = computed(() => ({
  left: `${Math.min(menuX.value, window.innerWidth - 180)}px`,
  top: `${Math.min(menuY.value, window.innerHeight - 100)}px`,
}));

const imageMedia = computed(() =>
  props.message.media.filter((m) => !isVideoMediaType(m.mediaType))
);

const videoMedia = computed(() =>
  props.message.media.filter((m) => isVideoMediaType(m.mediaType))
);

const canEdit = computed(() => props.canEdit ?? props.isOwn);
const canDelete = computed(() => props.canDelete ?? props.isOwn);

function onContextMenu(event: MouseEvent) {
  if (props.message.isDeleted) return;
  if (!canEdit.value && !canDelete.value) return;

  menuX.value = event.clientX;
  menuY.value = event.clientY;
  menuOpen.value = true;
}

function closeMenu() {
  menuOpen.value = false;
}

function handleEdit() {
  emit("edit", props.message);
  closeMenu();
}

function handleDelete() {
  emit("delete", props.message.id);
  closeMenu();
}

function formatTime(date: string) {
  return new Date(date).toLocaleTimeString(undefined, {
    hour: "2-digit",
    minute: "2-digit",
  });
}
</script>

<style scoped>
.bubble-row {
  display: flex;
  gap: 10px;
  align-items: flex-end;
  max-width: 78%;
}

.bubble-row.own {
  margin-left: auto;
  flex-direction: row-reverse;
}

.bubble-row :deep(.msg-avatar),
.bubble-row :deep(.user-avatar),
.bubble-row :deep(.initials) {
  width: 32px;
  height: 32px;
}

.bubble {
  position: relative;
  padding: 10px 12px 8px;
  border-radius: 16px;
  background: rgba(255, 255, 255, 0.06);
  border: 1px solid rgba(255, 255, 255, 0.05);
  min-width: 80px;
}

.bubble.own {
  background: rgba(65, 99, 252, 0.22);
  border-color: rgba(65, 99, 252, 0.35);
}

.bubble:not(.deleted) {
  cursor: context-menu;
}

.bubble.deleted {
  opacity: 0.7;
  font-style: italic;
  cursor: default;
}

.author {
  font-size: 12px;
  font-weight: 700;
  color: #9eb0ff;
  margin-bottom: 4px;
}

.text {
  margin: 0;
  color: rgba(255, 255, 255, 0.95);
  font-size: 14px;
  white-space: pre-wrap;
  word-break: break-word;
}

.deleted-text {
  color: rgba(255, 255, 255, 0.45);
}

.media-list {
  margin-top: 8px;
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.media {
  max-width: 280px;
  max-height: 260px;
  border-radius: 12px;
  display: block;
}

.meta {
  display: flex;
  gap: 8px;
  justify-content: flex-end;
  margin-top: 6px;
}

.time {
  font-size: 11px;
  color: rgba(255, 255, 255, 0.4);
}

.edited {
  font-size: 11px;
  color: rgba(255, 255, 255, 0.45);
}

.ctx-backdrop {
  position: fixed;
  inset: 0;
  z-index: 10003;
}

.ctx-menu {
  position: fixed;
  min-width: 160px;
  background: #171717;
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 12px;
  padding: 6px;
  box-shadow: 0 12px 32px rgba(0, 0, 0, 0.45);
}

.ctx-item {
  display: block;
  width: 100%;
  border: none;
  background: transparent;
  color: white;
  text-align: left;
  padding: 10px 12px;
  border-radius: 8px;
  cursor: pointer;
  font-size: 14px;
}

.ctx-item:hover {
  background: rgba(255, 255, 255, 0.06);
}

.ctx-item.danger {
  color: #ff8da8;
}
</style>
