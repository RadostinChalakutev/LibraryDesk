using Microsoft.Extensions.DependencyInjection;

namespace LibraryDesk.Application;

/// <summary>
/// Registers application-layer services. Feature services are added in their respective phases.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services;
    }
}
