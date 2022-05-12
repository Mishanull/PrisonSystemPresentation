using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;


namespace Utilities;

public static class Security
{
    public static string HashPassword(string password)
    {
        byte[] salt = new byte[128 / 8];
        using (var rngCsp = new RNGCryptoServiceProvider())
        {
            rngCsp.GetNonZeroBytes(salt);
        }

        string hashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password, salt, KeyDerivationPrf.HMACSHA256, 10000, 256 / 8));
        return hashedPassword;
    }

    public static string GeneratePassword(int length)
    {
        const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
        StringBuilder res = new StringBuilder();
        Random rnd = new Random();
        while (0 < length--)
        {
            res.Append(valid[rnd.Next(valid.Length)]);
        }
        return res.ToString();
    }
    
    

    
    
    
    
    // // generate a 128-bit salt using a cryptographically strong random sequence of nonzero values
    //     byte[] salt = new byte[128 / 8];
    //     using (var rngCsp = new RNGCryptoServiceProvider())
    //     {
    //         rngCsp.GetNonZeroBytes(salt);
    //     }
    //     Console.WriteLine($"Salt: {Convert.ToBase64String(salt)}");
    //
    //     // derive a 256-bit subkey (use HMACSHA256 with 100,000 iterations)
    //     string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
    //         password: password,
    //         salt: salt,
    //         prf: KeyDerivationPrf.HMACSHA256,
    //         iterationCount: 100000,
    //         numBytesRequested: 256 / 8));
    //     Console.WriteLine($"Hashed: {hashed}");
    
}