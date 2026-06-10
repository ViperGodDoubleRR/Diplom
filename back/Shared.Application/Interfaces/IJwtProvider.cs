using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Shared.Application.Contracts.AuthJWT;

namespace Shared.Application.Interfaces
{
    public interface IJwtProvider
    {
        string GenerateAccessToken(Guid userId, string email, string login);
        string GenerateRefreshToken();
        string GeneratePasswordResetToken(string email);
        PasswordResetTokenResult? ValidatePasswordResetToken(string token);
    }
}
