namespace Shared.MinIO.Models
{
    public class UploadFileRequest
    {
        public Stream File { get; set; } = default!;
        public string FileName { get; set; } = default!;
        public string ContentType { get; set; } = default!;
    }
}