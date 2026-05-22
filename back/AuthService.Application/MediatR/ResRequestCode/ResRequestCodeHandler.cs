using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AuthService.Domain.Interface;

using MediatR;

using Shared.Application.Contracts;
using Shared.Application.Interfaces;

namespace AuthService.Application.MediatR.ResRequestCode
{
    public class ResRequestCodeHandler : IRequestHandler<ResRequestCodeCommand, ApiResponse<string>>
    {
        private readonly IResRepository _resRepository;
        private readonly ICodeGenerate _code;
        public ResRequestCodeHandler(IResRepository respository,ICodeGenerate code)
        {
            _resRepository = respository;
            _code = code;
        }
        public async Task<ApiResponse<string>> Handle(ResRequestCodeCommand command, CancellationToken cancellationToken)
        {
            var code = _code.GenerateAlphaNumericCode();

            bool isBool = await _resRepository
                .ResRequestCode(command.Email, code, cancellationToken);

            ApiResponse<string> response = new ApiResponse<string>();
            response.Success = isBool;
            if (response.Success)
            {
                response.Data = command.Email;
            }
            else
            {
                response.Error = new ApiError
                {
                    Code = "EMAIL_NOT_FOUND",
                    Message = "Неккоректный email"
                };
            }
            return response;
        }
    }
}
