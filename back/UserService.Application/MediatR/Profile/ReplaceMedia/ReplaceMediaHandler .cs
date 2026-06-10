using MediatR;

using Microsoft.Extensions.Logging;

using Shared.Application.Contracts;
using Shared.MinIO.Interfaces;

using UserService.Application.DTO;
using UserService.Application.Mapping;
using UserService.Application.Validation;
using UserService.Domain.IRepository;

namespace UserService.Application.MediatR.ReplaceMedia
{
    public class ReplaceMediaHandler : IRequestHandler<ReplaceMediaCommand, ApiResponse<MediaDto>>
    {
        private readonly IMediaRepository _mediaRepository;
        private readonly IMinioService _minio;
        private readonly ILogger<ReplaceMediaHandler> _logger;

        public ReplaceMediaHandler(
            IMediaRepository mediaRepository,
            IMinioService minio,
            ILogger<ReplaceMediaHandler> logger)
        {
            _mediaRepository = mediaRepository;
            _minio = minio;
            _logger = logger;
        }

        public async Task<ApiResponse<MediaDto>> Handle(
            ReplaceMediaCommand request,
            CancellationToken cancellationToken)
        {
            var media = await _mediaRepository.GetByIdAsync(request.MediaId, cancellationToken);

            if (media is null || media.UserId != request.UserId)
                return Fail("MEDIA_NOT_FOUND", "Медиа не найдено");

            if (!MediaValidation.TryValidateUpload(
                    request.File,
                    request.MediaType,
                    out var code,
                    out var message))
            {
                return Fail(code, message);
            }

            await _minio.DeleteFileAsync(media.FileKey, media.Bucket);

            using var stream = request.File.OpenReadStream();

            var upload = await _minio.UploadFileAsync(
                stream,
                request.File.FileName,
                request.File.ContentType,
                media.Bucket);

            media.FileKey = upload.FileKey;
            media.ContentType = upload.ContentType;
            media.MediaType = request.MediaType;
            media.OriginalName = upload.OriginalName;
            media.Size = upload.Size;
            media.CreatedAt = DateTime.UtcNow;

            await _mediaRepository.UpdateAsync(media, cancellationToken);

            _logger.LogInformation(
                "Media {MediaId} replaced for user {UserId}",
                media.Id,
                request.UserId);

            return new ApiResponse<MediaDto>
            {
                Success = true,
                Data = await MediaMapper.ToDtoAsync(media, _minio, cancellationToken)
            };
        }

        private static ApiResponse<MediaDto> Fail(string code, string message) =>
            new()
            {
                Success = false,
                Error = new ApiError { Code = code, Message = message }
            };
    }
}
