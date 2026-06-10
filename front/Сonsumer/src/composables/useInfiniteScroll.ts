import { onUnmounted, ref, watch, type Ref } from "vue";

export function useInfiniteScroll(
  onLoadMore: () => void | Promise<void>,
  options?: {
    disabled?: Ref<boolean>;
    rootMargin?: string;
  }
) {
  const sentinel = ref<HTMLElement | null>(null);
  let observer: IntersectionObserver | null = null;

  function disconnect() {
    observer?.disconnect();
    observer = null;
  }

  watch(
    sentinel,
    (element) => {
      disconnect();
      if (!element) return;

      observer = new IntersectionObserver(
        (entries) => {
          const entry = entries[0];
          if (!entry?.isIntersecting) return;
          if (options?.disabled?.value) return;
          void onLoadMore();
        },
        { rootMargin: options?.rootMargin ?? "240px" }
      );

      observer.observe(element);
    },
    { flush: "post" }
  );

  onUnmounted(disconnect);

  return { sentinel };
}
