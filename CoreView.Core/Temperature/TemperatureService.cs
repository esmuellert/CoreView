using System.Collections.Concurrent;
using System.Diagnostics;
using CoreView.Core.Interfaces;
using CoreView.Core.Models;
using LibreHardwareMonitor.Hardware;

namespace CoreView.Core.Temperature;

/// <summary>
/// Service for monitoring and retrieving CPU temperature data
/// </summary>
public class TemperatureService : ITemperatureService, IDisposable
{
    private readonly Computer _computer;
    private readonly UpdateVisitor _updateVisitor;
    private readonly ConcurrentQueue<TemperatureReading> _temperatureHistory;
    private readonly Timer _updateTimer;
    private readonly object _lockObject = new();
    private bool _disposed;
    private float _currentTemperature;

    public event EventHandler<TemperatureChangedEventArgs>? TemperatureChanged;

    public TemperatureService()
    {
        _temperatureHistory = new ConcurrentQueue<TemperatureReading>();
        _updateVisitor = new UpdateVisitor();
        _computer = new Computer
        {
            IsCpuEnabled = true,
            IsGpuEnabled = false,
            IsMemoryEnabled = false,
            IsMotherboardEnabled = false,
            IsControllerEnabled = false,
            IsNetworkEnabled = false,
            IsStorageEnabled = false
        };
        
        _computer.Open();
        
        // Start a timer to update temperature values every 2 seconds
        _updateTimer = new Timer(UpdateTemperature, null, TimeSpan.Zero, TimeSpan.FromSeconds(2));
    }

    /// <summary>
    /// Updates the current temperature and raises events if changed
    /// </summary>
    private void UpdateTemperature(object? state)
    {
        lock (_lockObject)
        {
            if (_disposed) return;

            // Use the visitor to update all hardware
            _computer.Accept(_updateVisitor);

            var cpuTemp = 0f;
            var cpuTempSensors = 0;

            // Collect temperature values from all CPU cores
            foreach (var hardware in _computer.Hardware)
            {
                foreach (var sensor in hardware.Sensors)
                {
                    if (sensor.SensorType == SensorType.Temperature && sensor.Value.HasValue)
                    {
                        cpuTemp += sensor.Value.Value;
                        cpuTempSensors++;
                    }
                }
            }

            // Calculate average temperature
            if (cpuTempSensors > 0)
            {
                var avgTemp = cpuTemp / cpuTempSensors;
                
                // Update current temperature and add to history
                if (Math.Abs(_currentTemperature - avgTemp) > 0.1f)
                {
                    _currentTemperature = avgTemp;
                    var reading = new TemperatureReading(_currentTemperature, DateTime.Now);
                    _temperatureHistory.Enqueue(reading);
                    
                    // Trim history to keep about 10 minutes of readings (300 readings at 2-second intervals)
                    while (_temperatureHistory.Count > 300)
                    {
                        _temperatureHistory.TryDequeue(out _);
                    }
                    
                    // Notify subscribers about temperature change
                    TemperatureChanged?.Invoke(this, 
                        new TemperatureChangedEventArgs(_currentTemperature, DateTime.Now));
                }
            }
        }
    }

    /// <summary>
    /// Gets the current CPU temperature
    /// </summary>
    public float GetCurrentTemperature()
    {
        return _currentTemperature;
    }

    /// <summary>
    /// Gets temperature history for the specified time period
    /// </summary>
    public IEnumerable<TemperatureReading> GetTemperatureHistory(int minutes = 10)
    {
        var cutoffTime = DateTime.Now.AddMinutes(-minutes);
        return _temperatureHistory
            .Where(r => r.Timestamp >= cutoffTime)
            .OrderBy(r => r.Timestamp)
            .ToList();
    }

    /// <summary>
    /// Gets information about the top processes by CPU usage
    /// </summary>
    public IEnumerable<ProcessInfo> GetTopProcessesByCpuUsage(int count = 5)
    {
        var processes = Process.GetProcesses();
        var result = new List<ProcessInfo>();
        
        foreach (var process in processes)
        {
            try
            {
                var cpuUsage = 0f; // This is a placeholder; proper CPU usage would require sampling 
                var memoryMB = process.WorkingSet64 / 1024f / 1024f;
                
                result.Add(new ProcessInfo(
                    process.Id,
                    process.ProcessName,
                    cpuUsage,
                    memoryMB
                ));
            }
            catch
            {
                // Skip processes that can't be accessed
            }
        }
        
        // For accurate CPU measurement, we would need to implement a more complex solution
        // with process time sampling, but for demo purposes, we'll just sort by memory usage
        return result
            .OrderByDescending(p => p.MemoryUsageMB)
            .Take(count)
            .ToList();
    }

    public void Dispose()
    {
        if (_disposed) return;
        
        lock (_lockObject)
        {
            if (_disposed) return;
            _disposed = true;
            
            _updateTimer?.Dispose();
            _computer?.Close();
        }
        
        GC.SuppressFinalize(this);
    }

    ~TemperatureService()
    {
        Dispose();
    }
}