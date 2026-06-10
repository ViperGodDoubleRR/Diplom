using MediatR;

using Shared.Application.Contracts;
using Shared.MinIO.Interfaces;

using UserService.Application.DTO;
using UserService.Application.Mapping;
using UserService.Domain.IRepository;

namespace UserService.Application.MediatR.Profile.GetUserById
{
    public class GetUserByIdHandler : IRequestHandler<GetUserByIdQuery, ApiResponse<UserViewDto>>
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
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);

            if (user is null)
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

            var media = await _mediaRepository.GetByUserIdAsync(user.Id, cancellationToken);
            var mediaDtos = await MediaMapper.ToDtoListAsync(media, _minio, cancellationToken);

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
