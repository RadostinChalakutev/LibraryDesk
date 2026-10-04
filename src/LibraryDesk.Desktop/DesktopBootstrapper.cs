using LibraryDesk.Application;
using LibraryDesk.Infrastructure;
using LibraryDesk.Desktop.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using System.Globalization;

namespace LibraryDesk.Desktop;

internal static class DesktopBootstrapper
{
    private static readonly Lazy<IConfiguration> Configuration = new(BuildConfiguration);

    public static void ConfigureLogging()
    {
        var configuration = Configuration.Value;
        var minimumLevel = ParseLogLevel(configuration["Logging:MinimumLevel"]);
        var retainedFileCount = configuration.GetValue("Logging:RetainedFileCount", 14);
        var logDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "LibraryDesk",
            "logs");

        Directory.CreateDirectory(logDirectory);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is(minimumLevel)
            .WriteTo.File(
                Path.Combine(logDirectory, "librarydesk-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: retainedFileCount,
                formatProvider: CultureInfo.InvariantCulture,
                shared: true)
            .CreateLogger();
    }

    public static IServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddSingleton<Serilog.ILogger>(Log.Logger);
        services.AddApplication();
        services.AddInfrastructure(Configuration.Value);
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<DashboardPageViewModel>();
        services.AddTransient<ReaderPageViewModel>();
        services.AddTransient<BookPageViewModel>();
        services.AddTransient<LoanPageViewModel>();
        services.AddTransient<OverduePageViewModel>();
        services.AddTransient<ReportsPageViewModel>();
        services.AddTransient<AdministrationPageViewModel>();
        services.AddTransient<LoginWindowViewModel>();
        services.AddTransient<InitialAdministratorSetupViewModel>();
        return services.BuildServiceProvider(validateScopes: true);
    }

    private static IConfiguration BuildConfiguration() =>
        new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddEnvironmentVariables(prefix: "LIBRARYDESK_")
            .Build();

    private static LogEventLevel ParseLogLevel(string? configuredLevel) =>
        Enum.TryParse<LogEventLevel>(configuredLevel, ignoreCase: true, out var logLevel)
            ? logLevel
            : LogEventLevel.Information;
}
