using MediatR;

using Shared.Application.Contracts;

namespace CommentService.Application.MediatR.DeleteComment
{
    public class DeleteCommentCommand : IRequest<ApiResponse<bool>>
    {
        public Guid CommentId { get; set; }
        public Guid UserId { get; set; }
    }
}
