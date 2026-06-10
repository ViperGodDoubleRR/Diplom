using CommentService.Application.DTO;

using MediatR;

using Shared.Application.Contracts;

namespace CommentService.Application.MediatR.GetCommentReplies
{
    public class GetCommentRepliesQuery : IRequest<ApiResponse<GetPostCommentsResponse>>
    {
        public Guid ParentCommentId { get; set; }
        public Guid CurrentUserId { get; set; }
        public int Offset { get; set; }
        public int Limit { get; set; } = 3;
    }
}
