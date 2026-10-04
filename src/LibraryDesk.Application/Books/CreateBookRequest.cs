namespace LibraryDesk.Application.Books;

public sealed record CreateBookRequest(string Title, string Author, string? Isbn, string? Category, int? PublishedYear);
