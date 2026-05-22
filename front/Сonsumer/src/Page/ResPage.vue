<template>
  <div class="content">
    <figure>
      <img src="@/assets/image/logo.png" alt="GoatBridge" class="logo" />
      <figcaption>
        Достойный арт Социальной сети "GoatBridge"
      </figcaption>
    </figure>

    <div class="form">
      <h1 class="welcome-title">Reset Password</h1>

      <div class="fields-container">
        <!-- STEP 1 -->
        <template v-if="step === 1">
          <div class="field-group">
            <h2>Email</h2>

            <input v-model="email" type="email" placeholder="example@gmail.com" class="main-input" />
          </div>

          <div class="field-group">
            <h2>Verification Code</h2>

            <div class="input-wrapper">
              <input v-model="code" placeholder="H89AKSDS" class="verification-input" />

              <button class="request-btn" @click="requestCode" :disabled="loading">
                Request
              </button>
            </div>
          </div>
        </template>

        <!-- STEP 2 -->
        <template v-else>
          <div class="field-group">
            <h2>New Password</h2>

            <div class="input-wrapper">
              <input v-model="password1" :type="showPass1 ? 'text' : 'password'" placeholder="********"
                class="verification-input" />

              <button class="eye-btn" @click="showPass1 = !showPass1">
                <img src="@/assets/image/password.png" alt="view" class="eye-icon" />
              </button>
            </div>
          </div>

          <div class="field-group">
            <h2>Repeat Password</h2>

            <div class="input-wrapper">
              <input v-model="password2" :type="showPass2 ? 'text' : 'password'" placeholder="********"
                class="verification-input" />

              <button class="eye-btn" @click="showPass2 = !showPass2">
                <img src="@/assets/image/password.png" alt="view" class="eye-icon" />
              </button>
            </div>
          </div>

          <!-- PASSWORD RULES -->
          <div class="password-rules">
            <p :class="{ valid: passwordRules.minLength }">
              Минимум 8 символов
            </p>

            <p :class="{ valid: passwordRules.upperCase }">
              Хотя бы одна заглавная буква
            </p>

            <p :class="{ valid: passwordRules.lowerCase }">
              Хотя бы одна маленькая буква
            </p>

            <p :class="{ valid: passwordRules.number }">
              Хотя бы одна цифра
            </p>

            <p :class="{ valid: passwordRules.passwordsMatch }">
              Пароли совпадают
            </p>
          </div>
        </template>
      </div>

      <!-- ALERT -->
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
        <button class="action-btn cancel" @click="step = 1">
          Cancel
        </button>

        <button v-if="step === 1" class="action-btn next" @click="nextStep" :disabled="loading">
          Next
        </button>

        <button v-else class="action-btn next" @click="changePassword" :disabled="loading">
          Reset
        </button>
      </div>

      <div class="auth-footer">
        <div class="footer-row">
          <h4>Have account?</h4>

          <h3 @click="goToAuth">
            Click Authentification
          </h3>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from "vue";
import { useRouter } from "vue-router";
import { ResService } from "@/service/resService";
const router = useRouter();
const service = new ResService();

const step = ref(1);

const email = ref("");
const code = ref("");

const password1 = ref("");
const password2 = ref("");

const showPass1 = ref(false);
const showPass2 = ref(false);

const loading = ref(false);

const message = ref("");
const messageType = ref<"success" | "error">("success");

const setMessage = (
  text: string,
  type: "success" | "error"
) => {
  message.value = text;
  messageType.value = type;

  setTimeout(() => {
    message.value = "";
  }, 4000);
};

const passwordRules = computed(() => {
  return {
    minLength: password1.value.length >= 8,
    upperCase: /[A-Z]/.test(password1.value),
    lowerCase: /[a-z]/.test(password1.value),
    number: /\d/.test(password1.value),
    passwordsMatch:
      password1.value.length > 0 &&
      password1.value === password2.value
  };
});

const isPasswordValid = computed(() => {
  return Object.values(passwordRules.value).every(Boolean);
});

const requestCode = async () => {
  if (!email.value) {
    setMessage("Введите email", "error");
    return;
  }

  loading.value = true;

  try {
    const response = await service.requestCode(email.value);

    if (!response.success) {
      setMessage(
        response.error?.message ||
        "Ошибка отправки кода",
        "error"
      );
      return;
    }

    setMessage(
      response.data || "Код успешно отправлен",
      "success"
    );
  } catch {
    setMessage("Ошибка сервера", "error");
  } finally {
    loading.value = false;
  }
};

const nextStep = async () => {
  if (!email.value || !code.value) {
    setMessage(
      "Введите email и код подтверждения",
      "error"
    );
    return;
  }

  loading.value = true;

  try {
    const response = await service.checkCode(
      email.value,
      code.value
    );

    if (!response.success) {
      setMessage(
        response.error?.message ||
        "Неверный код",
        "error"
      );
      return;
    }

    setMessage(
      response.data || "Код подтвержден",
      "success"
    );

    setTimeout(() => {
      step.value = 2;
    }, 1000);
  } catch {
    setMessage("Ошибка сервера", "error");
  } finally {
    loading.value = false;
  }
};

const changePassword = async () => {
  if (!isPasswordValid.value) {
    setMessage(
      "Пароль не соответствует требованиям",
      "error"
    );
    return;
  }

  loading.value = true;

  try {
    const response = await service.changePassword(
      email.value,
      password1.value
    );

    if (!response.success) {
      setMessage(
        response.error?.message ||
        "Ошибка смены пароля",
        "error"
      );
      return;
    }

    setMessage(
      response.data ||
      "Пароль успешно изменен",
      "success"
    );

    setTimeout(() => {
      router.push("/auth");
    }, 2000);
  } catch {
    setMessage("Ошибка сервера", "error");
  } finally {
    loading.value = false;
  }
};

const goToAuth = () => {
  router.push("/auth");
};
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
  gap: 30px;
  width: 100%;
  align-items: center;
}

.field-group {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 10px;
  width: 100%;
}

h1,
h2,
h3,
h4,
button,
input,
p {
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

/* PASSWORD RULES */

.password-rules {
  width: 589px;
  margin-top: 10px;
}

.password-rules p {
  text-align: left;
  margin: 8px 0;
  color: #ffb3b3;
  transition: 0.3s;
}

.password-rules p.valid {
  color: #7CFF9B;
}

/* ALERT */

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
