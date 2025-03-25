using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CoreView.Core.Interfaces;
using CoreView.Core.Models;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace CoreView.App.ViewModels;

/// <summary>
/// ViewModel for CPU temperature monitoring
/// </summary>
public partial class TemperatureViewModel : BaseMetricViewModel
{
    private readonly ITemperatureService _temperatureService;
    private readonly System.Threading.Timer _updateTimer;

    [ObservableProperty]
    private float _currentTemperature;

    [ObservableProperty]
    private ObservableCollection<ProcessInfo> _topProcesses = new();

    [ObservableProperty]
    private ObservableCollection<ISeries> _temperatureSeries = new();

    [ObservableProperty]
    private ObservableCollection<Axis> _xAxes = new();

    [ObservableProperty]
    private ObservableCollection<Axis> _yAxes = new();

    public TemperatureViewModel(ITemperatureService temperatureService)
    {
        _temperatureService = temperatureService;

        // Set up chart series
        TemperatureSeries = new ObservableCollection<ISeries>
        {
            new LineSeries<ObservablePoint>
            {
                Name = "CPU Temperature",
                Values = new ObservableCollection<ObservablePoint>(),
                Stroke = new SolidColorPaint(SKColors.OrangeRed, 2),
                Fill = new SolidColorPaint(SKColors.Orange.WithAlpha(90)),
                GeometrySize = 0,
                LineSmoothness = 0.5
            }
        };

        // Configure axis
        XAxes = new ObservableCollection<Axis>
        {
            new Axis
            {
                LabelsRotation = 15,
                Labeler = value => DateTime.FromOADate(value).ToString("HH:mm:ss"),
                UnitWidth = TimeSpan.FromSeconds(1).TotalDays
            }
        };

        YAxes = new ObservableCollection<Axis>
        {
            new Axis
            {
                Name = "Temperature (°C)",
                NamePaint = new SolidColorPaint(SKColors.DarkGray),
                TextSize = 12,
                MinLimit = 0,
                MaxLimit = 100
            }
        };

        // Subscribe to temperature changes
        _temperatureService.TemperatureChanged += OnTemperatureChanged;

        // Start timer to update process list and chart data
        _updateTimer = new System.Threading.Timer(UpdateData, null, TimeSpan.Zero, TimeSpan.FromSeconds(3));

        // Initial update
        UpdateTemperature(_temperatureService.GetCurrentTemperature());
        UpdateProcessList();
    }

    /// <summary>
    /// Event handler for temperature changes
    /// </summary>
    private void OnTemperatureChanged(object? sender, TemperatureChangedEventArgs e)
    {
        UpdateTemperature(e.Temperature);
    }

    /// <summary>
    /// Updates the current temperature display
    /// </summary>
    private void UpdateTemperature(float temperature)
    {
        CurrentTemperature = temperature;
        UpdateLastUpdated();
    }

    /// <summary>
    /// Updates the process list and chart
    /// </summary>
    private void UpdateData(object? state)
    {
        if (_disposed) return;

        UpdateProcessList();
        UpdateTemperatureChart();
    }

    /// <summary>
    /// Updates the list of top processes by CPU usage
    /// </summary>
    private void UpdateProcessList()
    {
        try
        {
            var processes = _temperatureService.GetTopProcessesByCpuUsage();

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                TopProcesses.Clear();
                foreach (var process in processes)
                {
                    TopProcesses.Add(process);
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating process list: {ex.Message}");
        }
    }

    /// <summary>
    /// Updates the temperature chart with historical data
    /// </summary>
    private void UpdateTemperatureChart()
    {
        try
        {
            var historyData = _temperatureService.GetTemperatureHistory().ToList();

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                if (TemperatureSeries[0] is LineSeries<ObservablePoint> series)
                {
                    // Convert temperature data to ObservablePoint collection
                    var points = new ObservableCollection<ObservablePoint>();
                    foreach (var reading in historyData)
                    {
                        points.Add(new ObservablePoint(reading.Timestamp.ToOADate(), reading.Temperature));
                    }

                    // Update the values
                    series.Values = points;
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating temperature chart: {ex.Message}");
        }
    }

    protected override void RefreshData()
    {
        UpdateProcessList();
        UpdateTemperatureChart();
    }

    public override void Dispose()
    {
        if (_disposed) return;

        base.Dispose();
        _temperatureService.TemperatureChanged -= OnTemperatureChanged;
        _updateTimer?.Dispose();
    }
}