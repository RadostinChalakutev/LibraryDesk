namespace LibraryDesk.Domain.Security;

/// <summary>
/// Defines capabilities that application features may require.
/// </summary>
public enum Capability
{
    ViewDashboard = 0,
    ManageReaders = 1,
    ManageBooks = 2,
    ManageLoans = 3,
    ViewOverdue = 4,
    ViewReports = 5,
    ManageSettings = 6,
    ManageUsers = 7,
}
