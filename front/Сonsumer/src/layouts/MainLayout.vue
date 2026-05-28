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
  await userStore.getMy();

  await Promise.all([
    socialStore.getFriends(),
    socialStore.getBlocked(),
  ]);
});
</script>

<style scoped>
.layout {
  width: 100%;
  min-height: 100vh;
  display: flex;
  background-color: #0F0F0F;
}

.main-content {
  flex: 1;
  padding: 30px;
  overflow-y: auto;
}
</style>
