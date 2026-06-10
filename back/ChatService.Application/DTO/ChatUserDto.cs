namespace ChatService.Application.DTO
{
    public class ChatUserDto
    {
        public Guid Id { get; set; }
        public string Login { get; set; } = string.Empty;
        public string Tag { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;

        public bool AvatarIsVideo { get; set; }
    }
}
