namespace LibraryDesk.Application.Readers;

public sealed record UpdateReaderRequest(Guid Id, string FullName, string? Email, string? Phone);
