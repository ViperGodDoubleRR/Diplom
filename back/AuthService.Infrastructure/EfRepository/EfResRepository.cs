using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AuthService.Domain.Interface;
using AuthService.Domain.Models;
using AuthService.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.EfRepository
{
    public class EfResRepository : IResRepository
    {
        private readonly DbContextAuth _context;

        public EfResRepository(DbContextAuth context)
        {
            _context=context;
        }
        public async Task<bool> ResRequestCode(string email, string code, CancellationToken cancellationToken = default)
        {
                var isEmail = await _context.Users
                    .FirstOrDefaultAsync(
                        x => x.Email == email,
                        cancellationToken);

                if (isEmail == null)
                    return false;

                ResetCode resetCode = new ResetCode
                {
                    ResCodeUserId = isEmail.Id,
                    Code = code,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                    IsUsed = false
                };

                await _context.ResetCodes
                    .AddAsync(resetCode, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                return true;
        }
        public async Task<bool> CheckCode(string email, string code, CancellationToken cancellationToken = default)
        {

            var IsCheck = await _context.ResetCodes
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
        public async Task<bool> ResetPassword(string email,string newpassword,CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(x => x.Email == email, cancellationToken);

                if (user == null)
                    return false;

                user.PasswordHash = newpassword;

                await _context.SaveChangesAsync(cancellationToken);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
