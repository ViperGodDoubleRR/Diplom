namespace AuthService.Api.DTO
{
    public class ChangePasswordSettingsRequest
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string? RefreshToken { get; set; }
    }
}
