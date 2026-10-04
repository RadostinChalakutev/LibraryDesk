using LibraryDesk.Application.Administration;
using LibraryDesk.Application.Authentication;
using LibraryDesk.Application.Primitives;
using LibraryDesk.Domain.Security;
using LibraryDesk.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;

namespace LibraryDesk.Infrastructure.Administration;

/// <summary>
/// Creates SQLite-consistent point-in-time database backups without copying live database files directly.
/// </summary>
public sealed class DatabaseBackupService : IDatabaseBackupService
{
    private readonly DatabaseConnectionStringProvider _connectionStringProvider;
    private readonly IUserSession _userSession;

    public DatabaseBackupService(DatabaseConnectionStringProvider connectionStringProvider, IUserSession userSession)
    {
        _connectionStringProvider = connectionStringProvider ?? throw new ArgumentNullException(nameof(connectionStringProvider));
        _userSession = userSession ?? throw new ArgumentNullException(nameof(userSession));
    }

    public Task<OperationResult<DatabaseBackup>> CreateBackupAsync(
        string destinationDirectory,
        CancellationToken cancellationToken = default)
    {
        if (!_userSession.HasPermission(Capability.ManageSettings))
        {
            return Task.FromResult(OperationResults.Failure<DatabaseBackup>("Administrator permission is required."));
        }

        if (string.IsNullOrWhiteSpace(destinationDirectory))
        {
            return Task.FromResult(OperationResults.Failure<DatabaseBackup>("A backup destination is required."));
        }

        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            Directory.CreateDirectory(destinationDirectory);
            var createdAtUtc = DateTimeOffset.UtcNow;
            var filePath = Path.Combine(destinationDirectory, $"LibraryDesk-{createdAtUtc:yyyyMMdd-HHmmssfff}.db");
            using var source = new SqliteConnection(_connectionStringProvider.ConnectionString);
            using var destination = new SqliteConnection(new SqliteConnectionStringBuilder
            {
                DataSource = filePath,
                ForeignKeys = true,
            }.ToString());
            source.Open();
            destination.Open();
            source.BackupDatabase(destination);
            var sizeInBytes = new FileInfo(filePath).Length;
            return Task.FromResult(OperationResults.Success(new DatabaseBackup(filePath, createdAtUtc, sizeInBytes)));
        }
        catch (IOException exception)
        {
            return Task.FromResult(OperationResults.Failure<DatabaseBackup>($"The backup could not be created: {exception.Message}"));
        }
        catch (SqliteException exception)
        {
            return Task.FromResult(OperationResults.Failure<DatabaseBackup>($"The backup could not be created: {exception.Message}"));
        }
        catch (UnauthorizedAccessException exception)
        {
            return Task.FromResult(OperationResults.Failure<DatabaseBackup>($"The backup destination is not accessible: {exception.Message}"));
        }
    }
}
