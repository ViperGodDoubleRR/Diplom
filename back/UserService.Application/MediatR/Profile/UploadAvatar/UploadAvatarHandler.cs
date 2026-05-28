using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

using Shared.Application.Contracts;
using Shared.MinIO.Constants;
using Shared.MinIO.Interfaces;

using UserService.Application.DTO;
using UserService.Domain.IRepository;
using UserService.Domain.Models;

namespace UserService.Application.MediatR.UploadAvatar
{
    public class UploadAvatarHandler
       : IRequestHandler<
           UploadAvatarCommand,
           ApiResponse<MediaDto>>
    {
        private readonly IUserRepository _userRepository;

        private readonly IMediaRepository _mediaRepository;

        private readonly IMinioService _minio;

        public UploadAvatarHandler(
            IUserRepository userRepository,
            IMediaRepository mediaRepository,
            IMinioService minio)
        {
            _userRepository = userRepository;
            _mediaRepository = mediaRepository;
            _minio = minio;
        }

        public async Task<ApiResponse<MediaDto>> Handle(
            UploadAvatarCommand request,
            CancellationToken cancellationToken)
        {
            // =========================
            // USER
            // =========================
            var user = await _userRepository.GetByIdAsync(
                request.UserId
            );

            if (user is null)
            {
                return new ApiResponse<MediaDto>
                {
                    Success = false,
                    Error = new ApiError
                    {
                        Code = "USER_NOT_FOUND",
                        Message = "User not found"
                    }
                };
            }

            // =========================
            // EMPTY FILE
            // =========================
            if (request.File.Length == 0)
            {
                return new ApiResponse<MediaDto>
                {
                    Success = false,
                    Error = new ApiError
                    {
                        Code = "EMPTY_FILE",
                        Message = "File is empty"
                    }
                };
            }

            // =========================
            // FILE SIZE
            // =========================
            const long maxFileSize = 5 * 1024 * 1024;

            if (request.File.Length > maxFileSize)
            {
                return new ApiResponse<MediaDto>
                {
                    Success = false,
                    Error = new ApiError
                    {
                        Code = "FILE_TOO_LARGE",
                        Message = "File too large"
                    }
                };
            }

            // =========================
            // MIME TYPES
            // =========================
            var allowedContentTypes = new[]
            {
                "image/jpeg",
                "image/png",
                "image/webp"
            };

            if (!allowedContentTypes.Contains(
                    request.File.ContentType))
            {
                return new ApiResponse<MediaDto>
                {
                    Success = false,
                    Error = new ApiError
                    {
                        Code = "INVALID_FILE_TYPE",
                        Message = "Invalid file type"
                    }
                };
            }

            // =========================
            // MEDIA TYPE
            // =========================
            var allowedMediaTypes = new[]
            {
                "avatar",
                "gallery",
                "video"
            };

            if (!allowedMediaTypes.Contains(
                    request.MediaType))
            {
                return new ApiResponse<MediaDto>
                {
                    Success = false,
                    Error = new ApiError
                    {
                        Code = "INVALID_MEDIA_TYPE",
                        Message = "Invalid media type"
                    }
                };
            }

            // =========================
            // BUCKET
            // =========================
            var bucket = request.MediaType switch
            {
                "avatar" => Buckets.UserAvatars,
                "gallery" => Buckets.UserGallery,
                _ => Buckets.UserAvatars
            };

            // =========================
            // UPLOAD
            // =========================
            using var stream =
                request.File.OpenReadStream();

            var upload = await _minio.UploadFileAsync(
                stream,
                request.File.FileName,
                request.File.ContentType,
                bucket
            );

            // =========================
            // ENTITY
            // =========================
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

            await _mediaRepository.AddAsync(media);

            // =========================
            // URL
            // =========================
            var url = await _minio.GetFileUrlAsync(
                media.FileKey,
                media.Bucket
            );

            // =========================
            // RESPONSE
            // =========================
            return new ApiResponse<MediaDto>
            {
                Success = true,

                Data = new MediaDto
                {
                    Id = media.Id,

                    FileKey = media.FileKey,

                    Bucket = media.Bucket,

                    MediaType = media.MediaType,

                    ContentType = media.ContentType,

                    Url = url
                }
            };
        }
    }
}
