using LibraryDesk.Domain.Books;

namespace LibraryDesk.Application.Books;

public sealed record CopyDto(Guid Id, Guid BookId, string Barcode, CopyStatus Status, DateTimeOffset CreatedAtUtc);
