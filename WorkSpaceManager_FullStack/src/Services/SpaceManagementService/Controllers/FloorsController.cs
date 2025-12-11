using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WorkSpaceManager.Shared.DTOs;
using WorkSpaceManager.SpaceManagementService.Services;

namespace WorkSpaceManager.SpaceManagementService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FloorsController : ControllerBase
{
    private readonly IFloorService _floorService;

    public FloorsController(IFloorService floorService)
    {
        _floorService = floorService;
    }

    [HttpPost]
    [Authorize(Roles = "admin,facility_manager")]
    public async Task<IActionResult> CreateFloor([FromBody] CreateFloorRequest request)
    {
        var tenantId = GetTenantId();
        var result = await _floorService.CreateFloorAsync(tenantId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetFloor(Guid id)
    {
        var tenantId = GetTenantId();
        var result = await _floorService.GetFloorByIdAsync(id, tenantId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetFloors(
        [FromQuery] Guid? buildingId = null, 
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 20)
    {
        var tenantId = GetTenantId();
        var result = await _floorService.GetFloorsAsync(tenantId, buildingId, pageNumber, pageSize);
        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "admin,facility_manager")]
    public async Task<IActionResult> UpdateFloor(Guid id, [FromBody] UpdateFloorRequest request)
    {
        var tenantId = GetTenantId();
        var result = await _floorService.UpdateFloorAsync(id, tenantId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> DeleteFloor(Guid id)
    {
        var tenantId = GetTenantId();
        var result = await _floorService.DeleteFloorAsync(id, tenantId);
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
