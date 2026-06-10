using Microsoft.AspNetCore.Http;

namespace CommentService.Application.Validation
{
    public static class CommentValidation
    {
        public const int MaxTextLength = 2000;
        public const int MaxPageSize = 100;
        public const long MaxImageBytes = 30L * 1024 * 1024;
        public const long MaxVideoBytes = 30L * 1024 * 1024;
        public const long MaxMediaBytes = 30L * 1024 * 1024;

        private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png",
            "image/webp",
            "image/gif",
            "video/mp4",
            "video/webm",
            "video/quicktime"
        };

        private static readonly HashSet<string> AllowedMediaTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image",
            "video"
        };

        public static (int offset, int limit) NormalizeOffsetPaging(int offset, int limit)
        {
            var normalizedOffset = offset < 0 ? 0 : offset;
            var normalizedLimit = limit < 1 ? 50 : Math.Min(limit, MaxPageSize);
            return (normalizedOffset, normalizedLimit);
        }

        public static bool TryValidateText(
            string? text,
            out string code,
            out string message)
        {
            code = string.Empty;
            message = string.Empty;

            var normalized = text?.Trim() ?? string.Empty;

            if (normalized.Length > MaxTextLength)
            {
                code = "TEXT_TOO_LONG";
                message = $"Комментарий не длиннее {MaxTextLength} символов";
                return false;
            }

            return true;
        }

        public static bool TryValidateMediaUpload(
            IFormFile file,
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
            var maxBytes = MaxMediaBytes;

            if (file.Length > maxBytes)
            {
                code = "FILE_TOO_LARGE";
                message = "Файл слишком большой (максимум 30 МБ)";
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
