using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

using Shared.Application.Contracts;

namespace UserService.Application.MediatR.SendEmail
{
    public class SendEmailCommandCode : IRequest<ApiResponse<string>>
    {
        public string Email;
        public SendEmailCommandCode(string email)
        {
            Email=email;
        }
    }
}
