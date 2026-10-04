namespace LibraryDesk.Application.Authentication;

/// <summary>
/// Represents a login attempt without exposing implementation details to the presentation layer.
/// </summary>
public sealed record AuthenticationResult
{
    private AuthenticationResult(bool isSuccess, AuthenticatedUser? user, string? errorMessage)
    {
        IsSuccess = isSuccess;
        User = user;
        ErrorMessage = errorMessage;
    }

    public bool IsSuccess { get; }

    public AuthenticatedUser? User { get; }

    public string? ErrorMessage { get; }

    public static AuthenticationResult Success(AuthenticatedUser user) =>
        new(true, user ?? throw new ArgumentNullException(nameof(user)), null);

    public static AuthenticationResult Failure(string errorMessage) =>
        new(false, null, errorMessage ?? throw new ArgumentNullException(nameof(errorMessage)));
}
