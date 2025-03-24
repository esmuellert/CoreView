using System.Windows;
using CoreView.App.Services;
using CoreView.App.ViewModels;
using CoreView.Core.Temperature;

namespace CoreView.App;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    private TrayIconService? _trayIconService;
    private TemperatureService? _temperatureService;
    private TemperatureViewModel? _temperatureViewModel;

    private void Application_Startup(object sender, StartupEventArgs e)
    {
        // Initialize services
        _temperatureService = new TemperatureService();
        _temperatureViewModel = new TemperatureViewModel(_temperatureService);
        _trayIconService = new TrayIconService(_temperatureService);

        // Create and hide the main window
        var mainWindow = new MainWindow
        {
            DataContext = _temperatureViewModel,
            WindowStartupLocation = WindowStartupLocation.CenterScreen
        };

        // Store the main window in our application resources for access elsewhere
        Resources["MainWindow"] = mainWindow;

        // Don't show the main window at startup - we'll use the tray icon
        // instead, but keep the application running
        ShutdownMode = ShutdownMode.OnExplicitShutdown;
    }

    private void Application_Exit(object sender, ExitEventArgs e)
    {
        // Clean up resources
        _trayIconService?.Dispose();
        _temperatureViewModel?.Dispose();
        _temperatureService?.Dispose();
    }
}

