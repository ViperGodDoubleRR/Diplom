using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

using Shared.Application.Contracts;

using UserService.Domain.Models;

namespace UserService.Application.MediatR.UserCommand
{
    public class GetUserCommand : IRequest<ApiResponse<User>>
    {
        public Guid UserId { get; set; }
    }
}
