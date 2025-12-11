using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkSpaceManager.IdentityService.Services;
using WorkSpaceManager.Shared.DTOs;
using WorkSpaceManager.Shared.Common;

namespace WorkSpaceManager.IdentityService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;
    private readonly ILogger<RolesController> _logger;

    public RolesController(IRoleService roleService, ILogger<RolesController> logger)
    {
        _roleService = roleService;
        _logger = logger;
    }

    private Guid GetTenantId()
    {
        var tenantClaim = User.FindFirst("tenant_id")?.Value;
        return Guid.TryParse(tenantClaim, out var tenantId) ? tenantId : Guid.Empty;
    }

    /// <summary>
    /// Get all roles
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<RoleResponse>), 200)]
    public async Task<IActionResult> GetAllRoles(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var tenantId = GetTenantId();
        var result = await _roleService.GetAllRolesAsync(tenantId, pageNumber, pageSize, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get role by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<RoleResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<RoleResponse>), 404)]
    public async Task<IActionResult> GetRole(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        var result = await _roleService.GetRoleByIdAsync(tenantId, id, cancellationToken);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Create new role
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<RoleResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<RoleResponse>), 400)]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<RoleResponse>.ErrorResponse("Invalid request"));
        }

        var tenantId = GetTenantId();
        var result = await _roleService.CreateRoleAsync(tenantId, request, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Update role
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<RoleResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<RoleResponse>), 400)]
    [ProducesResponseType(typeof(ApiResponse<RoleResponse>), 404)]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateRoleRequest request, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        var result = await _roleService.UpdateRoleAsync(tenantId, id, request, cancellationToken);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Delete role
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 400)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 404)]
    public async Task<IActionResult> DeleteRole(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        var result = await _roleService.DeleteRoleAsync(tenantId, id, cancellationToken);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Add permission to role
    /// </summary>
    [HttpPost("{roleId}/permissions/{permissionId}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 400)]
    public async Task<IActionResult> AddPermission(Guid roleId, Guid permissionId, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        var result = await _roleService.AddPermissionToRoleAsync(tenantId, roleId, permissionId, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Remove permission from role
    /// </summary>
    [HttpDelete("{roleId}/permissions/{permissionId}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 400)]
    public async Task<IActionResult> RemovePermission(Guid roleId, Guid permissionId, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        var result = await _roleService.RemovePermissionFromRoleAsync(tenantId, roleId, permissionId, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
