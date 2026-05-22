using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

using Shared.Application.Contracts;

namespace RegService.Application.Mediatr.CheckCode
{
    public class CheckCodeCommand:IRequest<ApiResponse<string>>
    {
        public string Email;
        public string Code;
        public CheckCodeCommand( string email,string code)
        {
            Code=code;
            Email=email;
        }
    }
}
