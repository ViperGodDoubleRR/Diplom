using MediatR;

using Microsoft.Extensions.Logging;

using Shared.Application.Contracts;

using UserService.Application.DTO;
using UserService.Application.Mapping;
using UserService.Domain.IRepository;

namespace UserService.Application.MediatR.UserCommand
{
    public class GetUserHandler
        : IRequestHandler<GetUserCommand, ApiResponse<UserProfileDto>>
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<GetUserHandler> _logger;

        public GetUserHandler(
            IUserRepository userRepository,
            ILogger<GetUserHandler> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<ApiResponse<UserProfileDto>> Handle(
            GetUserCommand command,
            CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);

            if (user is null)
            {
                _logger.LogWarning("Profile not found for user {UserId}", command.UserId);

                return new ApiResponse<UserProfileDto>
                {
                    Success = false,
                    Error = new ApiError
                    {
                        Code = "USER_NOT_FOUND",
                        Message = "Пользователь не найден"
                    }
                };
            }

            return new ApiResponse<UserProfileDto>
            {
                Success = true,
                Data = UserProfileMapper.ToProfileDto(user)
            };
        }
    }
}
