<template>
  <div class="content auth-page">
    <figure class="hero">
      <img src="@/assets/image/logo.png" alt="GoatBridge" class="logo" />
      <figcaption>
        Достойный арт Социальной сети "GoatBridge"
      </figcaption>
    </figure>

    <div class="form">
      <h1 class="welcome-title">Reset Password</h1>

      <div class="fields-container">
        <template v-if="step === 1">
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
                {{ loading ? "wait..." : "Request" }}
              </button>
            </div>

            <p v-if="fieldErrors.code" class="field-error">{{ fieldErrors.code }}</p>
          </div>
        </template>

        <template v-else>
          <div class="field-group">
            <h2>New Password</h2>

            <div class="input-wrapper">
              <input
                v-model="password1"
                :type="showPass1 ? 'text' : 'password'"
                placeholder="********"
                class="verification-input"
                :class="{ 'input-error': fieldErrors.password }"
                @input="clearFieldError('password')"
              />

              <button class="eye-btn" type="button" @click="showPass1 = !showPass1">
                <img src="@/assets/image/password.png" alt="view" class="eye-icon" />
              </button>
            </div>

            <p v-if="fieldErrors.password" class="field-error">{{ fieldErrors.password }}</p>
          </div>

          <div class="field-group">
            <h2>Repeat Password</h2>

            <div class="input-wrapper">
              <input
                v-model="password2"
                :type="showPass2 ? 'text' : 'password'"
                placeholder="********"
                class="verification-input"
                :class="{ 'input-error': fieldErrors.passwordConfirm }"
                @input="clearFieldError('passwordConfirm')"
              />

              <button class="eye-btn" type="button" @click="showPass2 = !showPass2">
                <img src="@/assets/image/password.png" alt="view" class="eye-icon" />
              </button>
            </div>

            <p v-if="fieldErrors.passwordConfirm" class="field-error">
              {{ fieldErrors.passwordConfirm }}
            </p>
          </div>

          <div class="password-rules">
            <p :class="{ valid: passwordRules.minLength }">Минимум 8 символов</p>
            <p :class="{ valid: passwordRules.upperCase }">Хотя бы одна заглавная буква</p>
            <p :class="{ valid: passwordRules.lowerCase }">Хотя бы одна строчная буква</p>
            <p :class="{ valid: passwordRules.number }">Хотя бы одна цифра</p>
            <p :class="{ valid: passwordRules.passwordsMatch }">Пароли совпадают</p>
          </div>
        </template>
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
        <button class="action-btn cancel" @click="resetForm" :disabled="loading">
          {{ step === 1 ? "Cancel" : "Back" }}
        </button>

        <button
          v-if="step === 1"
          class="action-btn next"
          @click="nextStep"
          :disabled="loading"
        >
          {{ loading ? "Loading..." : "Next" }}
        </button>

        <button
          v-else
          class="action-btn next"
          @click="changePassword"
          :disabled="loading"
        >
          {{ loading ? "Loading..." : "Reset" }}
        </button>
      </div>

      <div class="auth-footer">
        <div class="footer-row">
          <h4>Have account?</h4>
          <h3 @click="goToAuth">Click Authentification</h3>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from "vue";
import { useRouter } from "vue-router";
import { ResService } from "@/service/resService";
import {
  applyResError,
  emptyResFieldErrors,
  getResetPasswordRules,
  hasResFieldErrors,
  isResetPasswordValid,
  validateResStep1,
  validateResStep2,
  type ResField,
  type ResFieldErrors,
} from "@/utils/resValidation";
import {
  apiErrorCode,
  apiErrorMessage,
  getApiData,
  isApiSuccess,
} from "@/utils/apiHelpers";
import { SUCCESS } from "@/utils/successMessages";

const router = useRouter();
const service = new ResService();

const step = ref(1);

const email = ref("");
const code = ref("");
const resetToken = ref("");

