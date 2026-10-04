namespace LibraryDesk.Application.Authentication;

/// <summary>
/// Manages the single first-run administrator setup operation.
/// </summary>
public interface IInitialAdministratorSetupService
{
    Task<bool> HasUserAccountsAsync(CancellationToken cancellationToken = default);

    Task<InitialAdministratorSetupResult> CreateInitialAdministratorAsync(
        InitialAdministratorRequest request,
        CancellationToken cancellationToken = default);
}
