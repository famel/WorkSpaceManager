using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WorkSpaceManager.Shared.DTOs;
using WorkSpaceManager.SpaceManagementService.Services;

namespace WorkSpaceManager.SpaceManagementService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MeetingRoomsController : ControllerBase
{
    private readonly IMeetingRoomService _meetingRoomService;

    public MeetingRoomsController(IMeetingRoomService meetingRoomService)
    {
        _meetingRoomService = meetingRoomService;
    }

    [HttpPost]
    [Authorize(Roles = "admin,facility_manager")]
    public async Task<IActionResult> CreateMeetingRoom([FromBody] CreateMeetingRoomRequest request)
    {
        var tenantId = GetTenantId();
        var result = await _meetingRoomService.CreateMeetingRoomAsync(tenantId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetMeetingRoom(Guid id)
    {
        var tenantId = GetTenantId();
        var result = await _meetingRoomService.GetMeetingRoomByIdAsync(id, tenantId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("search")]
    public async Task<IActionResult> SearchMeetingRooms([FromBody] MeetingRoomSearchRequest request)
    {
        var tenantId = GetTenantId();
        var result = await _meetingRoomService.SearchMeetingRoomsAsync(tenantId, request);
        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "admin,facility_manager")]
    public async Task<IActionResult> UpdateMeetingRoom(Guid id, [FromBody] UpdateMeetingRoomRequest request)
    {
        var tenantId = GetTenantId();
        var result = await _meetingRoomService.UpdateMeetingRoomAsync(id, tenantId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> DeleteMeetingRoom(Guid id)
    {
        var tenantId = GetTenantId();
        var result = await _meetingRoomService.DeleteMeetingRoomAsync(id, tenantId);
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
