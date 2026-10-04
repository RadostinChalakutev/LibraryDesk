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

public sealed class LoanWorkflowTests
{
    [Fact]
    public async Task BorrowsAndReturnsPhysicalCopyWithHistory()
    {
        var temporaryDirectory = Path.Combine(Path.GetTempPath(), $"LibraryDesk.Tests.{Guid.NewGuid():N}");
        var databasePath = Path.Combine(temporaryDirectory, "loans.db");

        try
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { ["Database:FilePath"] = databasePath })
                .Build();
            await using (var services = new ServiceCollection().AddApplication().AddInfrastructure(configuration).BuildServiceProvider())
            {
                await services.GetRequiredService<IDatabaseInitializer>().InitializeAsync();
                var readerService = services.GetRequiredService<IReaderService>();
                var bookService = services.GetRequiredService<IBookService>();
                var loanService = services.GetRequiredService<ILoanService>();

                var reader = Assert.IsType<ReaderDto>((await readerService.CreateAsync(
                    new CreateReaderRequest("R-2001", "Grace Hopper", null, null))).Value);
                var book = Assert.IsType<BookDto>((await bookService.CreateAsync(
                    new CreateBookRequest("The Compiler", "Grace Hopper", "9780000000001", "Computing", 1952))).Value);
                var copy = Assert.IsType<CopyDto>((await bookService.AddCopyAsync(new AddCopyRequest(book.Id, "BC-2001"))).Value);

                var borrowResult = await loanService.BorrowAsync(new BorrowRequest("r-2001", "bc-2001", DateOnly.FromDateTime(DateTime.UtcNow).AddDays(7)));
                Assert.True(borrowResult.IsSuccess);
                var loan = Assert.IsType<LoanDto>(borrowResult.Value);
                Assert.Equal(copy.Id, loan.PhysicalCopyId);
                Assert.Single(await loanService.GetActiveAsync());
                Assert.False((await loanService.BorrowAsync(new BorrowRequest("R-2001", "BC-2001", DateOnly.FromDateTime(DateTime.UtcNow).AddDays(7)))).IsSuccess);

                Assert.True((await loanService.ReturnAsync(loan.Id)).IsSuccess);
                Assert.Empty(await loanService.GetActiveAsync());
                var history = await loanService.GetHistoryAsync(reader.Id);
                Assert.Single(history);
                Assert.NotNull(history[0].ReturnedOn);
            }
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
}
