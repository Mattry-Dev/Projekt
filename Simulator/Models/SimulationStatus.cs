namespace Simulator.Models;

public class SimulationStatus
{
    public List<CheckoutInfo> Checkouts { get; set; } = new();
    public int QueueCount { get; set; }
    public bool IsRunning { get; set; }
    public string CurrentTime { get; set; } = string.Empty;
}

public class CheckoutInfo
{
    public int Id { get; set; }
    public bool IsBusy { get; set; }
    public int CustomersProcessed { get; set; }
}
