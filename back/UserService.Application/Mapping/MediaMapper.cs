using Shared.MinIO.Interfaces;

using UserService.Application.DTO;
using UserService.Domain.Models;

namespace UserService.Application.Mapping
{
    public static class MediaMapper
    {
        public static async Task<MediaDto> ToDtoAsync(
            MediaUser media,
            IMinioService minio,
            CancellationToken cancellationToken = default)
        {
            var url = await minio.GetFileUrlAsync(media.FileKey, media.Bucket);

            return new MediaDto
            {
                Id = media.Id,
                FileKey = media.FileKey,
                Bucket = media.Bucket,
                MediaType = media.MediaType,
                ContentType = media.ContentType,
                Url = url,
                CreatedAt = media.CreatedAt
            };
        }

        public static async Task<List<MediaDto>> ToDtoListAsync(
            IEnumerable<MediaUser> mediaList,
            IMinioService minio,
            CancellationToken cancellationToken = default)
        {
            var result = new List<MediaDto>();

            foreach (var media in mediaList)
            {
                result.Add(await ToDtoAsync(media, minio, cancellationToken));
            }

            return result;
        }

        public static async Task<Dictionary<Guid, string?>> BuildAvatarUrlMapAsync(
            IEnumerable<Guid> userIds,
            IReadOnlyList<MediaUser> avatars,
            IMinioService minio,
            CancellationToken cancellationToken = default)
        {
            var latestAvatars = avatars
                .GroupBy(a => a.UserId)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(x => x.CreatedAt).First());

            var map = new Dictionary<Guid, string?>();

            foreach (var userId in userIds)
            {
                if (!latestAvatars.TryGetValue(userId, out var avatar))
                {
                    map[userId] = null;
                    continue;
                }

                map[userId] = await minio.GetFileUrlAsync(
                    avatar.FileKey,
                    avatar.Bucket);
            }

            return map;
        }

        public static async Task<Dictionary<Guid, ProfilePreviewMedia>> BuildProfilePreviewMapAsync(
            IEnumerable<Guid> userIds,
            IReadOnlyList<MediaUser> media,
            IMinioService minio,
            CancellationToken cancellationToken = default)
        {
            var latestByUser = media
                .Where(m =>
                    string.Equals(m.MediaType, "avatar", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(m.MediaType, "video", StringComparison.OrdinalIgnoreCase))
                .GroupBy(m => m.UserId)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(x => x.CreatedAt).First());

            var map = new Dictionary<Guid, ProfilePreviewMedia>();

            foreach (var userId in userIds)
            {
                if (!latestByUser.TryGetValue(userId, out var item))
                {
                    map[userId] = new ProfilePreviewMedia();
                    continue;
                }

                map[userId] = new ProfilePreviewMedia
                {
                    Url = await minio.GetFileUrlAsync(item.FileKey, item.Bucket),
                    IsVideo = string.Equals(item.MediaType, "video", StringComparison.OrdinalIgnoreCase)
                };
            }

            return map;
        }
    }
}
