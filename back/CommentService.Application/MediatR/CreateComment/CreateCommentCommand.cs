using CommentService.Application.DTO;

using MediatR;

using Shared.Application.Contracts;

namespace CommentService.Application.MediatR.CreateComment
{
    public class CreateCommentCommand : IRequest<ApiResponse<CreateCommentResponse>>
    {
        public Guid PostId { get; set; }
        public Guid? ParentId { get; set; }
        public Guid UserId { get; set; }
        public string Text { get; set; } = null!;
    }
}
