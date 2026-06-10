using Microsoft.AspNetCore.Http;

namespace UserService.Application.Validation
{
    public static class MediaValidation
    {
        public const long MaxImageSizeBytes = 5 * 1024 * 1024;
        public const long MaxVideoSizeBytes = 30 * 1024 * 1024;

        private static readonly HashSet<string> AllowedImageContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png",
            "image/webp"
        };

        private static readonly HashSet<string> AllowedVideoContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "video/mp4",
            "video/webm",
            "video/quicktime"
        };

        private static readonly HashSet<string> AllowedMediaTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "avatar",
            "video",
            "gallery"
        };

        public static bool TryValidateUpload(
            IFormFile? file,
            string? mediaType,
            out string code,
            out string message)
        {
            code = string.Empty;
            message = string.Empty;

            if (file is null || file.Length == 0)
            {
                code = "EMPTY_FILE";
                message = "Файл пустой";
                return false;
            }

            if (string.IsNullOrWhiteSpace(mediaType) ||
                !AllowedMediaTypes.Contains(mediaType))
            {
                code = "INVALID_MEDIA_TYPE";
                message = "Недопустимый тип медиа";
                return false;
            }

            if (mediaType.Equals("avatar", StringComparison.OrdinalIgnoreCase) ||
                mediaType.Equals("gallery", StringComparison.OrdinalIgnoreCase))
            {
                if (file.Length > MaxImageSizeBytes)
                {
                    code = "FILE_TOO_LARGE";
                    message = "Фото слишком большое (максимум 5 МБ)";
                    return false;
                }

                if (!AllowedImageContentTypes.Contains(file.ContentType))
                {
                    code = "INVALID_FILE_TYPE";
                    message = "Для фото допустимы JPEG, PNG и WebP";
                    return false;
                }

                return true;
            }

            if (file.Length > MaxVideoSizeBytes)
            {
                code = "FILE_TOO_LARGE";
                message = "Видео слишком большое (максимум 30 МБ)";
                return false;
            }

            if (!AllowedVideoContentTypes.Contains(file.ContentType))
            {
                code = "INVALID_FILE_TYPE";
                message = "Для видео допустимы MP4, WebM и MOV";
                return false;
            }

            return true;
        }

        public static bool IsProfileDisplayMedia(string? mediaType) =>
            mediaType != null &&
            (mediaType.Equals("avatar", StringComparison.OrdinalIgnoreCase) ||
             mediaType.Equals("video", StringComparison.OrdinalIgnoreCase));
    }
}
