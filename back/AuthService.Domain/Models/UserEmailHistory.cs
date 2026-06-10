namespace AuthService.Domain.Models
{
    public class UserEmailHistory
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public string Email { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; }
    }
}
