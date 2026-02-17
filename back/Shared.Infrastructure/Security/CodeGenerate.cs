using System.Security.Cryptography;
using System.Text;
using Shared.Application.Interfaces;
namespace Shared.Infrastructure.Security
{
    public class CodeGenerator:ICodeGenerate
    {
        private const string _chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        public  string GenerateAlphaNumericCode(int length = 8)
        {
            var result = new StringBuilder(length);
            var bytes = new byte[length];

            RandomNumberGenerator.Fill(bytes);

            for (int i = 0; i < length; i++)
            {
                result.Append(_chars[bytes[i] % _chars.Length]);
            }

            return result.ToString();
        }
    }
}
