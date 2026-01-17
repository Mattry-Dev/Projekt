using Simulator.Data;
using Simulator.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Simulator.Services;

public class SimulationService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private SimulationConfig _config;
    private SimulationStatus _status;
    private SimulationReport? _lastReport;

    private bool _running;
    private Thread _simulationThread;

    private readonly object _lock = new object();

    private double _totalWaitTime;
    private int _customersServed;
    private int _customersLost;
    private Dictionary<int, double> _checkoutBusyTime;
    private DateTime _simulatedTime;

    private Queue<DateTime> _customerQueue;

    private Dictionary<int, double> _checkoutTimers;

    public SimulationService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
        _status = new SimulationStatus();
        _customerQueue = new Queue<DateTime>();
        _checkoutTimers = new Dictionary<int, double>();
        _checkoutBusyTime = new Dictionary<int, double>();
    }

    public void Start(SimulationConfig config)
    {
        lock (_lock)
        {
            if (_running) return;

            _config = config;
            _running = true;

            _status.IsRunning = true;
            _status.QueueCount = 0;
            _status.Checkouts.Clear();
            for (int i = 0; i < config.OpenCheckouts; i++)
            {
                _status.Checkouts.Add(new CheckoutInfo { Id = i + 1, IsBusy = false, CustomersProcessed = 0 });
                _checkoutBusyTime[i + 1] = 0;
                _checkoutTimers[i + 1] = 0;
            }

            _customerQueue.Clear();
            _totalWaitTime = 0;
            _customersServed = 0;
            _customersLost = 0;
            _simulatedTime = DateTime.Today.AddHours(8);

            _simulationThread = new Thread(SimulationLoop);
            _simulationThread.IsBackground = true;
            _simulationThread.Start();
        }
    }

    public void Stop()
    {
        bool wasRunning = false;
        lock (_lock)
        {
            if (_running)
            {
                _running = false;
                _status.IsRunning = false;
                wasRunning = true;
            }
        }

        if (wasRunning)
        {
            SaveReport();
        }
    }

    public SimulationStatus GetStatus()
    {
        lock (_lock)
        {
            return new SimulationStatus
            {
                IsRunning = _status.IsRunning,
                QueueCount = _status.QueueCount,
                CurrentTime = _simulatedTime.ToString("HH:mm:ss"),
                Checkouts = _status.Checkouts.Select(c => new CheckoutInfo
                {
                    Id = c.Id,
                    IsBusy = c.IsBusy,
                    CustomersProcessed = c.CustomersProcessed
                }).ToList()
            };
        }
    }

    public SimulationReport? GetLastReport()
    {
        lock (_lock)
        {
            return _lastReport;
        }
    }

    private void SimulationLoop()
    {
        var random = new Random();
        double timeStep = 1.0;

        while (true)
        {
            SimulationConfig currentConfig;
            int currentHour;

            lock (_lock)
            {
                if (!_running) break;
                currentConfig = _config;
                currentHour = _simulatedTime.Hour;
            }

            double timeMultiplier = GetIntensityMultiplier(currentHour);
            double adjustedIntensity = currentConfig.CustomerIntensity * timeMultiplier;

            double probability = adjustedIntensity / 3600.0;
            bool customerArrived = random.NextDouble() < probability;

            lock (_lock)
            {
                if (customerArrived)
                {
                    if (_customerQueue.Count > 100)
                    {
                        _customersLost++;
                    }
                    else
                    {
                        _customerQueue.Enqueue(_simulatedTime);
                        _status.QueueCount = _customerQueue.Count;
                    }
                }

                foreach (var checkout in _status.Checkouts)
                {
                    if (checkout.IsBusy)
                    {
                        _checkoutTimers[checkout.Id] -= timeStep;
                        _checkoutBusyTime[checkout.Id] += timeStep;

                        if (_checkoutTimers[checkout.Id] <= 0)
                        {
                            checkout.IsBusy = false;
                            checkout.CustomersProcessed++;
                        }
                    }

                    if (!checkout.IsBusy && _customerQueue.Count > 0)
                    {
                        var arrivalTime = _customerQueue.Dequeue();
                        _status.QueueCount = _customerQueue.Count;

                        _totalWaitTime += (_simulatedTime - arrivalTime).TotalMinutes;
                        _customersServed++;

                        int items = random.Next(1, 50);
                        double scanningSpeed = 2.0;
                        double paymentTime = 30.0;

                        double serviceTime = (items * scanningSpeed) + paymentTime;

                        checkout.IsBusy = true;
                        _checkoutTimers[checkout.Id] = serviceTime;
                    }
                }

                _simulatedTime = _simulatedTime.AddSeconds(timeStep);
            }

            double speed = currentConfig.SimulationSpeed;
            if (speed <= 0) speed = 1.0;

            int sleepTime = (int)(1000 / speed);
            if (sleepTime < 1) sleepTime = 1;

            Thread.Sleep(sleepTime);
        }
    }

    private double GetIntensityMultiplier(int hour)
    {
        return hour switch
        {
            < 8 => 0.0,
            8 => 0.5,
            9 or 10 => 0.8,
            11 or 12 => 1.3,
            13 or 14 => 0.8,
            15 or 16 => 1.0,
            17 or 18 => 1.5,
            19 => 0.7,
            _ => 0.2
        };
    }

    private void SaveReport()
    {
        double totalSimSeconds = (_simulatedTime - DateTime.Today.AddHours(8)).TotalSeconds;
        if (totalSimSeconds <= 0) totalSimSeconds = 1;

        double totalBusySecondsAllowed = _config.OpenCheckouts * totalSimSeconds;
        double totalBusySecondsActual = _checkoutBusyTime.Sum(x => x.Value);

        var report = new SimulationReport
        {
            Date = DateTime.Now,
            AverageWaitTime = _customersServed == 0 ? 0 : _totalWaitTime / _customersServed,
            CheckoutUtilization = totalBusySecondsAllowed == 0 ? 0 : (totalBusySecondsActual / totalBusySecondsAllowed) * 100,
            LostCustomers = _customersLost
        };

        lock (_lock)
        {
            _lastReport = report;
        }

        try
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<SimulatorContext>();
                context.Reports.Add(report);
                context.SaveChanges();
            }
        }
        catch (Exception)
        {
        }
    }
}
