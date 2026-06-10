namespace ChatService.Application.DTO
{
    public class ChatListItemDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string Type { get; set; } = string.Empty;
        public bool IsPublic { get; set; }
        public bool IsMember { get; set; }
        public string? MyRole { get; set; }
        public string? AvatarUrl { get; set; }

        public bool AvatarIsVideo { get; set; }
        public ChatUserDto? Companion { get; set; }
        public LastMessagePreviewDto? LastMessage { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
