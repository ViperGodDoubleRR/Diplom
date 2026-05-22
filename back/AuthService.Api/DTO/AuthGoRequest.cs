namespace AuthService.Api.DTO
{
    public class AuthGoRequest
    {
        public string Email { get; set; }=string.Empty;
        public string Password { get; set; }=string.Empty;
        public string Code {  get; set; }=string.Empty ;
        public string DeviceInfo { get; set; }=string .Empty ;
    }
}
