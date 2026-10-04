namespace LibraryDesk.Application.Authentication;

/// <summary>
/// Captures the first local administrator account configured for an installation.
/// </summary>
public sealed record InitialAdministratorRequest(
    string DisplayName,
    string Username,
    string Password);
