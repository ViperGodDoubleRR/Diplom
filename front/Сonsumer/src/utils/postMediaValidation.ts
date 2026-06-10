export const MAX_POST_IMAGE_MB = 10;
export const MAX_POST_VIDEO_MB = 300;

export const MAX_POST_IMAGE_BYTES = MAX_POST_IMAGE_MB * 1024 * 1024;
export const MAX_POST_VIDEO_BYTES = MAX_POST_VIDEO_MB * 1024 * 1024;

export const POST_IMAGE_ACCEPT = "image/jpeg,image/png,image/webp";
export const POST_VIDEO_ACCEPT = "video/mp4,video/webm,video/quicktime";
export const POST_MEDIA_ACCEPT = `${POST_IMAGE_ACCEPT},${POST_VIDEO_ACCEPT}`;

export function resolvePostMediaType(file: File): "image" | "video" {
  return file.type.startsWith("video") ? "video" : "image";
}

export function validatePostMediaFile(file: File): string | null {
  const isVideo = resolvePostMediaType(file) === "video";
  const maxBytes = isVideo ? MAX_POST_VIDEO_BYTES : MAX_POST_IMAGE_BYTES;
  const maxLabel = isVideo ? `${MAX_POST_VIDEO_MB} МБ` : `${MAX_POST_IMAGE_MB} МБ`;

  if (file.size > maxBytes) {
    return isVideo
      ? `Видео слишком большое (максимум ${maxLabel})`
      : `Фото слишком большое (максимум ${maxLabel})`;
  }

  if (isVideo) {
    const allowed = POST_VIDEO_ACCEPT.split(",");
    if (!allowed.includes(file.type)) {
      return "Недопустимый формат видео (mp4, webm, mov)";
    }
    return null;
  }

  const allowedImages = POST_IMAGE_ACCEPT.split(",");
  if (!allowedImages.includes(file.type)) {
    return "Недопустимый формат фото (jpg, png, webp)";
  }

  return null;
}
