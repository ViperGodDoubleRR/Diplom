using System.Security.Cryptography;
using System.Text;

namespace Shared.Application.Interfaces
{
    public interface ICodeGenerate
    {
        string GenerateAlphaNumericCode(int length = 8);
    }

}

