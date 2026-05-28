using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Application.Interfaces
{
    public interface IJwtProvider
    {
        string GenerateAccessToken(Guid userId, string email, string login);
        string GenerateRefreshToken();
    }
}
