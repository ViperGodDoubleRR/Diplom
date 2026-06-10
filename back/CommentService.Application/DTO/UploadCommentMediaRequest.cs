using Microsoft.AspNetCore.Http;

namespace CommentService.Application.DTO
{
    public class UploadCommentMediaRequest
    {
        public IFormFile File { get; set; } = null!;
        public string MediaType { get; set; } = null!;
    }
}
