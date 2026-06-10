<template>
  <article class="comment-card" :class="{ reply: isReply }">
    <button type="button" class="avatar-link" @click="openProfile">
      <UserAvatar
        avatar-class="avatar"
        :name="comment.user.login"
        :src="comment.user.avatar"
      />
    </button>

    <div class="body">
      <div class="top">
        <button type="button" class="author-link" @click="openProfile">
          {{ comment.user.login }}
        </button>
        <span class="date">{{ formatDate(comment.createdAt) }}</span>

        <button
          v-if="isOwn"
          type="button"
          class="delete-btn"
          title="Удалить комментарий"
          @click="$emit('delete', comment.id)"
        >
          🗑
        </button>
      </div>

      <ExpandableText
        v-if="comment.text?.trim()"
        class="comment-text"
        :text="comment.text"
        expand-label="ещё"
      />

      <div v-if="comment.media.length" class="media-wrap">
        <img
          v-if="!isVideoMediaType(comment.media[0]?.mediaType)"
          :src="comment.media[0]?.url"
          class="media-preview"
          alt=""
        />
        <video
          v-else
          :src="comment.media[0]?.url"
          class="media-preview"
          controls
          playsinline
          preload="metadata"
        />
      </div>

      <div class="actions">
        <button
          type="button"
          class="reaction"
          :class="{ active: comment.reactions.myReaction === REACTION_LIKE }"
          @click="$emit('react', comment.id, REACTION_LIKE)"
        >
          👍 {{ comment.reactions.likes }}
        </button>

        <button
          type="button"
          class="reaction"
          :class="{ active: comment.reactions.myReaction === REACTION_DISLIKE }"
          @click="$emit('react', comment.id, REACTION_DISLIKE)"
        >
          👎 {{ comment.reactions.dislikes }}
        </button>

        <button
          v-if="!isReply"
          type="button"
          class="reply-btn"
          @click="$emit('reply', comment.id)"
        >
          Ответить
        </button>
      </div>

      <div
        v-if="!isReply && (comment.repliesCount > 0 || repliesOpened || (replies?.length ?? 0) > 0)"
        class="replies-block"
      >
        <button
          v-if="!repliesOpened"
          type="button"
          class="open-replies"
          @click="$emit('open-replies', comment.id)"
        >
          Показать {{ repliesCountLabel }} {{ replyLabel(repliesCountLabel) }}
        </button>

        <template v-else>
          <div v-if="repliesLoading && !(replies?.length)" class="replies-loading">
            Загрузка ответов...
          </div>

          <CommentItem
            v-for="reply in replies ?? []"
            :key="reply.id"
            :comment="reply"
            :is-reply="true"
            :is-own="sameUser(reply.user.id)"
            @delete="$emit('delete-reply', comment.id, $event)"
            @react="(id, type) => $emit('react-reply', comment.id, id, type)"
          />

          <button
            v-if="repliesHasMore || hiddenReplies > 0"
            type="button"
            class="open-replies"
            :disabled="repliesLoadingMore"
            @click="$emit('load-more-replies', comment.id)"
          >
            {{
              repliesLoadingMore
                ? "Загрузка..."
                : `Показать ещё ${nextReplyBatch} ${replyLabel(nextReplyBatch)}`
            }}
          </button>

          <button
            type="button"
            class="open-replies collapse"
            @click="$emit('close-replies', comment.id)"
          >
            Скрыть ответы
          </button>
        </template>
      </div>
    </div>
  </article>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { useRouter } from "vue-router";

import CommentItem from "@/components/comment/CommentItem.vue";
import ExpandableText from "@/components/ui/ExpandableText.vue";
import UserAvatar from "@/components/ui/UserAvatar.vue";
import {
  COMMENT_REACTION_DISLIKE as REACTION_DISLIKE,
  COMMENT_REACTION_LIKE as REACTION_LIKE,
  COMMENT_REPLY_BATCH,
} from "@/constants/commentConstants";
import type { CommentDto } from "@/interface/DTO/comments/CommentDto";
import { useUserStore } from "@/store/userStore";
import { isVideoMediaType } from "@/utils/postNormalize";

const props = defineProps<{
  comment: CommentDto;
  isReply?: boolean;
  isOwn?: boolean;
  replies?: CommentDto[];
  repliesOpened?: boolean;
  repliesLoading?: boolean;
  repliesHasMore?: boolean;
  repliesLoadingMore?: boolean;
}>();

defineEmits<{
  delete: [commentId: string];
  react: [commentId: string, type: number];
  reply: [commentId: string];
  "open-replies": [commentId: string];
  "close-replies": [commentId: string];
  "load-more-replies": [commentId: string];
  "delete-reply": [parentId: string, replyId: string];
  "react-reply": [parentId: string, replyId: string, type: number];
}>();

