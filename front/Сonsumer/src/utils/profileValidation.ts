const LOGIN_REGEX = /^[a-zA-Z0-9_]{3,32}$/;
const TAG_REGEX = /^[a-zA-Z0-9_]{1,32}$/;

export const MAX_PROFILE_IMAGE_BYTES = 5 * 1024 * 1024;
export const MAX_PROFILE_VIDEO_BYTES = 30 * 1024 * 1024;
export const MAX_PROFILE_VIDEO_MB = 30;
export const MAX_PROFILE_VIDEO_SECONDS = 60;

const IMAGE_TYPES = ["image/jpeg", "image/png", "image/webp"];
const VIDEO_TYPES = ["video/mp4", "video/webm", "video/quicktime"];

export function validateProfileLogin(login: string): string | null {
  const value = login.trim();

  if (!value) return "Логин обязателен";
  if (!LOGIN_REGEX.test(value)) {
    return "Логин: 3–32 символа, только буквы, цифры и _";
  }

  return null;
}

export function validateProfileTag(tag: string): string | null {
  const value = tag.trim();

  if (!value) return null;

  if (!TAG_REGEX.test(value)) {
    return "Тег: 1–32 символа, только буквы, цифры и _";
  }

  return null;
}

export function validateFriendNickname(login: string): string | null {
  const value = login.trim();

  if (!value) return "Введите имя для друга";
  if (value.length > 32) return "Не больше 32 символов";

  return null;
}

export function normalizeMediaType(value?: string): string {
  return (value ?? "").toLowerCase();
}

export function isProfileDisplayMedia(mediaType?: string): boolean {
  const type = normalizeMediaType(mediaType);
  return type === "avatar" || type === "video";
}

export function filterProfileMedia<T extends { mediaType?: string }>(media: T[] = []): T[] {
  return media.filter((item) => isProfileDisplayMedia(item.mediaType));
}

function mediaTime(item: { createdAt?: string; id?: number }): number {
  if (item.createdAt) {
    return new Date(item.createdAt).getTime();
  }

  return item.id ?? 0;
}

/** Порядок загрузки: старые → новые (фото, видео, видео, фото…) */
export function sortProfileMediaChronological<T extends { mediaType?: string; createdAt?: string; id?: number }>(
  media: T[] = []
): T[] {
  return [...filterProfileMedia(media)].sort(
    (a, b) => mediaTime(a) - mediaTime(b)
  );
}

export type ProfileMediaPreview = {
  url: string | null;
  isVideo: boolean;
};

/** Для кружка профиля — последнее загруженное (фото или видео) */
export function pickProfileMedia(
  media: Array<{ mediaType?: string; url?: string; createdAt?: string; id?: number }> | undefined
): ProfileMediaPreview {
  const sorted = sortProfileMediaChronological(media ?? []);

  if (!sorted.length) {
    return { url: null, isVideo: false };
  }

  const latest = sorted[sorted.length - 1]!;

  return {
    url: latest.url ?? null,
    isVideo: normalizeMediaType(latest.mediaType) === "video",
  };
}

/** Для <img> (друзья, сайдбар) — последнее фото-аватар */
export function pickAvatarPhotoUrl(
  media: Array<{ mediaType?: string; url?: string; createdAt?: string; id?: number }> | undefined
): string | null {
  const photos = sortProfileMediaChronological(media ?? []).filter(
    (m) => normalizeMediaType(m.mediaType) === "avatar"
  );

  if (!photos.length) {
    return null;
  }

  return photos[photos.length - 1]?.url ?? null;
}

export function pickAvatarUrl(
  media: Array<{ mediaType?: string; url?: string; createdAt?: string; id?: number }> | undefined
): string | null {
  return pickAvatarPhotoUrl(media);
}

export function validateProfileImage(file: File): string | null {
  if (!IMAGE_TYPES.includes(file.type)) {
    return "Для фото допустимы JPEG, PNG и WebP";
  }

  if (file.size > MAX_PROFILE_IMAGE_BYTES) {
    return "Фото слишком большое (максимум 5 МБ)";
  }

  return null;
}

function getVideoDuration(file: File): Promise<number> {
  return new Promise((resolve, reject) => {
    const video = document.createElement("video");
    video.preload = "metadata";

    video.onloadedmetadata = () => {
      URL.revokeObjectURL(video.src);
      resolve(video.duration);
    };

    video.onerror = () => {
      URL.revokeObjectURL(video.src);
      reject(new Error("INVALID_VIDEO"));
    };

    video.src = URL.createObjectURL(file);
  });
}

export async function validateProfileVideo(file: File): Promise<string | null> {
  if (!VIDEO_TYPES.includes(file.type)) {
    return "Для видео допустимы MP4, WebM и MOV";
  }

  if (file.size > MAX_PROFILE_VIDEO_BYTES) {
    return "Видео слишком большое (максимум 30 МБ)";
  }

  try {
    const duration = await getVideoDuration(file);

    if (!Number.isFinite(duration) || duration <= 0) {
      return "Не удалось прочитать видео";
    }

    if (duration > MAX_PROFILE_VIDEO_SECONDS) {
      return `Видео не длиннее ${MAX_PROFILE_VIDEO_SECONDS} секунд`;
    }
  } catch {
    return "Не удалось прочитать видео";
  }

  return null;
}
