namespace RegService.Domain.IRepository
{
    public interface IRegRepository
    {
        Task<bool> CreateVerificationCode(string email, string code, CancellationToken cancellation);
        Task<bool> CheckCode(string email, string code, CancellationToken cancellationToken);
        Task<bool> IsEmailConfirmed(string email, CancellationToken cancellationToken);
        Task<bool> RegisterUser(Guid id, string email, string login, CancellationToken cancellationToken);
        Task<bool> EmailExistsAsync(
            string email,
            Guid? excludeUserId = null,
            CancellationToken cancellationToken = default);
        Task<bool> UpdateEmailAsync(
            Guid userId,
            string newEmail,
            CancellationToken cancellationToken = default);
    }
}
