using CoreView.Core.Models;

namespace CoreView.Core.Interfaces;

/// <summary>
/// Interface for the temperature service which provides CPU temperature data
/// </summary>
public interface ITemperatureService
{
    /// <summary>
    /// Gets the current CPU temperature
    /// </summary>
    /// <returns>The current CPU temperature in degrees Celsius</returns>
    float GetCurrentTemperature();

    /// <summary>
    /// Gets temperature history data for a specified time period
    /// </summary>
    /// <param name="minutes">Number of minutes of history to retrieve</param>
    /// <returns>A collection of temperature readings with timestamps</returns>
    IEnumerable<TemperatureReading> GetTemperatureHistory(int minutes = 10);

    /// <summary>
    /// Gets information about top processes by CPU usage
    /// </summary>
    /// <param name="count">Number of processes to retrieve</param>
    /// <returns>A collection of process information</returns>
    IEnumerable<ProcessInfo> GetTopProcessesByCpuUsage(int count = 5);

    /// <summary>
    /// Event that is raised when temperature changes
    /// </summary>
    event EventHandler<TemperatureChangedEventArgs> TemperatureChanged;
}

/// <summary>
/// Event arguments for temperature changed events
/// </summary>
public class TemperatureChangedEventArgs : EventArgs
{
    /// <summary>
    /// The new temperature value
    /// </summary>
    public float Temperature { get; }

    /// <summary>
    /// The timestamp when the temperature was recorded
    /// </summary>
    public DateTime Timestamp { get; }

    public TemperatureChangedEventArgs(float temperature, DateTime timestamp)
    {
        Temperature = temperature;
        Timestamp = timestamp;
    }
}