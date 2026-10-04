using LibraryDesk.Application.Primitives;

namespace LibraryDesk.Application.Loans;

/// <summary>
/// Coordinates borrowing, due dates, returning, and loan history.
/// </summary>
public interface ILoanService
{
    Task<OperationResult<LoanDto>> BorrowAsync(BorrowRequest request, CancellationToken cancellationToken = default);

    Task<OperationResult> ReturnAsync(Guid loanId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LoanDto>> GetActiveAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LoanDto>> GetOverdueAsync(DateOnly asOfDate, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LoanDto>> GetHistoryAsync(Guid? readerId = null, CancellationToken cancellationToken = default);
}
