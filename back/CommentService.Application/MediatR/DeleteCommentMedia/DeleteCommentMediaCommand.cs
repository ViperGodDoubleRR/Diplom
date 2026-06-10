using MediatR;

using Shared.Application.Contracts;

namespace CommentService.Application.MediatR.DeleteCommentMedia
{
    public class DeleteCommentMediaCommand : IRequest<ApiResponse<bool>>
    {
        public Guid CommentId { get; set; }
        public Guid UserId { get; set; }
    }
}
