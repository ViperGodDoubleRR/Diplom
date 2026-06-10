using AuthService.Domain.Interface;

using MediatR;

using Microsoft.Extensions.Logging;

using Shared.Application.Contracts;
using Shared.Application.Interfaces;
using Shared.Application.Validation;
using AuthService.Application.Settings;

namespace AuthService.Application.MediatR.Settings
{
    public class RequestChangeEmailHandler
        : IRequestHandler<RequestChangeEmailCommand, ApiResponse<string>>
    {
        private static readonly TimeSpan CodeCooldown = TimeSpan.FromSeconds(60);

        private readonly IAuthRepository _authRepository;
        private readonly ICodeGenerate _codeGenerator;
        private readonly IEmailSender _emailSender;
        private readonly IHasher _hasher;
        private readonly EmailChangeAvailabilityChecker _availabilityChecker;
        private readonly ILogger<RequestChangeEmailHandler> _logger;

        public RequestChangeEmailHandler(
            IAuthRepository authRepository,
            ICodeGenerate codeGenerator,
            IEmailSender emailSender,
            IHasher hasher,
            EmailChangeAvailabilityChecker availabilityChecker,
            ILogger<RequestChangeEmailHandler> logger)
        {
            _authRepository = authRepository;
            _codeGenerator = codeGenerator;
            _emailSender = emailSender;
            _hasher = hasher;
            _availabilityChecker = availabilityChecker;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(
            RequestChangeEmailCommand command,
            CancellationToken cancellationToken)
        {
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

            if (!await _authRepository.CanRequestVerificationCodeAsync(
                    command.UserId,
                    CodeCooldown,
                    cancellationToken))
            {
                return Fail("CODE_COOLDOWN", "Подождите 60 секунд перед повторной отправкой");
            }

            var code = _codeGenerator.GenerateAlphaNumericCode();
            var created = await _authRepository.CreateVerificationCodeForUserAsync(
                command.UserId,
                code,
                cancellationToken);

            if (!created)
                return Fail("CODE_CREATE_FAILED", "Не удалось создать код");

            try
            {
                await _emailSender.SendConfirmationCodeAsync(newEmail, code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send change-email code to {Email}", newEmail);
                return Fail("EMAIL_SEND_FAILED", "Не удалось отправить код на email");
            }

            return new ApiResponse<string>
            {
                Success = true,
                Data = "CODE_SENT"
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
