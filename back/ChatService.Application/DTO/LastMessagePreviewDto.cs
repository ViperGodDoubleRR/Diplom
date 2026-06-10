namespace ChatService.Application.DTO
{
    public class LastMessagePreviewDto
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public string UserLogin { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsDeleted { get; set; }
    }
}
