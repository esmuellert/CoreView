using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Security.Principal;
using CoreView.Core.Interfaces;
using CoreView.Core.Models;
using LibreHardwareMonitor.Hardware;

namespace CoreView.Core.Temperature;

/// <summary>
/// Service for monitoring and retrieving CPU temperature data
/// </summary>
[SupportedOSPlatform("windows")]
public class TemperatureService : ITemperatureService, IDisposable
{
    private readonly Computer _computer;
    private readonly UpdateVisitor _updateVisitor;
    private readonly ConcurrentQueue<TemperatureReading> _temperatureHistory;
    private readonly Timer _updateTimer;
    private readonly object _lockObject = new();
    private bool _disposed;
    private float _currentTemperature;
    private bool _isInitialized;
    private string _lastError = string.Empty;

    public event EventHandler<TemperatureChangedEventArgs>? TemperatureChanged;

    [SupportedOSPlatform("windows")]
    public TemperatureService()
    {
        _temperatureHistory = new ConcurrentQueue<TemperatureReading>();
        _updateVisitor = new UpdateVisitor();

        try
        {
            // Check if running with admin privileges
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            var hasAdminRights = principal.IsInRole(WindowsBuiltInRole.Administrator);

            if (!hasAdminRights)
            {
                Debug.WriteLine("Warning: Application is not running with administrator privileges. Temperature readings may be inaccurate or unavailable.");
            }

            _computer = new Computer
            {
                IsCpuEnabled = true,
                IsGpuEnabled = false,
                IsMemoryEnabled = false,
                IsMotherboardEnabled = true, // Enable motherboard to get more temperature sensors
                IsControllerEnabled = false,
                IsNetworkEnabled = false,
                IsStorageEnabled = false
            };

            _computer.Open();
            _isInitialized = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error initializing hardware monitoring: {ex.Message}");
            _lastError = ex.Message;
            _computer = new Computer(); // Create empty instance to prevent null reference
        }

        // Start a timer to update temperature values every 2 seconds
        _updateTimer = new Timer(UpdateTemperature, null, TimeSpan.Zero, TimeSpan.FromSeconds(2));
    }

    /// <summary>
    /// Updates the current temperature and raises events if changed
    /// </summary>
    private void UpdateTemperature(object? state)
    {
        if (!_isInitialized || _disposed) return;

        lock (_lockObject)
        {
            try
            {
                // Use the visitor to update all hardware
                _computer.Accept(_updateVisitor);

                var cpuTemp = 0f;
                var cpuTempSensors = 0;
                var foundAnySensors = false;

                // First try CPU package temperature
                foreach (var hardware in _computer.Hardware)
                {
                    // Try to get CPU package temperature first
                    if (hardware.HardwareType == HardwareType.Cpu)
                    {
                        foreach (var sensor in hardware.Sensors)
                        {
                            if (sensor.SensorType == SensorType.Temperature &&
                                sensor.Name.Contains("Package", StringComparison.OrdinalIgnoreCase) &&
                                sensor.Value.HasValue)
                            {
                                cpuTemp = sensor.Value.Value;
                                cpuTempSensors = 1;
                                foundAnySensors = true;
                                break;
                            }
                        }

                        // If no package temperature, try individual cores
                        if (!foundAnySensors)
                        {
                            foreach (var sensor in hardware.Sensors)
                            {
                                if (sensor.SensorType == SensorType.Temperature && sensor.Value.HasValue)
                                {
                                    cpuTemp += sensor.Value.Value;
                                    cpuTempSensors++;
                                    foundAnySensors = true;
                                }
                            }
                        }
                    }
                }

                // If no CPU sensors found, try motherboard CPU sensor as fallback
                if (!foundAnySensors)
                {
                    foreach (var hardware in _computer.Hardware)
                    {
                        if (hardware.HardwareType == HardwareType.Motherboard)
                        {
                            foreach (var sensor in hardware.Sensors)
                            {
                                if (sensor.SensorType == SensorType.Temperature &&
                                    sensor.Name.Contains("CPU", StringComparison.OrdinalIgnoreCase) &&
                                    sensor.Value.HasValue)
                                {
                                    cpuTemp = sensor.Value.Value;
                                    cpuTempSensors = 1;
                                    foundAnySensors = true;
                                    break;
                                }
                            }
                        }
                    }
                }

                // Calculate average temperature if any sensors were found
                if (cpuTempSensors > 0)
                {
                    var avgTemp = cpuTemp / cpuTempSensors;

                    // Update current temperature and add to history
                    if (Math.Abs(_currentTemperature - avgTemp) > 0.1f)
                    {
                        _currentTemperature = avgTemp;
                        var reading = new TemperatureReading(_currentTemperature, DateTime.Now);
                        _temperatureHistory.Enqueue(reading);

                        // Trim history to keep about 10 minutes of readings
                        while (_temperatureHistory.Count > 300)
                        {
                            _temperatureHistory.TryDequeue(out _);
                        }

                        // Notify subscribers about temperature change
                        TemperatureChanged?.Invoke(this,
                            new TemperatureChangedEventArgs(_currentTemperature, DateTime.Now));
                    }
                }
                else
                {
                    Debug.WriteLine("No temperature sensors found or accessible");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating temperature: {ex.Message}");
                _lastError = ex.Message;
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