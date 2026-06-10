<template>
  <div class="content auth-page">
    <figure class="hero">
      <img src="@/assets/image/logo.png" alt="Арт соц сети" class="logo" />

      <figcaption>
        Достойный арт Социальной сети "GoatBridge"
      </figcaption>
    </figure>

    <div class="form">
      <h1 class="welcome-title">Welcome</h1>

      <div class="fields-container">
        <div class="field-group">
          <h2>Email</h2>

          <input
            v-model="email"
            type="email"
            placeholder="example@gmail.com"
            class="main-input"
            :class="{ 'input-error': fieldErrors.email }"
            @input="clearFieldError('email')"
          />

          <p v-if="fieldErrors.email" class="field-error">{{ fieldErrors.email }}</p>
        </div>

        <div class="field-group">
          <h2>Verification Code</h2>

          <div class="input-wrapper">
            <input
              v-model="code"
              placeholder="H89AKSDS"
              class="verification-input"
              :class="{ 'input-error': fieldErrors.code }"
              @input="clearFieldError('code')"
            />

            <button class="request-btn" @click="requestCode" :disabled="loading">
              {{ loading ? "wait..." : "request" }}
            </button>
          </div>

          <p v-if="fieldErrors.code" class="field-error">{{ fieldErrors.code }}</p>
        </div>

        <div class="field-group">
          <h2>Password</h2>

          <div class="input-wrapper">
            <input
              v-model="password"
              :type="showPassword ? 'text' : 'password'"
              placeholder="********"
              class="verification-input"
              :class="{ 'input-error': fieldErrors.password }"
              @input="clearFieldError('password')"
            />

            <button class="eye-btn" @click="showPassword = !showPassword" type="button">
              <img src="@/assets/image/password.png" alt="view" class="eye-icon" />
            </button>
          </div>

          <p v-if="fieldErrors.password" class="field-error">{{ fieldErrors.password }}</p>

          <div v-if="password.length > 0" class="password-rules">
            <p :class="{ valid: passwordRules.minLength }">Минимум 8 символов</p>
            <p :class="{ valid: passwordRules.upperCase }">Хотя бы одна заглавная буква</p>
            <p :class="{ valid: passwordRules.lowerCase }">Хотя бы одна строчная буква</p>
            <p :class="{ valid: passwordRules.number }">Хотя бы одна цифра</p>
          </div>
        </div>
      </div>

      <transition name="fade">
        <div
          v-if="message"
          :class="[
            'message-box',
            messageType === 'success' ? 'message-success' : 'message-error',
          ]"
        >
          {{ message }}
        </div>
      </transition>

      <p v-if="fieldErrors.general" class="field-error general-error">
        {{ fieldErrors.general }}
      </p>

      <div class="buttonmenu">
        <button class="action-btn cancel" @click="clearFields" :disabled="loading">
          Cancel
        </button>

        <button class="action-btn next" @click="login" :disabled="loading">
          {{ loading ? "Loading..." : "Login" }}
        </button>
      </div>

      <div class="auth-footer">
        <div class="footer-row">
          <h4>No Have account?</h4>
          <h3 @click="goToRegistration">Click</h3>
        </div>

        <div class="footer-row">
          <h4>Reset password?</h4>
          <h3 @click="goToResetPassword">Click</h3>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from "vue";
import { useRouter } from "vue-router";
import { AuthService } from "@/service/authService";
import type { AuthGo } from "@/interface/DTO/AuthGo";
import {
  applyAuthError,
  emptyAuthFieldErrors,
  getPasswordRules,
  hasFieldErrors,
  validateAuthForm,
  type AuthField,
  type AuthFieldErrors,
} from "@/utils/authValidation";
import {
  apiErrorCode,
  apiErrorMessage,
  getApiData,
  isApiSuccess,
  readAuthTokens,
} from "@/utils/apiHelpers";
import { SUCCESS } from "@/utils/successMessages";

const router = useRouter();
const service = new AuthService();

const email = ref("");
const code = ref("");
const password = ref("");

const showPassword = ref(false);
const loading = ref(false);

const message = ref("");
const messageType = ref<"success" | "error">("success");
const fieldErrors = ref<AuthFieldErrors>(emptyAuthFieldErrors());

const passwordRules = computed(() => getPasswordRules(password.value));

const setMessage = (text: string, type: "success" | "error") => {
  message.value = text;
  messageType.value = type;

  setTimeout(() => {
    message.value = "";
  }, 4000);
};

const clearFieldError = (field: AuthField) => {
  fieldErrors.value[field] = "";
  fieldErrors.value.general = "";
};

const resetErrors = () => {
  fieldErrors.value = emptyAuthFieldErrors();
};

const requestCode = async () => {
  resetErrors();

  if (!email.value.trim()) {
    fieldErrors.value.email = "Email обязателен";
    return;
  }

  loading.value = true;

  try {
    const res = await service.requestCode(email.value);

    if (!isApiSuccess(res)) {
      fieldErrors.value = applyAuthError(
        fieldErrors.value,
        apiErrorCode(res),
        apiErrorMessage(res, "Не удалось отправить код")
      );
      return;
    }

    setMessage(SUCCESS.CODE_SENT, "success");
  } catch {
    fieldErrors.value.general = "Ошибка сервера";
  } finally {
    loading.value = false;
  }
};

const login = async () => {
  resetErrors();

  const validationErrors = validateAuthForm(
    email.value,
    code.value,
    password.value
  );

  if (hasFieldErrors(validationErrors)) {
    fieldErrors.value = validationErrors;
    return;
  }

  loading.value = true;

  try {
    const payload: AuthGo = {
      email: email.value,
      password: password.value,
      code: code.value,
      deviceInfo: navigator.userAgent,
    };

    const res = await service.authGo(payload);

    const tokens = readAuthTokens(getApiData(res));

    if (!isApiSuccess(res) || !tokens) {
      fieldErrors.value = applyAuthError(
        fieldErrors.value,
        apiErrorCode(res),
        apiErrorMessage(res, "Ошибка авторизации")
      );
      return;
    }

    localStorage.setItem("accessToken", tokens.accessToken);
    localStorage.setItem("refreshToken", tokens.refreshToken);

    setMessage(SUCCESS.LOGIN, "success");

    setTimeout(() => {
      router.push("/");
    }, 1000);
  } catch {
    fieldErrors.value.general = "Ошибка сервера";
  } finally {
    loading.value = false;
  }
};

const clearFields = () => {
  email.value = "";
  code.value = "";
  password.value = "";
  resetErrors();
  message.value = "";
};

const goToRegistration = () => router.push("/reg");
const goToResetPassword = () => router.push("/res");
</script>

<style scoped>
@import "@/assets/styles/auth-pages.css";
</style>
