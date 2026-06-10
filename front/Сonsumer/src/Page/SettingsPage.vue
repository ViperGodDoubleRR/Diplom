<template>
  <div class="settings-page">
    <header class="header">
      <div>
        <h1 class="title">Настройки</h1>
        <p class="subtitle">Аккаунт, безопасность и приватность</p>
      </div>
      <button class="btn logout" type="button" @click="openLogoutConfirm">Выйти</button>
    </header>

    <nav class="tabs">
      <button
        v-for="tab in tabs"
        :key="tab.id"
        class="tab"
        :class="{ active: activeTab === tab.id }"
        type="button"
        @click="activeTab = tab.id"
      >
        {{ tab.label }}
      </button>
    </nav>

    <!-- ACCOUNT -->
    <section v-if="activeTab === 'account'" class="panel">
      <div class="card">
        <h2>Смена email</h2>
        <p class="hint">Текущий: <strong>{{ currentEmail || "не указан" }}</strong></p>
        <p class="hint muted">Нельзя вернуться на старый email после смены.</p>

        <div class="field">
          <label>Текущий пароль</label>
          <input
            v-model="emailPassword"
            type="password"
            class="input"
            placeholder="Подтвердите паролем"
            :disabled="emailLoading"
          />
        </div>

        <div class="field">
          <label>Новый email</label>
          <input
            v-model="newEmail"
            type="email"
            class="input"
            placeholder="new@email.com"
            :disabled="emailLoading"
          />
        </div>

        <div v-if="emailStep === 2" class="field">
          <label>Код из письма</label>
          <input
            v-model="emailCode"
            class="input"
            placeholder="Код подтверждения"
            :disabled="emailLoading"
          />
        </div>

        <div class="actions">
          <button
            v-if="emailStep === 1"
            class="btn primary"
            :disabled="emailLoading || resendCooldown > 0"
            @click="requestEmailCode"
          >
            {{ emailLoading ? "Отправка..." : resendCooldown > 0 ? `Повтор через ${resendCooldown}с` : "Отправить код" }}
          </button>

          <template v-else>
            <button class="btn ghost" type="button" @click="resetEmailFlow">Назад</button>
            <button
              class="btn ghost"
              type="button"
              :disabled="emailLoading || resendCooldown > 0"
              @click="requestEmailCode"
            >
              {{ resendCooldown > 0 ? `${resendCooldown}с` : "Отправить снова" }}
            </button>
            <button class="btn primary" :disabled="emailLoading" @click="confirmEmailChange">
              {{ emailLoading ? "Сохранение..." : "Подтвердить" }}
            </button>
          </template>
        </div>
      </div>
    </section>

    <!-- SECURITY -->
    <section v-if="activeTab === 'security'" class="panel">
      <div class="card">
        <h2>Смена пароля</h2>
        <p class="hint muted">После смены все другие сессии будут завершены.</p>

        <div class="field">
          <label>Текущий пароль</label>
          <input v-model="currentPassword" type="password" class="input" :disabled="passwordLoading" />
        </div>
        <div class="field">
          <label>Новый пароль</label>
          <input v-model="newPassword" type="password" class="input" :disabled="passwordLoading" />
        </div>
        <div class="field">
          <label>Повторите пароль</label>
          <input v-model="confirmPassword" type="password" class="input" :disabled="passwordLoading" />
        </div>

        <div v-if="newPassword.length > 0" class="rules">
          <p :class="{ valid: passwordRules.minLength }">Минимум 8 символов</p>
          <p :class="{ valid: passwordRules.upperCase }">Заглавная буква</p>
          <p :class="{ valid: passwordRules.lowerCase }">Строчная буква</p>
          <p :class="{ valid: passwordRules.number }">Цифра</p>
          <p :class="{ valid: passwordRules.passwordsMatch }">Пароли совпадают</p>
          <p :class="{ valid: passwordRules.notSame }">Отличается от текущего</p>
        </div>

        <button class="btn primary" :disabled="passwordLoading" @click="changePassword">
          {{ passwordLoading ? "Сохранение..." : "Сменить пароль" }}
        </button>
      </div>

      <div class="card">
        <div class="card-head">
          <h2>Активные сессии</h2>
          <button
            class="btn danger-outline"
            type="button"
            :disabled="sessionsLoading || !otherSessionsCount"
            @click="openRevokeOthersConfirm"
          >
            Выйти везде
          </button>
        </div>

        <p v-if="sessionsLoading" class="hint">Загрузка...</p>
        <p v-else-if="!sessions.length" class="hint">Нет активных сессий</p>

        <div v-else class="session-list">
          <article
            v-for="session in sessions"
            :key="session.id"
            class="session-card"
            :class="{ current: session.isCurrent }"
          >
            <div class="session-main">
              <div class="session-title">
                <strong>{{ session.deviceLabel }}</strong>
                <span v-if="session.isCurrent" class="badge">Это устройство</span>
              </div>
              <p class="session-meta">IP: {{ session.ipAddress || "—" }}</p>
              <p class="session-meta">
                С {{ formatDate(session.createdAt) }} · до {{ formatDate(session.expiresAt) }}
              </p>
            </div>
            <button
              class="btn danger-outline small"
              :disabled="revokingId === session.id"
              @click="openRevokeSessionConfirm(session)"
            >
              {{ revokingId === session.id ? "..." : "Отозвать" }}
            </button>
          </article>
        </div>
      </div>
    </section>

    <!-- BLOCKED -->
    <section v-if="activeTab === 'blocked'" class="panel">
      <div class="card">
        <h2>Заблокированные</h2>
        <p v-if="blockedLoading" class="hint">Загрузка...</p>
        <p v-else-if="!blocked.length" class="hint">Список пуст</p>

        <ul v-else class="blocked-list">
          <li
            v-for="user in blocked"
            :key="user.id"
            class="blocked-row"
            @click="openProfile(user.id)"
          >
            <UserAvatar
              avatar-class="blocked-avatar"
              :name="user.login"
              :src="user.avatarUrl"
              :is-video="user.avatarIsVideo"
              :size="48"
            />
            <div class="blocked-info">
              <strong>{{ user.login }}</strong>
              <span v-if="user.tag" class="muted">@{{ user.tag }}</span>
            </div>
            <button class="btn danger-outline small" @click.stop="openUnblockConfirm(user.id)">
              Разблокировать
            </button>
          </li>
        </ul>
      </div>
    </section>

    <ConfirmModal
      v-model="confirmOpen"
      :title="confirmTitle"
      :message="confirmMessage"
      :confirm-label="confirmActionLabel"
      :loading="confirmLoading"
      @confirm="runConfirmAction"
    />

    <p v-if="toastMessage" :class="['toast', toastType]">{{ toastMessage }}</p>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref } from "vue";
