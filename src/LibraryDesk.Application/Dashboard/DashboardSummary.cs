namespace LibraryDesk.Application.Dashboard;

/// <summary>
/// Contains the small set of operational counts needed when opening LibraryDesk.
/// </summary>
public sealed record DashboardSummary(
    int ActiveReaders,
    int CatalogBooks,
    int TotalCopies,
    int AvailableCopies,
    int ActiveLoans,
    int OverdueLoans);
