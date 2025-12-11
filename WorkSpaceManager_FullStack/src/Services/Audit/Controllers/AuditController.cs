using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WorkSpaceManager.AuditService.Services;
using WorkSpaceManager.Shared.DTOs;
using WorkSpaceManager.Shared.Common;

namespace WorkSpaceManager.AuditService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AuditController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<AuditController> _logger;

    public AuditController(IAuditLogService auditLogService, ILogger<AuditController> logger)
    {
        _auditLogService = auditLogService;
        _logger = logger;
    }

    private Guid GetTenantId()
    {
        var tenantClaim = User.FindFirst("tenant_id")?.Value;
        return Guid.TryParse(tenantClaim, out var tenantId) ? tenantId : Guid.Empty;
    }

    private Guid? GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                       ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    private string? GetUserName()
    {
        return User.FindFirst(ClaimTypes.Name)?.Value 
            ?? User.FindFirst("name")?.Value
            ?? User.FindFirst(ClaimTypes.Email)?.Value;
    }

    /// <summary>
    /// Create a new audit log entry
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<AuditLogResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<AuditLogResponse>), 400)]
    public async Task<IActionResult> CreateAuditLog([FromBody] CreateAuditLogRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<AuditLogResponse>.ErrorResponse("Invalid request"));
        }

        var tenantId = GetTenantId();
        
        // Auto-fill user info if not provided
        request.UserId ??= GetUserId();
        request.UserName ??= GetUserName();
        request.IpAddress ??= HttpContext.Connection.RemoteIpAddress?.ToString();
        request.UserAgent ??= Request.Headers.UserAgent.ToString();

        var result = await _auditLogService.CreateAuditLogAsync(tenantId, request, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get audit log by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<AuditLogResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<AuditLogResponse>), 404)]
    public async Task<IActionResult> GetAuditLog(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        var result = await _auditLogService.GetAuditLogByIdAsync(tenantId, id, cancellationToken);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Search audit logs with filtering and pagination
    /// </summary>
    [HttpGet("logs")]
    [ProducesResponseType(typeof(AuditLogSearchResponse), 200)]
    public async Task<IActionResult> SearchAuditLogs(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] Guid? userId,
        [FromQuery] string? userName,
        [FromQuery] string? action,
        [FromQuery] string? resource,
        [FromQuery] string? resourceId,
        [FromQuery] bool? success,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var tenantId = GetTenantId();
        var request = new AuditLogSearchRequest
        {
            StartDate = startDate,
            EndDate = endDate,
            UserId = userId,
            UserName = userName,
            Action = action,
            Resource = resource,
            ResourceId = resourceId,
            Success = success,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _auditLogService.SearchAuditLogsAsync(tenantId, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Search audit logs with POST request (for complex queries)
    /// </summary>
    [HttpPost("search")]
    [ProducesResponseType(typeof(AuditLogSearchResponse), 200)]
    public async Task<IActionResult> SearchAuditLogsPost([FromBody] AuditLogSearchRequest request, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        var result = await _auditLogService.SearchAuditLogsAsync(tenantId, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get current user's audit logs
    /// </summary>
    [HttpGet("my-logs")]
    [ProducesResponseType(typeof(AuditLogSearchResponse), 200)]
    public async Task<IActionResult> GetMyAuditLogs(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] string? action,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        var request = new AuditLogSearchRequest
        {
            StartDate = startDate,
            EndDate = endDate,
            UserId = userId,
            Action = action,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _auditLogService.SearchAuditLogsAsync(tenantId, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get audit statistics
    /// </summary>
    [HttpGet("statistics")]
    [Authorize(Roles = "Admin,FacilityManager")]
    [ProducesResponseType(typeof(ApiResponse<AuditStatisticsResponse>), 200)]
    public async Task<IActionResult> GetStatistics(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        var result = await _auditLogService.GetStatisticsAsync(tenantId, startDate, endDate, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Delete old audit logs (Admin only)
    /// </summary>
    [HttpDelete("cleanup")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 400)]
    public async Task<IActionResult> CleanupAuditLogs([FromQuery] DateTime beforeDate, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        
        // Prevent deleting recent logs (minimum 30 days retention)
        var minDate = DateTime.UtcNow.AddDays(-30);
        if (beforeDate > minDate)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse("Cannot delete logs newer than 30 days"));
        }

        var result = await _auditLogService.DeleteAuditLogsAsync(tenantId, beforeDate, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
