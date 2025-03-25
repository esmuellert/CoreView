namespace CoreView.Core.Models;

/// <summary>
/// Model representing information about a system process
/// </summary>
public class ProcessInfo
{
    /// <summary>
    /// The process ID
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// The name of the process
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// CPU usage as a percentage (0-100)
    /// </summary>
    public float CpuUsage { get; }

    /// <summary>
    /// Memory usage in MB
    /// </summary>
    public float MemoryUsageMB { get; }

    public ProcessInfo(int id, string name, float cpuUsage, float memoryUsageMB)
    {
        Id = id;
        Name = name;
        CpuUsage = cpuUsage;
        MemoryUsageMB = memoryUsageMB;
    }
}