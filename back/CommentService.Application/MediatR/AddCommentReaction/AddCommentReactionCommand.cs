using CommentService.Domain.Models;

using MediatR;

using Shared.Application.Contracts;

namespace CommentService.Application.MediatR.AddCommentReaction
{
    public class AddCommentReactionCommand : IRequest<ApiResponse<bool>>
    {
        public Guid CommentId { get; set; }
        public Guid UserId { get; set; }
        public ReactionType Type { get; set; }
    }
}
