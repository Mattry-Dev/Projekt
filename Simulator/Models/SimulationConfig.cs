namespace Simulator.Models;

public class SimulationConfig
{
    public int OpenCheckouts { get; set; }
    public int CustomerIntensity { get; set; } // Customers per hour
    public double SimulationSpeed { get; set; } // Speed multiplier
}
