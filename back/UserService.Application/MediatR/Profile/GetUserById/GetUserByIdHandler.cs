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

namespace UserService.Application.MediatR.Profile.GetUserById
{
    public class GetUserByIdHandler
    : IRequestHandler<GetUserByIdQuery, ApiResponse<UserViewDto>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMediaRepository _mediaRepository;
        private readonly IMinioService _minio;

        public GetUserByIdHandler(
            IUserRepository userRepository,
            IMediaRepository mediaRepository,
            IMinioService minio)
        {
            _userRepository = userRepository;
            _mediaRepository = mediaRepository;
            _minio = minio;
        }

        public async Task<ApiResponse<UserViewDto>> Handle(
            GetUserByIdQuery request,
            CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);

            if (user == null)
            {
                return new ApiResponse<UserViewDto>
                {
                    Success = false,
                    Error = new ApiError
                    {
                        Code = "USER_NOT_FOUND",
                        Message = "Пользователь не найден"
                    }
                };
            }

            // 🔥 1. Берём медиа пользователя
            var media = await _mediaRepository.GetByUserIdAsync(user.Id);

            var mediaDtos = new List<MediaDto>();

            foreach (var m in media)
            {
                var url = await _minio.GetFileUrlAsync(m.FileKey, m.Bucket);

                mediaDtos.Add(new MediaDto
                {
                    Id = m.Id,
                    FileKey = m.FileKey,
                    Bucket = m.Bucket,
                    MediaType = m.MediaType,
                    ContentType = m.ContentType,
                    Url = url
                });
            }

            // 🔥 2. Собираем DTO
            return new ApiResponse<UserViewDto>
            {
                Success = true,
                Data = new UserViewDto
                {
                    Id = user.Id,
                    Login = user.Login,
                    Tag = user.Tag,
                    Description = user.Description,
                    Media = mediaDtos
                }
            };
        }
    }
}
