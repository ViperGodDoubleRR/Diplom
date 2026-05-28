using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

using PostService.Application.DTO;
using PostService.Application.DTO.PostService.Application.DTO;

using Shared.Application.Contracts;

namespace PostService.Application.MediatR.GetFavoritePosts
{
    public class GetFavoritePostsQuery : IRequest<ApiResponse<List<PostReactionCard>>>
    {
        public Guid UserId { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
