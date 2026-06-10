<template>
  <aside class="sidebar">
    <div>
      <h1 class="logo">GoatBridge</h1>

      <nav class="menu">
        <button class="menu-btn" :class="{ active: isActive('/feed') }" @click="go('/feed')">
          Лента
        </button>

        <button class="menu-btn" :class="{ active: isActive('/profile') }" @click="go('/profile')">
          Профиль
        </button>

        <button class="menu-btn" :class="{ active: isActive('/messages') }" @click="go('/messages')">
          Сообщения
        </button>

        <button class="menu-btn" :class="{ active: isActive('/settings') }" @click="go('/settings')">
          Настройки
        </button>
      </nav>
    </div>

    <div class="user-box">
      <UserAvatar
        avatar-class="avatar"
        :name="userName"
        :src="userAvatar"
      />

      <div>
        <h3>{{ userName }}</h3>
        <p>@{{ userTag }}</p>
      </div>
    </div>
  </aside>
</template>

<script setup lang="ts">
import { useRouter, useRoute } from "vue-router"
import { computed } from "vue"
import { useUserStore } from "@/store/userStore"
import { pickAvatarPhotoUrl } from "@/utils/profileValidation"
import UserAvatar from "@/components/ui/UserAvatar.vue"

const router = useRouter()
const route = useRoute()
const userStore = useUserStore()

// NAVIGATION
function go(path: string) {
  router.push(path)
}

function isActive(path: string) {
  return route.path.startsWith(path)
}

// USER DATA
const userName = computed(() => userStore.user?.login ?? "User")
const userTag = computed(() => userStore.user?.tag ?? "unknown")

const userAvatar = computed(() => pickAvatarPhotoUrl(userStore.user?.media))
</script>

<style scoped>
.sidebar {
  flex-shrink: 0;
  width: 290px;
  height: 100vh;
  padding: 28px 20px;
  box-sizing: border-box;

  display: flex;
  flex-direction: column;
  justify-content: space-between;

  background: rgba(255, 255, 255, 0.03);
  border-right: 1px solid rgba(255, 255, 255, 0.06);

  backdrop-filter: blur(16px);

  color: white;
  overflow-y: auto;
  overscroll-behavior: contain;
}

/* LOGO */
.logo {
  font-size: 34px;
  margin-bottom: 45px;

  color: white;
  letter-spacing: 1px;
}

/* MENU */
.menu {
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.menu-btn {
  width: 100%;
  height: 58px;

  border: none;
  border-radius: 18px;

  font-size: 18px;

  cursor: pointer;

  color: rgba(255, 255, 255, 0.7);
  background: rgba(255, 255, 255, 0.03);

  transition: 0.2s ease;

  text-align: left;
  padding-left: 16px;
}

.menu-btn:hover {
  background: rgba(65, 99, 252, 0.12);
  transform: translateX(4px);
  color: white;
}

/* ACTIVE */
.active {
  background: rgba(65, 99, 252, 0.18);
  border: 1px solid rgba(65, 99, 252, 0.35);
  color: white;
  box-shadow: 0 0 18px rgba(65, 99, 252, 0.25);
}

/* USER */
.user-box {
  display: flex;
  align-items: center;
  gap: 12px;

  padding: 14px;
  border-radius: 18px;

  background: rgba(255, 255, 255, 0.04);
  border: 1px solid rgba(255, 255, 255, 0.06);
}

.user-box :deep(.avatar),
.user-box :deep(.user-avatar),
.user-box :deep(.initials) {
  width: 52px;
  height: 52px;
  flex-shrink: 0;
  border: 2px solid rgba(65, 99, 252, 0.4);
}

.user-box :deep(.initials) {
  font-size: 1rem;
}

h3 {
  margin: 0;
  font-size: 15px;
}

p {
  margin: 0;
  font-size: 12px;
  opacity: 0.6;
}

@media (max-width: 900px) {
  .sidebar {
    width: 240px;
    padding: 20px 14px;
  }

  .logo {
    font-size: 26px;
    margin-bottom: 32px;
  }

  .menu-btn {
    height: 50px;
    font-size: 16px;
  }
}
</style>
