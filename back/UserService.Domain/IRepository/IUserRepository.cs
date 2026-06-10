using UserService.Domain.Models;

namespace UserService.Domain.IRepository
{
    public interface IUserRepository
    {
        Task CreateUser(User user, CancellationToken cancellationToken = default);
        Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<User?> GetByLoginAsync(string login, CancellationToken cancellationToken = default);
        Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> EmailExistsAsync(
            string email,
            Guid? excludeUserId = null,
            CancellationToken cancellationToken = default);
        Task<bool> UpdateEmailAsync(
            Guid userId,
            string newEmail,
            CancellationToken cancellationToken = default);
        Task UpdateAsync(User user, CancellationToken cancellationToken = default);
    }
}