import { useRouter } from "vue-router";

import ConfirmModal from "@/components/ui/ConfirmModal.vue";
import UserAvatar from "@/components/ui/UserAvatar.vue";
import type { Session } from "@/interface/models/settings/Session";
import { SettingsService } from "@/service/settingsService";
import { getPasswordRules, isModeratePassword } from "@/utils/authValidation";
import { parseUserAgent } from "@/utils/parseUserAgent";
import { resolveSettingsMessage } from "@/utils/settingsMessages";
import {
  apiErrorCode,
  apiErrorMessage,
  getApiData,
  isApiSuccess,
  readRevokeSessionResult,
} from "@/utils/apiHelpers";
import { useSocialStore } from "@/store/socialStore";
import { useUserStore } from "@/store/userStore";

function settingsError(res: unknown, fallback: string): string {
  const body = res as Parameters<typeof apiErrorMessage>[0];
  return resolveSettingsMessage(apiErrorCode(body), apiErrorMessage(body, fallback));
}

type TabId = "account" | "security" | "blocked";
type ConfirmAction =
  | { type: "revoke-session"; sessionId: number }
  | { type: "revoke-others" }
  | { type: "unblock"; userId: string }
  | { type: "logout" };

type SessionView = Session & { deviceLabel: string };

const service = new SettingsService();
const userStore = useUserStore();
const socialStore = useSocialStore();
const router = useRouter();

const tabs = [
  { id: "account" as TabId, label: "Аккаунт" },
  { id: "security" as TabId, label: "Безопасность" },
  { id: "blocked" as TabId, label: "Чёрный список" },
];

const activeTab = ref<TabId>("account");

const newEmail = ref("");
const emailCode = ref("");
const emailPassword = ref("");
const emailStep = ref<1 | 2>(1);
const emailLoading = ref(false);
const resendCooldown = ref(0);
let cooldownTimer: ReturnType<typeof setInterval> | null = null;

const currentPassword = ref("");
const newPassword = ref("");
const confirmPassword = ref("");
const passwordLoading = ref(false);