const password1 = ref("");
const password2 = ref("");

const showPass1 = ref(false);
const showPass2 = ref(false);
const loading = ref(false);

const message = ref("");
const messageType = ref<"success" | "error">("success");
const fieldErrors = ref<ResFieldErrors>(emptyResFieldErrors());

const passwordRules = computed(() =>
  getResetPasswordRules(password1.value, password2.value)
);

const setMessage = (text: string, type: "success" | "error") => {
  message.value = text;
  messageType.value = type;

  setTimeout(() => {
    message.value = "";
  }, 4000);
};

const clearFieldError = (field: ResField) => {
  fieldErrors.value[field] = "";
  fieldErrors.value.general = "";
};

const resetErrors = () => {
  fieldErrors.value = emptyResFieldErrors();
};

const requestCode = async () => {
  resetErrors();

  if (!email.value.trim()) {
    fieldErrors.value.email = "Email обязателен";
    return;
  }

  loading.value = true;

  try {
    const response = await service.requestCode(email.value);

    if (!isApiSuccess(response)) {
      fieldErrors.value = applyResError(
        fieldErrors.value,
        apiErrorCode(response),
        apiErrorMessage(response, "Не удалось отправить код")
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

const nextStep = async () => {
  resetErrors();

  const validationErrors = validateResStep1(email.value, code.value);
  if (hasResFieldErrors(validationErrors)) {
    fieldErrors.value = validationErrors;
    return;
  }

  loading.value = true;

  try {
    const response = await service.checkCode(email.value, code.value);

    const token = getApiData(response);

    if (!isApiSuccess(response) || !token) {
      fieldErrors.value = applyResError(
        fieldErrors.value,
        apiErrorCode(response),
        apiErrorMessage(response, "Неверный код")
      );
      return;
    }

    resetToken.value = token;

    setMessage(SUCCESS.RESET_CODE_VERIFIED, "success");

    setTimeout(() => {
      step.value = 2;
    }, 800);
  } catch {
    fieldErrors.value.general = "Ошибка сервера";
  } finally {
    loading.value = false;
  }
};

const changePassword = async () => {
  resetErrors();

  if (!resetToken.value) {
    fieldErrors.value.general = "Сначала подтвердите код";
    step.value = 1;
    return;
  }

  const validationErrors = validateResStep2(password1.value, password2.value);
  if (hasResFieldErrors(validationErrors)) {
    fieldErrors.value = validationErrors;
    return;
  }

  if (!isResetPasswordValid(password1.value, password2.value)) {
    fieldErrors.value.password = "Пароль не соответствует требованиям";
    return;
  }

  loading.value = true;

  try {
    const response = await service.changePassword(
      email.value,
      password1.value,
      resetToken.value
    );

    if (!isApiSuccess(response)) {
      fieldErrors.value = applyResError(
        fieldErrors.value,
        apiErrorCode(response),
        apiErrorMessage(response, "Не удалось сменить пароль")
      );

      if (apiErrorCode(response) === "INVALID_RESET_TOKEN") {
        step.value = 1;
        resetToken.value = "";
      }

      return;
    }

    setMessage(SUCCESS.PASSWORD_CHANGED, "success");

    setTimeout(() => {
      router.push("/auth");
    }, 1500);
  } catch {
    fieldErrors.value.general = "Ошибка сервера";
  } finally {
    loading.value = false;
  }
};

const goToAuth = () => router.push("/auth");

const resetForm = () => {
  if (step.value === 2) {
    step.value = 1;
    password1.value = "";
    password2.value = "";
    resetToken.value = "";
    resetErrors();
    message.value = "";
    return;
  }

  email.value = "";
  code.value = "";
  resetToken.value = "";
  password1.value = "";
  password2.value = "";
  resetErrors();
  message.value = "";
};
</script>

<style scoped>
@import "@/assets/styles/auth-pages.css";
</style>
