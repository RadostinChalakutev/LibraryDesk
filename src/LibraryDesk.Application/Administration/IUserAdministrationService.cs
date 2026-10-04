using LibraryDesk.Application.Primitives;

namespace LibraryDesk.Application.Administration;

public interface IUserAdministrationService
{
    Task<OperationResult<IReadOnlyList<UserAccountDto>>> GetUsersAsync(CancellationToken cancellationToken = default);

    Task<OperationResult<UserAccountDto>> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default);

    Task<OperationResult> SetUserActiveAsync(Guid userId, bool isActive, CancellationToken cancellationToken = default);
}
