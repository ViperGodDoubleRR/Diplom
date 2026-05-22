using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

using Shared.Application.Contracts;

namespace AuthService.Application.MediatR.ResPassword
{
    public class ResPasswordCommand :IRequest<ApiResponse<string>>
    {
        public string Email { get; set;  }
        public string NewPassword { get; set; }
        public ResPasswordCommand(string email, string newPassword)
        {
            Email=email;
            NewPassword=newPassword;
        }
    }
}
