using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace LibraryDesk.Infrastructure.Persistence;

/// <summary>
/// Resolves the local SQLite database path from configuration and ensures its directory exists.
/// </summary>
public sealed class DatabaseConnectionStringProvider
{
    private const string DefaultDatabaseFileName = "LibraryDesk.db";
    private readonly IConfiguration _configuration;
    private string? _connectionString;

    public DatabaseConnectionStringProvider(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public string ConnectionString => _connectionString ??= CreateConnectionString();

    private string CreateConnectionString()
    {
        var configuredFilePath = _configuration["Database:FilePath"];
        var databasePath = string.IsNullOrWhiteSpace(configuredFilePath)
            ? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "LibraryDesk",
                DefaultDatabaseFileName)
            : Path.GetFullPath(configuredFilePath);

        var databaseDirectory = Path.GetDirectoryName(databasePath);
        if (string.IsNullOrWhiteSpace(databaseDirectory))
        {
            throw new InvalidOperationException("The configured database path must include a directory.");
        }

        Directory.CreateDirectory(databaseDirectory);

        return new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            ForeignKeys = true,
            Pooling = true,
        }.ToString();
    }
}
