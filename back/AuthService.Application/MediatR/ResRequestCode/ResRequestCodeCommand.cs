using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

using Shared.Application.Contracts;

namespace AuthService.Application.MediatR.ResRequestCode
{
    public class ResRequestCodeCommand : IRequest<ApiResponse<string>>
    {
        public string Email;
        public ResRequestCodeCommand(string email)
        {
            Email=email;
        }
    }
}
