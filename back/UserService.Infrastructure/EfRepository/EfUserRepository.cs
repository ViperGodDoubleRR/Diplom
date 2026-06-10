using Microsoft.EntityFrameworkCore;

using UserService.Domain.IRepository;
using UserService.Domain.Models;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.EfRepository
{
    public class EfUserRepository : IUserRepository
    {
        private readonly DbContextUser _context;

        public EfUserRepository(DbContextUser context)
        {
            _context = context;
        }

        public async Task CreateUser(User user, CancellationToken cancellationToken = default)
        {
            await _context.Users.AddAsync(user, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            _context.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        public Task<User?> GetByLoginAsync(string login, CancellationToken cancellationToken = default) =>
            _context.Users.FirstOrDefaultAsync(x => x.Login == login, cancellationToken);

        public Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            _context.Users.AnyAsync(x => x.Id == id, cancellationToken);

        public Task<bool> EmailExistsAsync(
            string email,
            Guid? excludeUserId = null,
            CancellationToken cancellationToken = default)
        {
            var normalized = email.Trim().ToLowerInvariant();

            return excludeUserId.HasValue
                ? _context.Users.AnyAsync(
                    x => x.Email == normalized && x.Id != excludeUserId.Value,
                    cancellationToken)
                : _context.Users.AnyAsync(x => x.Email == normalized, cancellationToken);
        }

        public async Task<bool> UpdateEmailAsync(
            Guid userId,
            string newEmail,
            CancellationToken cancellationToken = default)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

            if (user is null)
                return false;

            user.Email = newEmail.Trim().ToLowerInvariant();
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }

        public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
