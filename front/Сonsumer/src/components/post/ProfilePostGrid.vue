<template>
  <section class="grid-wrap">
    <div v-if="loading && !items.length" class="state">Загрузка постов...</div>

    <div v-else-if="!items.length" class="state empty">
      <span class="empty-icon">{{ emptyIcon }}</span>
      <p>{{ emptyText }}</p>
    </div>

    <template v-else>
      <div class="grid">
        <article
          v-for="post in items"
          :key="post.id"
          class="post"
          @click="$emit('open', post)"
        >
          <div class="post-media">
            <video
              v-if="post.mediaUrl && isVideoMediaType(post.mediaType)"
              :src="post.mediaUrl"
              class="preview"
              muted
              playsinline
              preload="metadata"
            />
            <img
              v-else-if="post.mediaUrl"
              :src="post.mediaUrl"
              class="preview"
              alt=""
            />
            <div v-else class="no-media">
              <span>{{ post.description.slice(0, 80) || "Без медиа" }}</span>
            </div>

            <span v-if="isVideoMediaType(post.mediaType)" class="badge">▶</span>
          </div>

          <div class="post-info">
            <div class="text">{{ post.description }}</div>

            <div v-if="showAuthor && 'userLogin' in post && post.userLogin" class="author">
              {{ post.userLogin }}
            </div>

            <div class="post-meta">
              <span class="date">{{ formatDate(post.createdAt) }}</span>
              <span class="likes">❤️ {{ post.likesCount ?? 0 }}</span>
            </div>
          </div>
        </article>
      </div>

      <div ref="sentinel" class="scroll-sentinel" aria-hidden="true" />

      <p v-if="loadingMore" class="loading-more">Загрузка...</p>
      <p v-else-if="!hasMore && items.length" class="loading-more muted">Больше постов нет</p>
    </template>
  </section>
</template>

<script setup lang="ts">
import { computed } from "vue";

import { useInfiniteScroll } from "@/composables/useInfiniteScroll";
import type { PostProfileCard } from "@/interface/models/post/PostProfileCard";
import type { PostReactionCard } from "@/interface/models/post/PostReactionCard";
import { isVideoMediaType } from "@/utils/postNormalize";

export type GridPost = PostProfileCard | PostReactionCard;

const props = defineProps<{
  items: GridPost[];
  loading?: boolean;
  loadingMore?: boolean;
  hasMore?: boolean;
  emptyText?: string;
  emptyIcon?: string;
  showAuthor?: boolean;
}>();

const emit = defineEmits<{
  open: [post: GridPost];
  "load-more": [];
}>();

const scrollDisabled = computed(
  () => !props.hasMore || props.loading || props.loadingMore
);

const { sentinel } = useInfiniteScroll(
  () => emit("load-more"),
  { disabled: scrollDisabled }
);

function formatDate(value: string) {
  return new Date(value).toLocaleDateString();
}
</script>

<style scoped>
.grid-wrap {
  position: relative;
  z-index: 1;
  margin-top: 24px;
}

.state {
  padding: 32px 20px;
  text-align: center;
  border-radius: 16px;
  background: rgba(255, 255, 255, 0.03);
  border: 1px solid rgba(255, 255, 255, 0.06);
  opacity: 0.75;
}

.state.empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
}

.empty-icon {
  font-size: 28px;
}

.grid {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 16px;
}

.post {
  border-radius: 16px;
  overflow: hidden;
  background: rgba(255, 255, 255, 0.03);
  border: 1px solid rgba(255, 255, 255, 0.06);
  cursor: pointer;
  transition: transform 0.2s ease, border-color 0.2s ease, box-shadow 0.2s ease;
}

.post:hover {
  transform: translateY(-3px);
  border-color: rgba(65, 99, 252, 0.25);
  box-shadow: 0 10px 28px rgba(0, 0, 0, 0.28);
}

.post-media {
  position: relative;
  aspect-ratio: 4 / 3;
  background: #1a1a1a;
  overflow: hidden;
  display: flex;
  align-items: center;
  justify-content: center;
}

.preview {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.no-media {
  padding: 16px;
  font-size: 13px;
  line-height: 1.45;
  opacity: 0.65;
  text-align: center;
}

.badge {
  position: absolute;
  right: 10px;
  bottom: 10px;
  width: 28px;
  height: 28px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  background: rgba(0, 0, 0, 0.65);
  font-size: 11px;
}

.post-info {
  padding: 12px;
}

.text {
  font-size: 13px;
  line-height: 1.45;
  opacity: 0.9;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.author {
  margin-top: 6px;
  font-size: 12px;
  opacity: 0.55;
}

.post-meta {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 11px;
  opacity: 0.55;
  margin-top: 8px;
}

.likes {
  color: #ff6b8a;
}

.scroll-sentinel {
  width: 100%;
  height: 1px;
  margin-top: 8px;
}

.loading-more {
  text-align: center;
  padding: 16px 0 8px;
  font-size: 13px;
  opacity: 0.65;
}

.loading-more.muted {
  opacity: 0.45;
}

@media (max-width: 900px) {
  .grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

@media (max-width: 640px) {
  .grid {
    grid-template-columns: 1fr;
  }
}
</style>
