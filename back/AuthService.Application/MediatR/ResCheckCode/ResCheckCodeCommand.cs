using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AuthService.Domain.Interface;

using MediatR;

using Shared.Application.Contracts;

namespace AuthService.Application.MediatR.ResCheckCode
{
    public class ResCheckCodeCommand:IRequest<ApiResponse<string>>
    {
        public string Email { get; set; }
        public string Code { get; set; }
        public ResCheckCodeCommand(string email,string code)
        {
            Email = email; 
            Code = code;
        }
    }
}
