using AuthService.Domain.Interface;
using AuthService.Domain.Models;
using AuthService.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.EfRepository
{
    public class EfAuthRepository : IAuthRepository
    {
        private readonly DbContextAuth _context;

        public EfAuthRepository(DbContextAuth context)
        {
            _context = context;
        }

        public async Task<Guid?> CreateUser(string email, string login, string passwordHash)
        {
            if (await EmailExists(email) || await LoginExists(login))
                return null;

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                Login = login,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return user.Id;
        }

        public Task<bool> EmailExists(string email, CancellationToken cancellationToken = default) =>
            _context.Users.AnyAsync(x => x.Email == email, cancellationToken);

        public Task<bool> LoginExists(string login, CancellationToken cancellationToken = default) =>
            _context.Users.AnyAsync(x => x.Login == login, cancellationToken);

        public async Task<bool> RequestCode(
            string email,
            string code,
            CancellationToken cancellationToken = default)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Email == email, cancellationToken);

            if (user == null)
                return false;

            await InvalidateVerificationCodes(email, cancellationToken);

            var verificationCode = new VerificationCode
            {
                CodeUserId = user.Id,
                Code = code,
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
                    x => x.UserID.Email == email
                         && x.Code == code
                         && x.ExpiresAt > DateTime.UtcNow
                         && !x.IsUsed,
                    cancellationToken);

            if (verificationCode == null)
                return false;

            verificationCode.IsUsed = true;
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }

        public Task<User?> GetUserByEmail(string email, CancellationToken cancellationToken = default) =>
            _context.Users.FirstOrDefaultAsync(x => x.Email == email, cancellationToken);

        public Task<User?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
            _context.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

        public async Task<bool> CreateVerificationCodeForUserAsync(
            Guid userId,
            string code,
            CancellationToken cancellationToken = default)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

            if (user is null)
                return false;

            var activeCodes = await _context.VerificationCodes
                .Where(x => x.CodeUserId == userId && !x.IsUsed)
                .ToListAsync(cancellationToken);

            foreach (var activeCode in activeCodes)
                activeCode.IsUsed = true;

            var verificationCode = new VerificationCode
            {
                CodeUserId = userId,
                Code = code,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                IsUsed = false
            };

            await _context.VerificationCodes.AddAsync(verificationCode, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }

        public async Task<bool> VerifyUserCodeAsync(
            Guid userId,
            string code,
            CancellationToken cancellationToken = default)
        {
            var verificationCode = await _context.VerificationCodes
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync(
                    x => x.CodeUserId == userId
                         && x.Code == code
                         && x.ExpiresAt > DateTime.UtcNow
                         && !x.IsUsed,
                    cancellationToken);

            if (verificationCode is null)
                return false;

            verificationCode.IsUsed = true;
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }

        public Task<bool> IsEmailUsedByUserHistoryAsync(
            Guid userId,
            string email,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(false);

        public async Task<bool> UpdateUserEmailAsync(
            Guid userId,
            string newEmail,
            CancellationToken cancellationToken = default)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

            if (user is null)
                return false;

            user.Email = newEmail;
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }

        public async Task<bool> UpdateUserPasswordAsync(
            Guid userId,
            string passwordHash,
            CancellationToken cancellationToken = default)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

            if (user is null)
                return false;

            user.PasswordHash = passwordHash;
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }

        public Task<List<UserSession>> GetActiveSessionsByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default) =>
            _context.UserSessions
                .Where(x => x.UserId == userId && !x.IsRevoked && x.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);

        public async Task<bool> RevokeSessionAsync(
            int sessionId,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var session = await _context.UserSessions
                .FirstOrDefaultAsync(
                    x => x.Id == sessionId && x.UserId == userId && !x.IsRevoked,
                    cancellationToken);

            if (session is null)
                return false;

            session.IsRevoked = true;
            session.RevokedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }

        public Task<UserSession?> FindActiveSessionByFingerprintAsync(
            string fingerprint,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<UserSession?>(null);

        public async Task<int> RevokeOtherSessionsAsync(
            Guid userId,
            int? exceptSessionId,
            CancellationToken cancellationToken = default)
        {
            var sessions = await _context.UserSessions
                .Where(x => x.UserId == userId && !x.IsRevoked)
                .ToListAsync(cancellationToken);

            var revoked = 0;

            foreach (var session in sessions)
            {
                if (exceptSessionId.HasValue && session.Id == exceptSessionId.Value)
                    continue;

                session.IsRevoked = true;
                session.RevokedAt = DateTime.UtcNow;
                revoked++;
            }

            if (revoked > 0)
                await _context.SaveChangesAsync(cancellationToken);

            return revoked;
        }

        public async Task<bool> CanRequestVerificationCodeAsync(
            Guid userId,
            TimeSpan cooldown,
            CancellationToken cancellationToken = default)
        {
            var latest = await _context.VerificationCodes
                .Where(x => x.CodeUserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => x.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (latest == default)
                return true;

            return latest.Add(cooldown) <= DateTime.UtcNow;
        }

        public async Task CreateSession(
            Guid userId,
            string refreshTokenHash,
            string? tokenFingerprint,
            string deviceInfo,
            string ipAddress,
            DateTime expiresAt,
            CancellationToken cancellationToken = default)
        {
            var session = new UserSession
            {
                UserId = userId,
                RefreshToken = refreshTokenHash,
                DeviceInfo = deviceInfo,
                IpAddress = ipAddress,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = expiresAt,
                IsRevoked = false
            };

            await _context.UserSessions.AddAsync(session, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateSessionRefreshTokenAsync(
            int sessionId,
            string newRefreshTokenHash,
            string? tokenFingerprint,
            DateTime expiresAt,
            CancellationToken cancellationToken = default)
        {
            var session = await _context.UserSessions
                .FirstOrDefaultAsync(x => x.Id == sessionId, cancellationToken);

            if (session == null)
                return;

            session.RefreshToken = newRefreshTokenHash;
            session.ExpiresAt = expiresAt;

            await _context.SaveChangesAsync(cancellationToken);
        }

        public Task<List<UserSession>> GetActiveSessions(CancellationToken cancellationToken = default) =>
            _context.UserSessions
                .Include(x => x.User)
                .Where(x => !x.IsRevoked && x.ExpiresAt > DateTime.UtcNow)
                .ToListAsync(cancellationToken);

        public async Task RevokeAllUserSessions(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var sessions = await _context.UserSessions
                .Where(x => x.UserId == userId && !x.IsRevoked)
                .ToListAsync(cancellationToken);

            foreach (var session in sessions)
            {
                session.IsRevoked = true;
                session.RevokedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task InvalidateVerificationCodes(
            string email,
            CancellationToken cancellationToken = default)
        {
            var codes = await _context.VerificationCodes
                .Where(x => x.UserID.Email == email && !x.IsUsed)
                .ToListAsync(cancellationToken);

            foreach (var code in codes)
                code.IsUsed = true;

            if (codes.Count > 0)
                await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
