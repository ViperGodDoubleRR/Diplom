using MediatR;

using Microsoft.Extensions.Logging;

using Shared.Application.Contracts;

using UserService.Application.DTO;
using UserService.Application.Mapping;
using UserService.Application.Validation;
using UserService.Domain.IRepository;

namespace UserService.Application.MediatR.UpdateUser
{
    public class UpdateUserHandler : IRequestHandler<UpdateUserCommand, ApiResponse<UserProfileDto>>
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UpdateUserHandler> _logger;

        public UpdateUserHandler(
            IUserRepository userRepository,
            ILogger<UpdateUserHandler> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<ApiResponse<UserProfileDto>> Handle(
            UpdateUserCommand command,
            CancellationToken cancellationToken)
        {
            if (command.Dto is null)
            {
                return Fail("INVALID_REQUEST", "Данные профиля обязательны");
            }

            if (!UserValidation.IsValidUpdate(
                    command.Dto.Login,
                    command.Dto.Tag,
                    command.Dto.Description,
                    out var validationError))
            {
                return Fail("VALIDATION_ERROR", validationError!);
            }

            var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);

            if (user is null)
                return Fail("USER_NOT_FOUND", "Пользователь не найден");

            var normalizedLogin = command.Dto.Login.Trim();
            var existingByLogin = await _userRepository.GetByLoginAsync(normalizedLogin, cancellationToken);

            if (existingByLogin is not null && existingByLogin.Id != user.Id)
                return Fail("LOGIN_TAKEN", "Этот логин уже занят");

            user.Login = normalizedLogin;
            user.Tag = string.IsNullOrWhiteSpace(command.Dto.Tag)
                ? null
                : command.Dto.Tag.Trim();
            user.Description = command.Dto.Description;

            await _userRepository.UpdateAsync(user, cancellationToken);

            _logger.LogInformation("Profile updated for user {UserId}", user.Id);

            return new ApiResponse<UserProfileDto>
            {
                Success = true,
                Data = UserProfileMapper.ToProfileDto(user)
            };
        }

        private static ApiResponse<UserProfileDto> Fail(string code, string message) =>
            new()
            {
                Success = false,
                Error = new ApiError { Code = code, Message = message }
            };
    }
}
