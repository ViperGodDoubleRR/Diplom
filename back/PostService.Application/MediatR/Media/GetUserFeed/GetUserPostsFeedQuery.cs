using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

using PostService.Application.DTO;

using Shared.Application.Contracts;

namespace PostService.Application.MediatR.Media.GetUserFeed
{
    public class GetUserPostsFeedQuery
    : IRequest<ApiResponse<List<PostFullDto>>>
    {
        public Guid UserId { get; set; }

        public Guid CurrentUserId { get; set; }

        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
