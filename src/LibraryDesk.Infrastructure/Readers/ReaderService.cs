using LibraryDesk.Application.Abstractions;
using LibraryDesk.Application.Primitives;
using LibraryDesk.Application.Readers;
using LibraryDesk.Domain.Readers;
using LibraryDesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LibraryDesk.Infrastructure.Readers;

/// <summary>
/// EF Core implementation of reader registration workflows.
/// </summary>
public sealed class ReaderService : IReaderService
{
    private readonly IDbContextFactory<LibraryDeskDbContext> _dbContextFactory;
    private readonly IClock _clock;

    public ReaderService(IDbContextFactory<LibraryDeskDbContext> dbContextFactory, IClock clock)
    {
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public async Task<IReadOnlyList<ReaderDto>> SearchAsync(
        string? searchText,
        bool includeInactive,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var readers = dbContext.Set<Reader>().AsNoTracking();

        if (!includeInactive)
        {
            readers = readers.Where(reader => reader.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var search = searchText.Trim();
            readers = readers.Where(reader =>
                reader.MembershipNumber.Contains(search)
                || reader.FullName.Contains(search)
                || (reader.Email != null && reader.Email.Contains(search)));
        }

        return await readers
            .OrderBy(reader => reader.FullName)
            .Select(reader => new ReaderDto(
                reader.Id,
                reader.MembershipNumber,
                reader.FullName,
                reader.Email,
                reader.Phone,
                reader.IsActive,
                reader.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }

    public async Task<OperationResult<ReaderDto>> CreateAsync(
        CreateReaderRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        Reader reader;
        try
        {
            reader = Reader.Create(
                request.MembershipNumber,
                request.FullName,
                request.Email,
                request.Phone,
                _clock.UtcNow);
        }
        catch (ArgumentException exception)
        {
            return OperationResults.Failure<ReaderDto>(exception.Message);
        }

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        if (await dbContext.Set<Reader>().AnyAsync(
                existingReader => existingReader.MembershipNumber == reader.MembershipNumber,
                cancellationToken))
        {
            return OperationResults.Failure<ReaderDto>("A reader with this membership number already exists.");
        }

        dbContext.Set<Reader>().Add(reader);
        await dbContext.SaveChangesAsync(cancellationToken);
        return OperationResults.Success(Map(reader));
    }

    public async Task<OperationResult<ReaderDto>> UpdateAsync(
        UpdateReaderRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var reader = await dbContext.Set<Reader>().FindAsync([request.Id], cancellationToken);
        if (reader is null)
        {
            return OperationResults.Failure<ReaderDto>("The reader was not found.");
        }

        try
        {
            reader.Update(request.FullName, request.Email, request.Phone);
        }
        catch (ArgumentException exception)
        {
            return OperationResults.Failure<ReaderDto>(exception.Message);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return OperationResults.Success(Map(reader));
    }

    public async Task<OperationResult> SetActiveAsync(
        Guid readerId,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var reader = await dbContext.Set<Reader>().FindAsync([readerId], cancellationToken);
        if (reader is null)
        {
            return OperationResult.Failure("The reader was not found.");
        }

        if (isActive)
        {
            reader.Reactivate();
        }
        else
        {
            reader.Deactivate();
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return OperationResult.Success();
    }

    private static ReaderDto Map(Reader reader) =>
        new(reader.Id, reader.MembershipNumber, reader.FullName, reader.Email, reader.Phone, reader.IsActive, reader.CreatedAtUtc);
}
