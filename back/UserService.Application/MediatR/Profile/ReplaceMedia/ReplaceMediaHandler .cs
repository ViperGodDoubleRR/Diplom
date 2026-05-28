using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

using Shared.Application.Contracts;
using Shared.MinIO.Interfaces;

using UserService.Application.DTO;
using UserService.Domain.IRepository;
using UserService.Domain.Models;

namespace UserService.Application.MediatR.ReplaceMedia
{
    public class ReplaceMediaHandler : IRequestHandler<ReplaceMediaCommand, ApiResponse<MediaDto>>
    {
        private readonly IMediaRepository _mediaRepository;
        private readonly IMinioService _minio;

        public ReplaceMediaHandler(IMediaRepository mediaRepository, IMinioService minio)
        {
            _mediaRepository = mediaRepository;
            _minio = minio;
        }

        public async Task<ApiResponse<MediaDto>> Handle(ReplaceMediaCommand request, CancellationToken cancellationToken)
        {
            var media = await _mediaRepository.GetByIdAsync(request.MediaId);

            if (media is null || media.UserId != request.UserId)
            {
                return new ApiResponse<MediaDto>
                {
                    Success = false,
                    Error = new ApiError
                    {
                        Code = "MEDIA_NOT_FOUND",
                        Message = "Media not found"
                    }
                };
            }

            // delete old file
            await _minio.DeleteFileAsync(media.FileKey, media.Bucket);

            // upload new file
            var newFileKey = $"{Guid.NewGuid()}_{request.File.FileName}";

            using (var stream = request.File.OpenReadStream())
            {
                await _minio.UploadFileAsync(
                    stream,
                    newFileKey,
                    request.File.ContentType,
                    media.Bucket
                );
            }

            // update db
            media.FileKey = newFileKey;
            media.ContentType = request.File.ContentType;
            media.MediaType = request.MediaType;

            await _mediaRepository.UpdateAsync(media);

            // dto
            var dto = new MediaDto
            {
                Id = media.Id,
                FileKey = media.FileKey,
                Bucket = media.Bucket,
                MediaType = media.MediaType,
                ContentType = media.ContentType,
                Url = await _minio.GetFileUrlAsync(media.FileKey, media.Bucket)
            };

            return new ApiResponse<MediaDto>
            {
                Success = true,
                Data = dto
            };
        }
    }
}
