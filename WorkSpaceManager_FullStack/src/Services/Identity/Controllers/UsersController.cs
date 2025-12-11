using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WorkSpaceManager.IdentityService.Services;
using WorkSpaceManager.Shared.DTOs;
using WorkSpaceManager.Shared.Common;

namespace WorkSpaceManager.IdentityService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    private Guid GetTenantId()
    {
        var tenantClaim = User.FindFirst("tenant_id")?.Value;
        return Guid.TryParse(tenantClaim, out var tenantId) ? tenantId : Guid.Empty;
    }

    private Guid? GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<UserResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<UserResponse>), 404)]
    public async Task<IActionResult> GetUser(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        var result = await _userService.GetUserByIdAsync(tenantId, id, cancellationToken);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Search users
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<UserResponse>), 200)]
    public async Task<IActionResult> SearchUsers(
        [FromQuery] string? searchTerm,
        [FromQuery] string? department,
        [FromQuery] bool? isActive,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var tenantId = GetTenantId();
        var request = new UserSearchRequest
        {
            SearchTerm = searchTerm,
            Department = department,
            IsActive = isActive,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _userService.SearchUsersAsync(tenantId, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Create new user
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<UserResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<UserResponse>), 400)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<UserResponse>.ErrorResponse("Invalid request"));
        }

        var tenantId = GetTenantId();
        var result = await _userService.CreateUserAsync(tenantId, request, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Update user
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<UserResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<UserResponse>), 400)]
    [ProducesResponseType(typeof(ApiResponse<UserResponse>), 404)]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        var currentUserId = GetUserId();

        // Users can only update their own profile unless they're Admin
        if (currentUserId != id && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var result = await _userService.UpdateUserAsync(tenantId, id, request, cancellationToken);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Delete user (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 404)]
    public async Task<IActionResult> DeleteUser(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        var result = await _userService.DeleteUserAsync(tenantId, id, cancellationToken);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Assign role to user
    /// </summary>
    [HttpPost("{userId}/roles/{roleId}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 400)]
    public async Task<IActionResult> AssignRole(Guid userId, Guid roleId, [FromQuery] DateTime? expiresAt, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        var request = new AssignRoleRequest
        {
            UserId = userId,
            RoleId = roleId,
            ExpiresAt = expiresAt
        };

        var result = await _userService.AssignRoleAsync(tenantId, request, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Revoke role from user
    /// </summary>
    [HttpDelete("{userId}/roles/{roleId}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 400)]
    public async Task<IActionResult> RevokeRole(Guid userId, Guid roleId, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        var request = new RevokeRoleRequest
        {
            UserId = userId,
            RoleId = roleId
        };

        var result = await _userService.RevokeRoleAsync(tenantId, request, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
