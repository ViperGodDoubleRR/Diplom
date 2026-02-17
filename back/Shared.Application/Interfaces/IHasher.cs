using System.Security.Cryptography;
using System.Text;

namespace Shared.Application.Interfaces
{
    public interface IHasher
    {
        string Hash(string password);
        bool Verify(string password, string stored);
    }

}
