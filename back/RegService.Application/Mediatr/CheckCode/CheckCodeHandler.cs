using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

using RegService.Domain.IRepository;

using Shared.Application.Contracts;
using Shared.Application.Interfaces;

namespace RegService.Application.Mediatr.CheckCode
{
    public class CheckCodeHandler:IRequestHandler<CheckCodeCommand, ApiResponse<string>>
    {
        private readonly IRegRepository _regRepository;
        public CheckCodeHandler(IRegRepository regpository)
        {
            _regRepository = regpository;
        }
        public async Task<ApiResponse<string>> Handle(CheckCodeCommand command,CancellationToken cancellationToken)
        {
            ApiResponse<string> response = new ApiResponse<string>();
            response.Success = await _regRepository.CheckCode(command.Email, command.Code,cancellationToken);
            if (response.Success)
            {
                response.Data = command.Email;
            }
            else
            {
                response.Error =  new ApiError
                {
                    Code ="INVALID_CODE",
                    Message = "Неверный код, попробуйте заново"
                };
            }
            return response;
        }
    }
}
