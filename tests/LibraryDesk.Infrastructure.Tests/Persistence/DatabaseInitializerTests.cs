using LibraryDesk.Application.Abstractions;
using LibraryDesk.Infrastructure;
using LibraryDesk.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryDesk.Infrastructure.Tests.Persistence;

public sealed class DatabaseInitializerTests
{
    [Fact]
    public async Task InitializeAsyncCreatesAndMigratesConfiguredSQLiteDatabase()
    {
        var temporaryDirectory = Path.Combine(Path.GetTempPath(), $"LibraryDesk.Tests.{Guid.NewGuid():N}");
        var databasePath = Path.Combine(temporaryDirectory, "librarydesk-test.db");

        try
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(
                    new Dictionary<string, string?>
                    {
                        ["Database:FilePath"] = databasePath,
                    })
                .Build();

            await using (var serviceProvider = new ServiceCollection()
                             .AddInfrastructure(configuration)
                             .BuildServiceProvider())
            {
                var initializer = serviceProvider.GetRequiredService<IDatabaseInitializer>();
                await initializer.InitializeAsync();
                await initializer.InitializeAsync();

                Assert.True(File.Exists(databasePath));

                var factory = serviceProvider.GetRequiredService<IDbContextFactory<LibraryDeskDbContext>>();
                await using var dbContext = await factory.CreateDbContextAsync();
                var appliedMigrations = await dbContext.Database.GetAppliedMigrationsAsync();

                Assert.Contains(appliedMigrations, migration =>
                    migration.EndsWith("InitialInfrastructure", StringComparison.Ordinal));
                Assert.Contains(appliedMigrations, migration =>
                    migration.EndsWith("AddAuthentication", StringComparison.Ordinal));

                await dbContext.Database.OpenConnectionAsync();
                try
                {
                    await using var command = dbContext.Database.GetDbConnection().CreateCommand();
                    command.CommandText = "SELECT Value FROM application_metadata WHERE Key = $key";
                    var keyParameter = command.CreateParameter();
                    keyParameter.ParameterName = "$key";
                    keyParameter.Value = "database.created_at_utc";
                    command.Parameters.Add(keyParameter);

                    Assert.IsType<string>(await command.ExecuteScalarAsync());
                }
                finally
                {
                    await dbContext.Database.CloseConnectionAsync();
                }
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
