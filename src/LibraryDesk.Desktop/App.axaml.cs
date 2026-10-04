using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using LibraryDesk.Application.Abstractions;
using LibraryDesk.Desktop.ViewModels;
using LibraryDesk.Desktop.Views;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace LibraryDesk.Desktop;

public partial class App : Avalonia.Application
{
    private ServiceProvider? _services;

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        _services = (ServiceProvider)DesktopBootstrapper.BuildServiceProvider();

        try
        {
            _services.GetRequiredService<IDatabaseInitializer>()
                .InitializeAsync()
                .GetAwaiter()
                .GetResult();
            Log.Information("Local database initialization completed");
        }
        catch (Exception exception)
        {
            _services.Dispose();
            Log.Fatal(exception, "LibraryDesk could not initialize its local database");
            throw;
        }

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = _services.GetRequiredService<MainWindowViewModel>(),
            };
            desktop.Exit += (_, _) => _services.Dispose();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
