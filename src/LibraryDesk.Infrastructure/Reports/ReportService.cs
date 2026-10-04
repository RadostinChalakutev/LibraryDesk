using System.Globalization;
using System.Text;
using LibraryDesk.Application.Primitives;
using LibraryDesk.Application.Reports;
using LibraryDesk.Domain.Books;
using LibraryDesk.Domain.Loans;
using LibraryDesk.Domain.Readers;
using LibraryDesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LibraryDesk.Infrastructure.Reports;

/// <summary>
/// Generates portable, spreadsheet-safe CSV exports from the local circulation history.
/// </summary>
public sealed class ReportService : IReportService
{
    private readonly IDbContextFactory<LibraryDeskDbContext> _dbContextFactory;

    public ReportService(IDbContextFactory<LibraryDeskDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
    }

    public async Task<OperationResult<ReportExport>> ExportLoanHistoryCsvAsync(
        DateOnly fromDate,
        DateOnly toDate,
        string destinationDirectory,
        CancellationToken cancellationToken = default)
    {
        if (toDate < fromDate)
        {
            return OperationResults.Failure<ReportExport>("The end date cannot be before the start date.");
        }

        if (string.IsNullOrWhiteSpace(destinationDirectory))
        {
            return OperationResults.Failure<ReportExport>("A destination directory is required.");
        }

        var records = await GetLoanHistoryAsync(fromDate, toDate, cancellationToken);
        string filePath;
        try
        {
            Directory.CreateDirectory(destinationDirectory);
            filePath = Path.Combine(
                destinationDirectory,
                $"Loan-history-{fromDate:yyyyMMdd}-{toDate:yyyyMMdd}-{DateTime.UtcNow:yyyyMMddHHmmssfff}.csv");
            await WriteCsvAsync(filePath, records, cancellationToken);
        }
        catch (IOException exception)
        {
            return OperationResults.Failure<ReportExport>($"The report could not be written: {exception.Message}");
        }
        catch (UnauthorizedAccessException exception)
        {
            return OperationResults.Failure<ReportExport>($"The report destination is not accessible: {exception.Message}");
        }

        return OperationResults.Success(new ReportExport(filePath, records.Count, fromDate, toDate));
    }

    private async Task<List<LoanHistoryRow>> GetLoanHistoryAsync(
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await (
            from loan in dbContext.Set<Loan>().AsNoTracking()
            where loan.BorrowedOn >= fromDate && loan.BorrowedOn <= toDate
            join reader in dbContext.Set<Reader>().AsNoTracking() on loan.ReaderId equals reader.Id
            join copy in dbContext.Set<PhysicalCopy>().AsNoTracking() on loan.PhysicalCopyId equals copy.Id
            join book in dbContext.Set<Book>().AsNoTracking() on copy.BookId equals book.Id
            orderby loan.BorrowedOn, reader.FullName
            select new LoanHistoryRow(
                loan.BorrowedOn,
                loan.DueOn,
                loan.ReturnedOn,
                reader.MembershipNumber,
                reader.FullName,
                copy.Barcode,
                book.Title,
                book.Author)).ToListAsync(cancellationToken);
    }

    private static async Task WriteCsvAsync(string filePath, IEnumerable<LoanHistoryRow> records, CancellationToken cancellationToken)
    {
        await using var writer = new StreamWriter(filePath, append: false, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
        await writer.WriteLineAsync("BorrowedOn,DueOn,ReturnedOn,MembershipNumber,ReaderName,Barcode,BookTitle,BookAuthor");
        foreach (var record in records)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var row = string.Join(",", new[]
            {
                record.BorrowedOn.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                record.DueOn.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                record.ReturnedOn?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty,
                EscapeField(record.MembershipNumber),
                EscapeField(record.ReaderName),
                EscapeField(record.Barcode),
                EscapeField(record.BookTitle),
                EscapeField(record.BookAuthor),
            });
            await writer.WriteLineAsync(row);
        }
    }

    private static string EscapeField(string value)
    {
        var protectedValue = value.Length > 0 && value[0] is '=' or '+' or '-' or '@' ? $"'{value}" : value;
        return $"\"{protectedValue.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
    }

    private sealed record LoanHistoryRow(
        DateOnly BorrowedOn,
        DateOnly DueOn,
        DateOnly? ReturnedOn,
        string MembershipNumber,
        string ReaderName,
        string Barcode,
        string BookTitle,
        string BookAuthor);
}
