using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AuthService.Domain.Interface;
using AuthService.Domain.Models;
using AuthService.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;

using RabbitMQ.Client;

namespace AuthService.Infrastructure.EfRepository
{
    public class EfAuthRepository : IAuthRepository
    {
        private readonly DbContextAuth _context;
        public EfAuthRepository(DbContextAuth context)
        {
            _context=context;
        }
        public async Task<Guid> CreateUser(string email, string login, string password)
        {
            User user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                Login = login,
                PasswordHash = password,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return user.Id;
        }
        public async Task<bool> RequestCode(string email, string code, CancellationToken cancellationToken = default)
        {
            var isEmail = await _context.Users
                .FirstOrDefaultAsync(
                    x => x.Email == email,
                    cancellationToken);

            if (isEmail == null)
                return false;

            VerificationCode verificationcode = new VerificationCode
            {
                CodeUserId = isEmail.Id,
                Code = code,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                IsUsed = false
            };

            await _context.VerificationCodes
                .AddAsync(verificationcode, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
        public async Task<bool> CheckCode(string email, string code, CancellationToken cancellationToken = default)
        {

            var IsCheck = await _context.VerificationCodes
         .OrderByDescending(x => x.CreatedAt)
        .FirstOrDefaultAsync(
         x => x.UserID.Email == email && x.Code == code && x.ExpiresAt > DateTime.UtcNow
         && x.IsUsed == false, cancellationToken);

            if (IsCheck == null)
                return false;

            IsCheck.IsUsed = true;
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
        public async Task<User?> GetUserByEmail(string email, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .FirstOrDefaultAsync(
                    x => x.Email == email,
                    cancellationToken);
        }
        public async Task CreateSession(Guid userId,string refreshToken,string deviceInfo,string ipAddress,CancellationToken cancellationToken = default)
        {
            UserSession session = new()
            {
                UserId = userId,
                RefreshToken = refreshToken,

                DeviceInfo = deviceInfo,
                IpAddress = ipAddress,

                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(30),

                IsRevoked = false
            };

            await _context.UserSessions.AddAsync(
                session,
                cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);
        }
        public async Task UpdateSessionRefreshTokenAsync(int sessionId,string newRefreshTokenHash,
            DateTime expiresAt,CancellationToken cancellationToken = default)
        {
            var session = await _context.UserSessions
                .FirstOrDefaultAsync(x => x.Id == sessionId, cancellationToken);

            if (session == null)
                return;

            session.RefreshToken = newRefreshTokenHash;
            session.ExpiresAt = expiresAt;

            await _context.SaveChangesAsync(cancellationToken);
        }
        public async Task<List<UserSession>> GetActiveSessions(CancellationToken cancellationToken = default)
        {
            return await _context.UserSessions
                .Include(x => x.User)
                .Where(x =>
                    !x.IsRevoked &&
                    x.ExpiresAt > DateTime.UtcNow)
                .ToListAsync(cancellationToken);
        }
    }
}
