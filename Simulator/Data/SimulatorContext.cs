using Microsoft.EntityFrameworkCore;
using Simulator.Models;

namespace Simulator.Data;

public class SimulatorContext : DbContext
{
    public SimulatorContext(DbContextOptions<SimulatorContext> options) : base(options)
    {
    }

    public DbSet<SimulationReport> Reports { get; set; }
}
