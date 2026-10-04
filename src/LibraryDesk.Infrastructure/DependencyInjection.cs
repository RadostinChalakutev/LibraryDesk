using LibraryDesk.Application.Abstractions;
using LibraryDesk.Application.Authentication;
using LibraryDesk.Infrastructure.Authentication;
using LibraryDesk.Infrastructure.Persistence;
using LibraryDesk.Infrastructure.Readers;
using LibraryDesk.Application.Readers;
using LibraryDesk.Application.Books;
using LibraryDesk.Infrastructure.Books;
using LibraryDesk.Application.Loans;
using LibraryDesk.Infrastructure.Loans;
using LibraryDesk.Infrastructure.Time;
using LibraryDesk.Application.Dashboard;
using LibraryDesk.Infrastructure.Dashboard;
using LibraryDesk.Application.Reports;
using LibraryDesk.Infrastructure.Reports;
using LibraryDesk.Application.Administration;
using LibraryDesk.Infrastructure.Administration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryDesk.Infrastructure;

/// <summary>
/// Registers infrastructure implementations.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddSingleton(configuration);
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<DatabaseConnectionStringProvider>();
        services.AddDbContextFactory<LibraryDeskDbContext>((serviceProvider, options) =>
        {
            var connectionStringProvider = serviceProvider.GetRequiredService<DatabaseConnectionStringProvider>();
            options.UseSqlite(connectionStringProvider.ConnectionString);
        });
        services.AddSingleton<IDatabaseInitializer, DatabaseInitializer>();
        services.AddSingleton<IInitialAdministratorSetupService, InitialAdministratorSetupService>();
        services.AddSingleton<IAuthenticationService, AuthenticationService>();
        services.AddSingleton<IReaderService, ReaderService>();
        services.AddSingleton<IBookService, BookService>();
        services.AddSingleton<ILoanService, LoanService>();
        services.AddSingleton<IDashboardService, DashboardService>();
        services.AddSingleton<IReportService, ReportService>();
        services.AddSingleton<IUserAdministrationService, UserAdministrationService>();
        services.AddSingleton<IDatabaseBackupService, DatabaseBackupService>();
        return services;
    }
}
