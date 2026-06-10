using ChatService.Application.DTO;
using ChatService.Domain.Models;

using Shared.MinIO.Interfaces;

namespace ChatService.Application.Mapping
{
    public static class ChatMediaMapper
    {
        public static async Task<ChatMediaDto> ToDtoAsync(
            ChatMedia media,
            IMinioService minio,
            CancellationToken cancellationToken = default) =>
            new()
            {
                Id = media.Id,
                Url = await minio.GetFileUrlAsync(media.FileKey, media.Bucket),
                MediaType = media.MediaType,
                OriginalName = media.OriginalName,
                Size = media.Size,
                CreatedAt = media.CreatedAt
            };

        public static async Task<List<ChatMediaDto>> ToDtoListAsync(
            IEnumerable<ChatMedia> media,
            IMinioService minio,
            CancellationToken cancellationToken = default)
        {
            var result = new List<ChatMediaDto>();

            foreach (var item in media)
            {
                result.Add(await ToDtoAsync(item, minio, cancellationToken));
            }

            return result;
        }
    }
}
