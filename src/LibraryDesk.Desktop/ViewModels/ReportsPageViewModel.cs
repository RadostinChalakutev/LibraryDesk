using System.Globalization;
using LibraryDesk.Application.Reports;

namespace LibraryDesk.Desktop.ViewModels;

/// <summary>
/// Starts circulation-history exports into the employee's documents folder.
/// </summary>
public sealed class ReportsPageViewModel : ViewModelBase
{
    private readonly IReportService _reportService;
    private string _fromDate = DateTime.UtcNow.Date.AddMonths(-1).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    private string _toDate = DateTime.UtcNow.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    private string? _statusMessage;

    public ReportsPageViewModel(IReportService reportService)
    {
        _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
        ExportLoanHistoryCommand = new AsyncRelayCommand(ExportLoanHistoryAsync);
    }

    public AsyncRelayCommand ExportLoanHistoryCommand { get; }

    public string FromDate
    {
        get => _fromDate;
        set => SetProperty(ref _fromDate, value);
    }

    public string ToDate
    {
        get => _toDate;
        set => SetProperty(ref _toDate, value);
    }

    public string? StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    private async Task ExportLoanHistoryAsync()
    {
        if (!DateOnly.TryParseExact(FromDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var fromDate) ||
            !DateOnly.TryParseExact(ToDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var toDate))
        {
            StatusMessage = "Dates must use YYYY-MM-DD.";
            return;
        }

        var destinationDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "LibraryDesk",
            "reports");
        var result = await _reportService.ExportLoanHistoryCsvAsync(fromDate, toDate, destinationDirectory);
        StatusMessage = result.IsSuccess
            ? $"Exported {result.Value?.RecordCount} record(s) to {result.Value?.FilePath}."
            : result.Error;
    }
}
