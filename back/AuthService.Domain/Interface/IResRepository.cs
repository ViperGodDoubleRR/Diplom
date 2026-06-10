namespace AuthService.Domain.Interface
{
    public interface IResRepository
    {
        Task<bool> ResRequestCode(string email, string code, CancellationToken cancellation);
        Task<bool> CheckCode(string email, string code, CancellationToken cancellationToken);
        Task<bool> ResetPassword(string email, string passwordHash, CancellationToken cancellationToken);
        Task InvalidateResetCodes(string email, CancellationToken cancellationToken = default);
    }
}
