namespace LibraryDesk.Application.Reports;

/// <summary>
/// A circulation-history row suitable for on-screen reports or export.
/// </summary>
public sealed record LoanHistoryReportRow(
    DateOnly BorrowedOn,
    DateOnly DueOn,
    DateOnly? ReturnedOn,
    string MembershipNumber,
    string ReaderName,
    string Barcode,
    string BookTitle,
    string BookAuthor);
