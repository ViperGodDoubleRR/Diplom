using System.Text.RegularExpressions;

using Shared.Application.Validation;

namespace UserService.Application.Validation
{
    public static class UserValidation
    {
        private static readonly Regex TagRegex = new(
            @"^[a-zA-Z0-9_]{1,32}$",
            RegexOptions.Compiled);

        public const int MaxDescriptionLength = 1000;
        public const int MaxSearchLength = 64;
        public const int MaxPageSize = 50;

        public static bool IsValidTag(string? tag, out string? error)
        {
            error = null;

            if (string.IsNullOrWhiteSpace(tag))
                return true;

            tag = tag.Trim();

            if (!TagRegex.IsMatch(tag))
            {
                error = "Тег: 1–32 символа, только буквы, цифры и _";
                return false;
            }

            return true;
        }

        public static bool IsValidDescription(string? description, out string? error)
        {
            error = null;

            if (description is null)
                return true;

            if (description.Length > MaxDescriptionLength)
            {
                error = $"Описание не должно превышать {MaxDescriptionLength} символов";
                return false;
            }

            return true;
        }

        public static bool IsValidUpdate(
            string? login,
            string? tag,
            string? description,
            out string? error)
        {
            if (!InputValidator.IsValidLogin(login, out error))
                return false;

            if (!IsValidTag(tag, out error))
                return false;

            if (!IsValidDescription(description, out error))
                return false;

            return true;
        }

        public static (int page, int pageSize) NormalizePaging(int page, int pageSize)
        {
            var normalizedPage = page < 1 ? 1 : page;
            var normalizedSize = pageSize < 1 ? 20 : Math.Min(pageSize, MaxPageSize);
            return (normalizedPage, normalizedSize);
        }
    }
}
