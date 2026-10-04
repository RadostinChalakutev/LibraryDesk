namespace LibraryDesk.Application.Dashboard;

public interface IDashboardService
{
    Task<DashboardSummary> GetSummaryAsync(DateOnly asOfDate, CancellationToken cancellationToken = default);
}
