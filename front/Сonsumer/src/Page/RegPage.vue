<template>
  <div class="content auth-page">
    <figure class="hero">
      <img src="@/assets/image/logo.png" alt="Арт соц сети" class="logo" />
      <figcaption>Достойный арт Социальной сети "GoatBridge"</figcaption>
    </figure>

    <div class="form">
      <h1 class="welcome-title">Registration</h1>

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
              <button
                class="request-btn"
                @click="sendEmail"
                :disabled="loading"
              >
                {{ loading ? "wait..." : "request" }}
              </button>
            </div>
            <p v-if="fieldErrors.code" class="field-error">{{ fieldErrors.code }}</p>
          </div>
        </template>

        <template v-else>
          <div class="field-group">
            <h2>Login</h2>
            <input
              v-model="login"
              type="text"
              placeholder="Your_Nickname"
              class="main-input"
              :class="{ 'input-error': fieldErrors.login }"
              @input="clearFieldError('login')"
            />
            <p v-if="fieldErrors.login" class="field-error">{{ fieldErrors.login }}</p>
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
              <button
                class="eye-btn"
                type="button"
                @click="showPassword = !showPassword"
              >
                <img src="@/assets/image/password.png" alt="view" class="eye-icon" />
              </button>
            </div>
            <p v-if="fieldErrors.password" class="field-error">{{ fieldErrors.password }}</p>
          </div>

          <div class="password-rules">
            <p :class="{ valid: loginValid }">Логин: 3–32 символа, буквы, цифры, _</p>
            <p :class="{ valid: passwordRules.minLength }">Минимум 8 символов</p>
            <p :class="{ valid: passwordRules.upperCase }">Хотя бы одна заглавная буква</p>
            <p :class="{ valid: passwordRules.lowerCase }">Хотя бы одна строчная буква</p>
            <p :class="{ valid: passwordRules.number }">Хотя бы одна цифра</p>
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
        <button class="action-btn cancel" @click="handleCancel" :disabled="loading">
          {{ step === 1 ? "Cancel" : "Back" }}
        </button>

        <button class="action-btn next" @click="handleNext" :disabled="loading">
          {{ loading ? "Loading..." : step === 1 ? "Next" : "Finish" }}
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
import { RegService } from "@/service/regService";
import { apiErrorCode, apiErrorMessage, isApiSuccess } from "@/utils/apiHelpers";
import {
  applyRegError,
  emptyRegFieldErrors,
  getPasswordRules,
  hasRegFieldErrors,
  isModeratePassword,
  isValidLogin,
  validateRegStep1,
  validateRegStep2,
  type RegField,
  type RegFieldErrors,
} from "@/utils/regValidation";
import { SUCCESS } from "@/utils/successMessages";

const router = useRouter();
const regService = new RegService();

const step = ref(1);
const showPassword = ref(false);
const loading = ref(false);

const email = ref("");
const code = ref("");
const login = ref("");
const password = ref("");

const message = ref("");
const messageType = ref<"success" | "error">("success");
const fieldErrors = ref<RegFieldErrors>(emptyRegFieldErrors());

const loginValid = computed(() => isValidLogin(login.value));
const passwordRules = computed(() => getPasswordRules(password.value));

const setMessage = (text: string, type: "success" | "error") => {
  message.value = text;
  messageType.value = type;

  setTimeout(() => {
    message.value = "";
  }, 4000);
};

const clearFieldError = (field: RegField) => {
  fieldErrors.value[field] = "";
  fieldErrors.value.general = "";
};

const resetErrors = () => {
  fieldErrors.value = emptyRegFieldErrors();
};

const sendEmail = async () => {
  resetErrors();

  if (!email.value.trim()) {
    fieldErrors.value.email = "Email обязателен";
    return;
  }

  loading.value = true;

  try {
    const response = await regService.sendEmail(email.value);

    if (!isApiSuccess(response)) {
      fieldErrors.value = applyRegError(
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

const verifyCode = async () => {
  resetErrors();

  const validationErrors = validateRegStep1(email.value, code.value);
  if (hasRegFieldErrors(validationErrors)) {
    fieldErrors.value = validationErrors;
    return;
  }

  loading.value = true;

  try {
    const response = await regService.checkCode(email.value, code.value);

    if (!isApiSuccess(response)) {
      fieldErrors.value = applyRegError(
        fieldErrors.value,
        apiErrorCode(response),
        apiErrorMessage(response, "Неверный код")
      );
      return;
    }

    setMessage(SUCCESS.EMAIL_CONFIRMED, "success");
    setTimeout(() => {
      step.value = 2;
    }, 800);
  } catch {
    fieldErrors.value.general = "Ошибка сервера";
  } finally {
    loading.value = false;
  }
};

const register = async () => {
  resetErrors();

  const validationErrors = validateRegStep2(login.value, password.value);
  if (hasRegFieldErrors(validationErrors)) {
    fieldErrors.value = validationErrors;
    return;
  }

  if (!isModeratePassword(password.value)) {
    fieldErrors.value.password = "Пароль не соответствует требованиям";
    return;
  }

  loading.value = true;

  try {
    const response = await regService.registerUser(
      email.value,
      login.value,
      password.value
    );

    if (!isApiSuccess(response)) {
      fieldErrors.value = applyRegError(
        fieldErrors.value,
        apiErrorCode(response),
        apiErrorMessage(response, "Не удалось зарегистрироваться")
      );
      return;
    }

    setMessage(SUCCESS.REGISTRATION_COMPLETE, "success");

    setTimeout(() => {
      router.push("/auth");
    }, 1500);
  } catch {
    fieldErrors.value.general = "Ошибка сервера";
  } finally {
    loading.value = false;
  }
};

const handleNext = async () => {
  if (step.value === 1) {
    await verifyCode();
  } else {
    await register();
  }
};

const handleCancel = () => {
  if (step.value === 2) {
    step.value = 1;
    login.value = "";
    password.value = "";
    resetErrors();
    message.value = "";
    return;
  }

  email.value = "";
  code.value = "";
  resetErrors();
  message.value = "";
};

const goToAuth = () => router.push("/auth");
</script>

<style scoped>
@import "@/assets/styles/auth-pages.css";
</style>
