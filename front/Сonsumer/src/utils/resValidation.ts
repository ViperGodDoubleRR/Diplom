import {
  getPasswordRules,
  isModeratePassword,
  isValidEmail,
  type PasswordRules,
} from "@/utils/authValidation";
import { RES_ERROR_MESSAGES, resolveErrorMessage } from "@/utils/errorMessages";

export type ResField = "email" | "code" | "password" | "passwordConfirm" | "general";

export interface ResFieldErrors {
  email: string;
  code: string;
  password: string;
  passwordConfirm: string;
  general: string;
}

export const emptyResFieldErrors = (): ResFieldErrors => ({
  email: "",
  code: "",
  password: "",
  passwordConfirm: "",
  general: "",
});

const RES_ERROR_FIELDS: Record<string, ResField> = {
  EMAIL_REQUIRED: "email",
  INVALID_EMAIL: "email",
  EMAIL_SEND_FAILED: "email",
  CODE_REQUIRED: "code",
  INVALID_CODE: "code",
  PASSWORD_REQUIRED: "password",
  INVALID_PASSWORD: "password",
  RESET_TOKEN_REQUIRED: "general",
  INVALID_RESET_TOKEN: "general",
  RESET_PASSWORD_FAILED: "general",
  NETWORK_ERROR: "general",
};

export function applyResError(
  errors: ResFieldErrors,
  code?: string,
  backendMessage?: string
): ResFieldErrors {
  const next = { ...errors };
  const message = resolveErrorMessage(RES_ERROR_MESSAGES, code, backendMessage);

  if (!code) {
    next.general = message;
    return next;
  }

  const field = RES_ERROR_FIELDS[code] ?? "general";
  next[field] = message;

  return next;
}

export interface ResetPasswordRules extends PasswordRules {
  passwordsMatch: boolean;
}

export function getResetPasswordRules(
  password: string,
  passwordConfirm: string
): ResetPasswordRules {
  return {
    ...getPasswordRules(password),
    passwordsMatch: password.length > 0 && password === passwordConfirm,
  };
}

export function isResetPasswordValid(
  password: string,
  passwordConfirm: string
): boolean {
  const rules = getResetPasswordRules(password, passwordConfirm);
  return Object.values(rules).every(Boolean);
}

export function validateResStep1(email: string, code: string): ResFieldErrors {
  const errors = emptyResFieldErrors();

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

export function validateResStep2(
  password: string,
  passwordConfirm: string
): ResFieldErrors {
  const errors = emptyResFieldErrors();

  if (!password) {
    errors.password = "Пароль обязателен";
  } else if (!isModeratePassword(password)) {
    errors.password = "Пароль не соответствует требованиям";
  }

  if (!passwordConfirm) {
    errors.passwordConfirm = "Повторите пароль";
  } else if (password !== passwordConfirm) {
    errors.passwordConfirm = "Пароли не совпадают";
  }

  return errors;
}

export function hasResFieldErrors(errors: ResFieldErrors): boolean {
  return Boolean(
    errors.email ||
      errors.code ||
      errors.password ||
      errors.passwordConfirm ||
      errors.general
  );
}

export { getPasswordRules, isModeratePassword };
