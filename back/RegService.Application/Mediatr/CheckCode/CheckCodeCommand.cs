using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

namespace RegService.Application.Mediatr.CheckCode
{
    public class CheckCodeCommand:IRequest<bool>
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
