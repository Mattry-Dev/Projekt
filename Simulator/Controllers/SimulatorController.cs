using Microsoft.AspNetCore.Mvc;
using Simulator.Models;
using Simulator.Services;
using Simulator.Data;
using Microsoft.EntityFrameworkCore;

namespace Simulator.Controllers;

[ApiController]
public class SimulatorController : ControllerBase
{
    private readonly SimulationService _simulationService;
    private readonly SimulatorContext _context;

    public SimulatorController(SimulationService simulationService, SimulatorContext context)
    {
        _simulationService = simulationService;
        _context = context;
    }

    [HttpPost("api/start")]
    public IActionResult Start([FromBody] SimulationConfig config)
    {
        _simulationService.Start(config);
        return Ok(new { message = "Simulation started", config });
    }

    [HttpPost("api/stop")]
    public IActionResult Stop()
    {
        _simulationService.Stop();
        var report = _simulationService.GetLastReport();
        return Ok(new { message = "Simulation stopped", report });
    }

    [HttpGet("api/status")]
    public ActionResult<SimulationStatus> GetStatus()
    {
        return Ok(_simulationService.GetStatus());
    }

    [HttpGet("api/report")]
    public ActionResult<IEnumerable<SimulationReport>> GetReports()
    {
        return Ok(_context.Reports.OrderByDescending(r => r.Date).ToList());
    }
}
