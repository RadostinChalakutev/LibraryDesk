using LibraryDesk.Application.Primitives;

namespace LibraryDesk.Application.Reports;

public interface IReportService
{
    Task<OperationResult<ReportExport>> ExportLoanHistoryCsvAsync(
        DateOnly fromDate,
        DateOnly toDate,
        string destinationDirectory,
        CancellationToken cancellationToken = default);
}
