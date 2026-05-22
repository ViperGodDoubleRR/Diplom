using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AuthService.Domain.Models;

namespace AuthService.Domain.Interface
{
    public interface IAuthRepository
    {
        Task<Guid> CreateUser(string email, string login, string password);
        Task<bool> RequestCode(string email, string code, CancellationToken cancellationToken);
        Task<bool> CheckCode(string email, string code, CancellationToken cancellationToken);
        Task<User?> GetUserByEmail(string email, CancellationToken cancellationToken = default);
        Task CreateSession(Guid userId, string refreshToken, string deviceInfo,
            string ipAddress, CancellationToken cancellationToken);
    }
}
