using PostService.Application.DTO;
using PostService.Domain.Models;

using Shared.MinIO.Interfaces;

namespace PostService.Application.Mapping
{
    public static class PostMapper
    {
        public static async Task<PostMediaDto> ToDtoAsync(
            PostMedia media,
            IMinioService minio,
            CancellationToken cancellationToken = default)
        {
            var url = await minio.GetFileUrlAsync(media.FileKey, media.Bucket);

            return new PostMediaDto
            {
                Id = media.Id,
                Url = url,
                FileKey = media.FileKey,
                Bucket = media.Bucket,
                MediaType = media.MediaType
            };
        }

        public static async Task<List<PostMediaDto>> ToDtoListAsync(
            IEnumerable<PostMedia> mediaList,
            IMinioService minio,
            CancellationToken cancellationToken = default)
        {
            var result = new List<PostMediaDto>();

            foreach (var media in mediaList)
            {
                result.Add(await ToDtoAsync(media, minio, cancellationToken));
            }

            return result;
        }

        public static async Task<string?> GetPreviewUrlAsync(
            PostMedia? media,
            IMinioService minio,
            CancellationToken cancellationToken = default)
        {
            if (media is null)
                return null;

            return await minio.GetFileUrlAsync(media.FileKey, media.Bucket);
        }
    }
}
