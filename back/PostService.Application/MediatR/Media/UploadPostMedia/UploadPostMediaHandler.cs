using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

using Minio.DataModel;

using PostService.Application.DTO;
using PostService.Domain.IRepository;
using PostService.Domain.Models;

using Shared.Application.Contracts;
using Shared.MinIO.Constants;
using Shared.MinIO.Interfaces;

namespace PostService.Application.MediatR.Media.UploadPostMedia
{
    public class UploadPostMediaHandler
        : IRequestHandler<UploadPostMediaCommand, ApiResponse<PostMediaDto>>
    {
        private readonly IPostRepository _postRepository;
        private readonly IPostMediaRepository _mediaRepository;
        private readonly IMinioService _minio;

        public UploadPostMediaHandler(
            IPostRepository postRepository,
            IPostMediaRepository mediaRepository,
            IMinioService minio)
        {
            _postRepository = postRepository;
            _mediaRepository = mediaRepository;
            _minio = minio;
        }

        public async Task<ApiResponse<PostMediaDto>> Handle(
            UploadPostMediaCommand request,
            CancellationToken cancellationToken)
        {
            var post = await _postRepository.GetByIdAsync(request.PostId);

            if (post is null)
            {
                return Error("POST_NOT_FOUND", "Post not found");
            }

       
            if (post.UserId != request.UserId)
            {
                return Error("FORBIDDEN", "You can't upload media to this post");
            }

            if (request.File.Length == 0)
                return Error("EMPTY_FILE", "File is empty");

            const long maxSize = 10 * 1024 * 1024;

            if (request.File.Length > maxSize)
                return Error("FILE_TOO_LARGE", "File too large");

            var allowedTypes = new[]
            {
                "image/jpeg",
                "image/png",
                "image/webp",
                "video/mp4"
            };

            if (!allowedTypes.Contains(request.File.ContentType))
                return Error("INVALID_FILE_TYPE", "Invalid file type");

            var allowedMedia = new[] { "image", "video" };

            if (!allowedMedia.Contains(request.MediaType))
                return Error("INVALID_MEDIA_TYPE", "Invalid media type");

            var bucket = request.MediaType switch
            {
                "image" => Buckets.PostImages,
                "video" => Buckets.PostVideos,
                _ => Buckets.PostImages
            };

            var fileKey = Guid.NewGuid().ToString();

            using var stream = request.File.OpenReadStream();

            var upload = await _minio.UploadFileAsync(
                stream,
                fileKey,
                request.File.ContentType,
                bucket
            );

            var media = new PostMedia
            {
                Id = Guid.NewGuid(),
                PostId = post.Id,

                Bucket = upload.Bucket,
                FileKey = upload.FileKey,

                OriginalName = request.File.FileName,
                ContentType = upload.ContentType,
                Size = upload.Size,

                MediaType = request.MediaType,
                CreatedAt = DateTime.UtcNow
            };

            await _mediaRepository.AddAsync(media);

            var url = await _minio.GetFileUrlAsync(
                media.FileKey,
                media.Bucket
            );

            return new ApiResponse<PostMediaDto>
            {
                Success = true,
                Data = new PostMediaDto
                {
                    Id = media.Id,
                    FileKey = media.FileKey,
                    Bucket = media.Bucket,
                    MediaType = media.MediaType,
                    Url = url
                }
            };
        }

        private ApiResponse<PostMediaDto> Error(string code, string message)
        {
            return new ApiResponse<PostMediaDto>
            {
                Success = false,
                Error = new ApiError
                {
                    Code = code,
                    Message = message
                }
            };

        }
    }
}
