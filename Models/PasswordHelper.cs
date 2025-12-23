using System.Security.Cryptography;
using System.Text;

namespace ITTicketSystem.Models
{
    public static class PasswordHelper
    {
        public static string Hash(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes); // uppercase hex
        }

        public static bool Verify(string password, string hash)
            => Hash(password) == hash;
    }
}
