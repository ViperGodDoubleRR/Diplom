using CommentService.Application.DTO;

using MediatR;

using Shared.Application.Contracts;

namespace CommentService.Application.MediatR.GetPostComments
{
    public class GetPostCommentsQuery : IRequest<ApiResponse<List<CommentDto>>>
    {
        public Guid PostId { get; set; }
    }
}
