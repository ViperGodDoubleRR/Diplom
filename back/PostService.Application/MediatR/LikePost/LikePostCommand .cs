using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

using Shared.Application.Contracts;

namespace PostService.Application.MediatR.LikePost
{
    public class LikePostCommand : IRequest<ApiResponse<bool>>
    {
        public Guid PostId { get; set; }
        public Guid UserId { get; set; }
    }
}
