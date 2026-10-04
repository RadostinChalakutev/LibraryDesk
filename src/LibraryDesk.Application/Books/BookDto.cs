namespace LibraryDesk.Application.Books;

public sealed record BookDto(Guid Id, string Title, string Author, string? Isbn, string? Category, int? PublishedYear, int CopyCount, int AvailableCopyCount);
