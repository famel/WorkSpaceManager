using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WorkSpaceManager.Shared.Common;
using WorkSpaceManager.Shared.DTOs;
using WorkSpaceManager.SpaceManagementService.Services;

namespace WorkSpaceManager.SpaceManagementService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BuildingsController : ControllerBase
{
    private readonly IBuildingService _buildingService;

    public BuildingsController(IBuildingService buildingService)
    {
        _buildingService = buildingService;
    }

    /// <summary>
    /// Create a new building
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "admin,facility_manager")]
    [ProducesResponseType(typeof(BuildingResponse), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateBuilding([FromBody] CreateBuildingRequest request)
    {
        var tenantId = GetTenantId();
        var result = await _buildingService.CreateBuildingAsync(tenantId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get building by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BuildingResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetBuilding(Guid id)
    {
        var tenantId = GetTenantId();
        var result = await _buildingService.GetBuildingByIdAsync(id, tenantId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Get all buildings
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<BuildingResponse>), 200)]
    public async Task<IActionResult> GetBuildings([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var tenantId = GetTenantId();
        var result = await _buildingService.GetBuildingsAsync(tenantId, pageNumber, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Update building
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "admin,facility_manager")]
    [ProducesResponseType(typeof(BuildingResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateBuilding(Guid id, [FromBody] UpdateBuildingRequest request)
    {
        var tenantId = GetTenantId();
        var result = await _buildingService.UpdateBuildingAsync(id, tenantId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Delete building
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteBuilding(Guid id)
    {
        var tenantId = GetTenantId();
        var result = await _buildingService.DeleteBuildingAsync(id, tenantId);
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
