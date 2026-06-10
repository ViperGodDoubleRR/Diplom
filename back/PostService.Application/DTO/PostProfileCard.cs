namespace PostService.Application.DTO
{
    public class PostProfileCard
    {
        public Guid Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public string? MediaUrl { get; set; }
        public string? MediaType { get; set; }

        public int LikesCount { get; set; }
    }
}
