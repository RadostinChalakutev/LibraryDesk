using LibraryDesk.Application.Authentication;
using LibraryDesk.Domain.Security;
using LibraryDesk.Infrastructure.Persistence;
using LibraryDesk.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryDesk.Infrastructure.Authentication;

/// <summary>
/// Creates the first administrator account for a new LibraryDesk installation.
/// </summary>
public sealed class InitialAdministratorSetupService : IInitialAdministratorSetupService
{
    private const string InitialAdministratorMetadataKey = "authentication.initial_administrator_configured";
    private readonly IDbContextFactory<LibraryDeskDbContext> _dbContextFactory;
    private readonly IPasswordHasher _passwordHasher;

    public InitialAdministratorSetupService(
        IDbContextFactory<LibraryDeskDbContext> dbContextFactory,
        IPasswordHasher passwordHasher)
    {
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
    }

    public async Task<bool> HasUserAccountsAsync(CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await dbContext.Set<UserAccount>().AnyAsync(cancellationToken);
    }

    public async Task<InitialAdministratorSetupResult> CreateInitialAdministratorAsync(
        InitialAdministratorRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var validationError = AuthenticationInputValidator.ValidateDisplayName(request.DisplayName)
                              ?? AuthenticationInputValidator.ValidateUsername(request.Username)
                              ?? AuthenticationInputValidator.ValidatePassword(request.Password);
        if (validationError is not null)
        {
            return InitialAdministratorSetupResult.Failure(validationError);
        }

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var userAccounts = dbContext.Set<UserAccount>();
        var applicationMetadata = dbContext.Set<ApplicationMetadata>();
        if (await userAccounts.AnyAsync(cancellationToken)
            || await applicationMetadata.AnyAsync(
                metadata => metadata.Key == InitialAdministratorMetadataKey,
                cancellationToken))
        {
            return InitialAdministratorSetupResult.Failure(
                "An administrator account has already been configured for this installation.");
        }

        var passwordHash = _passwordHasher.Hash(request.Password);
        var account = new UserAccount(
            Guid.NewGuid(),
            request.DisplayName.Trim(),
            request.Username.Trim(),
            AuthenticationInputValidator.NormalizeUsername(request.Username),
            passwordHash.Hash,
            passwordHash.Salt,
            UserRole.Administrator,
            DateTimeOffset.UtcNow);

        userAccounts.Add(account);
        applicationMetadata.Add(new ApplicationMetadata(
            InitialAdministratorMetadataKey,
            DateTimeOffset.UtcNow.ToString("O", System.Globalization.CultureInfo.InvariantCulture)));

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return InitialAdministratorSetupResult.Failure(
                "An administrator account has already been configured for this installation.");
        }

        return InitialAdministratorSetupResult.Success(
            new AuthenticatedUser(account.Id, account.Username, account.DisplayName, account.Role));
    }
}
