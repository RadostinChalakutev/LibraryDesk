using LibraryDesk.Domain.Security;

namespace LibraryDesk.Application.Authentication;

/// <summary>
/// Describes the authenticated identity available to the desktop session.
/// </summary>
public sealed record AuthenticatedUser(
    Guid Id,
    string Username,
    string DisplayName,
    UserRole Role);
