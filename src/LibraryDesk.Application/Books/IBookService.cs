using LibraryDesk.Application.Primitives;

namespace LibraryDesk.Application.Books;

/// <summary>
/// Manages catalogue titles and their physical copies.
/// </summary>
public interface IBookService
{
    Task<IReadOnlyList<BookDto>> SearchAsync(string? searchText, CancellationToken cancellationToken = default);

    Task<OperationResult<BookDto>> CreateAsync(CreateBookRequest request, CancellationToken cancellationToken = default);

    Task<OperationResult<CopyDto>> AddCopyAsync(AddCopyRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CopyDto>> GetCopiesAsync(Guid bookId, CancellationToken cancellationToken = default);
}
