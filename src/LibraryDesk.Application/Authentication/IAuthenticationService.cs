namespace LibraryDesk.Application.Authentication;

/// <summary>
/// Authenticates local user accounts.
/// </summary>
public interface IAuthenticationService
{
    Task<AuthenticationResult> AuthenticateAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default);
}
