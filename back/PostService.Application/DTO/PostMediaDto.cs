namespace PostService.Application.DTO
{
    public class PostMediaDto
    {
        public Guid Id { get; set; }
        public string Url { get; set; } = null!;
        public string FileKey { get; set; } = null!;
        public string Bucket { get; set; } = null!;
        public string MediaType { get; set; } = null!;
    }
}
