namespace LibraryDesk.Application.Authentication;

/// <summary>
/// Provides shared validation for local account credentials.
/// </summary>
public static class AuthenticationInputValidator
{
    public static string NormalizeUsername(string username) =>
        (username ?? throw new ArgumentNullException(nameof(username))).Trim().ToUpperInvariant();

    public static string? ValidateDisplayName(string displayName) =>
        string.IsNullOrWhiteSpace(displayName) || displayName.Trim().Length > 120
            ? "Enter a display name of up to 120 characters."
            : null;

    public static string? ValidateUsername(string username)
    {
        var trimmedUsername = username?.Trim() ?? string.Empty;
        if (trimmedUsername.Length is < 3 or > 64)
        {
            return "Username must contain 3 to 64 characters.";
        }

        return trimmedUsername.All(character =>
            char.IsLetterOrDigit(character) || character is '.' or '-' or '_')
            ? null
            : "Username may contain only letters, digits, periods, hyphens, and underscores.";
    }

    public static string? ValidatePassword(string password)
    {
        if (string.IsNullOrEmpty(password) || password.Length < 12)
        {
            return "Password must contain at least 12 characters.";
        }

        if (!password.Any(char.IsUpper) || !password.Any(char.IsLower) || !password.Any(char.IsDigit))
        {
            return "Password must include uppercase, lowercase, and numeric characters.";
        }

        return null;
    }
}
