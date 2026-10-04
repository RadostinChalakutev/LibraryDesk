using LibraryDesk.Domain.Security;

namespace LibraryDesk.Domain.Tests.Security;

public sealed class RolePermissionsTests
{
    [Fact]
    public void LibrarianCanOperateLibraryButCannotAdministerUsersOrSettings()
    {
        Assert.True(RolePermissions.HasPermission(UserRole.Librarian, Capability.ManageLoans));
        Assert.True(RolePermissions.HasPermission(UserRole.Librarian, Capability.ViewReports));
        Assert.False(RolePermissions.HasPermission(UserRole.Librarian, Capability.ManageUsers));
        Assert.False(RolePermissions.HasPermission(UserRole.Librarian, Capability.ManageSettings));
    }
}
