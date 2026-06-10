using AuthService.Domain.Interface;

using MediatR;

using Microsoft.Extensions.Logging;

using Shared.Application.Contracts;
using Shared.Application.Interfaces;
using Shared.Application.Validation;
using Shared.RabbitMQ.EventBus.Abstractions;
using Shared.RabbitMQ.EventBus.Events.User;
using AuthService.Application.Settings;

namespace AuthService.Application.MediatR.Settings
{
    public class ConfirmChangeEmailHandler
        : IRequestHandler<ConfirmChangeEmailCommand, ApiResponse<string>>
    {
        private readonly IAuthRepository _authRepository;
        private readonly IEventBus _eventBus;
        private readonly IHasher _hasher;
        private readonly EmailChangeAvailabilityChecker _availabilityChecker;
        private readonly ILogger<ConfirmChangeEmailHandler> _logger;

        public ConfirmChangeEmailHandler(
            IAuthRepository authRepository,
            IEventBus eventBus,
            IHasher hasher,
            EmailChangeAvailabilityChecker availabilityChecker,
            ILogger<ConfirmChangeEmailHandler> logger)
        {
            _authRepository = authRepository;
            _eventBus = eventBus;
            _hasher = hasher;
            _availabilityChecker = availabilityChecker;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(
            ConfirmChangeEmailCommand command,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(command.Code))
                return Fail("CODE_REQUIRED", "Код подтверждения обязателен");

            if (string.IsNullOrWhiteSpace(command.CurrentPassword))
                return Fail("PASSWORD_REQUIRED", "Введите текущий пароль");

            var user = await _authRepository.GetUserByIdAsync(command.UserId, cancellationToken);

            if (user is null)
                return Fail("USER_NOT_FOUND", "Пользователь не найден");

            if (!_hasher.Verify(command.CurrentPassword, user.PasswordHash))
                return Fail("INVALID_PASSWORD", "Неверный текущий пароль");

            var availability = await _availabilityChecker.CheckAsync(
                command.UserId,
                command.NewEmail,
                cancellationToken);

            if (!availability.Available)
                return Fail(availability.ErrorCode!, availability.ErrorMessage!);

            var newEmail = InputValidator.NormalizeEmail(command.NewEmail);
            var code = InputValidator.NormalizeCode(command.Code);

            var isCodeValid = await _authRepository.VerifyUserCodeAsync(
                command.UserId,
                code,
                cancellationToken);

            if (!isCodeValid)
                return Fail("INVALID_CODE", "Неверный или просроченный код");

            var oldEmail = user.Email;

            var updated = await _authRepository.UpdateUserEmailAsync(
                command.UserId,
                newEmail,
                cancellationToken);

            if (!updated)
                return Fail("UPDATE_FAILED", "Не удалось обновить email");

            try
            {
                await _eventBus.Publish(new UpdateUserEmailEvent
                {
                    UserId = command.UserId,
                    Email = newEmail,
                    OldEmail = oldEmail
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Failed to publish UpdateUserEmailEvent for {UserId}",
                    command.UserId);
            }

            return new ApiResponse<string>
            {
                Success = true,
                Data = newEmail
            };
        }

        private static ApiResponse<string> Fail(string code, string message) =>
            new()
            {
                Success = false,
                Error = new ApiError { Code = code, Message = message }
            };
    }
}
