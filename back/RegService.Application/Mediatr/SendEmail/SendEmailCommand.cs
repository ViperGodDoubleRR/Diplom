using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

namespace RegService.Application.Mediatr.SendEmail
{
    public class SendEmailCommandCode:IRequest<string>
    {
        public string Email;
        public SendEmailCommandCode(string email)
        {
            Email=email;    
        }
    }
}
