using LibraryDesk.Application;
using LibraryDesk.Application.Administration;
using LibraryDesk.Application.Authentication;
using LibraryDesk.Application.Abstractions;
using LibraryDesk.Application.Readers;
using LibraryDesk.Domain.Security;
using LibraryDesk.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryDesk.Infrastructure.Tests.Administration;

public sealed class AdministrationAndBackupTests
{
    [Fact]
    public async Task AdministratorCanProvisionStaffAndCreateUsableDatabaseBackup()
    {
        var temporaryDirectory = Path.Combine(Path.GetTempPath(), $"LibraryDesk.Tests.{Guid.NewGuid():N}");
        var databasePath = Path.Combine(temporaryDirectory, "administration.db");
        var backupDirectory = Path.Combine(temporaryDirectory, "backups");

        try
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { ["Database:FilePath"] = databasePath })
                .Build();
            await using var services = new ServiceCollection().AddApplication().AddInfrastructure(configuration).BuildServiceProvider();
            await services.GetRequiredService<IDatabaseInitializer>().InitializeAsync();
            var setup = services.GetRequiredService<IInitialAdministratorSetupService>();
            var setupResult = await setup.CreateInitialAdministratorAsync(
                new InitialAdministratorRequest("Admin User", "admin", "SecurePassword123"));
            var administrator = Assert.IsType<AuthenticatedUser>(setupResult.User);
            services.GetRequiredService<IUserSession>().Start(administrator);

            var administration = services.GetRequiredService<IUserAdministrationService>();
            var createLibrarian = await administration.CreateUserAsync(
                new CreateUserRequest("Library Staff", "librarian", "AnotherSecure123", UserRole.Librarian));
            Assert.True(createLibrarian.IsSuccess);
            var usersResult = await administration.GetUsersAsync();
            Assert.True(usersResult.IsSuccess);
            var users = Assert.IsAssignableFrom<IReadOnlyList<UserAccountDto>>(usersResult.Value);
            Assert.Equal(2, users.Count);

            var readerService = services.GetRequiredService<IReaderService>();
            Assert.True((await readerService.CreateAsync(new CreateReaderRequest("R-6001", "Backup Reader", null, null))).IsSuccess);
            var backupResult = await services.GetRequiredService<IDatabaseBackupService>().CreateBackupAsync(backupDirectory);
            var backup = Assert.IsType<DatabaseBackup>(backupResult.Value);
            Assert.True(backupResult.IsSuccess);
            Assert.True(backup.SizeInBytes > 0);

            using var backupConnection = new SqliteConnection($"Data Source={backup.FilePath}");
            backupConnection.Open();
            using var command = backupConnection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM readers;";
            Assert.Equal(1L, (long)command.ExecuteScalar()!);
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

    [Fact]
    public async Task LibrarianCannotManageUserAccounts()
    {
        var temporaryDirectory = Path.Combine(Path.GetTempPath(), $"LibraryDesk.Tests.{Guid.NewGuid():N}");
        var databasePath = Path.Combine(temporaryDirectory, "permissions.db");

        try
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { ["Database:FilePath"] = databasePath })
                .Build();
            await using var services = new ServiceCollection().AddApplication().AddInfrastructure(configuration).BuildServiceProvider();
            await services.GetRequiredService<IDatabaseInitializer>().InitializeAsync();
            services.GetRequiredService<IUserSession>().Start(new AuthenticatedUser(Guid.NewGuid(), "staff", "Library Staff", UserRole.Librarian));

            var result = await services.GetRequiredService<IUserAdministrationService>().GetUsersAsync();
            var backupResult = await services.GetRequiredService<IDatabaseBackupService>().CreateBackupAsync(
                Path.Combine(temporaryDirectory, "backups"));

            Assert.False(result.IsSuccess);
            Assert.Equal("Administrator permission is required.", result.Error);
            Assert.False(backupResult.IsSuccess);
            Assert.Equal("Administrator permission is required.", backupResult.Error);
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
