using Microsoft.EntityFrameworkCore;

using RegService.Domain.IRepository;
using RegService.Domain.Models;
using RegService.Infrastructure.Data;

namespace RegService.Infrastructure.IEfRepository
{
    public class EfRegRepository : IRegRepository
    {
        private readonly DbContextReg _context;

        public EfRegRepository(DbContextReg context)
        {
            _context = context;
        }

        public async Task<bool> CreateVerificationCode(
            string email,
            string code,
            CancellationToken cancellationToken = default)
        {
            var exists = await _context.Users.AnyAsync(u => u.Email == email, cancellationToken);
            if (exists)
                return false;

            var oldCodes = await _context.VerificationCodes
                .Where(x => x.Email == email && !x.IsUsed)
                .ToListAsync(cancellationToken);

            foreach (var oldCode in oldCodes)
                oldCode.IsUsed = true;

            var verificationCode = new VerificationCode
            {
                Code = code,
                Email = email,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                IsUsed = false
            };

            await _context.VerificationCodes.AddAsync(verificationCode, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }

        public async Task<bool> CheckCode(
            string email,
            string code,
            CancellationToken cancellationToken = default)
        {
            var verificationCode = await _context.VerificationCodes
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync(
                    x => x.Email == email
                         && x.Code == code
                         && x.ExpiresAt > DateTime.UtcNow
                         && !x.IsUsed,
                    cancellationToken);

            if (verificationCode == null)
                return false;

            verificationCode.IsUsed = true;

            await _context.ConfirmedEmails.AddAsync(
                new ConfirmedEmail
                {
                    Email = email,
                    ConfirmedAt = DateTime.UtcNow
                },
                cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }

        public Task<bool> IsEmailConfirmed(
            string email,
            CancellationToken cancellationToken = default) =>
            _context.ConfirmedEmails.AnyAsync(
                x => x.Email == email && x.ConfirmedAt.AddMinutes(10) > DateTime.UtcNow,
                cancellationToken);

        public async Task<bool> RegisterUser(
            Guid id,
            string email,
            string login,
            CancellationToken cancellationToken)
        {
            var isConfirmed = await IsEmailConfirmed(email, cancellationToken);
            if (!isConfirmed)
                return false;

            var alreadyRegistered = await _context.Users
                .AnyAsync(x => x.Email == email, cancellationToken);

            if (alreadyRegistered)
                return false;

            await _context.ConfirmedEmails
                .Where(x => x.Email == email)
                .ExecuteDeleteAsync(cancellationToken);

            var user = new User
            {
                Id = id,
                Email = email,
                Login = login,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Users.AddAsync(user, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }

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
    }
}
