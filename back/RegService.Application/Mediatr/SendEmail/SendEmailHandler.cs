using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RegService.Domain.IRepository;
using MediatR;
using Shared.Application.Interfaces;

namespace RegService.Application.Mediatr.SendEmail
{
    public class SendEmailHandler : IRequestHandler<SendEmailCommandCode, string>
    {
        private readonly IRegRepository _regRepository;
        private readonly ICodeGenerate _code;
        private readonly IEmailSender _emailSender;
        public SendEmailHandler(IRegRepository regRepository, ICodeGenerate code,IEmailSender emailsender)
        {
            _regRepository = regRepository;
            _code = code;
            _emailSender = emailsender;
        }

        public async Task<string> Handle(SendEmailCommandCode command, CancellationToken cancellationToken)
        {
            var code = _code.GenerateAlphaNumericCode();
            var success = await _regRepository.CreateVerificationCode(command.Email,code, cancellationToken);
            if (success)
            {
                await _emailSender.SendConfirmationCodeAsync(command.Email, code);
                return "Код отправлен на email";
            }
            else
                return " Email уже занят,воспользуйтесь другим";
        }
    }
}
