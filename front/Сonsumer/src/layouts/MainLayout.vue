<template>
  <div class="layout">
    <MainSidebarVue />

    <main class="main-content">
      <router-view />
    </main>
  </div>
</template>

<script setup lang="ts">
import { onMounted } from "vue";
import MainSidebarVue from "@/components/navigation/MainSidebar.vue";

import { useUserStore } from "@/store/userStore";
import { useSocialStore } from "@/store/socialStore";

const userStore = useUserStore();
const socialStore = useSocialStore();

onMounted(async () => {
  try {
    await userStore.getMy();
    await Promise.all([
      socialStore.getFriends(),
      socialStore.getBlocked(),
    ]);
  } catch (error) {
    console.error("Failed to load user profile:", error);
  }
});
</script>

<style scoped>
.layout {
  display: flex;
  width: 100%;
  height: 100vh;
  overflow: hidden;
  background-color: #0f0f0f;
}

.main-content {
  flex: 1;
  min-width: 0;
  height: 100%;
  padding: clamp(16px, 2.5vw, 32px);
  overflow-x: hidden;
  overflow-y: auto;
  overscroll-behavior: contain;
  scrollbar-gutter: stable;
}
</style>
