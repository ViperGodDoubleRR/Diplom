namespace Shared.Application.Contracts.AuthJWT
{
    public sealed class PasswordResetTokenResult
    {
        public string Email { get; init; } = string.Empty;
    }
}
