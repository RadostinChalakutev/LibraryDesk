using LibraryDesk.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryDesk.Infrastructure.Persistence;

/// <summary>
/// Represents LibraryDesk's local persisted state. Business tables are added in later phases.
/// </summary>
public sealed class LibraryDeskDbContext : DbContext
{
    public LibraryDeskDbContext(DbContextOptions<LibraryDeskDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApplicationMetadata>(entity =>
        {
            entity.ToTable("application_metadata");
            entity.HasKey(metadata => metadata.Key);
            entity.Property(metadata => metadata.Key).HasMaxLength(128);
            entity.Property(metadata => metadata.Value).HasMaxLength(512).IsRequired();
        });
    }
}
