using AuthService.Domain.Interface;

using MediatR;

using Microsoft.Extensions.Configuration;

using Shared.Application.Contracts;
using Shared.Application.Contracts.AuthJWT;
using Shared.Application.Interfaces;
using Shared.Application.Validation;

namespace AuthService.Application.MediatR.ResRequestCode
{
    public class ResRequestCodeHandler : IRequestHandler<ResRequestCodeCommand, ApiResponse<string>>
    {
        private const string SuccessMessage = "CODE_SENT";

        private readonly IResRepository _resRepository;
        private readonly ICodeGenerate _codeGenerator;
        private readonly IEmailSender _emailSender;

        public ResRequestCodeHandler(
            IResRepository resRepository,
            ICodeGenerate codeGenerator,
            IEmailSender emailSender)
        {
            _resRepository = resRepository;
            _codeGenerator = codeGenerator;
            _emailSender = emailSender;
        }

        public async Task<ApiResponse<string>> Handle(
            ResRequestCodeCommand command,
            CancellationToken cancellationToken)
        {
            if (!InputValidator.IsValidEmail(command.Email))
            {
                return Fail("INVALID_EMAIL", "Некорректный email");
            }

            var email = InputValidator.NormalizeEmail(command.Email);
            var code = _codeGenerator.GenerateAlphaNumericCode();
            var userExists = await _resRepository.ResRequestCode(email, code, cancellationToken);

            if (userExists)
            {
                await _emailSender.SendConfirmationCodeAsync(email, code);
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
