namespace UserService.Application.DTO
{
    public class UpdateUserDto
    {
        public string Login { get; set; } = string.Empty;
        public string? Tag { get; set; }
        public string? Description { get; set; }
    }
}
