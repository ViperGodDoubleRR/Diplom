using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

using Shared.Application.Contracts;

namespace PostService.Application.MediatR.FavoritePost
{
    public class FavoritePostCommand : IRequest<ApiResponse<bool>>
    {
        public Guid PostId { get; set; }
        public Guid UserId { get; set; }
    }
}
