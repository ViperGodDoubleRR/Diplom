<template>
  <div class="avatar-shell" :class="avatarClass" :style="shellStyle">
    <video
      v-if="showVideo"
      :src="effectiveSrc!"
      class="avatar-media"
      autoplay
      loop
      muted
      playsinline
      @error="onMediaError"
    />
    <img
      v-else-if="showImage"
      :src="effectiveSrc!"
      :alt="name || 'Avatar'"
      class="avatar-media"
      @error="onImageError"
    />
    <div v-else class="avatar-media initials" :style="{ backgroundColor: bgColor }">
      <span>{{ initials }}</span>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from "vue";
import { getAvatarColor, getAvatarInitials } from "@/utils/avatarUtils";

const props = withDefaults(
  defineProps<{
    name?: string | null;
    src?: string | null;
    isVideo?: boolean;
    size?: number | string;
    avatarClass?: string | string[] | Record<string, boolean>;
  }>(),
  {
    name: "",
    src: null,
    isVideo: false,
    size: undefined,
    avatarClass: "",
  }
);

const imageError = ref(false);

const effectiveSrc = computed(() => {
  const value = props.src?.trim();
  return value || null;
});

watch(
  () => props.src,
  () => {
    imageError.value = false;
  }
);

const resolvedIsVideo = computed(() => {
  if (props.isVideo) return true;
  const src = effectiveSrc.value;
  if (!src) return false;
  return /\.(mp4|webm|mov)(\?|#|$)/i.test(src);
});

const showVideo = computed(
  () => resolvedIsVideo.value && !!effectiveSrc.value && !imageError.value
);

const showImage = computed(
  () => !resolvedIsVideo.value && !!effectiveSrc.value && !imageError.value
);

const initials = computed(() => getAvatarInitials(props.name));
const bgColor = computed(() => getAvatarColor(props.name));

const shellStyle = computed(() => {
  if (props.size === undefined || props.size === null || props.size === "") {
    return {};
  }

  const value = typeof props.size === "number" ? `${props.size}px` : props.size;
  const fontSize =
    typeof props.size === "number" ? `${Math.max(11, Math.round(props.size * 0.36))}px` : "0.85rem";

  return {
    width: value,
    height: value,
    minWidth: value,
    minHeight: value,
    fontSize,
  };
});

function onImageError() {
  imageError.value = true;
}

function onMediaError() {
  imageError.value = true;
}
</script>

<style scoped>
.avatar-shell {
  flex-shrink: 0;
  border-radius: 50%;
  overflow: hidden;
  border: 2px solid rgba(65, 99, 252, 0.45);
  background: #1a1a1a;
  box-sizing: border-box;
}

.avatar-media {
  display: block;
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.initials {
  display: flex;
  align-items: center;
  justify-content: center;
  color: #fff;
  font-weight: 700;
  letter-spacing: 0.04em;
  text-transform: uppercase;
  user-select: none;
}

.initials span {
  line-height: 1;
}
</style>
