using LibraryDesk.Application.Administration;
using LibraryDesk.Application.Authentication;
using LibraryDesk.Application.Primitives;
using LibraryDesk.Domain.Security;
using LibraryDesk.Infrastructure.Persistence;
using LibraryDesk.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryDesk.Infrastructure.Administration;

/// <summary>
/// Manages local staff accounts after initial setup, with server-side administrator checks.
/// </summary>
public sealed class UserAdministrationService : IUserAdministrationService
{
    private readonly IDbContextFactory<LibraryDeskDbContext> _dbContextFactory;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUserSession _userSession;

    public UserAdministrationService(
        IDbContextFactory<LibraryDeskDbContext> dbContextFactory,
        IPasswordHasher passwordHasher,
        IUserSession userSession)
    {
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _userSession = userSession ?? throw new ArgumentNullException(nameof(userSession));
    }

    public async Task<OperationResult<IReadOnlyList<UserAccountDto>>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        if (!IsAdministrator())
        {
            return OperationResults.Failure<IReadOnlyList<UserAccountDto>>("Administrator permission is required.");
        }

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var users = await dbContext.Set<UserAccount>()
            .AsNoTracking()
            .OrderBy(user => user.DisplayName)
            .Select(user => new UserAccountDto(
                user.Id,
                user.DisplayName,
                user.Username,
                user.Role,
                user.IsActive,
                user.CreatedAtUtc,
                user.LastSignedInAtUtc))
            .ToListAsync(cancellationToken);
        return OperationResults.Success<IReadOnlyList<UserAccountDto>>(users);
    }

    public async Task<OperationResult<UserAccountDto>> CreateUserAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!IsAdministrator())
        {
            return OperationResults.Failure<UserAccountDto>("Administrator permission is required.");
        }

        var validationError = AuthenticationInputValidator.ValidateDisplayName(request.DisplayName)
                              ?? AuthenticationInputValidator.ValidateUsername(request.Username)
                              ?? AuthenticationInputValidator.ValidatePassword(request.Password);
        if (validationError is not null)
        {
            return OperationResults.Failure<UserAccountDto>(validationError);
        }

        if (!Enum.IsDefined(request.Role))
        {
            return OperationResults.Failure<UserAccountDto>("The selected user role is invalid.");
        }

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var normalizedUsername = AuthenticationInputValidator.NormalizeUsername(request.Username);
        if (await dbContext.Set<UserAccount>().AnyAsync(user => user.NormalizedUsername == normalizedUsername, cancellationToken))
        {
            return OperationResults.Failure<UserAccountDto>("That username is already in use.");
        }

        var passwordHash = _passwordHasher.Hash(request.Password);
        var account = new UserAccount(
            Guid.NewGuid(),
            request.DisplayName.Trim(),
            request.Username.Trim(),
            normalizedUsername,
            passwordHash.Hash,
            passwordHash.Salt,
            request.Role,
            DateTimeOffset.UtcNow);
        dbContext.Set<UserAccount>().Add(account);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return OperationResults.Failure<UserAccountDto>("That username is already in use.");
        }

        return OperationResults.Success(ToDto(account));
    }

    public async Task<OperationResult> SetUserActiveAsync(
        Guid userId,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        if (!IsAdministrator())
        {
            return OperationResult.Failure("Administrator permission is required.");
        }

        if (!isActive && _userSession.CurrentUser?.Id == userId)
        {
            return OperationResult.Failure("You cannot deactivate the account currently signed in.");
        }

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var account = await dbContext.Set<UserAccount>().FindAsync([userId], cancellationToken);
        if (account is null)
        {
            return OperationResult.Failure("The user account was not found.");
        }

        if (!isActive && account.Role == UserRole.Administrator)
        {
            var activeAdministrators = await dbContext.Set<UserAccount>()
                .CountAsync(user => user.Role == UserRole.Administrator && user.IsActive, cancellationToken);
            if (activeAdministrators <= 1)
            {
                return OperationResult.Failure("At least one active administrator account must remain.");
            }
        }

        account.SetActive(isActive);
        await dbContext.SaveChangesAsync(cancellationToken);
        return OperationResult.Success();
    }

    private bool IsAdministrator() => _userSession.CurrentUser?.Role == UserRole.Administrator;

    private static UserAccountDto ToDto(UserAccount account) =>
        new(
            account.Id,
            account.DisplayName,
            account.Username,
            account.Role,
            account.IsActive,
            account.CreatedAtUtc,
            account.LastSignedInAtUtc);
}
