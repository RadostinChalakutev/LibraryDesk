namespace LibraryDesk.Application.Loans;

public sealed record BorrowRequest(string MembershipNumber, string Barcode, DateOnly? DueOn);
