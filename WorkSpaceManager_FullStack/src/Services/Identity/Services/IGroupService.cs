using WorkSpaceManager.Shared.DTOs;
using WorkSpaceManager.Shared.Common;

namespace WorkSpaceManager.IdentityService.Services;

public interface IGroupService
{
    Task<ApiResponse<GroupResponse>> GetGroupByIdAsync(Guid tenantId, Guid groupId, CancellationToken cancellationToken = default);
    Task<PagedResponse<GroupResponse>> GetAllGroupsAsync(Guid tenantId, int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<ApiResponse<GroupResponse>> CreateGroupAsync(Guid tenantId, CreateGroupRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<GroupResponse>> UpdateGroupAsync(Guid tenantId, Guid groupId, UpdateGroupRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteGroupAsync(Guid tenantId, Guid groupId, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> AddMemberAsync(Guid tenantId, AddGroupMemberRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> RemoveMemberAsync(Guid tenantId, RemoveGroupMemberRequest request, CancellationToken cancellationToken = default);
    Task<PagedResponse<GroupMemberResponse>> GetGroupMembersAsync(Guid tenantId, Guid groupId, int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default);
}
