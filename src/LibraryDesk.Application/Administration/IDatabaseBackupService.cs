using LibraryDesk.Application.Primitives;

namespace LibraryDesk.Application.Administration;

public interface IDatabaseBackupService
{
    Task<OperationResult<DatabaseBackup>> CreateBackupAsync(
        string destinationDirectory,
        CancellationToken cancellationToken = default);
}
