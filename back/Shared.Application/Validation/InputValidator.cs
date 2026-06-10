using System.Net.Mail;
using System.Text.RegularExpressions;

namespace Shared.Application.Validation
{
    public static class InputValidator
    {
        private static readonly Regex LoginRegex = new(
            @"^[a-zA-Z0-9_]{3,32}$",
            RegexOptions.Compiled);

        public static bool IsValidEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            email = email.Trim();

            if (email.Length > 254)
                return false;

            try
            {
                _ = new MailAddress(email);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Умеренная сложность: 8+ символов, заглавная, строчная, цифра.
        /// </summary>
        public static bool IsValidPassword(string? password, out string? error)
        {
            error = null;

            if (string.IsNullOrWhiteSpace(password))
            {
                error = "Пароль обязателен";
                return false;
            }

            if (password.Length < 8)
            {
                error = "Пароль должен содержать минимум 8 символов";
                return false;
            }

            if (password.Length > 128)
            {
                error = "Пароль слишком длинный";
                return false;
            }

            if (!password.Any(char.IsUpper))
            {
                error = "Пароль должен содержать хотя бы одну заглавную букву";
                return false;
            }

            if (!password.Any(char.IsLower))
            {
                error = "Пароль должен содержать хотя бы одну строчную букву";
                return false;
            }

            if (!password.Any(char.IsDigit))
            {
                error = "Пароль должен содержать хотя бы одну цифру";
                return false;
            }

            return true;
        }

        public static bool IsValidLogin(string? login, out string? error)
        {
            error = null;

            if (string.IsNullOrWhiteSpace(login))
            {
                error = "Логин обязателен";
                return false;
            }

            login = login.Trim();

            if (!LoginRegex.IsMatch(login))
            {
                error = "Логин: 3–32 символа, только буквы, цифры и _";
                return false;
            }

            return true;
        }

        public static string NormalizeEmail(string email) =>
            email.Trim().ToLowerInvariant();

        public static string NormalizeCode(string code) =>
            code.Trim().ToUpperInvariant();
    }
}
