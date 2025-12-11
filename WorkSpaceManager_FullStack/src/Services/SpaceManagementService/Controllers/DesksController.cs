using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WorkSpaceManager.Shared.DTOs;
using WorkSpaceManager.SpaceManagementService.Services;

namespace WorkSpaceManager.SpaceManagementService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DesksController : ControllerBase
{
    private readonly IDeskService _deskService;

    public DesksController(IDeskService deskService)
    {
        _deskService = deskService;
    }

    [HttpPost]
    [Authorize(Roles = "admin,facility_manager")]
    public async Task<IActionResult> CreateDesk([FromBody] CreateDeskRequest request)
    {
        var tenantId = GetTenantId();
        var result = await _deskService.CreateDeskAsync(tenantId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDesk(Guid id)
    {
        var tenantId = GetTenantId();
        var result = await _deskService.GetDeskByIdAsync(id, tenantId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("search")]
    public async Task<IActionResult> SearchDesks([FromBody] DeskSearchRequest request)
    {
        var tenantId = GetTenantId();
        var result = await _deskService.SearchDesksAsync(tenantId, request);
        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "admin,facility_manager")]
    public async Task<IActionResult> UpdateDesk(Guid id, [FromBody] UpdateDeskRequest request)
    {
        var tenantId = GetTenantId();
        var result = await _deskService.UpdateDeskAsync(id, tenantId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> DeleteDesk(Guid id)
    {
        var tenantId = GetTenantId();
        var result = await _deskService.DeleteDeskAsync(id, tenantId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    private Guid GetTenantId()
    {
        var tenantIdClaim = User.FindFirst("tenant_id")?.Value;
        if (string.IsNullOrEmpty(tenantIdClaim) || !Guid.TryParse(tenantIdClaim, out var tenantId))
        {
            throw new UnauthorizedAccessException("Tenant ID not found in token");
        }
        return tenantId;
    }
}
