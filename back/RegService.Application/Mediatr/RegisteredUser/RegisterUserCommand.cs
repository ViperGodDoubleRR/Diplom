using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

namespace RegService.Application.Mediatr.RegisteredUser
{
    public class RegisterUserCommand:IRequest<string>
    {
        public string Email;
        public string Login;
        public string Password;
        public RegisterUserCommand(string email, string login, string password)
        {
            Email = email;
            Login = login;
            Password= password;
        }
    }
}
