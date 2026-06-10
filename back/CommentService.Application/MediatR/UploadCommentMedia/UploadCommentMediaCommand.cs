using CommentService.Application.DTO;

using MediatR;

using Microsoft.AspNetCore.Http;

using Shared.Application.Contracts;

namespace CommentService.Application.MediatR.UploadCommentMedia
{
    public class UploadCommentMediaCommand : IRequest<ApiResponse<CommentMediaDto>>
    {
        public Guid CommentId { get; set; }
        public Guid UserId { get; set; }
        public IFormFile File { get; set; } = null!;
        public string MediaType { get; set; } = null!;
    }
}
