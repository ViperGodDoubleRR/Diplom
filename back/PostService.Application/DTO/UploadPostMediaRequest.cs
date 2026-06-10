using Microsoft.AspNetCore.Http;

namespace PostService.Application.DTO
{
    public class UploadPostMediaRequest
    {
        public IFormFile File { get; set; } = null!;
        public string MediaType { get; set; } = null!;
    }
}
