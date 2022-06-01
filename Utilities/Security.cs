namespace Utilities;

public static class Security
{
    public static bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }

    public static String HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
}