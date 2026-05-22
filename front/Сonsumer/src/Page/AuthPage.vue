<template>
  <div class="content">
    <figure>
      <img src="@/assets/image/logo.png" alt="Арт соц сети" class="logo" />

      <figcaption>
        Достойный арт Социальной сети "GoatBridge"
      </figcaption>
    </figure>

    <div class="form">
      <h1 class="welcome-title">Welcome</h1>

      <div class="fields-container">

        <!-- EMAIL -->
        <div class="field-group">
          <h2>Email</h2>

          <input v-model="email" type="email" placeholder="example@gmail.com" class="main-input" />
        </div>

        <!-- CODE -->
        <div class="field-group">
          <h2>Verification Code</h2>

          <div class="input-wrapper">
            <input v-model="code" placeholder="H89AKSDS" class="verification-input" />

            <button class="request-btn" @click="requestCode" :disabled="loading">
              {{ loading ? "wait..." : "request" }}
            </button>
          </div>
        </div>

        <!-- PASSWORD -->
        <div class="field-group">
          <h2>Password</h2>

          <div class="input-wrapper">
            <input v-model="password" :type="showPassword ? 'text' : 'password'" placeholder="********"
              class="verification-input" />

            <button class="eye-btn" @click="showPassword = !showPassword" type="button">
              <img src="@/assets/image/password.png" alt="view" class="eye-icon" />
            </button>
          </div>
        </div>
      </div>

      <!-- MESSAGE -->
      <transition name="fade">
        <div v-if="message" :class="[
          'message-box',
          messageType === 'success'
            ? 'message-success'
            : 'message-error'
        ]">
          {{ message }}
        </div>
      </transition>

      <!-- BUTTONS -->
      <div class="buttonmenu">
        <button class="action-btn cancel" @click="clearFields" :disabled="loading">
          Cancel
        </button>

        <button class="action-btn next" @click="login" :disabled="loading">
          {{ loading ? "Loading..." : "Login" }}
        </button>
      </div>

      <!-- FOOTER -->
      <div class="auth-footer">
        <div class="footer-row">
          <h4>No Have account?</h4>

          <h3 @click="goToRegistration">
            Click
          </h3>
        </div>

        <div class="footer-row">
          <h4>Reset password?</h4>

          <h3 @click="goToResetPassword">
            Click
          </h3>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { useRouter } from "vue-router";
import { AuthService } from "@/service/authService";
import type { AuthGo } from "@/interface/DTO/AuthGo";

const router = useRouter();
const service = new AuthService();

/* state */
const email = ref("");
const code = ref("");
const password = ref("");

const showPassword = ref(false);
const loading = ref(false);

/* message */
const message = ref("");
const messageType = ref<"success" | "error">("success");

const setMessage = (text: string, type: "success" | "error") => {
  message.value = text;
  messageType.value = type;

  setTimeout(() => {
    message.value = "";
  }, 3500);
};

/* request code */
const requestCode = async () => {
  if (!email.value.trim()) {
    setMessage("Введите email", "error");
    return;
  }

  loading.value = true;

  try {
    const res = await service.requestCode(email.value);

    if (!res.success) {
      setMessage(res.error?.message ?? "Ошибка отправки кода", "error");
      return;
    }

    setMessage(res.data ?? "Код отправлен", "success");
  } catch {
    setMessage("Ошибка сервера", "error");
  } finally {
    loading.value = false;
  }
};

/* login */
const login = async () => {
  if (!email.value.trim() || !code.value.trim() || !password.value.trim()) {
    setMessage("Заполни все поля", "error");
    return;
  }

  loading.value = true;

  try {
    const payload: AuthGo = {
      email: email.value,
      password: password.value,
      code: code.value,
      deviceinfo: navigator.userAgent
    };

    const res = await service.authGo(payload);

    if (!res.success || !res.data) {
      setMessage(res.error?.message ?? "Ошибка авторизации", "error");
      return;
    }

    // сохраняем токены
    localStorage.setItem("accessToken", res.data.accessToken);
    localStorage.setItem("refreshToken", res.data.refreshToken);

    setMessage("Успешный вход", "success");

    setTimeout(() => {
      router.push("/");
    }, 1000);

  } catch {
    setMessage("Ошибка сервера", "error");
  } finally {
    loading.value = false;
  }
};

/* utils */
const clearFields = () => {
  email.value = "";
  code.value = "";
  password.value = "";
};

