using LibraryDesk.Application;
using LibraryDesk.Application.Abstractions;
using LibraryDesk.Application.Books;
using LibraryDesk.Application.Loans;
using LibraryDesk.Application.Readers;
using LibraryDesk.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryDesk.Infrastructure.Tests.Loans;

public sealed class OverdueLoanTests
{
    [Fact]
    public async Task FindsActiveLoansWhoseDueDateHasPassed()
    {
        var temporaryDirectory = Path.Combine(Path.GetTempPath(), $"LibraryDesk.Tests.{Guid.NewGuid():N}");
        var databasePath = Path.Combine(temporaryDirectory, "overdue.db");

        try
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { ["Database:FilePath"] = databasePath })
                .Build();
            await using var services = new ServiceCollection()
                .AddApplication()
                .AddInfrastructure(configuration)
                .AddSingleton<IClock>(new FixedClock(new DateTimeOffset(2026, 1, 10, 9, 0, 0, TimeSpan.Zero)))
                .BuildServiceProvider();
            await services.GetRequiredService<IDatabaseInitializer>().InitializeAsync();
            var readers = services.GetRequiredService<IReaderService>();
            var books = services.GetRequiredService<IBookService>();
            var loans = services.GetRequiredService<ILoanService>();

            Assert.True((await readers.CreateAsync(new CreateReaderRequest("R-3001", "Katherine Johnson", null, null))).IsSuccess);
            var book = Assert.IsType<BookDto>((await books.CreateAsync(new CreateBookRequest("Orbital Mechanics", "Katherine Johnson", null, null, null))).Value);
            Assert.True((await books.AddCopyAsync(new AddCopyRequest(book.Id, "BC-3001"))).IsSuccess);
            Assert.True((await loans.BorrowAsync(new BorrowRequest("R-3001", "BC-3001", new DateOnly(2026, 1, 11)))).IsSuccess);

            var overdue = await loans.GetOverdueAsync(new DateOnly(2026, 1, 13));

            var loan = Assert.Single(overdue);
            Assert.Equal("R-3001", loan.MembershipNumber);
            Assert.Equal(new DateOnly(2026, 1, 11), loan.DueOn);
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
