using System.Windows;
using CoreView.App.Services;
using CoreView.Core.Temperature;
using Application = System.Windows.Application;

namespace CoreView.App;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private TemperatureService? _temperatureService;
    private TemperatureTrayIconService? _temperatureTrayService;
    // Add more services here as needed, for example:
    // private CpuUsageService? _cpuUsageService;
    // private CpuTrayIconService? _cpuTrayService;

    private void Application_Startup(object sender, StartupEventArgs e)
    {
        // Initialize services
        _temperatureService = new TemperatureService();
        _temperatureTrayService = new TemperatureTrayIconService(_temperatureService);

        // Don't show any window at startup - we'll use tray icons instead
        ShutdownMode = ShutdownMode.OnExplicitShutdown;
    }

    private void Application_Exit(object sender, ExitEventArgs e)
    {
        // Clean up resources
        _temperatureTrayService?.Dispose();
        _temperatureService?.Dispose();
    }
}

