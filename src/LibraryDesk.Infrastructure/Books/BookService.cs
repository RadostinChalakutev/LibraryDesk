using LibraryDesk.Application.Abstractions;
using LibraryDesk.Application.Books;
using LibraryDesk.Application.Primitives;
using LibraryDesk.Domain.Books;
using LibraryDesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LibraryDesk.Infrastructure.Books;

/// <summary>
/// EF Core catalogue and physical-copy implementation.
/// </summary>
public sealed class BookService : IBookService
{
    private readonly IDbContextFactory<LibraryDeskDbContext> _dbContextFactory;
    private readonly IClock _clock;

    public BookService(IDbContextFactory<LibraryDeskDbContext> dbContextFactory, IClock clock)
    {
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public async Task<IReadOnlyList<BookDto>> SearchAsync(string? searchText, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var books = dbContext.Set<Book>().AsNoTracking();
        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var search = searchText.Trim();
            books = books.Where(book => book.Title.Contains(search)
                || book.Author.Contains(search)
                || (book.Isbn != null && book.Isbn.Contains(search)));
        }

        return await books.OrderBy(book => book.Title)
            .Select(book => new BookDto(
                book.Id,
                book.Title,
                book.Author,
                book.Isbn,
                book.Category,
                book.PublishedYear,
                book.Copies.Count,
                book.Copies.Count(copy => copy.Status == CopyStatus.Available)))
            .ToListAsync(cancellationToken);
    }

    public async Task<OperationResult<BookDto>> CreateAsync(CreateBookRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        Book book;
        try
        {
            book = Book.Create(request.Title, request.Author, request.Isbn, request.Category, request.PublishedYear, _clock.UtcNow);
        }
        catch (ArgumentException exception)
        {
            return OperationResults.Failure<BookDto>(exception.Message);
        }

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        if (!string.IsNullOrWhiteSpace(book.Isbn) && await dbContext.Set<Book>().AnyAsync(existing => existing.Isbn == book.Isbn, cancellationToken))
        {
            return OperationResults.Failure<BookDto>("A book with this ISBN already exists.");
        }

        dbContext.Set<Book>().Add(book);
        await dbContext.SaveChangesAsync(cancellationToken);
        return OperationResults.Success(new BookDto(book.Id, book.Title, book.Author, book.Isbn, book.Category, book.PublishedYear, 0, 0));
    }

    public async Task<OperationResult<CopyDto>> AddCopyAsync(AddCopyRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        PhysicalCopy copy;
        try
        {
            copy = PhysicalCopy.Create(request.BookId, request.Barcode, _clock.UtcNow);
        }
        catch (ArgumentException exception)
        {
            return OperationResults.Failure<CopyDto>(exception.Message);
        }

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        if (!await dbContext.Set<Book>().AnyAsync(book => book.Id == request.BookId, cancellationToken))
        {
            return OperationResults.Failure<CopyDto>("The selected book was not found.");
        }

        if (await dbContext.Set<PhysicalCopy>().AnyAsync(existing => existing.Barcode == copy.Barcode, cancellationToken))
        {
            return OperationResults.Failure<CopyDto>("A copy with this barcode already exists.");
        }

        dbContext.Set<PhysicalCopy>().Add(copy);
        await dbContext.SaveChangesAsync(cancellationToken);
        return OperationResults.Success(new CopyDto(copy.Id, copy.BookId, copy.Barcode, copy.Status, copy.CreatedAtUtc));
    }

    public async Task<OperationResult<string>> GenerateNextBarcodeAsync(Guid bookId, CancellationToken cancellationToken = default)
    {
        if (bookId == Guid.Empty)
        {
            return OperationResults.Failure<string>("A catalogue title must be selected.");
        }

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        if (!await dbContext.Set<Book>().AnyAsync(book => book.Id == bookId, cancellationToken))
        {
            return OperationResults.Failure<string>("The selected book was not found.");
        }

        var prefix = $"LD-{bookId.ToString("N")[..8].ToUpperInvariant()}-";
        var existingBarcodes = await dbContext.Set<PhysicalCopy>()
            .AsNoTracking()
            .Where(copy => copy.BookId == bookId && copy.Barcode.StartsWith(prefix))
            .Select(copy => copy.Barcode)
            .ToListAsync(cancellationToken);
        var highestSequence = existingBarcodes
            .Select(barcode => int.TryParse(barcode[prefix.Length..], out var sequence) ? sequence : 0)
            .DefaultIfEmpty()
            .Max();
        return OperationResults.Success($"{prefix}{highestSequence + 1:D3}");
    }

    public async Task<IReadOnlyList<CopyDto>> GetCopiesAsync(Guid bookId, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await dbContext.Set<PhysicalCopy>().AsNoTracking()
            .Where(copy => copy.BookId == bookId)
            .OrderBy(copy => copy.Barcode)
            .Select(copy => new CopyDto(copy.Id, copy.BookId, copy.Barcode, copy.Status, copy.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }
}