const sessions = ref<SessionView[]>([]);
const sessionsLoading = ref(false);
const revokingId = ref<number | null>(null);

const blockedLoading = ref(false);
const toastMessage = ref("");
const toastType = ref<"success" | "error">("success");

const confirmOpen = ref(false);
const confirmTitle = ref("");
const confirmMessage = ref("");
const confirmActionLabel = ref("Подтвердить");
const confirmLoading = ref(false);
const pendingAction = ref<ConfirmAction | null>(null);

const currentEmail = computed(() => userStore.user?.email ?? "");
const blocked = computed(() => socialStore.blocked);

const passwordRules = computed(() => ({
  ...getPasswordRules(newPassword.value),
  passwordsMatch: newPassword.value.length > 0 && newPassword.value === confirmPassword.value,
  notSame:
    newPassword.value.length > 0 &&
    currentPassword.value.length > 0 &&
    newPassword.value !== currentPassword.value,
}));

const otherSessionsCount = computed(
  () => sessions.value.filter((s) => !s.isCurrent).length
);

function showToast(text: string, type: "success" | "error" = "success") {
  toastMessage.value = text;
  toastType.value = type;
  setTimeout(() => {
    toastMessage.value = "";
  }, 3500);
}

function formatDate(value: string): string {
  if (!value) return "—";
  return new Date(value).toLocaleString("ru-RU");
}

function normalizeSessions(raw: unknown): SessionView[] {
  if (!Array.isArray(raw)) return [];

  return raw.map((item) => {
    const row = item as Record<string, unknown>;
    const deviceInfo = String(row.deviceInfo ?? row.DeviceInfo ?? "");
    return {
      id: Number(row.id ?? row.Id ?? 0),
      deviceInfo,
      ipAddress: String(row.ipAddress ?? row.IpAddress ?? ""),
      createdAt: String(row.createdAt ?? row.CreatedAt ?? ""),
      expiresAt: String(row.expiresAt ?? row.ExpiresAt ?? ""),
      isCurrent: Boolean(row.isCurrent ?? row.IsCurrent ?? false),
      deviceLabel: parseUserAgent(deviceInfo),
    };
  });
}

function startCooldown(seconds = 60) {
  resendCooldown.value = seconds;
  if (cooldownTimer) clearInterval(cooldownTimer);
  cooldownTimer = setInterval(() => {
    resendCooldown.value -= 1;
    if (resendCooldown.value <= 0 && cooldownTimer) {
      clearInterval(cooldownTimer);
      cooldownTimer = null;
    }
  }, 1000);
}

function resetEmailFlow() {
  emailStep.value = 1;
  emailCode.value = "";
}

async function loadSessions() {
  sessionsLoading.value = true;
  try {
    const res = await service.getSessions();
    const data = getApiData(res);
    if (isApiSuccess(res) && data) {
      sessions.value = normalizeSessions(data);
    }
  } finally {
    sessionsLoading.value = false;
  }
}

async function requestEmailCode() {
  const email = newEmail.value.trim();
  if (!email || !emailPassword.value) {
    showToast("Введите новый email и текущий пароль", "error");
    return;
  }

  emailLoading.value = true;
  try {
    const res = await service.requestChangeEmail(email, emailPassword.value);
    if (!isApiSuccess(res)) {
      showToast(settingsError(res, "Не удалось отправить код"), "error");
      return;
    }

    emailStep.value = 2;
    startCooldown();
    showToast("Код отправлен на новый email");
  } finally {
    emailLoading.value = false;
  }
}

async function confirmEmailChange() {
  const email = newEmail.value.trim();
  const code = emailCode.value.trim();

  if (!email || !code || !emailPassword.value) {
    showToast("Заполните email, код и пароль", "error");
    return;
  }

  emailLoading.value = true;
  try {
    const res = await service.confirmChangeEmail(email, code, emailPassword.value);
    if (!isApiSuccess(res)) {
      showToast(settingsError(res, "Не удалось сменить email"), "error");
      return;
    }

    const updatedEmail = getApiData(res) ?? email;

    if (userStore.user) {
      userStore.setUser({
        ...userStore.user,
        email: updatedEmail,
      });
    }

    newEmail.value = "";
    emailCode.value = "";
    emailPassword.value = "";
    emailStep.value = 1;
    showToast("Email обновлён");
  } finally {
    emailLoading.value = false;
  }
}