const userStore = useUserStore();
const router = useRouter();

const repliesCountLabel = computed(() => {
  const fromComment = props.comment.repliesCount;
  if (fromComment > 0) return fromComment;
  return props.replies?.length ?? 0;
});

const hiddenReplies = computed(() => {
  const total = Math.max(props.comment.repliesCount, props.replies?.length ?? 0);
  const loaded = props.replies?.length ?? 0;
  return Math.max(0, total - loaded);
});

const nextReplyBatch = computed(() => {
  const remaining = hiddenReplies.value;
  if (remaining > 0) return Math.min(COMMENT_REPLY_BATCH, remaining);
  return COMMENT_REPLY_BATCH;
});

function sameUser(userId: string) {
  return (
    !!userStore.user?.id &&
    userId.toLowerCase() === userStore.user.id.toLowerCase()
  );
}

function openProfile() {
  const authorId = props.comment.user.id;
  if (!authorId) return;

  if (sameUser(authorId)) {
    void router.push({ name: "profile" });
    return;
  }

  void router.push({ name: "profile-view", params: { id: authorId } });
}

function formatDate(date: string) {
  return new Date(date).toLocaleString(undefined, {
    day: "2-digit",
    month: "short",
    hour: "2-digit",
    minute: "2-digit",
  });
}

function replyLabel(count: number) {
  const mod10 = count % 10;
  const mod100 = count % 100;
  if (mod10 === 1 && mod100 !== 11) return "ответ";
  if (mod10 >= 2 && mod10 <= 4 && (mod100 < 10 || mod100 >= 20)) return "ответа";
  return "ответов";
}
</script>

<style scoped>
.comment-card {
  display: flex;
  gap: 14px;
  padding: 14px;
  border-radius: 18px;
  background: rgba(255, 255, 255, 0.03);
  border: 1px solid rgba(255, 255, 255, 0.05);
}

.comment-card.reply {
  margin-top: 10px;
  margin-left: 12px;
  padding: 12px;
  border-radius: 14px;
  background: rgba(255, 255, 255, 0.02);
}

.comment-card :deep(.avatar),
.comment-card :deep(.user-avatar),
.comment-card :deep(.initials) {
  width: 44px;
  height: 44px;
  flex-shrink: 0;
}

.comment-card.reply :deep(.avatar),
.comment-card.reply :deep(.user-avatar),
.comment-card.reply :deep(.initials) {
  width: 36px;
  height: 36px;
}

.body {
  flex: 1;
  min-width: 0;
}

.top {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-bottom: 6px;
  flex-wrap: wrap;
}

.avatar-link,
.author-link {
  border: none;
  padding: 0;
  background: transparent;
  cursor: pointer;
  text-align: left;
}

.avatar-link {
  flex-shrink: 0;
}

.author-link {
  font-size: 14px;
  font-weight: 700;
  color: white;
}

.author-link:hover {
  color: #9eb0ff;
}

.login {
  font-size: 14px;
  font-weight: 700;
  color: white;
}

.date {
  font-size: 12px;
  color: rgba(255, 255, 255, 0.45);
}

.delete-btn {
  margin-left: auto;
  border: none;
  background: rgba(255, 255, 255, 0.06);
  color: rgba(255, 255, 255, 0.75);
  border-radius: 8px;
  width: 30px;
  height: 30px;
  cursor: pointer;
}

.delete-btn:hover {
  background: rgba(255, 107, 138, 0.18);
  color: #ff8da8;
}

.comment-text {
  font-size: 14px;
  color: rgba(255, 255, 255, 0.92);
}

.media-wrap {
  margin-top: 10px;
  border-radius: 14px;
  overflow: hidden;
  max-width: 320px;
  background: #1a1a1a;
}

.media-preview {
  display: block;
  width: 100%;
  max-height: 240px;
  object-fit: cover;
}

.actions {
  display: flex;
  gap: 8px;
  margin-top: 10px;
  flex-wrap: wrap;
}

.reaction,
.reply-btn,
.open-replies {
  border: none;
  background: rgba(255, 255, 255, 0.05);
  color: rgba(255, 255, 255, 0.8);
  border-radius: 999px;
  padding: 6px 12px;
  font-size: 12px;
  cursor: pointer;
}

.reaction.active {
  background: rgba(65, 99, 252, 0.2);
  color: #dbe3ff;
}

.reply-btn {
  background: transparent;
  color: #7c9bff;
}

.open-replies {
  margin-top: 8px;
  background: transparent;
  color: #7c9bff;
  padding-left: 0;
}

.open-replies.collapse {
  margin-top: 10px;
  margin-bottom: 0;
  opacity: 0.85;
}

.replies-block {
  margin-top: 4px;
}

.replies-loading {
  font-size: 12px;
  opacity: 0.55;
  padding: 6px 0;
}
</style>
