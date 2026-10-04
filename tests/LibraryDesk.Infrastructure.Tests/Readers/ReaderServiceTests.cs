using LibraryDesk.Application;
using LibraryDesk.Application.Abstractions;
using LibraryDesk.Application.Readers;
using LibraryDesk.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryDesk.Infrastructure.Tests.Readers;

public sealed class ReaderServiceTests
{
    [Fact]
    public async Task RegistersSearchesAndDeactivatesReader()
    {
        var temporaryDirectory = Path.Combine(Path.GetTempPath(), $"LibraryDesk.Tests.{Guid.NewGuid():N}");
        var databasePath = Path.Combine(temporaryDirectory, "readers.db");

        try
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { ["Database:FilePath"] = databasePath })
                .Build();
            await using (var services = new ServiceCollection().AddApplication().AddInfrastructure(configuration).BuildServiceProvider())
            {
                await services.GetRequiredService<IDatabaseInitializer>().InitializeAsync();
                var readerService = services.GetRequiredService<IReaderService>();

                var createResult = await readerService.CreateAsync(
                    new CreateReaderRequest("R-1001", "Ada Lovelace", "ada@example.test", "555-0100"));
                var duplicateResult = await readerService.CreateAsync(
                    new CreateReaderRequest("r-1001", "Duplicate", null, null));

                var reader = Assert.IsType<ReaderDto>(createResult.Value);
                Assert.True(createResult.IsSuccess);
                Assert.False(duplicateResult.IsSuccess);
                Assert.Single(await readerService.SearchAsync("Ada", includeInactive: false));

                var deactivateResult = await readerService.SetActiveAsync(reader.Id, isActive: false);
                Assert.True(deactivateResult.IsSuccess);
                Assert.Empty(await readerService.SearchAsync("R-1001", includeInactive: false));
                Assert.Single(await readerService.SearchAsync("R-1001", includeInactive: true));
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
