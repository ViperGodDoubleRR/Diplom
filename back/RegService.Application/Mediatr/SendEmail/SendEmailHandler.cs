using MediatR;

using Microsoft.Extensions.Logging;

using RegService.Domain.IRepository;

using Shared.Application.Contracts;
using Shared.Application.Interfaces;
using Shared.Application.Validation;

namespace RegService.Application.Mediatr.SendEmail
{
    public class SendEmailHandler : IRequestHandler<SendEmailCommandCode, ApiResponse<string>>
    {
        private readonly IRegRepository _regRepository;
        private readonly ICodeGenerate _codeGenerator;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<SendEmailHandler> _logger;

        public SendEmailHandler(
            IRegRepository regRepository,
            ICodeGenerate codeGenerator,
            IEmailSender emailSender,
            ILogger<SendEmailHandler> logger)
        {
            _regRepository = regRepository;
            _codeGenerator = codeGenerator;
            _emailSender = emailSender;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(
            SendEmailCommandCode command,
            CancellationToken cancellationToken)
        {
            if (!InputValidator.IsValidEmail(command.Email))
            {
                return Fail("INVALID_EMAIL", "Некорректный email");
            }

            var email = InputValidator.NormalizeEmail(command.Email);
            var code = _codeGenerator.GenerateAlphaNumericCode();

            var canSend = await _regRepository.CreateVerificationCode(email, code, cancellationToken);
            if (!canSend)
            {
                return Fail("EMAIL_BUSY", "Email уже зарегистрирован");
            }

            try
            {
                await _emailSender.SendConfirmationCodeAsync(email, code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send verification email to {Email}", email);
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
