using System.Globalization;
using LibraryDesk.Application.Abstractions;
using LibraryDesk.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryDesk.Infrastructure.Persistence;

/// <summary>
/// Creates or upgrades the local SQLite database using EF Core migrations.
/// </summary>
public sealed class DatabaseInitializer : IDatabaseInitializer
{
    private const string CreatedAtMetadataKey = "database.created_at_utc";
    private readonly IDbContextFactory<LibraryDeskDbContext> _dbContextFactory;

    public DatabaseInitializer(IDbContextFactory<LibraryDeskDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        await dbContext.Database.MigrateAsync(cancellationToken);

        var metadataSet = dbContext.Set<ApplicationMetadata>();
        var hasCreationTimestamp = await metadataSet
            .AnyAsync(metadata => metadata.Key == CreatedAtMetadataKey, cancellationToken);

        if (!hasCreationTimestamp)
        {
            metadataSet.Add(new ApplicationMetadata(
                CreatedAtMetadataKey,
                DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture)));
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
