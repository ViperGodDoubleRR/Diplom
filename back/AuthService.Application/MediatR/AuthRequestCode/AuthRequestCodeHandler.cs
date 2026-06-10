using AuthService.Domain.Interface;

using MediatR;

using Microsoft.Extensions.Logging;

using Shared.Application.Contracts;
using Shared.Application.Interfaces;
using Shared.Application.Validation;

namespace AuthService.Application.MediatR.AuthRequestCode
{
    public class AuthRequestCodeHandler : IRequestHandler<AuthRequestCodeCommand, ApiResponse<string>>
    {
        private const string SuccessMessage = "CODE_SENT";

        private readonly IAuthRepository _authRepository;
        private readonly ICodeGenerate _codeGenerator;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<AuthRequestCodeHandler> _logger;

        public AuthRequestCodeHandler(
            IAuthRepository authRepository,
            ICodeGenerate codeGenerator,
            IEmailSender emailSender,
            ILogger<AuthRequestCodeHandler> logger)
        {
            _authRepository = authRepository;
            _codeGenerator = codeGenerator;
            _emailSender = emailSender;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(
            AuthRequestCodeCommand command,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(command.Email))
                return Fail("EMAIL_REQUIRED", "Email обязателен");

            if (!InputValidator.IsValidEmail(command.Email))
                return Fail("INVALID_EMAIL", "Некорректный формат email");

            var email = InputValidator.NormalizeEmail(command.Email);
            var code = _codeGenerator.GenerateAlphaNumericCode();
            var userExists = await _authRepository.RequestCode(email, code, cancellationToken);

            if (!userExists)
            {
                return new ApiResponse<string>
                {
                    Success = true,
                    Data = SuccessMessage
                };
            }

            try
            {
                await _emailSender.SendConfirmationCodeAsync(email, code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send auth code to {Email}", email);
                return Fail("EMAIL_SEND_FAILED", "Не удалось отправить код на email");
            }

            return new ApiResponse<string>
            {
                Success = true,
                Data = SuccessMessage
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
