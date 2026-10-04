using LibraryDesk.Domain.Security;

namespace LibraryDesk.Application.Authentication;

/// <summary>
/// Holds the currently authenticated user for the running desktop application.
/// </summary>
public interface IUserSession
{
    AuthenticatedUser? CurrentUser { get; }

    bool IsAuthenticated { get; }

    bool HasPermission(Capability capability);

    void Start(AuthenticatedUser user);

    void Clear();
}
