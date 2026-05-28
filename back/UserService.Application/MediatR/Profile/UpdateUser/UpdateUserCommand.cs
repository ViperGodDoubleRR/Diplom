using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

using Shared.Application.Contracts;

using UserService.Application.DTO;
using UserService.Domain.Models;
namespace UserService.Application.MediatR.UpdateUser
{
    public class UpdateUserCommand
       : IRequest<ApiResponse<User>>
    {
        public Guid UserId { get; set; }

        public UpdateUserDto Dto { get; set; } = null!;
    }
}
