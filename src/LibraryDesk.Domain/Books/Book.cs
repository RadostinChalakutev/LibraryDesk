namespace LibraryDesk.Domain.Books;

/// <summary>
/// A bibliographic title that may have one or more physical copies.
/// </summary>
public sealed class Book
{
    private Book()
    {
        Title = string.Empty;
        Author = string.Empty;
        Copies = new List<PhysicalCopy>();
    }

    private Book(Guid id, string title, string author, string? isbn, string? category, int? publishedYear, DateTimeOffset createdAtUtc)
    {
        Id = id;
        Title = NormalizeRequired(title, "Title", 240);
        Author = NormalizeRequired(author, "Author", 160);
        Isbn = NormalizeOptional(isbn, 32);
        Category = NormalizeOptional(category, 100);
        PublishedYear = NormalizeYear(publishedYear);
        CreatedAtUtc = createdAtUtc;
        Copies = new List<PhysicalCopy>();
    }

    public Guid Id { get; private set; }

    public string Title { get; private set; }

    public string Author { get; private set; }

    public string? Isbn { get; private set; }

    public string? Category { get; private set; }

    public int? PublishedYear { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public ICollection<PhysicalCopy> Copies { get; private set; }

    public static Book Create(string title, string author, string? isbn, string? category, int? publishedYear, DateTimeOffset createdAtUtc) =>
        new(Guid.NewGuid(), title, author, isbn, category, publishedYear, createdAtUtc);

    public void Update(string title, string author, string? isbn, string? category, int? publishedYear)
    {
        Title = NormalizeRequired(title, "Title", 240);
        Author = NormalizeRequired(author, "Author", 160);
        Isbn = NormalizeOptional(isbn, 32);
        Category = NormalizeOptional(category, 100);
        PublishedYear = NormalizeYear(publishedYear);
    }

    private static string NormalizeRequired(string value, string label, int maximumLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{label} is required.", nameof(value));
        }

        var normalized = value.Trim();
        if (normalized.Length > maximumLength)
        {
            throw new ArgumentException($"{label} must not exceed {maximumLength} characters.", nameof(value));
        }

        return normalized;
    }

    private static string? NormalizeOptional(string? value, int maximumLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (normalized.Length > maximumLength)
        {
            throw new ArgumentException($"Value must not exceed {maximumLength} characters.", nameof(value));
        }

        return normalized;
    }

    private static int? NormalizeYear(int? year)
    {
        if (year is null)
        {
            return null;
        }

        if (year < 1000 || year > DateTime.UtcNow.Year + 1)
        {
            throw new ArgumentException("Published year is invalid.", nameof(year));
        }

        return year;
    }
}
