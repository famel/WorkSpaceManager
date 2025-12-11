using WorkSpaceManager.Shared.DTOs;
using WorkSpaceManager.Shared.Common;

namespace WorkSpaceManager.IdentityService.Services;

public interface IRoleService
{
    Task<ApiResponse<RoleResponse>> GetRoleByIdAsync(Guid tenantId, Guid roleId, CancellationToken cancellationToken = default);
    Task<PagedResponse<RoleResponse>> GetAllRolesAsync(Guid tenantId, int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<ApiResponse<RoleResponse>> CreateRoleAsync(Guid tenantId, CreateRoleRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<RoleResponse>> UpdateRoleAsync(Guid tenantId, Guid roleId, UpdateRoleRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteRoleAsync(Guid tenantId, Guid roleId, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> AddPermissionToRoleAsync(Guid tenantId, Guid roleId, Guid permissionId, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> RemovePermissionFromRoleAsync(Guid tenantId, Guid roleId, Guid permissionId, CancellationToken cancellationToken = default);
}
