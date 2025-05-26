using System.Security.Cryptography;
using System.Text;

namespace BlogBackend.Helpers
{
    public static class TokenGenerator
    {
        public static string GenerateSecureToken()
        {
            var randomBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes)
                .Replace("/", "_")
                .Replace("+", "-")
                .Replace("=", "");
        }

        public static string HashToken(string token)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(hashedBytes);
        }
    }
} 