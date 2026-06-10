using MediatR;

using PostService.Application.DTO;

using Shared.Application.Contracts;

namespace PostService.Application.MediatR.GetLikedPosts
{
    public class GetLikedPostsQuery : IRequest<ApiResponse<List<PostReactionCard>>>
    {
        public Guid UserId { get; set; }

        public int Page { get; set; }

        public int PageSize { get; set; }
    }
}
