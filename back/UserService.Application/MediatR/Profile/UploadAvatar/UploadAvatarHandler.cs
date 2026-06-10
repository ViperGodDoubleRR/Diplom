using MediatR;

using Microsoft.Extensions.Logging;

using Shared.Application.Contracts;
using Shared.MinIO.Constants;
using Shared.MinIO.Interfaces;

using UserService.Application.DTO;
using UserService.Application.Mapping;
using UserService.Application.Validation;
using UserService.Domain.IRepository;
using UserService.Domain.Models;

namespace UserService.Application.MediatR.UploadAvatar
{
    public class UploadAvatarHandler : IRequestHandler<UploadAvatarCommand, ApiResponse<MediaDto>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMediaRepository _mediaRepository;
        private readonly IMinioService _minio;
        private readonly ILogger<UploadAvatarHandler> _logger;

        public UploadAvatarHandler(
            IUserRepository userRepository,
            IMediaRepository mediaRepository,
            IMinioService minio,
            ILogger<UploadAvatarHandler> logger)
        {
            _userRepository = userRepository;
            _mediaRepository = mediaRepository;
            _minio = minio;
            _logger = logger;
        }

        public async Task<ApiResponse<MediaDto>> Handle(
            UploadAvatarCommand request,
            CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);

            if (user is null)
                return Fail("USER_NOT_FOUND", "Пользователь не найден");

            if (!MediaValidation.TryValidateUpload(
                    request.File,
                    request.MediaType,
                    out var code,
                    out var message))
            {
                return Fail(code, message);
            }

            var bucket = request.MediaType.Equals("video", StringComparison.OrdinalIgnoreCase)
                || request.MediaType.Equals("avatar", StringComparison.OrdinalIgnoreCase)
                ? Buckets.UserAvatars
                : Buckets.UserGallery;

            using var stream = request.File.OpenReadStream();

            var upload = await _minio.UploadFileAsync(
                stream,
                request.File.FileName,
                request.File.ContentType,
                bucket);

            var media = new MediaUser
            {
                UserId = user.Id,
                Bucket = upload.Bucket,
                FileKey = upload.FileKey,
                ContentType = upload.ContentType,
                MediaType = request.MediaType,
                OriginalName = upload.OriginalName,
                Size = upload.Size,
                CreatedAt = DateTime.UtcNow
            };

            await _mediaRepository.AddAsync(media, cancellationToken);

            _logger.LogInformation(
                "Media uploaded for user {UserId}, type {MediaType}",
                user.Id,
                request.MediaType);

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
