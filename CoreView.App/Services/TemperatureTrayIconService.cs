using CoreView.App.Icons;
using CoreView.App.ViewModels;
using CoreView.App.Views;
using CoreView.Core.Interfaces;

namespace CoreView.App.Services;

/// <summary>
/// Temperature-specific implementation of tray icon service
/// </summary>
public class TemperatureTrayIconService : BaseTrayIconService
{
    private readonly ITemperatureService _temperatureService;

    public TemperatureTrayIconService(ITemperatureService temperatureService)
        : base("Temperature", TemperatureIconGenerator.CreateTrayIcon("--°"))
    {
        _temperatureService = temperatureService;
        _temperatureService.TemperatureChanged += OnTemperatureChanged;
    }

    private void OnTemperatureChanged(object? sender, TemperatureChangedEventArgs e)
    {
        var iconText = $"{e.Temperature:F0}";
        var tooltipText = $"CPU Temperature: {e.Temperature:F1}°C";
        UpdateTrayIcon(TemperatureIconGenerator.CreateTrayIcon(iconText), tooltipText);
    }

    protected override BaseMetricWindow CreateMetricWindow()
    {
        return new TemperatureWindow
        {
            DataContext = new TemperatureViewModel(_temperatureService)
        };
    }

    public override void Dispose()
    {
        _temperatureService.TemperatureChanged -= OnTemperatureChanged;
        base.Dispose();
    }
}