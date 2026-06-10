namespace AuthService.Api.DTO
{
    public class ChangePasswordRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ResetToken { get; set; } = string.Empty;
    }
}
