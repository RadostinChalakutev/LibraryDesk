using LibraryDesk.Application;
using LibraryDesk.Application.Abstractions;
using LibraryDesk.Application.Books;
using LibraryDesk.Application.Dashboard;
using LibraryDesk.Application.Loans;
using LibraryDesk.Application.Readers;
using LibraryDesk.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryDesk.Infrastructure.Tests.Dashboard;

public sealed class DashboardServiceTests
{
    [Fact]
    public async Task SummarizesCatalogAndCirculationCounts()
    {
        var temporaryDirectory = Path.Combine(Path.GetTempPath(), $"LibraryDesk.Tests.{Guid.NewGuid():N}");
        var databasePath = Path.Combine(temporaryDirectory, "dashboard.db");

        try
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { ["Database:FilePath"] = databasePath })
                .Build();
            await using var services = new ServiceCollection()
                .AddApplication()
                .AddInfrastructure(configuration)
                .AddSingleton<IClock>(new FixedClock(new DateTimeOffset(2026, 2, 10, 9, 0, 0, TimeSpan.Zero)))
                .BuildServiceProvider();
            await services.GetRequiredService<IDatabaseInitializer>().InitializeAsync();
            var readers = services.GetRequiredService<IReaderService>();
            var books = services.GetRequiredService<IBookService>();
            var loans = services.GetRequiredService<ILoanService>();

            Assert.True((await readers.CreateAsync(new CreateReaderRequest("R-4001", "Dorothy Vaughan", null, null))).IsSuccess);
            var book = Assert.IsType<BookDto>((await books.CreateAsync(new CreateBookRequest("Fortran", "Dorothy Vaughan", null, null, null))).Value);
            Assert.True((await books.AddCopyAsync(new AddCopyRequest(book.Id, "BC-4001"))).IsSuccess);
            Assert.True((await books.AddCopyAsync(new AddCopyRequest(book.Id, "BC-4002"))).IsSuccess);
            Assert.True((await loans.BorrowAsync(new BorrowRequest("R-4001", "BC-4001", new DateOnly(2026, 2, 12)))).IsSuccess);

            var summary = await services.GetRequiredService<IDashboardService>().GetSummaryAsync(new DateOnly(2026, 2, 13));

            Assert.Equal(1, summary.ActiveReaders);
            Assert.Equal(1, summary.CatalogBooks);
            Assert.Equal(2, summary.TotalCopies);
            Assert.Equal(1, summary.AvailableCopies);
            Assert.Equal(1, summary.ActiveLoans);
            Assert.Equal(1, summary.OverdueLoans);
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
