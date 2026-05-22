using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RegService.Domain.IRepository;
using MediatR;
using Shared.Application.Interfaces;
using Shared.Application.Contracts;

namespace RegService.Application.Mediatr.SendEmail
{
    public class SendEmailHandler : IRequestHandler<SendEmailCommandCode, ApiResponse<string>>
    {
        private readonly IRegRepository _regRepository;
        private readonly ICodeGenerate _code;
        private readonly IEmailSender _emailSender;
        public SendEmailHandler(IRegRepository regRepository, ICodeGenerate code, IEmailSender emailsender)
        {
            _regRepository = regRepository;
            _code = code;
            _emailSender = emailsender;
        }

        public async Task<ApiResponse<string>> Handle(SendEmailCommandCode command, CancellationToken cancellationToken)
        {
            var code = _code.GenerateAlphaNumericCode();
            var success = await _regRepository.CreateVerificationCode(command.Email, code, cancellationToken);
            var apiResponse = new ApiResponse<string>();
            apiResponse.Success = success;
            if (success)
            {
                await _emailSender.SendConfirmationCodeAsync(command.Email, code);
                apiResponse.Data = command.Email;
            }
            else
            {
                apiResponse.Error =  new ApiError
                {
                    Code ="IS_EMAIL_BUSY",
                    Message = "Email занят,Попробуйте другой"
                }; 
            }
            return apiResponse;
        }
    }
}