const goToRegistration = () => router.push("/reg");
const goToResetPassword = () => router.push("/res");
</script>

<style scoped>
.content {
  display: flex;
  width: 100%;
  height: 100vh;
  align-items: center;
  justify-content: center;
  background-color: #0F0F0F;
}

.logo {
  width: 720px;
  height: auto;
  margin-right: 100px;
}

figcaption {
  color: #F8F9FA;
  font-family: 'Ouroboros', sans-serif;
  text-align: center;
  margin-top: 10px;
  opacity: 0.7;
}

.form {
  width: 704px;
  min-height: 850px;
  padding: 60px 40px 40px 40px;
  box-sizing: border-box;

  background: linear-gradient(to bottom right,
      rgba(169, 68, 189, 0.83) 8%,
      rgba(65, 99, 252, 0.61) 100%);

  box-shadow:
    6px 6px 4px 6px rgba(65, 99, 252, 0.59),
    inset 6px 6px 4px 6px rgba(169, 68, 189, 0.59);

  border-radius: 40px;

  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: space-between;
}

.welcome-title {
  font-size: 64px;
  margin-top: -20px;
  margin-bottom: 20px;
}

.fields-container {
  display: flex;
  flex-direction: column;
  gap: 25px;
  width: 100%;
  align-items: center;
}

.field-group {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
  width: 100%;
}

h1,
h2,
h3,
h4,
button,
input,
div {
  font-family: 'Ouroboros', sans-serif;
  color: #F8F9FA;
  margin: 0;
  text-align: center;
}

h2 {
  font-size: 32px;
  width: 100%;
}

h3 {
  font-size: 24px;
  cursor: pointer;
  text-decoration: underline;
  opacity: 0.9;
}

h4 {
  font-size: 20px;
  opacity: 0.7;
}

.main-input,
.verification-input {
  width: 589px;
  height: 67px;

  border-radius: 50px;
  border: none;

  font-size: 28px;

  padding: 0 30px;
  box-sizing: border-box;

  outline: none;

  background: linear-gradient(to right,
      rgba(108, 40, 197, 0.9),
      rgba(52, 19, 95, 0.9));
}

.input-wrapper {
  position: relative;
  width: 589px;
  height: 67px;
}

.verification-input {
  width: 100%;
}

.request-btn {
  position: absolute;
  right: 0;
  top: 0;

  width: 170px;
  height: 100%;

  background-color: #4163FC;

  border-radius: 50px;
  border: none;

  font-size: 24px;

  cursor: pointer;

  opacity: 0.85;
}

.eye-btn {
  position: absolute;
  right: 12px;
  top: 50%;

  transform: translateY(-50%);

  width: 60px;
  height: 45px;

  background-color: white;

  border-radius: 30px;
  border: none;

  display: flex;
  align-items: center;
  justify-content: center;

  cursor: pointer;
}

.eye-icon {
  width: 32px;
  height: auto;
}

.buttonmenu {
  display: flex;
  gap: 30px;
  margin-top: 30px;
  margin-bottom: 10px;
}

.action-btn {
  width: 252px;
  height: 82px;

  border-radius: 50px;
  border: none;

  font-size: 32px;

  background-color: #4163FC;

  opacity: 0.8;
  cursor: pointer;

  transition: 0.3s;
}

.action-btn:hover {
  transform: scale(1.03);
}

.cancel {
  background-color: rgba(255, 255, 255, 0.2);
}

.auth-footer {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
  margin-top: 10px;
}

.footer-row {
  display: flex;
  align-items: baseline;
  gap: 10px;
}

/* MESSAGE */

.message-box {
  width: 589px;

  min-height: 60px;

  border-radius: 20px;

  display: flex;
  align-items: center;
  justify-content: center;

  padding: 15px;

  box-sizing: border-box;

  font-size: 20px;

  margin-top: 20px;

  animation: pulse 0.3s ease;
}

.message-success {
  background: rgba(60, 255, 120, 0.18);

  border: 2px solid rgba(60, 255, 120, 0.5);
}

.message-error {
  background: rgba(255, 60, 60, 0.18);

  border: 2px solid rgba(255, 60, 60, 0.5);
}

.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.4s;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}

button:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

@keyframes pulse {
  0% {
    transform: scale(0.95);
  }

  100% {
    transform: scale(1);
  }
}
</style>
