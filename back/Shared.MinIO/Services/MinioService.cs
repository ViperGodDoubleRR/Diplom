using Minio;
using Minio.DataModel.Args;

using Microsoft.Extensions.Options;

using Shared.MinIO.Interfaces;
using Shared.MinIO.Models;
using Shared.MinIO.Options;

namespace Shared.MinIO.Services
{
    public class MinioService : IMinioService
    {
        private readonly IMinioClient _client;
        private readonly IMinioClient _presignClient;
        private readonly MinioOptions _options;

        public MinioService(IMinioClient client, IOptions<MinioOptions> options)
        {
            _client = client;
            _options = options.Value;
            _presignClient = CreatePresignClient(_options);
        }

        private static IMinioClient CreatePresignClient(MinioOptions options)
        {
            var publicEndpoint = options.PublicEndpoint?.Trim();
            var internalEndpoint = options.Endpoint.Trim();

            if (string.IsNullOrWhiteSpace(publicEndpoint) ||
                publicEndpoint.Equals(internalEndpoint, StringComparison.OrdinalIgnoreCase))
            {
                return new MinioClient()
                    .WithEndpoint(internalEndpoint)
                    .WithCredentials(options.AccessKey, options.SecretKey)
                    .WithSSL(options.Secure)
                    .Build();
            }

            return new MinioClient()
                .WithEndpoint(publicEndpoint)
                .WithCredentials(options.AccessKey, options.SecretKey)
                .WithSSL(options.Secure)
                .Build();
        }

        public async Task EnsureBucketAsync(string bucket, CancellationToken cancellationToken = default)
        {
            var exists = await _client.BucketExistsAsync(
                new BucketExistsArgs().WithBucket(bucket),
                cancellationToken);

            if (exists) return;

            await _client.MakeBucketAsync(
                new MakeBucketArgs().WithBucket(bucket),
                cancellationToken);
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
            return await _presignClient.PresignedGetObjectAsync(
                new PresignedGetObjectArgs()
                    .WithBucket(bucket)
                    .WithObject(fileName)
                    .WithExpiry(60 * 60)
            );
        }
    }
}