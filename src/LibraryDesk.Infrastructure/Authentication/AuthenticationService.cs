using LibraryDesk.Application.Authentication;
using LibraryDesk.Infrastructure.Persistence;
using LibraryDesk.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryDesk.Infrastructure.Authentication;

/// <summary>
/// Authenticates persisted local user accounts.
/// </summary>
public sealed class AuthenticationService : IAuthenticationService
{
    private const string InvalidCredentialsMessage = "Invalid username or password.";
    private readonly IDbContextFactory<LibraryDeskDbContext> _dbContextFactory;
    private readonly IPasswordHasher _passwordHasher;

    public AuthenticationService(
        IDbContextFactory<LibraryDeskDbContext> dbContextFactory,
        IPasswordHasher passwordHasher)
    {
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
    }

    public async Task<AuthenticationResult> AuthenticateAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
        {
            return AuthenticationResult.Failure(InvalidCredentialsMessage);
        }

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var normalizedUsername = AuthenticationInputValidator.NormalizeUsername(username);
        var account = await dbContext.Set<UserAccount>()
            .SingleOrDefaultAsync(user => user.NormalizedUsername == normalizedUsername, cancellationToken);

        if (account is null || !account.IsActive || !_passwordHasher.Verify(
                password,
                new PasswordHash(account.PasswordHash, account.PasswordSalt)))
        {
            return AuthenticationResult.Failure(InvalidCredentialsMessage);
        }

        account.RecordSuccessfulSignIn(DateTimeOffset.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);

        return AuthenticationResult.Success(CreateAuthenticatedUser(account));
    }

    private static AuthenticatedUser CreateAuthenticatedUser(UserAccount account) =>
        new(account.Id, account.Username, account.DisplayName, account.Role);
}
