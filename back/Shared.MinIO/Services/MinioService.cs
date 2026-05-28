using Minio;
using Minio.DataModel.Args;

using Shared.MinIO.Interfaces;
using Shared.MinIO.Models;

namespace Shared.MinIO.Services
{
    public class MinioService : IMinioService
    {
        private readonly IMinioClient _client;
        public MinioService(IMinioClient client)
        {
            _client = client;
        }

        public async Task<UploadFileResult> UploadFileAsync(Stream file,string fileKey,string contentType,string bucket)
        {
            if (file.CanSeek)
                file.Position = 0;

            await _client.PutObjectAsync(new PutObjectArgs()
                .WithBucket(bucket)
                .WithObject(fileKey)
                .WithStreamData(file)
                .WithObjectSize(file.Length)
                .WithContentType(contentType));

            var url = await GetFileUrlAsync(fileKey, bucket);

            return new UploadFileResult
            {
                FileKey = fileKey,
                Bucket = bucket,
                Url = url,
                ContentType = contentType,
                Size = file.Length,
                OriginalName = fileKey
            };
        }

        public async Task DeleteFileAsync(string fileName, string bucket)
        {
            await _client.RemoveObjectAsync(new RemoveObjectArgs()
                .WithBucket(bucket)
                .WithObject(fileName));
        }

        public async Task<string> GetFileUrlAsync(string fileName, string bucket)
        {
            return await _client.PresignedGetObjectAsync(
                new PresignedGetObjectArgs()
                    .WithBucket(bucket)
                    .WithObject(fileName)
                    .WithExpiry(60 * 60)
            );
        }
    }
}