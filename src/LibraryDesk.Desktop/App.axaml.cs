using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using LibraryDesk.Application.Abstractions;
using LibraryDesk.Application.Authentication;
using LibraryDesk.Desktop.ViewModels;
using LibraryDesk.Desktop.Views;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace LibraryDesk.Desktop;

public partial class App : Avalonia.Application
{
    private ServiceProvider? _services;
    private IClassicDesktopStyleApplicationLifetime? _desktopLifetime;

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
            _desktopLifetime = desktop;
            desktop.Exit += (_, _) => _services.Dispose();
            ShowAuthenticationWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void ShowAuthenticationWindow()
    {
        var services = GetServices();
        var setupService = services.GetRequiredService<IInitialAdministratorSetupService>();

        if (setupService.HasUserAccountsAsync().GetAwaiter().GetResult())
        {
            ShowLoginWindow();
        }
        else
        {
            ShowInitialAdministratorSetupWindow();
        }
    }

    private void ShowInitialAdministratorSetupWindow()
    {
        var viewModel = GetServices().GetRequiredService<InitialAdministratorSetupViewModel>();
        viewModel.AdministratorCreated += (_, _) => ShowMainWindow();
        ReplaceMainWindow(new InitialAdministratorSetupWindow { DataContext = viewModel });
    }

    private void ShowLoginWindow()
    {
        var viewModel = GetServices().GetRequiredService<LoginWindowViewModel>();
        viewModel.SignedIn += (_, _) => ShowMainWindow();
        ReplaceMainWindow(new LoginWindow { DataContext = viewModel });
    }

    private void ShowMainWindow()
    {
        var viewModel = GetServices().GetRequiredService<MainWindowViewModel>();
        viewModel.SignedOut += (_, _) => ShowLoginWindow();
        ReplaceMainWindow(new MainWindow { DataContext = viewModel });
    }

    private void ReplaceMainWindow(Window nextWindow)
    {
        var desktopLifetime = _desktopLifetime
                              ?? throw new InvalidOperationException("Desktop application lifetime is unavailable.");
        var previousWindow = desktopLifetime.MainWindow;
        desktopLifetime.MainWindow = nextWindow;

        if (previousWindow is not null)
        {
            nextWindow.Show();
            previousWindow.Close();
        }
    }

    private ServiceProvider GetServices() =>
        _services ?? throw new InvalidOperationException("The service provider has not been initialized.");
}
