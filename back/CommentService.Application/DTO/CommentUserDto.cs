namespace CommentService.Application.DTO
{
    public class CommentUserDto
    {
        public Guid Id { get; set; }
        public string Login { get; set; } = null!;
        public string Tag { get; set; } = null!;
        public string Avatar { get; set; } = string.Empty;
    }
}
