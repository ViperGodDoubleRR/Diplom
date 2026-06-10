using CommentService.Application.DTO;
using CommentService.Application.Mapping;
using CommentService.Application.Validation;
using CommentService.Domain.IRepository;
using CommentService.Domain.Models;

using MediatR;

using Shared.Application.Contracts;
using Shared.MinIO.Constants;
using Shared.MinIO.Interfaces;

namespace CommentService.Application.MediatR.UploadCommentMedia
{
    public class UploadCommentMediaHandler
        : IRequestHandler<UploadCommentMediaCommand, ApiResponse<CommentMediaDto>>
    {
        private readonly ICommentRepository _commentRepository;
        private readonly ICommentMediaRepository _mediaRepository;
        private readonly IMinioService _minio;

        public UploadCommentMediaHandler(
            ICommentRepository commentRepository,
            ICommentMediaRepository mediaRepository,
            IMinioService minio)
        {
            _commentRepository = commentRepository;
            _mediaRepository = mediaRepository;
            _minio = minio;
        }

        public async Task<ApiResponse<CommentMediaDto>> Handle(
            UploadCommentMediaCommand request,
            CancellationToken cancellationToken)
        {
            var comment = await _commentRepository.GetByIdAsync(request.CommentId, cancellationToken);

            if (comment is null || comment.IsDeleted)
                return Fail("COMMENT_NOT_FOUND", "Комментарий не найден");

            if (comment.UserId != request.UserId)
                return Fail("FORBIDDEN", "Нельзя загружать медиа в чужой комментарий");

            var existing = await _mediaRepository.GetByCommentIdAsync(comment.Id, cancellationToken);

            if (existing is not null)
                return Fail("MEDIA_ALREADY_EXISTS", "У комментария уже есть медиа. Удалите его перед загрузкой нового");

            if (!CommentValidation.TryValidateMediaUpload(
                    request.File,
                    request.MediaType,
                    out var code,
                    out var message))
            {
                return Fail(code, message);
            }

            var bucket = request.MediaType.Equals("video", StringComparison.OrdinalIgnoreCase)
                ? Buckets.CommentVideos
                : Buckets.CommentImages;

            using var stream = request.File.OpenReadStream();

            var upload = await _minio.UploadFileAsync(
                stream,
                request.File.FileName,
                request.File.ContentType,
                bucket);

            var media = new CommentMedia
            {
                CommentId = comment.Id,
                Bucket = upload.Bucket,
                FileKey = upload.FileKey,
                OriginalName = request.File.FileName,
                ContentType = upload.ContentType,
                Size = upload.Size,
                MediaType = request.MediaType,
                CreatedAt = DateTime.UtcNow
            };

            await _mediaRepository.AddAsync(media, cancellationToken);

            return new ApiResponse<CommentMediaDto>
            {
                Success = true,
                Data = await CommentMapper.ToMediaDtoAsync(media, _minio, cancellationToken)
            };
        }

        private static ApiResponse<CommentMediaDto> Fail(string code, string message) =>
            new()
            {
                Success = false,
                Error = new ApiError { Code = code, Message = message }
            };
    }
}