async function changePassword() {
  if (!currentPassword.value || !newPassword.value || !confirmPassword.value) {
    showToast("Заполните все поля", "error");
    return;
  }

  if (newPassword.value !== confirmPassword.value) {
    showToast("Пароли не совпадают", "error");
    return;
  }

  if (!isModeratePassword(newPassword.value)) {
    showToast("Пароль не соответствует требованиям", "error");
    return;
  }

  if (newPassword.value === currentPassword.value) {
    showToast("Новый пароль должен отличаться от текущего", "error");
    return;
  }

  passwordLoading.value = true;
  try {
    const res = await service.changePassword(currentPassword.value, newPassword.value);
    if (!isApiSuccess(res)) {
      showToast(settingsError(res, "Не удалось сменить пароль"), "error");
      return;
    }

    currentPassword.value = "";
    newPassword.value = "";
    confirmPassword.value = "";
    await loadSessions();
    showToast("Пароль изменён. Другие сессии завершены.");
  } finally {
    passwordLoading.value = false;
  }
}

function openProfile(userId: string) {
  router.push({ name: "profile-view", params: { id: userId } });
}

function openUnblockConfirm(userId: string) {
  pendingAction.value = { type: "unblock", userId };
  confirmTitle.value = "Разблокировать пользователя?";
  confirmMessage.value = "Он снова сможет взаимодействовать с вами.";
  confirmActionLabel.value = "Разблокировать";
  confirmOpen.value = true;
}

function openRevokeSessionConfirm(session: SessionView) {
  pendingAction.value = { type: "revoke-session", sessionId: session.id };
  confirmTitle.value = session.isCurrent ? "Выйти на этом устройстве?" : "Отозвать сессию?";
  confirmMessage.value = session.isCurrent
    ? "Вы будете разлогинены после подтверждения."
    : `Завершить сессию «${session.deviceLabel}»?`;
  confirmActionLabel.value = session.isCurrent ? "Выйти" : "Отозвать";
  confirmOpen.value = true;
}

function openRevokeOthersConfirm() {
  pendingAction.value = { type: "revoke-others" };
  confirmTitle.value = "Выйти на всех устройствах?";
  confirmMessage.value = "Текущая сессия останется активной, остальные будут завершены.";
  confirmActionLabel.value = "Завершить другие";
  confirmOpen.value = true;
}

function openLogoutConfirm() {
  pendingAction.value = { type: "logout" };
  confirmTitle.value = "Выйти из аккаунта?";
  confirmMessage.value = "Потребуется снова войти на этом устройстве.";
  confirmActionLabel.value = "Выйти";
  confirmOpen.value = true;
}

async function runConfirmAction() {
  if (!pendingAction.value) return;

  confirmLoading.value = true;
  try {
    const action = pendingAction.value;

    if (action.type === "unblock") {
      const res = await socialStore.unblockUser(action.userId);
      if (!isApiSuccess(res)) {
        showToast(settingsError(res, "Не удалось разблокировать"), "error");
        return;
      }
      showToast("Пользователь разблокирован");
    }

    if (action.type === "revoke-session") {
      revokingId.value = action.sessionId;
      const res = await service.revokeSession(action.sessionId);
      if (!isApiSuccess(res)) {
        showToast(settingsError(res, "Не удалось отозвать сессию"), "error");
        return;
      }

      sessions.value = sessions.value.filter((s) => s.id !== action.sessionId);
      showToast("Сессия отозвана");

      const revokeResult = readRevokeSessionResult(getApiData(res));

      if (revokeResult?.wasCurrentSession) {
        userStore.logout();
        router.push("/auth");
      }
    }

    if (action.type === "revoke-others") {
      const res = await service.revokeOtherSessions();
      if (!isApiSuccess(res)) {
        showToast(settingsError(res, "Не удалось завершить другие сессии"), "error");
        return;
      }
      await loadSessions();
      showToast(`Завершено сессий: ${getApiData(res) ?? 0}`);
    }

    if (action.type === "logout") {
      userStore.logout();
      router.push("/auth");
    }

    confirmOpen.value = false;
    pendingAction.value = null;
  } finally {
    confirmLoading.value = false;
    revokingId.value = null;
  }
}

onMounted(async () => {
  blockedLoading.value = true;
  try {
    if (!userStore.user) {
      await userStore.getMy();
    }

    await Promise.all([socialStore.getBlocked(), loadSessions()]);
  } finally {
    blockedLoading.value = false;
  }
});

