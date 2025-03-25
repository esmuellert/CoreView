using System.Diagnostics;
using LibreHardwareMonitor.Hardware;

namespace CoreView.Core.Temperature;

/// <summary>
/// Implementation of IVisitor for LibreHardwareMonitor to update hardware information
/// </summary>
public class UpdateVisitor : IVisitor
{
    public void VisitComputer(IComputer computer)
    {
        Debug.WriteLine("Visiting computer hardware...");
        computer.Traverse(this);
    }

    public void VisitHardware(IHardware hardware)
    {
        Debug.WriteLine($"Found hardware: {hardware.Name} ({hardware.HardwareType})");
        hardware.Update();
        foreach (var subHardware in hardware.SubHardware)
        {
            Debug.WriteLine($"  Sub-hardware: {subHardware.Name}");
            subHardware.Accept(this);
        }

        foreach (var sensor in hardware.Sensors)
        {
            if (sensor.SensorType == SensorType.Temperature)
            {
                Debug.WriteLine($"  Temperature sensor: {sensor.Name} = {sensor.Value:F1}°C");
            }
        }
    }

    public void VisitSensor(ISensor sensor)
    {
        // No action needed for individual sensors
    }

    public void VisitParameter(IParameter parameter)
    {
        // No action needed for parameters
    }
}