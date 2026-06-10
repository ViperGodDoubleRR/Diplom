import { resolveMessage as baseResolve } from "@/utils/profileMessages";

export const SETTINGS_ERROR_MESSAGES: Record<string, string> = {
  INVALID_EMAIL: "Некорректный email",
  SAME_EMAIL: "Это уже ваш текущий email",
  EMAIL_EXISTS: "Email уже используется",
  EMAIL_USED_BEFORE: "Нельзя вернуться на ранее использованный email",
  PASSWORD_REQUIRED: "Введите текущий пароль",
  CURRENT_PASSWORD_REQUIRED: "Введите текущий пароль",
  INVALID_PASSWORD: "Неверный пароль",
  SAME_PASSWORD: "Новый пароль должен отличаться от текущего",
  CODE_REQUIRED: "Введите код подтверждения",
  INVALID_CODE: "Неверный или просроченный код",
  CODE_COOLDOWN: "Подождите 60 секунд перед повторной отправкой",
  EMAIL_SEND_FAILED: "Не удалось отправить код",
  SESSION_NOT_FOUND: "Сессия не найдена",
  NETWORK_ERROR: "Сервер недоступен",
};

export function resolveSettingsMessage(code?: string, fallback?: string): string {
  return baseResolve(SETTINGS_ERROR_MESSAGES, code, fallback);
}
