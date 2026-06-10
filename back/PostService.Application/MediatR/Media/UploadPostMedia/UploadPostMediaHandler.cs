using MediatR;

using PostService.Application.DTO;
using PostService.Application.Mapping;
using PostService.Application.Validation;
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
            var post = await _postRepository.GetByIdAsync(request.PostId, cancellationToken);

            if (post is null || post.IsDeleted)
                return Fail("POST_NOT_FOUND", "Пост не найден");

            if (post.UserId != request.UserId)
                return Fail("FORBIDDEN", "Нельзя загружать медиа в чужой пост");

            if (!PostValidation.TryValidateMediaUpload(
                    request.File,
                    request.MediaType,
                    out var code,
                    out var message))
            {
                return Fail(code, message);
            }

            var bucket = request.MediaType.Equals("video", StringComparison.OrdinalIgnoreCase)
                ? Buckets.PostVideos
                : Buckets.PostImages;

            using var stream = request.File.OpenReadStream();

            var upload = await _minio.UploadFileAsync(
                stream,
                request.File.FileName,
                request.File.ContentType,
                bucket);

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

            await _mediaRepository.AddAsync(media, cancellationToken);

            return new ApiResponse<PostMediaDto>
            {
                Success = true,
                Data = await PostMapper.ToDtoAsync(media, _minio, cancellationToken)
            };
        }

        private static ApiResponse<PostMediaDto> Fail(string code, string message) =>
            new()
            {
                Success = false,
                Error = new ApiError { Code = code, Message = message }
            };
    }
}
