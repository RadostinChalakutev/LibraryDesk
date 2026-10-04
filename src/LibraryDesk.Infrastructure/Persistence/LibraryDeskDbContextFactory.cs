using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LibraryDesk.Infrastructure.Persistence;

/// <summary>
/// Supplies a context to EF Core tooling without starting the desktop application.
/// </summary>
public sealed class LibraryDeskDbContextFactory : IDesignTimeDbContextFactory<LibraryDeskDbContext>
{
    public LibraryDeskDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<LibraryDeskDbContext>()
            .UseSqlite("Data Source=LibraryDesk.design-time.db")
            .Options;

        return new LibraryDeskDbContext(options);
    }
}
