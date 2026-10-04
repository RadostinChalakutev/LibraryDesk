namespace LibraryDesk.Application.Reports;

/// <summary>
/// Identifies a generated report file and the records included in it.
/// </summary>
public sealed record ReportExport(string FilePath, int RecordCount, DateOnly FromDate, DateOnly ToDate);
