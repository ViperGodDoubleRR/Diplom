namespace PostService.Application.Validation
{
    public static class PostValidation
    {
        public const int MaxDescriptionLength = 2000;
        public const int MaxPageSize = 50;
        public const long MaxImageBytes = 10 * 1024 * 1024;
        public const long MaxVideoBytes = 300L * 1024 * 1024;

        private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png",
            "image/webp",
            "video/mp4",
            "video/webm",
            "video/quicktime"
        };

        private static readonly HashSet<string> AllowedMediaTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image",
            "video"
        };

        public static (int page, int pageSize) NormalizePaging(int page, int pageSize)
        {
            var normalizedPage = page < 1 ? 1 : page;
            var normalizedSize = pageSize < 1 ? 12 : Math.Min(pageSize, MaxPageSize);
            return (normalizedPage, normalizedSize);
        }

        public static bool TryValidateDescription(
            string? description,
            out string code,
            out string message)
        {
            code = string.Empty;
            message = string.Empty;

            if (string.IsNullOrWhiteSpace(description))
            {
                code = "DESCRIPTION_REQUIRED";
                message = "Описание поста обязательно";
                return false;
            }

            if (description.Trim().Length > MaxDescriptionLength)
            {
                code = "DESCRIPTION_TOO_LONG";
                message = $"Описание не длиннее {MaxDescriptionLength} символов";
                return false;
            }

            return true;
        }

        public static bool TryValidateMediaUpload(
            Microsoft.AspNetCore.Http.IFormFile file,
            string? mediaType,
            out string code,
            out string message)
        {
            code = string.Empty;
            message = string.Empty;

            if (file.Length == 0)
            {
                code = "EMPTY_FILE";
                message = "Файл пустой";
                return false;
            }

            var isVideo = string.Equals(mediaType, "video", StringComparison.OrdinalIgnoreCase);
            var maxBytes = isVideo ? MaxVideoBytes : MaxImageBytes;

            if (file.Length > maxBytes)
            {
                code = "FILE_TOO_LARGE";
                message = isVideo
                    ? "Видео слишком большое (максимум 300 МБ)"
                    : "Фото слишком большое (максимум 10 МБ)";
                return false;
            }

            if (!AllowedContentTypes.Contains(file.ContentType))
            {
                code = "INVALID_FILE_TYPE";
                message = "Недопустимый тип файла";
                return false;
            }

            if (string.IsNullOrWhiteSpace(mediaType) || !AllowedMediaTypes.Contains(mediaType))
            {
                code = "INVALID_MEDIA_TYPE";
                message = "Недопустимый тип медиа";
                return false;
            }

            return true;
        }
    }
}
