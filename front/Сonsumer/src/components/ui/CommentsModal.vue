<template>
  <Teleport to="body">
    <Transition name="fade">
      <div v-if="open" class="overlay" @click.self="close">
        <div class="modal" @click.stop>
          <div class="header">
            <div>
              <h3>Комментарии</h3>
              <p>{{ totalLabel }}</p>
            </div>

            <button class="close-btn" type="button" @click="close">✕</button>
          </div>

          <div ref="listRef" class="comments" @scroll.passive="onCommentsScroll">
            <p v-if="commentStore.rootLoading && !commentStore.rootComments.length" class="state">
              Загрузка комментариев...
            </p>

            <p v-else-if="loadError" class="state error">{{ loadError }}</p>

            <template v-else-if="commentStore.rootComments.length">
              <CommentItem
                v-for="comment in commentStore.rootComments"
                :key="comment.id"
                :comment="comment"
                :is-own="isOwnComment(comment)"
                :replies="commentStore.getReplies(comment.id)"
                :replies-opened="commentStore.repliesOpened(comment.id)"
                :replies-loading="commentStore.repliesLoading(comment.id)"
                :replies-has-more="commentStore.repliesHasMore(comment.id)"
                :replies-loading-more="commentStore.repliesLoadingMore(comment.id)"
                @delete="onDeleteComment"
                @react="(id, type) => commentStore.toggleReaction(id, type)"
                @reply="onReply"
                @open-replies="(id) => commentStore.openReplies(id)"
                @close-replies="(id) => commentStore.closeReplies(id)"
                @load-more-replies="(id) => commentStore.loadMoreReplies(id)"
                @delete-reply="onDeleteReply"
                @react-reply="(parentId, id, type) => commentStore.toggleReaction(id, type, parentId)"
              />

              <p v-if="commentStore.rootLoadingMore" class="load-hint">Загрузка...</p>
              <p v-else-if="!commentStore.rootHasMore" class="load-hint muted">
                Больше комментариев нет
              </p>
            </template>

            <div v-else class="empty">
              <div class="empty-icon">💬</div>
              <h4>Пока нет комментариев</h4>
              <p>Будьте первым — напишите что-нибудь</p>
            </div>
          </div>

          <div class="input-area">
            <p v-if="replyHint" class="reply-hint">
              Ответ на комментарий
              <button type="button" class="cancel-reply" @click="commentStore.setReplyTarget(null)">
                отмена
              </button>
            </p>

            <div v-if="attachmentPreview" class="attachment-preview">
              <img
                v-if="attachmentPreview.kind === 'image'"
                :src="attachmentPreview.url"
                alt=""
              />
              <video v-else :src="attachmentPreview.url" muted playsinline />
              <button type="button" class="remove-attachment" @click="clearAttachment">✕</button>
            </div>

            <div class="input-row">
              <input
                ref="fileInputRef"
                type="file"
                accept="image/jpeg,image/png,image/webp,image/gif,video/mp4,video/webm,video/quicktime"
                class="hidden-file"
                @change="onFileSelected"
              />

              <button
                class="attach-btn"
                type="button"
                title="Фото, GIF или видео до 30 МБ"
                :disabled="commentStore.sending"
                @click="openFilePicker"
              >
                📎
              </button>

              <input
                v-model="newComment"
                :placeholder="replyHint ? 'Написать ответ...' : 'Написать комментарий...'"
                :disabled="commentStore.sending"
                @keyup.enter="sendComment"
              />

              <button
                type="button"
                :disabled="commentStore.sending || !canSend"
                @click="sendComment"
              >
                {{ commentStore.sending ? "..." : "Отправить" }}
              </button>
            </div>

            <p v-if="errorMessage" class="error">{{ errorMessage }}</p>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup lang="ts">
import { computed, ref, watch } from "vue";

import CommentItem from "@/components/comment/CommentItem.vue";
import { COMMENT_MEDIA_MAX_BYTES } from "@/constants/commentConstants";
import type { CommentDto } from "@/interface/DTO/comments/CommentDto";
import { useCommentStore } from "@/store/commentStore";
import { useUserStore } from "@/store/userStore";
import { isApiSuccess } from "@/utils/apiHelpers";

