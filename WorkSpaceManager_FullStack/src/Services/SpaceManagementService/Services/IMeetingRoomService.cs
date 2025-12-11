using WorkSpaceManager.Shared.Common;
using WorkSpaceManager.Shared.DTOs;

namespace WorkSpaceManager.SpaceManagementService.Services;

public interface IMeetingRoomService
{
    Task<ApiResponse<MeetingRoomResponse>> CreateMeetingRoomAsync(Guid tenantId, CreateMeetingRoomRequest request);
    Task<ApiResponse<MeetingRoomResponse>> GetMeetingRoomByIdAsync(Guid id, Guid tenantId);
    Task<ApiResponse<PagedResponse<MeetingRoomResponse>>> SearchMeetingRoomsAsync(Guid tenantId, MeetingRoomSearchRequest request);
    Task<ApiResponse<MeetingRoomResponse>> UpdateMeetingRoomAsync(Guid id, Guid tenantId, UpdateMeetingRoomRequest request);
    Task<ApiResponse<bool>> DeleteMeetingRoomAsync(Guid id, Guid tenantId);
}
