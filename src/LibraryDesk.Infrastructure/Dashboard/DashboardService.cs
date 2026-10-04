using LibraryDesk.Application.Dashboard;
using LibraryDesk.Domain.Books;
using LibraryDesk.Domain.Loans;
using LibraryDesk.Domain.Readers;
using LibraryDesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LibraryDesk.Infrastructure.Dashboard;

/// <summary>
/// Reads aggregate operational information without exposing persistence to the desktop layer.
/// </summary>
public sealed class DashboardService : IDashboardService
{
    private readonly IDbContextFactory<LibraryDeskDbContext> _dbContextFactory;

    public DashboardService(IDbContextFactory<LibraryDeskDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
    }

    public async Task<DashboardSummary> GetSummaryAsync(DateOnly asOfDate, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var readers = dbContext.Set<Reader>().AsNoTracking();
        var books = dbContext.Set<Book>().AsNoTracking();
        var copies = dbContext.Set<PhysicalCopy>().AsNoTracking();
        var loans = dbContext.Set<Loan>().AsNoTracking();

        return new DashboardSummary(
            await readers.CountAsync(reader => reader.IsActive, cancellationToken),
            await books.CountAsync(cancellationToken),
            await copies.CountAsync(cancellationToken),
            await copies.CountAsync(copy => copy.Status == CopyStatus.Available, cancellationToken),
            await loans.CountAsync(loan => loan.ReturnedOn == null, cancellationToken),
            await loans.CountAsync(loan => loan.ReturnedOn == null && loan.DueOn < asOfDate, cancellationToken));
    }
}
