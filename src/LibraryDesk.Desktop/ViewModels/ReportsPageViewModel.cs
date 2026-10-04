using System.Globalization;
using System.Collections.ObjectModel;
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
        History = new ObservableCollection<LoanHistoryReportRow>();
        ShowHistoryCommand = new AsyncRelayCommand(ShowHistoryAsync);
        ExportLoanHistoryCommand = new AsyncRelayCommand(ExportLoanHistoryAsync);
        ShowHistoryCommand.Execute(null);
    }

    public ObservableCollection<LoanHistoryReportRow> History { get; }

    public AsyncRelayCommand ShowHistoryCommand { get; }

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
        if (!TryGetDateRange(out var fromDate, out var toDate))
        {
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

    private async Task ShowHistoryAsync()
    {
        if (!TryGetDateRange(out var fromDate, out var toDate))
        {
            return;
        }

        var result = await _reportService.GetLoanHistoryAsync(fromDate, toDate);
        if (!result.IsSuccess || result.Value is null)
        {
            StatusMessage = result.Error;
            return;
        }

        History.Clear();
        foreach (var row in result.Value)
        {
            History.Add(row);
        }

        StatusMessage = $"Showing {History.Count} loan record(s) from {fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}.";
    }

    private bool TryGetDateRange(out DateOnly fromDate, out DateOnly toDate)
    {
        fromDate = default;
        toDate = default;
        if (!DateOnly.TryParseExact(FromDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out fromDate) ||
            !DateOnly.TryParseExact(ToDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out toDate))
        {
            StatusMessage = "Dates must use YYYY-MM-DD.";
            return false;
        }

        if (toDate < fromDate)
        {
            StatusMessage = "The end date cannot be before the start date.";
            return false;
        }

        return true;
    }
}
