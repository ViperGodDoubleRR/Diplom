/** Понятные сообщения для пользователя по коду ошибки с бэка */

export const AUTH_ERROR_MESSAGES: Record<string, string> = {
  EMAIL_REQUIRED: "Укажите email",
  INVALID_EMAIL: "Введите корректный email, например user@gmail.com",
  USER_NOT_FOUND: "Аккаунт с таким email не найден. Проверьте адрес или зарегистрируйтесь",
  EMAIL_SEND_FAILED: "Не удалось отправить код на email. Попробуйте позже",
  CODE_REQUIRED: "Введите код из письма",
  INVALID_CODE: "Код неверный или истёк. Нажмите Request и запросите новый",
  PASSWORD_REQUIRED: "Введите пароль",
  INVALID_PASSWORD: "Неверный пароль",
  INVALID_CREDENTIALS: "Неверный email или пароль",
  NETWORK_ERROR: "Не удалось связаться с сервером. Проверьте, что бэкенд запущен",
};

export const REG_ERROR_MESSAGES: Record<string, string> = {
  EMAIL_REQUIRED: "Укажите email",
  INVALID_EMAIL: "Введите корректный email, например user@gmail.com",
  EMAIL_BUSY: "Этот email уже зарегистрирован. Войдите или используйте другой",
  EMAIL_EXISTS: "Этот email уже зарегистрирован",
  EMAIL_SEND_FAILED: "Не удалось отправить код на email. Попробуйте позже",
  CODE_REQUIRED: "Введите код из письма",
  INVALID_CODE: "Код неверный или истёк. Запросите новый через request",
  EMAIL_NOT_CONFIRMED: "Сначала подтвердите email — введите код и нажмите Next",
  INVALID_LOGIN: "Логин: от 3 до 32 символов, только буквы, цифры и _",
  LOGIN_EXISTS: "Этот логин уже занят. Выберите другой",
  PASSWORD_REQUIRED: "Придумайте пароль",
  INVALID_PASSWORD: "Пароль слишком простой — выполните все требования ниже",
  AUTH_SERVICE_UNAVAILABLE: "Сервис авторизации недоступен. Попробуйте позже",
  REGISTRATION_FAILED: "Не удалось завершить регистрацию. Попробуйте снова",
  CREATE_USER_FAILED: "Не удалось создать аккаунт",
  NETWORK_ERROR: "Не удалось связаться с сервером. Проверьте, что бэкенд запущен",
};

export const RES_ERROR_MESSAGES: Record<string, string> = {
  EMAIL_REQUIRED: "Укажите email",
  INVALID_EMAIL: "Введите корректный email, например user@gmail.com",
  EMAIL_SEND_FAILED: "Не удалось отправить код на email. Попробуйте позже",
  CODE_REQUIRED: "Введите код из письма",
  INVALID_CODE: "Код неверный или истёк. Запросите новый через Request",
  PASSWORD_REQUIRED: "Введите новый пароль",
  INVALID_PASSWORD: "Пароль слишком простой — выполните все требования ниже",
  RESET_TOKEN_REQUIRED: "Сессия сброса истекла. Подтвердите код заново",
  INVALID_RESET_TOKEN: "Сессия сброса истекла. Вернитесь назад и запросите код снова",
  RESET_PASSWORD_FAILED: "Не удалось сменить пароль. Попробуйте ещё раз",
  NETWORK_ERROR: "Не удалось связаться с сервером. Проверьте, что бэкенд запущен",
};

export function resolveErrorMessage(
  map: Record<string, string>,
  code?: string,
  backendMessage?: string
): string {
  if (code && map[code]) {
    return map[code];
  }

  if (backendMessage?.trim()) {
    return backendMessage;
  }

  return "Что-то пошло не так. Попробуйте ещё раз";
}
