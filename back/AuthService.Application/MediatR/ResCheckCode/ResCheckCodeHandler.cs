using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AuthService.Application.MediatR.ResRequestCode;
using AuthService.Domain.Interface;

using MediatR;

using Shared.Application.Contracts;

namespace AuthService.Application.MediatR.ResCheckCode
{
    public class ResCheckCodeHandler: IRequestHandler<ResCheckCodeCommand, ApiResponse<string>>
    {
        private readonly IResRepository _resRepository;
        public ResCheckCodeHandler(IResRepository resRepository)
        {
            _resRepository=resRepository;
        }
        public async Task<ApiResponse<string>> Handle(ResCheckCodeCommand command, CancellationToken cancellationToken)
        {
            ApiResponse<string> response = new ApiResponse<string>();
            try
            {
                var IsCheck = await _resRepository.CheckCode(command.Email, command.Code, cancellationToken);
                response.Success = IsCheck;
                if (response.Success)
                {
                    response.Data = command.Code;
                    
                }
                else
                {
                    response.Error = new ApiError
                    {
                        Code = "INVALID_CODE",
                        Message = "Неправильный код"
                    };
                }
                return response;
            }
            catch
            {
                response.Error = new ApiError
                {
                    Code = "SERVER_PROBLEM",
                    Message = "Проблема в сервере,повторите попытку позже"
                };
                return response;
            }
        }
    }
}
