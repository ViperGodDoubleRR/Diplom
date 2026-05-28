using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

using Shared.Application.Contracts;

namespace UserService.Application.MediatR.DeleteAvatar
{
    public class DeleteMediaCommand : IRequest<ApiResponse<bool>>
    {
        public Guid UserId { get; set; }
        public int MediaId { get; set; }
    }
}
