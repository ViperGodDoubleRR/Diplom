namespace AuthService.Api.DTO
{
    public class ChangeEmailConfirmRequest
    {
        public string NewEmail { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string CurrentPassword { get; set; } = string.Empty;
    }
}
