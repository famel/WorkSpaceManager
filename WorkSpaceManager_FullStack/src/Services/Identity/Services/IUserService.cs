using WorkSpaceManager.Shared.DTOs;
using WorkSpaceManager.Shared.Common;

namespace WorkSpaceManager.IdentityService.Services;

public interface IUserService
{
    Task<ApiResponse<UserResponse>> GetUserByIdAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
    Task<ApiResponse<UserResponse>> GetUserByEmailAsync(Guid tenantId, string email, CancellationToken cancellationToken = default);
    Task<PagedResponse<UserResponse>> SearchUsersAsync(Guid tenantId, UserSearchRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<UserResponse>> CreateUserAsync(Guid tenantId, CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<UserResponse>> UpdateUserAsync(Guid tenantId, Guid userId, UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteUserAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> AssignRoleAsync(Guid tenantId, AssignRoleRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> RevokeRoleAsync(Guid tenantId, RevokeRoleRequest request, CancellationToken cancellationToken = default);
    Task<List<string>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken = default);
}
