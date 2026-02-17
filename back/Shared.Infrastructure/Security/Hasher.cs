using System.Security.Cryptography;
using System.Text;
using Shared.Application.Interfaces;
using Konscious.Security.Cryptography;

public class Hasher:IHasher
{
    
    public string Hash(string password)
    {
        var salt = GenerateSalt(16); 

        var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = 4,
            MemorySize = 65536, 
            Iterations = 4
           
        };

        var hashBytes = argon2.GetBytes(32); 
        var hashBase64 = Convert.ToBase64String(hashBytes);
        var saltBase64 = Convert.ToBase64String(salt);

        return $"{saltBase64}:{hashBase64}";
    }

    public bool Verify(string password, string stored)
    {
        var parts = stored.Split(':');
        if (parts.Length != 2) return false;

        var salt = Convert.FromBase64String(parts[0]);
        var expectedHash = parts[1];

        var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = 4,
            MemorySize = 65536,
            Iterations = 4
            
        };

        var hashBytes = argon2.GetBytes(32);
        var hashBase64 = Convert.ToBase64String(hashBytes);

        return hashBase64 == expectedHash;
    }

    private byte[] GenerateSalt(int length)
    {
        var salt = new byte[length];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(salt);
        return salt;
    }
}
