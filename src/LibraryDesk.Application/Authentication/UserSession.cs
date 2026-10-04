using LibraryDesk.Domain.Security;

namespace LibraryDesk.Application.Authentication;

/// <summary>
/// In-memory session state for the current desktop process.
/// </summary>
public sealed class UserSession : IUserSession
{
    public AuthenticatedUser? CurrentUser { get; private set; }

    public bool IsAuthenticated => CurrentUser is not null;

    public bool HasPermission(Capability capability) =>
        CurrentUser is not null && RolePermissions.HasPermission(CurrentUser.Role, capability);

    public void Start(AuthenticatedUser user)
    {
        CurrentUser = user ?? throw new ArgumentNullException(nameof(user));
    }

    public void Clear() => CurrentUser = null;
}
