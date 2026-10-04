using LibraryDesk.Application;
using LibraryDesk.Application.Abstractions;
using LibraryDesk.Application.Books;
using LibraryDesk.Application.Loans;
using LibraryDesk.Application.Readers;
using LibraryDesk.Application.Reports;
using LibraryDesk.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryDesk.Infrastructure.Tests.Reports;

public sealed class ReportServiceTests
{
    [Fact]
    public async Task ExportsLoanHistoryCsvWithSpreadsheetSafeText()
    {
        var temporaryDirectory = Path.Combine(Path.GetTempPath(), $"LibraryDesk.Tests.{Guid.NewGuid():N}");
        var databasePath = Path.Combine(temporaryDirectory, "reports.db");
        var reportDirectory = Path.Combine(temporaryDirectory, "exports");

        try
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { ["Database:FilePath"] = databasePath })
                .Build();
            await using var services = new ServiceCollection()
                .AddApplication()
                .AddInfrastructure(configuration)
                .AddSingleton<IClock>(new FixedClock(new DateTimeOffset(2026, 3, 10, 9, 0, 0, TimeSpan.Zero)))
                .BuildServiceProvider();
            await services.GetRequiredService<IDatabaseInitializer>().InitializeAsync();
            var readers = services.GetRequiredService<IReaderService>();
            var books = services.GetRequiredService<IBookService>();
            var loans = services.GetRequiredService<ILoanService>();

            Assert.True((await readers.CreateAsync(new CreateReaderRequest("R-5001", "=Formula Reader", null, null))).IsSuccess);
            var book = Assert.IsType<BookDto>((await books.CreateAsync(new CreateBookRequest("CSV, Safety", "Test Author", null, null, null))).Value);
            Assert.True((await books.AddCopyAsync(new AddCopyRequest(book.Id, "BC-5001"))).IsSuccess);
            Assert.True((await loans.BorrowAsync(new BorrowRequest("R-5001", "BC-5001", new DateOnly(2026, 3, 20)))).IsSuccess);

            var historyResult = await services.GetRequiredService<IReportService>().GetLoanHistoryAsync(
                new DateOnly(2026, 3, 1),
                new DateOnly(2026, 3, 31));
            Assert.True(historyResult.IsSuccess);
            var displayedRow = Assert.Single(Assert.IsAssignableFrom<IReadOnlyList<LoanHistoryReportRow>>(historyResult.Value));
            Assert.Equal("BC-5001", displayedRow.Barcode);

            var result = await services.GetRequiredService<IReportService>().ExportLoanHistoryCsvAsync(
                new DateOnly(2026, 3, 1),
                new DateOnly(2026, 3, 31),
                reportDirectory);

            Assert.True(result.IsSuccess);
            var export = Assert.IsType<ReportExport>(result.Value);
            Assert.Equal(1, export.RecordCount);
            Assert.True(File.Exists(export.FilePath));
            var contents = await File.ReadAllTextAsync(export.FilePath);
            Assert.Contains("BorrowedOn,DueOn,ReturnedOn", contents, StringComparison.Ordinal);
            Assert.Contains("\"'=Formula Reader\"", contents, StringComparison.Ordinal);
            Assert.Contains("\"CSV, Safety\"", contents, StringComparison.Ordinal);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (Directory.Exists(temporaryDirectory))
            {
                Directory.Delete(temporaryDirectory, recursive: true);
            }
        }
    }

    private sealed class FixedClock : IClock
    {
        public FixedClock(DateTimeOffset utcNow) => UtcNow = utcNow;

        public DateTimeOffset UtcNow { get; }
    }
}
