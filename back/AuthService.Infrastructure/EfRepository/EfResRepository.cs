using AuthService.Domain.Interface;
using AuthService.Domain.Models;
using AuthService.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.EfRepository
{
    public class EfResRepository : IResRepository
    {
        private readonly DbContextAuth _context;
        private readonly IAuthRepository _authRepository;

        public EfResRepository(DbContextAuth context, IAuthRepository authRepository)
        {
            _context = context;
            _authRepository = authRepository;
        }

        public async Task<bool> ResRequestCode(
            string email,
            string code,
            CancellationToken cancellationToken = default)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Email == email, cancellationToken);

            if (user == null)
                return false;

            await InvalidateResetCodes(email, cancellationToken);

            var resetCode = new ResetCode
            {
                ResCodeUserId = user.Id,
                Code = code,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                IsUsed = false
            };

            await _context.ResetCodes.AddAsync(resetCode, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }

        public async Task<bool> CheckCode(
            string email,
            string code,
            CancellationToken cancellationToken = default)
        {
            var resetCode = await _context.ResetCodes
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync(
                    x => x.UserID.Email == email
                         && x.Code == code
                         && x.ExpiresAt > DateTime.UtcNow
                         && !x.IsUsed,
                    cancellationToken);

            if (resetCode == null)
                return false;

            resetCode.IsUsed = true;
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }

        public async Task<bool> ResetPassword(
            string email,
            string passwordHash,
            CancellationToken cancellationToken = default)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Email == email, cancellationToken);

            if (user == null)
                return false;

            user.PasswordHash = passwordHash;

            await _authRepository.RevokeAllUserSessions(user.Id, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }

        public async Task InvalidateResetCodes(
            string email,
            CancellationToken cancellationToken = default)
        {
            var codes = await _context.ResetCodes
                .Where(x => x.UserID.Email == email && !x.IsUsed)
                .ToListAsync(cancellationToken);

            foreach (var code in codes)
                code.IsUsed = true;

            if (codes.Count > 0)
                await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
