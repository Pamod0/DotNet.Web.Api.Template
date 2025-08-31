using System.Security.Cryptography;
using System.Text;

namespace DotNet.Web.Api.Template.Helpers
{
    public static class PasswordGenerator
    {
        public static string Generate(int length = 12)
        {
            if (length < 6) throw new ArgumentException("Password length must be at least 6 characters.");

            const string upperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lowerCase = "abcdefghijklmnopqrstuvwxyz";
            const string digits = "0123456789";
            const string specialChars = "!@#$%^&*()-_=+[]{}|;:,.<>?";

            var random = new Random();
            var password = new StringBuilder();

            // Ensure at least one character from each required set  
            password.Append(upperCase[random.Next(upperCase.Length)]);
            password.Append(lowerCase[random.Next(lowerCase.Length)]);
            password.Append(digits[random.Next(digits.Length)]);
            password.Append(specialChars[random.Next(specialChars.Length)]);

            // Fill the rest of the password with a random mix of all character sets  
            var allChars = upperCase + lowerCase + digits + specialChars;
            for (int i = 4; i < length; i++)
            {
                password.Append(allChars[random.Next(allChars.Length)]);
            }

            // Shuffle the password to ensure randomness  
            return new string(password.ToString().OrderBy(_ => random.Next()).ToArray());
        }
    }
}
