using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

using Shared.Application.Contracts;

namespace AuthService.Application.MediatR.AuthRequestCode
{
    public class AuthRequestCodeCommand : IRequest<ApiResponse<string>>
    {
        public string Email;
        public AuthRequestCodeCommand(string email)
        {
            Email=email;
        }
    }
}
