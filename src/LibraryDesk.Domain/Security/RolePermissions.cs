namespace LibraryDesk.Domain.Security;

/// <summary>
/// Centralizes the permission policy for each role.
/// </summary>
public static class RolePermissions
{
    public static bool HasPermission(UserRole role, Capability capability) =>
        role switch
        {
            UserRole.Administrator => true,
            UserRole.Librarian => capability is
                Capability.ViewDashboard or
                Capability.ManageReaders or
                Capability.ManageBooks or
                Capability.ManageLoans or
                Capability.ViewOverdue or
                Capability.ViewReports,
            _ => false,
        };
}
