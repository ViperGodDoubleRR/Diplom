using ChatService.Application.DTO;
using ChatService.Domain.Models;

using Shared.MinIO.Interfaces;

namespace ChatService.Application.Mapping
{
    public static class ChatMapper
    {
        public static async Task<(string? Url, bool IsVideo)> GetChatAvatarPreviewAsync(
            Chat chat,
            IMinioService minio,
            CancellationToken cancellationToken = default)
        {
            var avatar = chat.Media
                .Where(m => m.MediaType is "avatar" or "image" or "video")
                .OrderByDescending(m => m.CreatedAt)
                .FirstOrDefault();

            if (avatar is null)
                return (null, false);

            var url = await minio.GetFileUrlAsync(avatar.FileKey, avatar.Bucket);
            var isVideo = string.Equals(avatar.MediaType, "video", StringComparison.OrdinalIgnoreCase);

            return (url, isVideo);
        }

        public static async Task<string?> GetChatAvatarUrlAsync(
            Chat chat,
            IMinioService minio,
            CancellationToken cancellationToken = default)
        {
            var (url, _) = await GetChatAvatarPreviewAsync(chat, minio, cancellationToken);
            return url;
        }

        public static async Task<MessageMediaDto> ToMediaDtoAsync(
            MessageMedia media,
            IMinioService minio,
            CancellationToken cancellationToken = default)
        {
            return new MessageMediaDto
            {
                Id = media.Id,
                Url = await minio.GetFileUrlAsync(media.FileKey, media.Bucket),
                MediaType = media.MediaType,
                OriginalName = media.OriginalName,
                Size = media.Size
            };
        }

        public static MessageDto ToMessageDto(
            Message message,
            ChatUserDto user,
            IReadOnlyList<MessageMediaDto> media)
        {
            return new MessageDto
            {
                Id = message.Id,
                ChatId = message.ChatId,
                User = user,
                Text = message.IsDeleted ? string.Empty : message.Text,
                ReplyToMessageId = message.ReplyToMessageId,
                CreatedAt = message.CreatedAt,
                UpdatedAt = message.UpdatedAt,
                IsEdited = !message.IsDeleted &&
                           message.UpdatedAt.HasValue &&
                           message.UpdatedAt > message.CreatedAt,
                IsDeleted = message.IsDeleted,
                Media = message.IsDeleted ? [] : media.ToList()
            };
        }

        public static LastMessagePreviewDto? ToLastMessagePreview(
            Message? message,
            ChatUserDto? user)
        {
            if (message is null || user is null)
                return null;

            var text = message.IsDeleted
                ? "Сообщение удалено"
                : string.IsNullOrWhiteSpace(message.Text)
                    ? "Медиа"
                    : message.Text;

            return new LastMessagePreviewDto
            {
                Id = message.Id,
                UserId = message.UserId,
                UserLogin = user.Login,
                Text = text,
                CreatedAt = message.CreatedAt,
                IsDeleted = message.IsDeleted
            };
        }
    }
}
