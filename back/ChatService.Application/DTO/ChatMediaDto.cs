namespace ChatService.Application.DTO
{
    public class ChatMediaDto
    {
        public int Id { get; set; }
        public string Url { get; set; } = string.Empty;
        public string MediaType { get; set; } = string.Empty;
        public string OriginalName { get; set; } = string.Empty;
        public long Size { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