const open = defineModel<boolean>({ required: true });

const props = defineProps<{
  postId: string;
}>();

const commentStore = useCommentStore();
const userStore = useUserStore();
const listRef = ref<HTMLElement | null>(null);
const fileInputRef = ref<HTMLInputElement | null>(null);
const newComment = ref("");
const errorMessage = ref("");
const loadError = ref("");
const selectedFile = ref<File | null>(null);
const attachmentPreview = ref<{ url: string; kind: "image" | "video" } | null>(null);

const totalLabel = computed(() => {
  const count = commentStore.allCommentsCount;
  return `${count} ${pluralComments(count)}`;
});

const replyHint = computed(() => !!commentStore.replyTargetId);

const canSend = computed(() => !!newComment.value.trim() || !!selectedFile.value);

const scrollDisabled = computed(
  () =>
    !commentStore.rootHasMore ||
    commentStore.rootLoading ||
    commentStore.rootLoadingMore
);

function isOwnComment(comment: CommentDto) {
  const myId = userStore.user?.id;
  const authorId = comment.user?.id;
  if (!myId || !authorId) return false;
  return authorId.toLowerCase() === myId.toLowerCase();
}

function onCommentsScroll() {
  const el = listRef.value;
  if (!el || scrollDisabled.value) return;

  if (el.scrollTop + el.clientHeight >= el.scrollHeight - 140) {
    void commentStore.loadMoreRootComments();
  }
}

function pluralComments(count: number) {
  const mod10 = count % 10;
  const mod100 = count % 100;
  if (mod10 === 1 && mod100 !== 11) return "комментарий";
  if (mod10 >= 2 && mod10 <= 4 && (mod100 < 10 || mod100 >= 20)) return "комментария";
  return "комментариев";
}

function close() {
  open.value = false;
}

function openFilePicker() {
  fileInputRef.value?.click();
}

function clearAttachment() {
  if (attachmentPreview.value?.url.startsWith("blob:")) {
    URL.revokeObjectURL(attachmentPreview.value.url);
  }
  attachmentPreview.value = null;
  selectedFile.value = null;
  if (fileInputRef.value) fileInputRef.value.value = "";
}

function onFileSelected(event: Event) {
  errorMessage.value = "";
  const input = event.target as HTMLInputElement;
  const file = input.files?.[0];
  if (!file) return;

  if (file.size > COMMENT_MEDIA_MAX_BYTES) {
    errorMessage.value = "Файл не больше 30 МБ";
    input.value = "";
    return;
  }

  clearAttachment();
  selectedFile.value = file;
  attachmentPreview.value = {
    url: URL.createObjectURL(file),
    kind: file.type.startsWith("video") ? "video" : "image",
  };
}

async function loadComments() {
  loadError.value = "";
  errorMessage.value = "";

  const ok = await commentStore.loadPostComments(props.postId, true);
  if (!ok) {
    loadError.value = "Не удалось загрузить комментарии";
  }
}

function onReply(commentId: string) {
  commentStore.setReplyTarget(commentId);
}

async function onDeleteComment(commentId: string) {
  if (!confirm("Удалить комментарий и медиа?")) return;

  errorMessage.value = "";
  const res = await commentStore.deleteComment(commentId);

  if (!isApiSuccess(res)) {
    errorMessage.value = res?.error?.message ?? "Не удалось удалить";
  }
}

async function onDeleteReply(parentId: string, replyId: string) {
  if (!confirm("Удалить ответ?")) return;

  errorMessage.value = "";
  const res = await commentStore.deleteComment(replyId, parentId);

  if (!isApiSuccess(res)) {
    errorMessage.value = res?.error?.message ?? "Не удалось удалить";
  }
}

async function sendComment() {
  if (!canSend.value || commentStore.sending) return;

  errorMessage.value = "";

  try {
    const text = newComment.value.trim();
    const res = await commentStore.createComment(
      props.postId,
      text,
      selectedFile.value,
      commentStore.replyTargetId
    );

    if (!isApiSuccess(res)) {
      errorMessage.value = res?.error?.message ?? "Не удалось отправить";
      return;
    }

    newComment.value = "";
    clearAttachment();
  } catch {
    errorMessage.value = "Не удалось отправить комментарий";
  }
}

