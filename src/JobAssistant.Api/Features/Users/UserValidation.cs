using System.Text.RegularExpressions;
using JobAssistant.Application.Common.Exceptions;

namespace JobAssistant.Api.Features.Users;

internal static partial class UserValidation
{
    private static readonly Regex UserNamePattern = AllowedUserNameRegex();

    internal static string NormalizeUserName(string userName)
    {
        EnsureValidUserName(userName);
        return userName.Trim().ToUpperInvariant();
    }

    internal static void EnsureValidUserName(string userName)
    {
        var trimmed = userName.Trim();

        if (trimmed.Length < 2)
        {
            throw new ValidationException(
                "Validation failed.",
                new Dictionary<string, string[]>
                {
                    ["userName"] = ["Username must be at least 2 characters."]
                });
        }

        if (!UserNamePattern.IsMatch(trimmed))
        {
            throw new ValidationException(
                "Validation failed.",
                new Dictionary<string, string[]>
                {
                    ["userName"] = ["Username contains invalid characters."]
                });
        }
    }

    internal static void EnsureValidSkills(IReadOnlyCollection<string> skills)
    {
        if (skills.Count == 0)
        {
            throw new ValidationException(
                "Validation failed.",
                new Dictionary<string, string[]>
                {
                    ["skills"] = ["At least one skill is required."]
                });
        }

        var invalid = skills.Any(s => string.IsNullOrWhiteSpace(s) || s.Trim().Length < 2);
        if (invalid)
        {
            throw new ValidationException(
                "Validation failed.",
                new Dictionary<string, string[]>
                {
                    ["skills"] = ["Each skill must be at least 2 characters."]
                });
        }
    }

    [GeneratedRegex("^[\\p{L} .'`-]+$", RegexOptions.CultureInvariant)]
    private static partial Regex AllowedUserNameRegex();
}
