export const SOCIAL_ERROR_MESSAGES: Record<string, string> = {
  SELF_FRIEND: "Нельзя добавить себя в друзья",
  USER_NOT_FOUND: "Пользователь не найден",
  ALREADY_FRIEND: "Пользователь уже в друзьях",
  USER_BLOCKED: "Невозможно добавить этого пользователя",
  FRIEND_NOT_FOUND: "Пользователь не найден в друзьях",
  SELF_BLOCK: "Нельзя заблокировать себя",
  ALREADY_BLOCKED: "Пользователь уже заблокирован",
  BLOCK_NOT_FOUND: "Пользователь не найден в чёрном списке",
  INVALID_NICKNAME: "Имя для друга: от 1 до 32 символов",
  SEARCH_TOO_LONG: "Слишком длинный поисковый запрос",
  NETWORK_ERROR: "Не удалось связаться с сервером",
};

export const PROFILE_ERROR_MESSAGES: Record<string, string> = {
  USER_NOT_FOUND: "Пользователь не найден",
  VALIDATION_ERROR: "Проверьте данные профиля",
  LOGIN_TAKEN: "Этот логин уже занят",
  EMPTY_FILE: "Файл пустой",
  FILE_TOO_LARGE: "Файл слишком большой (максимум 5 МБ)",
  INVALID_FILE_TYPE: "Для фото — JPEG, PNG, WebP. Для видео — MP4, WebM, MOV",
  INVALID_MEDIA_TYPE: "Недопустимый тип медиа",
  MEDIA_NOT_FOUND: "Медиа не найдено",
  NETWORK_ERROR: "Не удалось связаться с сервером",
};

export function resolveMessage(
  map: Record<string, string>,
  code?: string,
  fallback?: string
): string {
  if (code && map[code]) return map[code];
  return fallback ?? "Произошла ошибка";
}
