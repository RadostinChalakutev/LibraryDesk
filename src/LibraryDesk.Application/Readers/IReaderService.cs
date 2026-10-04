using LibraryDesk.Application.Primitives;

namespace LibraryDesk.Application.Readers;

/// <summary>
/// Manages reader registration and status.
/// </summary>
public interface IReaderService
{
    Task<IReadOnlyList<ReaderDto>> SearchAsync(string? searchText, bool includeInactive, CancellationToken cancellationToken = default);

    Task<OperationResult<ReaderDto>> CreateAsync(CreateReaderRequest request, CancellationToken cancellationToken = default);

    Task<OperationResult<ReaderDto>> UpdateAsync(UpdateReaderRequest request, CancellationToken cancellationToken = default);

    Task<OperationResult> SetActiveAsync(Guid readerId, bool isActive, CancellationToken cancellationToken = default);
}
