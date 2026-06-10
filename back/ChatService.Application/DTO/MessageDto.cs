namespace ChatService.Application.DTO
{
    public class MessageDto
    {
        public int Id { get; set; }
        public int ChatId { get; set; }
        public ChatUserDto User { get; set; } = new();
        public string Text { get; set; } = string.Empty;
        public int? ReplyToMessageId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsEdited { get; set; }
        public bool IsDeleted { get; set; }
        public List<MessageMediaDto> Media { get; set; } = [];
    }
}
