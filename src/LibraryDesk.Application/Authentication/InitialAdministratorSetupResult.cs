namespace LibraryDesk.Application.Authentication;

/// <summary>
/// Represents the outcome of first-run administrator account setup.
/// </summary>
public sealed record InitialAdministratorSetupResult
{
    private InitialAdministratorSetupResult(bool isSuccess, AuthenticatedUser? user, string? errorMessage)
    {
        IsSuccess = isSuccess;
        User = user;
        ErrorMessage = errorMessage;
    }

    public bool IsSuccess { get; }

    public AuthenticatedUser? User { get; }

    public string? ErrorMessage { get; }

    public static InitialAdministratorSetupResult Success(AuthenticatedUser user) =>
        new(true, user ?? throw new ArgumentNullException(nameof(user)), null);

    public static InitialAdministratorSetupResult Failure(string errorMessage) =>
        new(false, null, errorMessage ?? throw new ArgumentNullException(nameof(errorMessage)));
}
