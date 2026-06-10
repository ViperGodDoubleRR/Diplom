import {
  getPasswordRules,
  isModeratePassword,
  isValidEmail,
  type PasswordRules,
} from "@/utils/authValidation";
import { REG_ERROR_MESSAGES, resolveErrorMessage } from "@/utils/errorMessages";

export type RegField = "email" | "code" | "login" | "password" | "general";

export interface RegFieldErrors {
  email: string;
  code: string;
  login: string;
  password: string;
  general: string;
}

export const emptyRegFieldErrors = (): RegFieldErrors => ({
  email: "",
  code: "",
  login: "",
  password: "",
  general: "",
});

const REG_ERROR_FIELDS: Record<string, RegField> = {
  EMAIL_REQUIRED: "email",
  INVALID_EMAIL: "email",
  EMAIL_BUSY: "email",
  EMAIL_SEND_FAILED: "email",
  CODE_REQUIRED: "code",
  INVALID_CODE: "code",
  EMAIL_NOT_CONFIRMED: "email",
  INVALID_LOGIN: "login",
  LOGIN_EXISTS: "login",
  INVALID_PASSWORD: "password",
  PASSWORD_REQUIRED: "password",
  EMAIL_EXISTS: "email",
  AUTH_SERVICE_UNAVAILABLE: "general",
  REGISTRATION_FAILED: "general",
  CREATE_USER_FAILED: "general",
  NETWORK_ERROR: "general",
};

export function applyRegError(
  errors: RegFieldErrors,
  code?: string,
  backendMessage?: string
): RegFieldErrors {
  const next = { ...errors };
  const message = resolveErrorMessage(REG_ERROR_MESSAGES, code, backendMessage);

  if (!code) {
    next.general = message;
    return next;
  }

  const field = REG_ERROR_FIELDS[code] ?? "general";
  next[field] = message;

  return next;
}

export function isValidLogin(login: string): boolean {
  return /^[a-zA-Z0-9_]{3,32}$/.test(login.trim());
}

export function validateRegStep1(email: string, code: string): RegFieldErrors {
  const errors = emptyRegFieldErrors();

  if (!email.trim()) {
    errors.email = "Email обязателен";
  } else if (!isValidEmail(email)) {
    errors.email = "Некорректный формат email";
  }

  if (!code.trim()) {
    errors.code = "Код подтверждения обязателен";
  }

  return errors;
}

export function validateRegStep2(login: string, password: string): RegFieldErrors {
  const errors = emptyRegFieldErrors();

  if (!login.trim()) {
    errors.login = "Логин обязателен";
  } else if (!isValidLogin(login)) {
    errors.login = "Логин: 3–32 символа, только буквы, цифры и _";
  }

  if (!password) {
    errors.password = "Пароль обязателен";
  } else if (!isModeratePassword(password)) {
    errors.password = "Пароль не соответствует требованиям";
  }

  return errors;
}

export function hasRegFieldErrors(errors: RegFieldErrors): boolean {
  return Boolean(
    errors.email || errors.code || errors.login || errors.password || errors.general
  );
}

export { getPasswordRules, isModeratePassword, type PasswordRules };
