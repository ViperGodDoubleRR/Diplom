using Microsoft.AspNetCore.Http;

namespace ChatService.Application.Validation
{
    public static class ChatValidation
    {
        public static bool TryValidateGroupName(
            string? name,
            out string code,
            out string message)
        {
            code = string.Empty;
            message = string.Empty;

            var trimmed = name?.Trim() ?? string.Empty;

            if (trimmed.Length < 2)
            {
                code = "INVALID_NAME";
                message = "Название группы — минимум 2 символа";
                return false;
            }

            if (trimmed.Length > Constants.ChatConstants.MaxGroupNameLength)
            {
                code = "INVALID_NAME";
                message = "Название группы слишком длинное";
                return false;
            }

            return true;
        }

        public static bool TryValidateMessageText(
            string? text,
            bool allowEmptyWithMedia,
            out string code,
            out string message)
        {
            code = string.Empty;
            message = string.Empty;

            var trimmed = text?.Trim() ?? string.Empty;

            if (trimmed.Length == 0 && !allowEmptyWithMedia)
            {
                code = "EMPTY_MESSAGE";
                message = "Сообщение не может быть пустым";
                return false;
            }

            if (trimmed.Length > Constants.ChatConstants.MaxMessageLength)
            {
                code = "MESSAGE_TOO_LONG";
                message = "Сообщение слишком длинное";
                return false;
            }

            return true;
        }

        public static bool TryValidateMediaUpload(
            IFormFile file,
            string mediaType,
            out string code,
            out string message)
        {
            code = string.Empty;
            message = string.Empty;

            if (file.Length <= 0)
            {
                code = "EMPTY_FILE";
                message = "Файл пустой";
                return false;
            }

            if (file.Length > Constants.ChatConstants.MaxMediaBytes)
            {
                code = "FILE_TOO_LARGE";
                message = "Файл не больше 300 МБ";
                return false;
            }

            var normalized = mediaType.Trim().ToLowerInvariant();

            if (normalized is not ("image" or "video"))
            {
                code = "INVALID_MEDIA_TYPE";
                message = "Допустимы только image и video";
                return false;
            }

            var contentType = file.ContentType.ToLowerInvariant();

            if (normalized == "image" &&
                !contentType.StartsWith("image/") &&
                contentType != "application/octet-stream")
            {
                code = "INVALID_CONTENT_TYPE";
                message = "Неверный тип изображения";
                return false;
            }

            if (normalized == "video" &&
                !contentType.StartsWith("video/") &&
                contentType != "application/octet-stream")
            {
                code = "INVALID_CONTENT_TYPE";
                message = "Неверный тип видео";
                return false;
            }

            return true;
        }

        public static bool TryValidateGalleryImage(
            IFormFile file,
            out string code,
            out string message)
        {
            code = string.Empty;
            message = string.Empty;

            if (file.Length <= 0)
            {
                code = "EMPTY_FILE";
                message = "Файл пустой";
                return false;
            }

            if (file.Length > Constants.ChatConstants.MaxGalleryImageBytes)
            {
                code = "FILE_TOO_LARGE";
                message = "Фото не больше 5 МБ";
                return false;
            }

            var contentType = file.ContentType.ToLowerInvariant();
            if (!contentType.StartsWith("image/") && contentType != "application/octet-stream")
            {
                code = "INVALID_CONTENT_TYPE";
                message = "Неверный тип изображения";
                return false;
            }

            return true;
        }

        public static bool TryValidateGalleryVideo(
            IFormFile file,
            out string code,
            out string message)
        {
            code = string.Empty;
            message = string.Empty;

            if (file.Length <= 0)
            {
                code = "EMPTY_FILE";
                message = "Файл пустой";
                return false;
            }

            if (file.Length > Constants.ChatConstants.MaxGalleryVideoBytes)
            {
                code = "FILE_TOO_LARGE";
                message = "Видео не больше 30 МБ";
                return false;
            }

            var contentType = file.ContentType.ToLowerInvariant();
            if (!contentType.StartsWith("video/") && contentType != "application/octet-stream")
            {
                code = "INVALID_CONTENT_TYPE";
                message = "Неверный тип видео";
                return false;
            }

            return true;
        }
    }
}
