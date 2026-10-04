namespace LibraryDesk.Application.Loans;

public sealed record LoanDto(
    Guid Id,
    Guid ReaderId,
    string MembershipNumber,
    string ReaderName,
    Guid PhysicalCopyId,
    string Barcode,
    string BookTitle,
    DateOnly BorrowedOn,
    DateOnly DueOn,
    DateOnly? ReturnedOn);
