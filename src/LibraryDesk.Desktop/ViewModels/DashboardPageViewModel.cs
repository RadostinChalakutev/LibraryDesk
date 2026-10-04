using LibraryDesk.Application.Abstractions;
using LibraryDesk.Application.Dashboard;

namespace LibraryDesk.Desktop.ViewModels;

/// <summary>
/// Supplies the main window's fast, at-a-glance library workflow metrics.
/// </summary>
public sealed class DashboardPageViewModel : ViewModelBase
{
    private readonly IDashboardService _dashboardService;
    private readonly IClock _clock;
    private DashboardSummary? _summary;
    private string? _statusMessage;

    public DashboardPageViewModel(IDashboardService dashboardService, IClock clock)
    {
        _dashboardService = dashboardService ?? throw new ArgumentNullException(nameof(dashboardService));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        RefreshCommand = new AsyncRelayCommand(RefreshAsync);
        RefreshCommand.Execute(null);
    }

    public AsyncRelayCommand RefreshCommand { get; }

    public DashboardSummary? Summary
    {
        get => _summary;
        private set => SetProperty(ref _summary, value);
    }

    public string? StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    private async Task RefreshAsync()
    {
        var asOfDate = DateOnly.FromDateTime(_clock.UtcNow.UtcDateTime);
        Summary = await _dashboardService.GetSummaryAsync(asOfDate);
        StatusMessage = $"Current as of {asOfDate:yyyy-MM-dd}.";
    }
}
