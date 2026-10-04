namespace LibraryDesk.Application.Books;

public sealed record AddCopyRequest(Guid BookId, string Barcode);
