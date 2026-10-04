using LibraryDesk.Application;
using LibraryDesk.Application.Abstractions;
using LibraryDesk.Application.Authentication;
using LibraryDesk.Domain.Security;
using LibraryDesk.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryDesk.Infrastructure.Tests.Authentication;

public sealed class AuthenticationServiceTests
{
    [Fact]
    public async Task CreatesInitialAdministratorAndAuthenticatesSecurely()
    {
        var temporaryDirectory = Path.Combine(Path.GetTempPath(), $"LibraryDesk.Tests.{Guid.NewGuid():N}");
        var databasePath = Path.Combine(temporaryDirectory, "librarydesk-authentication-test.db");
        const string password = "LibraryDesk!2026";

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
                             .AddApplication()
                             .AddInfrastructure(configuration)
                             .BuildServiceProvider())
            {
                var databaseInitializer = serviceProvider.GetRequiredService<IDatabaseInitializer>();
                await databaseInitializer.InitializeAsync();

                var setupService = serviceProvider.GetRequiredService<IInitialAdministratorSetupService>();
                Assert.False(await setupService.HasUserAccountsAsync());

                var setupResult = await setupService.CreateInitialAdministratorAsync(
                    new InitialAdministratorRequest("Library Administrator", "admin", password));

                Assert.True(setupResult.IsSuccess);
                var initialUser = Assert.IsType<AuthenticatedUser>(setupResult.User);
                Assert.Equal(UserRole.Administrator, initialUser.Role);
                Assert.True(await setupService.HasUserAccountsAsync());

                var duplicateSetupResult = await setupService.CreateInitialAdministratorAsync(
                    new InitialAdministratorRequest("Another Administrator", "otheradmin", password));
                Assert.False(duplicateSetupResult.IsSuccess);

                var authenticationService = serviceProvider.GetRequiredService<IAuthenticationService>();
                var invalidLogin = await authenticationService.AuthenticateAsync("admin", "not-the-password");
                var validLogin = await authenticationService.AuthenticateAsync("ADMIN", password);

                Assert.False(invalidLogin.IsSuccess);
                Assert.Null(invalidLogin.User);
                Assert.True(validLogin.IsSuccess);
                var authenticatedUser = Assert.IsType<AuthenticatedUser>(validLogin.User);
                Assert.Equal("admin", authenticatedUser.Username);

                var userSession = serviceProvider.GetRequiredService<IUserSession>();
                userSession.Start(authenticatedUser);
                Assert.True(userSession.IsAuthenticated);
                Assert.True(userSession.HasPermission(Capability.ManageUsers));
                userSession.Clear();
                Assert.False(userSession.IsAuthenticated);

                var factory = serviceProvider.GetRequiredService<
                    IDbContextFactory<LibraryDesk.Infrastructure.Persistence.LibraryDeskDbContext>>();
                await using var dbContext = await factory.CreateDbContextAsync();
                await dbContext.Database.OpenConnectionAsync();
                try
                {
                    await using var command = dbContext.Database.GetDbConnection().CreateCommand();
                    command.CommandText = "SELECT PasswordHash FROM user_accounts WHERE Username = $username";
                    var usernameParameter = command.CreateParameter();
                    usernameParameter.ParameterName = "$username";
                    usernameParameter.Value = "admin";
                    command.Parameters.Add(usernameParameter);

                    var storedHash = Assert.IsType<string>(await command.ExecuteScalarAsync());
                    Assert.NotEqual(password, storedHash);
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
