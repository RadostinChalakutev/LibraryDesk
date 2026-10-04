using LibraryDesk.Application.Abstractions;
using LibraryDesk.Infrastructure.Time;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryDesk.Infrastructure;

/// <summary>
/// Registers infrastructure implementations. Persistence is intentionally introduced in Phase 2.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddSingleton<IClock, SystemClock>();
        return services;
    }
}
