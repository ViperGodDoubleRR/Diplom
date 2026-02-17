
using Microsoft.EntityFrameworkCore;
using RegService.Domain.IRepository;
using RegService.Domain.Models;
using RegService.Infrastructure.Data;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
namespace RegService.Infrastructure.IEfRepository
{
    public class EfRegRepository : IRegRepository
    {
        private readonly DbContextReg _context;
        private readonly ILogger<EfRegRepository> _logger;
        public EfRegRepository(DbContextReg context, ILogger<EfRegRepository> logger)
        {
            _context = context;
            _logger=logger;
        }
        public async Task<bool> CreateVerificationCode(string email,string code, CancellationToken cancellationToken = default)
        {
            bool exists = await _context.Users.AnyAsync(u => u.Email == email, cancellationToken);
            if (exists)
                return false; 
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

        public async Task<bool> CheckCode(string email, string code, CancellationToken cancellationToken = default)
        {
            var verificationcode = await _context.VerificationCodes
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync(x => x.Email == email && x.Code == code
                && x.ExpiresAt>DateTime.UtcNow && x.IsUsed==false, cancellationToken);

            if (verificationcode != null)
            {
                verificationcode.IsUsed= true;
                await _context.ConfirmedEmails.AddAsync(new ConfirmedEmail
                {
                    Email = email,
                    ConfirmedAt = DateTime.UtcNow,
                });
                await _context.SaveChangesAsync(cancellationToken);
                return true;
            }

            return false;
        }
        public async Task<bool> RegisterUser(Guid id, string email, string login, CancellationToken cancellationToken)
        {
            var confirmEmail = await _context.ConfirmedEmails
                .AnyAsync(x => x.Email==email && x.ConfirmedAt.AddMinutes(10)>DateTime.UtcNow,cancellationToken);
            if (confirmEmail)
            {
                await _context.ConfirmedEmails
                    .Where(x => x.Email == email)
                    .ExecuteDeleteAsync(cancellationToken);

                User user = new User
                {
                    Id = id,
                    Email = email,
                    Login = login,
                    CreatedAt = DateTime.UtcNow
                };
                await _context.Users.AddAsync(user, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                return confirmEmail;
            }
            return confirmEmail;
        }
    }
}
