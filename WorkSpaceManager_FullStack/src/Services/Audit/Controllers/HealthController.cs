using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkSpaceManager.AuditService.Data;

namespace WorkSpaceManager.AuditService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly AuditDbContext _context;
    private readonly ILogger<HealthController> _logger;

    public HealthController(AuditDbContext context, ILogger<HealthController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetHealth()
    {
        try
        {
            // Check database connectivity
            var canConnect = await _context.Database.CanConnectAsync();
            
            var health = new
            {
                Status = canConnect ? "Healthy" : "Unhealthy",
                Service = "AuditService",
                Timestamp = DateTime.UtcNow,
                Database = canConnect ? "Connected" : "Disconnected",
                Version = "1.0.0"
            };

            return canConnect ? Ok(health) : StatusCode(503, health);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            return StatusCode(503, new
            {
                Status = "Unhealthy",
                Service = "AuditService",
                Timestamp = DateTime.UtcNow,
                Error = ex.Message
            });
        }
    }

    [HttpGet("ready")]
    public async Task<IActionResult> GetReadiness()
    {
        try
        {
            await _context.Database.CanConnectAsync();
            return Ok(new { Status = "Ready" });
        }
        catch
        {
            return StatusCode(503, new { Status = "Not Ready" });
        }
    }

    [HttpGet("live")]
    public IActionResult GetLiveness()
    {
        return Ok(new { Status = "Alive" });
    }
}
