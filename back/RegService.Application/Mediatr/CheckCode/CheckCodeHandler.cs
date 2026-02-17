using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

using RegService.Domain.IRepository;

namespace RegService.Application.Mediatr.CheckCode
{
    public class CheckCodeHandler:IRequestHandler<CheckCodeCommand,bool>
    {
        private readonly IRegRepository _regRepository;
        public CheckCodeHandler(IRegRepository regpository)
        {
            _regRepository = regpository;
        }
        public async Task<bool> Handle(CheckCodeCommand command,CancellationToken cancellationToken)
        {
            return await _regRepository.CheckCode(command.Email, command.Code,cancellationToken);
        }
    }
}
