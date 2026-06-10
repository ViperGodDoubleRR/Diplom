using System.Security.Cryptography;
using System.Text;

namespace Shared.Application.Security
{
    public static class RefreshTokenFingerprint
    {
        public static string Compute(string refreshToken)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));
            return Convert.ToHexString(bytes);
        }
    }
}