watch(
  () => [open.value, props.postId] as const,
  ([isOpen, postId]) => {
    if (isOpen && postId) {
      void loadComments();
    }
  },
  { immediate: true }
);

watch(open, (isOpen) => {
  if (!isOpen) {
    commentStore.resetActivePost();
    newComment.value = "";
    clearAttachment();
    errorMessage.value = "";
    loadError.value = "";
  }
});
</script>

<style scoped>
.overlay {
  position: fixed;
  inset: 0;
  z-index: 10000;
  display: flex;
  justify-content: center;
  align-items: center;
  background: rgba(0, 0, 0, 0.75);
  backdrop-filter: blur(12px);
}

.modal {
  width: 820px;
  max-width: 95vw;
  height: 800px;
  max-height: 92vh;
  display: flex;
  flex-direction: column;
  border-radius: 24px;
  background: #111;
  border: 1px solid rgba(255, 255, 255, 0.08);
  overflow: hidden;
}

.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 24px;
  border-bottom: 1px solid rgba(255, 255, 255, 0.06);
}

.header h3 {
  margin: 0;
  font-size: 24px;
  color: #fff;
}

.header p {
  margin-top: 4px;
  font-size: 13px;
  color: rgba(255, 255, 255, 0.5);
}

.close-btn {
  width: 42px;
  height: 42px;
  border: none;
  border-radius: 12px;
  background: rgba(255, 255, 255, 0.05);
  color: white;
  cursor: pointer;
}

.comments {
  flex: 1;
  overflow-y: auto;
  padding: 24px;
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.state {
  text-align: center;
  font-size: 13px;
  color: rgba(255, 255, 255, 0.55);
  padding: 8px 0;
}

.state.error {
  color: #ff8da8;
}

.load-hint {
  text-align: center;
  font-size: 13px;
  color: rgba(255, 255, 255, 0.55);
}

.load-hint.muted {
  opacity: 0.45;
}

.empty {
  flex: 1;
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
  text-align: center;
  color: rgba(255, 255, 255, 0.55);
}

.empty-icon {
  font-size: 56px;
  margin-bottom: 12px;
}

.input-area {
  padding: 18px;
  border-top: 1px solid rgba(255, 255, 255, 0.06);
}

.reply-hint {
  margin: 0 0 10px;
  font-size: 13px;
  color: rgba(255, 255, 255, 0.65);
}

.cancel-reply {
  margin-left: 8px;
  border: none;
  background: none;
  color: #7c9bff;
  cursor: pointer;
}

.attachment-preview {
  position: relative;
  width: 120px;
  margin-bottom: 12px;
  border-radius: 12px;
  overflow: hidden;
}

.attachment-preview img,
.attachment-preview video {
  width: 100%;
  height: 80px;
  object-fit: cover;
  display: block;
}

.remove-attachment {
  position: absolute;
  top: 6px;
  right: 6px;
  width: 24px;
  height: 24px;
  border: none;
  border-radius: 50%;
  background: rgba(0, 0, 0, 0.65);
  color: white;
  cursor: pointer;
}

.input-row {
  display: flex;
  gap: 10px;
  align-items: center;
}

.hidden-file {
  display: none;
}

.attach-btn,
.input-row button:last-child {
  height: 52px;
  border: none;
  border-radius: 14px;
  cursor: pointer;
  flex-shrink: 0;
}

.attach-btn {
  width: 52px;
  background: rgba(255, 255, 255, 0.05);
  color: white;
  font-size: 18px;
}

.input-row input {
  flex: 1;
  height: 52px;
  border: 1px solid transparent;
  outline: none;
  border-radius: 14px;
  padding: 0 16px;
  background: rgba(255, 255, 255, 0.05);
  color: white;
}

.input-row button:last-child {
  padding: 0 22px;
  background: #4163fc;
  color: white;
  font-weight: 600;
}

.input-row button:last-child:disabled,
.attach-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.error {
  margin: 10px 0 0;
  font-size: 13px;
  color: #ff6b8a;
}

.fade-enter-active,
.fade-leave-active {
  transition: 0.25s;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
