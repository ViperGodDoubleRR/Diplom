namespace CommentService.Application.DTO
{
    public class CommentMediaDto
    {
        public int Id { get; set; }
        public string Url { get; set; } = null!;
        public string MediaType { get; set; } = null!;
        public string OriginalName { get; set; } = null!;
    }
}
