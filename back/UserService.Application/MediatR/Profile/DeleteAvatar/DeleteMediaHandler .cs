using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

using Shared.Application.Contracts;
using Shared.MinIO.Interfaces;

using UserService.Domain.IRepository;

namespace UserService.Application.MediatR.DeleteAvatar
{
    public class DeleteMediaHandler : IRequestHandler<DeleteMediaCommand, ApiResponse<bool>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMediaRepository _mediaRepository;
        private readonly IMinioService _minio;

        public DeleteMediaHandler(
            IUserRepository userRepository,
            IMediaRepository mediaRepository,
            IMinioService minio)
        {
            _userRepository = userRepository;
            _mediaRepository = mediaRepository;
            _minio = minio;
        }

        public async Task<ApiResponse<bool>> Handle(DeleteMediaCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);

            if (user is null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Error = new ApiError
                    {
                        Code = "USER_NOT_FOUND",
                        Message = "User not found"
                    }
                };
            }

            var media = await _mediaRepository.GetByIdAsync(request.MediaId);

            if (media is null || media.UserId != request.UserId)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Error = new ApiError
                    {
                        Code = "MEDIA_NOT_FOUND",
                        Message = "Media not found"
                    }
                };
            }

            await _minio.DeleteFileAsync(media.FileKey, media.Bucket);
            await _mediaRepository.DeleteAsync(media);

            return new ApiResponse<bool>
            {
                Success = true,
                Data = true
            };
        }
    }
}
