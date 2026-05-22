using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AuthService.Application.MediatR.ResRequestCode;
using AuthService.Domain.Interface;

using MediatR;

using Shared.Application.Contracts;
using Shared.Application.Interfaces;

namespace AuthService.Application.MediatR.ResPassword
{
    public class ResPasswordHandler :IRequestHandler<ResPasswordCommand,ApiResponse<string>>
    {
        private readonly IResRepository _resRepository;
        private readonly IHasher _hasher;
        public ResPasswordHandler(IResRepository respository,IHasher hash)
        {
            _resRepository = respository;
            _hasher = hash;
        }
        public async Task<ApiResponse<string>> Handle(ResPasswordCommand command, CancellationToken cancellationToken)
        {
            var password = _hasher.Hash(command.NewPassword);
            var IsBool = await _resRepository.ResetPassword(command.Email,password, cancellationToken);
            ApiResponse<string> response = new ApiResponse<string>();
            response.Success = IsBool;
            if (response.Success)
            {
                response.Data = command.Email;
            }
            else
            {
                response.Error = new ApiError
                {
                    Code = "RESET_PASSWORD_FALSH",
                    Message = "Проблемы на сервере,попробуйте позже"
                };
            }
            return response;
        }
    }
}
