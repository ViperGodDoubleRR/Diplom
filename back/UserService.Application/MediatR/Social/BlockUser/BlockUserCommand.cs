using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

using Shared.Application.Contracts;

namespace UserService.Application.MediatR.BlockUser
{
    public class BlockUserCommand
        : IRequest<ApiResponse<bool>>
    {
        public Guid MyId { get; set; }
        public Guid BlackId { get; set; }
    }
}
