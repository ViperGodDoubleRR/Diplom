namespace AuthService.Api.DTO
{
    public class ChangeEmailRequest
    {
        public string NewEmail { get; set; } = string.Empty;
        public string CurrentPassword { get; set; } = string.Empty;
    }
}
