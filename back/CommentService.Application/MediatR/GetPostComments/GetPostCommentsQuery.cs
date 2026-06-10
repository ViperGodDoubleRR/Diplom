using CommentService.Application.DTO;

using MediatR;

using Shared.Application.Contracts;

namespace CommentService.Application.MediatR.GetPostComments
{
    public class GetPostCommentsQuery : IRequest<ApiResponse<GetPostCommentsResponse>>
    {
        public Guid PostId { get; set; }
        public Guid CurrentUserId { get; set; }
        public int Offset { get; set; }
        public int Limit { get; set; } = 50;
    }
}
