using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AuthService.Application.MediatR.ResRequestCode;
using AuthService.Domain.Interface;

using MediatR;

using Shared.Application.Contracts;
using Shared.Application.Interfaces;

namespace AuthService.Application.MediatR.AuthRequestCode
{
    public class AuthRequestCodeHandler : IRequestHandler<AuthRequestCodeCommand, ApiResponse<string>>
    {
        private readonly IAuthRepository _authRepository;
        private readonly ICodeGenerate _code;
        private readonly IEmailSender _emailSender;
        public AuthRequestCodeHandler(
            IAuthRepository respository,
            ICodeGenerate code,
            IEmailSender emailSender)
        {
            _authRepository = respository;
            _code = code;
            _emailSender = emailSender;
        }
        public async Task<ApiResponse<string>> Handle(AuthRequestCodeCommand command, CancellationToken cancellationToken)
        {
            var code = _code.GenerateAlphaNumericCode();

            bool isBool = await _authRepository
                .RequestCode(command.Email, code, cancellationToken);

            ApiResponse<string> response = new ApiResponse<string>();
            response.Success = isBool;
            if (response.Success)
            {
                await _emailSender.SendConfirmationCodeAsync(command.Email, code);
                response.Data = command.Email;
            }
            else
            {
                response.Error = new ApiError
                {
                    Code = "EMAIL_NOT_FOUND",
                    Message = "Неккоректный email"
                };
            }
            return response;
        }
    }
}
