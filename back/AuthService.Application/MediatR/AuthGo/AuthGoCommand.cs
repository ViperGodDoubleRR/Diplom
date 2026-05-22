using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

using Shared.Application.Contracts;
using Shared.Application.Contracts.AuthJWT;

namespace AuthService.Application.MediatR.AuthGo
{
    public class AuthGoCommand : IRequest<ApiResponse<AuthGoResponse>>
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Code { get; set; }
        public string DeviceInfo { get; set; }

        public string IpAddress { get; set; }
        public AuthGoCommand(string email, string password, string code, string deviceInfo, string ipAddress)
        {
            Email = email;
            Password = password;
            Code = code;
            DeviceInfo = deviceInfo;
            IpAddress = ipAddress;
        }

    }
}
