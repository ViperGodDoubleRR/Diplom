<template>
  <div class="expandable-text">
    <p ref="textRef" class="text" :class="{ clamped: collapsible && !expanded }">
      {{ text }}
    </p>

    <button
      v-if="collapsible"
      type="button"
      class="toggle"
      @click="expanded = !expanded"
    >
      {{ expanded ? collapseLabel : expandLabel }}
    </button>
  </div>
</template>

<script setup lang="ts">
import { computed, nextTick, onMounted, ref, watch } from "vue";

const props = withDefaults(
  defineProps<{
    text: string;
    maxLines?: number;
    collapseAt?: number;
    expandLabel?: string;
    collapseLabel?: string;
  }>(),
  {
    maxLines: 4,
    collapseAt: 220,
    expandLabel: "раскрыть",
    collapseLabel: "скрыть",
  }
);

const textRef = ref<HTMLElement | null>(null);
const expanded = ref(false);
const overflowByHeight = ref(false);

const lineClamp = computed(() => String(props.maxLines));

const lineCount = computed(() => props.text.split("\n").length);

const collapsible = computed(
  () =>
    lineCount.value > props.maxLines ||
    props.text.length > props.collapseAt ||
    overflowByHeight.value
);

async function measureOverflow() {
  if (expanded.value) return;

  await nextTick();

  const el = textRef.value;
  if (!el) {
    overflowByHeight.value = false;
    return;
  }

  overflowByHeight.value = el.scrollHeight > el.clientHeight + 1;
}

watch(
  () => props.text,
  () => {
    expanded.value = false;
    void measureOverflow();
  }
);

onMounted(() => {
  void measureOverflow();
});
</script>

<style scoped>
.expandable-text {
  min-width: 0;
}

.text {
  margin: 0;
  font-size: inherit;
  line-height: 1.65;
  color: inherit;
  white-space: pre-wrap;
  word-break: break-word;
}

.text.clamped {
  display: -webkit-box;
  -webkit-box-orient: vertical;
  -webkit-line-clamp: v-bind(lineClamp);
  overflow: hidden;
}

.toggle {
  margin-top: 4px;
  padding: 0;
  border: none;
  background: none;
  color: #7c9bff;
  font-size: 13px;
  font-weight: 600;
  cursor: pointer;
}

.toggle:hover {
  color: #a8bbff;
}
</style>