onUnmounted(() => {
  if (cooldownTimer) clearInterval(cooldownTimer);
});
</script>

<style scoped>
.settings-page {
  max-width: 920px;
  margin: 0 auto;
  color: #fff;
}

.header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 16px;
  margin-bottom: 20px;
}

.title {
  margin: 0;
  font-size: 32px;
}

.subtitle {
  margin: 6px 0 0;
  color: #9aa0b5;
}

.tabs {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
  margin-bottom: 18px;
}

.tab {
  border: 1px solid rgba(255, 255, 255, 0.1);
  background: rgba(255, 255, 255, 0.03);
  color: #c9d0e3;
  border-radius: 999px;
  padding: 10px 16px;
  cursor: pointer;
}

.tab.active {
  color: #fff;
  border-color: rgba(65, 99, 252, 0.45);
  background: rgba(65, 99, 252, 0.18);
}

.panel {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.card {
  background: rgba(255, 255, 255, 0.04);
  border: 1px solid rgba(255, 255, 255, 0.08);
  border-radius: 18px;
  padding: 20px;
}

.card-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  margin-bottom: 12px;
}

.card h2 {
  margin: 0 0 12px;
  font-size: 20px;
}

.card-head h2 {
  margin: 0;
}

.hint {
  margin: 0 0 12px;
  color: #b8bfd4;
  font-size: 14px;
}

.hint.muted {
  color: #8b93a7;
}

.field {
  margin-bottom: 12px;
}

.field label {
  display: block;
  margin-bottom: 6px;
  font-size: 13px;
  color: #aeb6cb;
}

.input {
  width: 100%;
  box-sizing: border-box;
  padding: 12px 14px;
  border-radius: 12px;
  border: 1px solid rgba(255, 255, 255, 0.12);
  background: rgba(0, 0, 0, 0.25);
  color: #fff;
}

.actions {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
}

.btn {
  border: none;
  border-radius: 12px;
  padding: 10px 16px;
  cursor: pointer;
  font-weight: 600;
}

.btn.primary {
  background: linear-gradient(135deg, #4163fc, #5b7cff);
  color: #fff;
}

.btn.ghost {
  background: rgba(255, 255, 255, 0.08);
  color: #fff;
}

.btn.logout,
.btn.danger-outline {
  background: rgba(255, 77, 77, 0.12);
  color: #ff7b7b;
  border: 1px solid rgba(255, 77, 77, 0.35);
}

.btn.small {
  padding: 7px 12px;
  font-size: 12px;
}

.btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.rules {
  margin: 8px 0 14px;
  font-size: 13px;
  color: #8f97ab;
}

.rules p {
  margin: 4px 0;
}

.rules .valid {
  color: #5fd38d;
}

.session-list {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.session-card {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  padding: 14px;
  border-radius: 14px;
  background: rgba(0, 0, 0, 0.22);
  border: 1px solid rgba(255, 255, 255, 0.06);
}

.session-card.current {
  border-color: rgba(65, 99, 252, 0.45);
  box-shadow: 0 0 0 1px rgba(65, 99, 252, 0.15);
}

.session-title {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}

.badge {
  font-size: 11px;
  padding: 3px 8px;
  border-radius: 999px;
  background: rgba(65, 99, 252, 0.2);
  color: #9eb0ff;
}

.session-meta {
  margin: 4px 0 0;
  font-size: 12px;
  color: #9aa0b5;
}

.blocked-list {
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.blocked-row {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px;
  border-radius: 14px;
  background: rgba(0, 0, 0, 0.2);
  cursor: pointer;
  transition: background 0.15s ease;
}

.blocked-row:hover {
  background: rgba(65, 99, 252, 0.08);
}

.blocked-info {
  flex: 1;
  min-width: 0;
}

.blocked-info .muted {
  display: block;
  color: #9aa0b5;
  font-size: 13px;
}

.blocked-row :deep(.blocked-avatar) {
  width: 48px;
  height: 48px;
}

.toast {
  position: fixed;
  right: 24px;
  bottom: 24px;
  padding: 12px 16px;
  border-radius: 12px;
  z-index: 10000;
}

.toast.success {
  background: rgba(46, 160, 87, 0.92);
}

.toast.error {
  background: rgba(220, 53, 69, 0.92);
}

@media (max-width: 720px) {
  .header {
    flex-direction: column;
  }

  .session-card,
  .blocked-row {
    flex-direction: column;
    align-items: flex-start;
  }
}
</style>
