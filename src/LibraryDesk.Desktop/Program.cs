using Avalonia;
using Serilog;

namespace LibraryDesk.Desktop;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            DesktopBootstrapper.ConfigureLogging();
            Log.Information("Starting LibraryDesk");
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception exception)
        {
            Log.Fatal(exception, "LibraryDesk terminated unexpectedly");
            throw;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
