using LibraryDesk.Infrastructure.Persistence.Models;
using LibraryDesk.Domain.Readers;
using LibraryDesk.Domain.Books;
using LibraryDesk.Domain.Loans;
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

        modelBuilder.Entity<UserAccount>(entity =>
        {
            entity.ToTable("user_accounts");
            entity.HasKey(user => user.Id);
            entity.HasIndex(user => user.NormalizedUsername).IsUnique();
            entity.Property(user => user.DisplayName).HasMaxLength(120).IsRequired();
            entity.Property(user => user.Username).HasMaxLength(64).IsRequired();
            entity.Property(user => user.NormalizedUsername).HasMaxLength(64).IsRequired();
            entity.Property(user => user.PasswordHash).HasMaxLength(256).IsRequired();
            entity.Property(user => user.PasswordSalt).HasMaxLength(128).IsRequired();
            entity.Property(user => user.Role).HasConversion<string>().HasMaxLength(32).IsRequired();
        });

        modelBuilder.Entity<Reader>(entity =>
        {
            entity.ToTable("readers");
            entity.HasKey(reader => reader.Id);
            entity.HasIndex(reader => reader.MembershipNumber).IsUnique();
            entity.Property(reader => reader.MembershipNumber).HasMaxLength(32).IsRequired();
            entity.Property(reader => reader.FullName).HasMaxLength(160).IsRequired();
            entity.Property(reader => reader.Email).HasMaxLength(256);
            entity.Property(reader => reader.Phone).HasMaxLength(64);
        });

        modelBuilder.Entity<Book>(entity =>
        {
            entity.ToTable("books");
            entity.HasKey(book => book.Id);
            entity.HasIndex(book => book.Isbn).IsUnique();
            entity.Property(book => book.Title).HasMaxLength(240).IsRequired();
            entity.Property(book => book.Author).HasMaxLength(160).IsRequired();
            entity.Property(book => book.Isbn).HasMaxLength(32);
            entity.Property(book => book.Category).HasMaxLength(100);
        });

        modelBuilder.Entity<PhysicalCopy>(entity =>
        {
            entity.ToTable("physical_copies");
            entity.HasKey(copy => copy.Id);
            entity.HasIndex(copy => copy.Barcode).IsUnique();
            entity.Property(copy => copy.Barcode).HasMaxLength(64).IsRequired();
            entity.Property(copy => copy.Status).HasConversion<string>().HasMaxLength(24).IsRequired();
            entity.HasOne<Book>().WithMany(book => book.Copies).HasForeignKey(copy => copy.BookId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Loan>(entity =>
        {
            entity.ToTable("loans");
            entity.HasKey(loan => loan.Id);
            entity.HasIndex(loan => loan.ReaderId);
            entity.HasIndex(loan => loan.PhysicalCopyId).IsUnique().HasFilter("\"ReturnedOn\" IS NULL");
            entity.HasOne<Reader>().WithMany().HasForeignKey(loan => loan.ReaderId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<PhysicalCopy>().WithMany().HasForeignKey(loan => loan.PhysicalCopyId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
