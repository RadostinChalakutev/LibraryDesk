using LibraryDesk.Application.Abstractions;
using LibraryDesk.Application.Loans;
using LibraryDesk.Application.Primitives;
using LibraryDesk.Domain.Books;
using LibraryDesk.Domain.Loans;
using LibraryDesk.Domain.Readers;
using LibraryDesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LibraryDesk.Infrastructure.Loans;

/// <summary>
/// Performs transactional circulation operations over readers, copies, and loan history.
/// </summary>
public sealed class LoanService : ILoanService
{
    private readonly IDbContextFactory<LibraryDeskDbContext> _dbContextFactory;
    private readonly IClock _clock;

    public LoanService(IDbContextFactory<LibraryDeskDbContext> dbContextFactory, IClock clock)
    {
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public async Task<OperationResult<LoanDto>> BorrowAsync(BorrowRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.MembershipNumber) || string.IsNullOrWhiteSpace(request.Barcode))
        {
            return OperationResults.Failure<LoanDto>("Reader membership number and copy barcode are required.");
        }

        var now = _clock.UtcNow;
        var borrowedOn = DateOnly.FromDateTime(now.UtcDateTime);
        var dueOn = request.DueOn ?? borrowedOn.AddDays(14);
        if (dueOn <= borrowedOn)
        {
            return OperationResults.Failure<LoanDto>("Due date must be after today.");
        }

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var membershipNumber = request.MembershipNumber.Trim().ToUpperInvariant();
        var barcode = request.Barcode.Trim().ToUpperInvariant();
        var reader = await dbContext.Set<Reader>().SingleOrDefaultAsync(
            candidate => candidate.MembershipNumber == membershipNumber,
            cancellationToken);
        if (reader is null || !reader.IsActive)
        {
            return OperationResults.Failure<LoanDto>("An active reader with this membership number was not found.");
        }

        var hasOverdueLoan = await dbContext.Set<Loan>().AnyAsync(
            loan => loan.ReaderId == reader.Id && loan.ReturnedOn == null && loan.DueOn < borrowedOn,
            cancellationToken);
        if (hasOverdueLoan)
        {
            return OperationResults.Failure<LoanDto>("This reader has an overdue loan and cannot borrow another copy.");
        }

        var copy = await dbContext.Set<PhysicalCopy>().SingleOrDefaultAsync(
            candidate => candidate.Barcode == barcode,
            cancellationToken);
        if (copy is null)
        {
            return OperationResults.Failure<LoanDto>("No physical copy with this barcode was found.");
        }

        if (copy.Status != CopyStatus.Available)
        {
            return OperationResults.Failure<LoanDto>("This physical copy is not available for borrowing.");
        }

        var book = await dbContext.Set<Book>().FindAsync([copy.BookId], cancellationToken);
        if (book is null)
        {
            return OperationResults.Failure<LoanDto>("The copy's book record was not found.");
        }

        Loan loan;
        try
        {
            loan = Loan.Create(reader.Id, copy.Id, borrowedOn, dueOn, now);
            copy.StartLoan();
        }
        catch (ArgumentException exception)
        {
            return OperationResults.Failure<LoanDto>(exception.Message);
        }

        dbContext.Set<Loan>().Add(loan);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return OperationResults.Failure<LoanDto>("This copy was borrowed by another operation. Refresh and try again.");
        }

        return OperationResults.Success(new LoanDto(
            loan.Id,
            reader.Id,
            reader.MembershipNumber,
            reader.FullName,
            copy.Id,
            copy.Barcode,
            book.Title,
            loan.BorrowedOn,
            loan.DueOn,
            loan.ReturnedOn));
    }

    public async Task<OperationResult> ReturnAsync(Guid loanId, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var loan = await dbContext.Set<Loan>().FindAsync([loanId], cancellationToken);
        if (loan is null || loan.ReturnedOn is not null)
        {
            return OperationResult.Failure("The active loan was not found.");
        }

        var copy = await dbContext.Set<PhysicalCopy>().FindAsync([loan.PhysicalCopyId], cancellationToken);
        if (copy is null)
        {
            return OperationResult.Failure("The physical copy was not found.");
        }

        try
        {
            loan.Return(DateOnly.FromDateTime(_clock.UtcNow.UtcDateTime));
            copy.ReturnToLibrary();
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
        {
            return OperationResult.Failure(exception.Message);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return OperationResult.Success();
    }

    public Task<IReadOnlyList<LoanDto>> GetActiveAsync(CancellationToken cancellationToken = default) =>
        QueryLoansAsync(onlyOverdueBefore: null, includeReturned: false, readerId: null, cancellationToken);

    public Task<IReadOnlyList<LoanDto>> GetOverdueAsync(DateOnly asOfDate, CancellationToken cancellationToken = default) =>
        QueryLoansAsync(asOfDate, includeReturned: false, readerId: null, cancellationToken);

    public Task<IReadOnlyList<LoanDto>> GetHistoryAsync(Guid? readerId = null, CancellationToken cancellationToken = default) =>
        QueryLoansAsync(onlyOverdueBefore: null, includeReturned: true, readerId, cancellationToken);

    private async Task<IReadOnlyList<LoanDto>> QueryLoansAsync(
        DateOnly? onlyOverdueBefore,
        bool includeReturned,
        Guid? readerId,
        CancellationToken cancellationToken)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var loans = dbContext.Set<Loan>().AsNoTracking();
        if (!includeReturned)
        {
            loans = loans.Where(loan => loan.ReturnedOn == null);
        }

        if (onlyOverdueBefore is not null)
        {
            loans = loans.Where(loan => loan.DueOn < onlyOverdueBefore);
        }

        if (readerId is not null)
        {
            loans = loans.Where(loan => loan.ReaderId == readerId);
        }

        return await (
            from loan in loans
            join reader in dbContext.Set<Reader>().AsNoTracking() on loan.ReaderId equals reader.Id
            join copy in dbContext.Set<PhysicalCopy>().AsNoTracking() on loan.PhysicalCopyId equals copy.Id
            join book in dbContext.Set<Book>().AsNoTracking() on copy.BookId equals book.Id
            orderby loan.DueOn, reader.FullName
            select new LoanDto(
                loan.Id,
                reader.Id,
                reader.MembershipNumber,
                reader.FullName,
                copy.Id,
                copy.Barcode,
                book.Title,
                loan.BorrowedOn,
                loan.DueOn,
                loan.ReturnedOn)).ToListAsync(cancellationToken);
    }
}
