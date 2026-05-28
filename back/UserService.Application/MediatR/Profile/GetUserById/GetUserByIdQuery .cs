using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

using Shared.Application.Contracts;

using UserService.Application.DTO;
using UserService.Domain.Models;

namespace UserService.Application.MediatR.Profile.GetUserById
{
    public class GetUserByIdQuery : IRequest <ApiResponse<UserViewDto>>
    {
        public Guid UserId { get; set; }
    }
}
