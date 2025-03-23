namespace CoreView.Core.Models;

/// <summary>
/// Model representing a temperature reading with a timestamp
/// </summary>
public class TemperatureReading
{
    /// <summary>
    /// The temperature value in degrees Celsius
    /// </summary>
    public float Temperature { get; }
    
    /// <summary>
    /// The timestamp when the temperature was recorded
    /// </summary>
    public DateTime Timestamp { get; }

    public TemperatureReading(float temperature, DateTime timestamp)
    {
        Temperature = temperature;
        Timestamp = timestamp;
    }
}