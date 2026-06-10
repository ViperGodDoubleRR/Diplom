using CommentService.Domain.Models;

namespace CommentService.Application.DTO
{
    public class AddCommentReactionRequest
    {
        public ReactionType Type { get; set; }
    }
}
