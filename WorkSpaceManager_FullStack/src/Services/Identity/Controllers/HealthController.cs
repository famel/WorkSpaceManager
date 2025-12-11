using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkSpaceManager.IdentityService.Data;

namespace WorkSpaceManager.IdentityService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly IdentityDbContext _context;
    private readonly ILogger<HealthController> _logger;

    public HealthController(IdentityDbContext context, ILogger<HealthController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetHealth()
    {
        try
        {
            var canConnect = await _context.Database.CanConnectAsync();
            
            var health = new
            {
                Status = canConnect ? "Healthy" : "Unhealthy",
                Service = "IdentityService",
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
                Service = "IdentityService",
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
