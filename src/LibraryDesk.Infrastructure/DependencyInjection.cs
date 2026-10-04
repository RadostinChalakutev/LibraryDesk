using LibraryDesk.Application.Abstractions;
using LibraryDesk.Infrastructure.Persistence;
using LibraryDesk.Infrastructure.Time;
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
        services.AddSingleton<DatabaseConnectionStringProvider>();
        services.AddDbContextFactory<LibraryDeskDbContext>((serviceProvider, options) =>
        {
            var connectionStringProvider = serviceProvider.GetRequiredService<DatabaseConnectionStringProvider>();
            options.UseSqlite(connectionStringProvider.ConnectionString);
        });
        services.AddSingleton<IDatabaseInitializer, DatabaseInitializer>();
        return services;
    }
}
