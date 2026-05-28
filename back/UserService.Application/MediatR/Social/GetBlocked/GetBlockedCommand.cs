using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

using Shared.Application.Contracts;

using UserService.Application.DTO;

namespace UserService.Application.MediatR.GetBlocked
{
    public class GetBlockedCommand
        : IRequest<ApiResponse<List<BlackListDto>>>
    {
        public Guid UserId { get; set; }
    }
}
