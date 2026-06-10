namespace UserService.Application.DTO
{
    /// <summary>
    /// Профиль текущего пользователя (с email — только для авторизованного владельца).
    /// </summary>
    public class UserProfileDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
        public string? Tag { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
