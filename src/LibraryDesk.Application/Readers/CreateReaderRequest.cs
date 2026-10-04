namespace LibraryDesk.Application.Readers;

public sealed record CreateReaderRequest(string MembershipNumber, string FullName, string? Email, string? Phone);
