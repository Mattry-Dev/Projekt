namespace Simulator.Models;

public class SimulationReport
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public double AverageWaitTime { get; set; } // Minutes
    public double CheckoutUtilization { get; set; } // Percentage 0-100
    public int LostCustomers { get; set; }
}
