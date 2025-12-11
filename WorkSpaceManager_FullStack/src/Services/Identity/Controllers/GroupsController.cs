using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkSpaceManager.IdentityService.Services;
using WorkSpaceManager.Shared.DTOs;
using WorkSpaceManager.Shared.Common;

namespace WorkSpaceManager.IdentityService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GroupsController : ControllerBase
{
    private readonly IGroupService _groupService;
    private readonly ILogger<GroupsController> _logger;

    public GroupsController(IGroupService groupService, ILogger<GroupsController> logger)
    {
        _groupService = groupService;
        _logger = logger;
    }

    private Guid GetTenantId()
    {
        var tenantClaim = User.FindFirst("tenant_id")?.Value;
        return Guid.TryParse(tenantClaim, out var tenantId) ? tenantId : Guid.Empty;
    }

    /// <summary>
    /// Get all groups
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<GroupResponse>), 200)]
    public async Task<IActionResult> GetAllGroups(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var tenantId = GetTenantId();
        var result = await _groupService.GetAllGroupsAsync(tenantId, pageNumber, pageSize, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get group by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<GroupResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<GroupResponse>), 404)]
    public async Task<IActionResult> GetGroup(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        var result = await _groupService.GetGroupByIdAsync(tenantId, id, cancellationToken);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Create new group
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<GroupResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<GroupResponse>), 400)]
    public async Task<IActionResult> CreateGroup([FromBody] CreateGroupRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<GroupResponse>.ErrorResponse("Invalid request"));
        }

        var tenantId = GetTenantId();
        var result = await _groupService.CreateGroupAsync(tenantId, request, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Update group
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<GroupResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<GroupResponse>), 400)]
    [ProducesResponseType(typeof(ApiResponse<GroupResponse>), 404)]
    public async Task<IActionResult> UpdateGroup(Guid id, [FromBody] UpdateGroupRequest request, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        var result = await _groupService.UpdateGroupAsync(tenantId, id, request, cancellationToken);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Delete group
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 400)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 404)]
    public async Task<IActionResult> DeleteGroup(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        var result = await _groupService.DeleteGroupAsync(tenantId, id, cancellationToken);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Get group members
    /// </summary>
    [HttpGet("{id}/members")]
    [ProducesResponseType(typeof(PagedResponse<GroupMemberResponse>), 200)]
    public async Task<IActionResult> GetGroupMembers(
        Guid id,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var tenantId = GetTenantId();
        var result = await _groupService.GetGroupMembersAsync(tenantId, id, pageNumber, pageSize, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Add member to group
    /// </summary>
    [HttpPost("{groupId}/members/{userId}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 400)]
    public async Task<IActionResult> AddMember(Guid groupId, Guid userId, [FromQuery] DateTime? expiresAt, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        var request = new AddGroupMemberRequest
        {
            GroupId = groupId,
            UserId = userId,
            ExpiresAt = expiresAt
        };

        var result = await _groupService.AddMemberAsync(tenantId, request, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Remove member from group
    /// </summary>
    [HttpDelete("{groupId}/members/{userId}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 400)]
    public async Task<IActionResult> RemoveMember(Guid groupId, Guid userId, CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        var request = new RemoveGroupMemberRequest
        {
            GroupId = groupId,
            UserId = userId
        };

        var result = await _groupService.RemoveMemberAsync(tenantId, request, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
