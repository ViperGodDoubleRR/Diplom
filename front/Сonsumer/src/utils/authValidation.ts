import { AUTH_ERROR_MESSAGES, resolveErrorMessage } from "@/utils/errorMessages";

export type AuthField = "email" | "code" | "password" | "general";

export interface AuthFieldErrors {
  email: string;
  code: string;
  password: string;
  general: string;
}

export const emptyAuthFieldErrors = (): AuthFieldErrors => ({
  email: "",
  code: "",
  password: "",
  general: "",
});

const AUTH_ERROR_FIELDS: Record<string, AuthField> = {
  EMAIL_REQUIRED: "email",
  INVALID_EMAIL: "email",
  USER_NOT_FOUND: "email",
  EMAIL_SEND_FAILED: "email",
  CODE_REQUIRED: "code",
  INVALID_CODE: "code",
  PASSWORD_REQUIRED: "password",
  INVALID_PASSWORD: "password",
  NETWORK_ERROR: "general",
};

export function applyAuthError(
  errors: AuthFieldErrors,
  code?: string,
  backendMessage?: string
): AuthFieldErrors {
  const next = { ...errors };
  const message = resolveErrorMessage(AUTH_ERROR_MESSAGES, code, backendMessage);

  if (!code) {
    next.general = message;
    return next;
  }

  const field = AUTH_ERROR_FIELDS[code] ?? "general";
  next[field] = message;

  return next;
}

export function isValidEmail(email: string): boolean {
  const value = email.trim();
  if (!value || value.length > 254) return false;

  return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value);
}

export interface PasswordRules {
  minLength: boolean;
  upperCase: boolean;
  lowerCase: boolean;
  number: boolean;
}

export function getPasswordRules(password: string): PasswordRules {
  return {
    minLength: password.length >= 8,
    upperCase: /[A-Z]/.test(password),
    lowerCase: /[a-z]/.test(password),
    number: /\d/.test(password),
  };
}

export function isModeratePassword(password: string): boolean {
  const rules = getPasswordRules(password);
  return Object.values(rules).every(Boolean);
}

export function validateAuthForm(
  email: string,
  code: string,
  password: string
): AuthFieldErrors {
  const errors = emptyAuthFieldErrors();

  if (!email.trim()) {
    errors.email = "Email обязателен";
  } else if (!isValidEmail(email)) {
    errors.email = "Некорректный формат email";
  }

  if (!code.trim()) {
    errors.code = "Код подтверждения обязателен";
  }

  if (!password) {
    errors.password = "Пароль обязателен";
  }

  return errors;
}

export function hasFieldErrors(errors: AuthFieldErrors): boolean {
  return Boolean(errors.email || errors.code || errors.password || errors.general);
}
