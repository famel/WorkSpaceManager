using WorkSpaceManager.Shared.Common;
using WorkSpaceManager.Shared.DTOs;

namespace WorkSpaceManager.SpaceManagementService.Services;

public interface IDeskService
{
    Task<ApiResponse<DeskResponse>> CreateDeskAsync(Guid tenantId, CreateDeskRequest request);
    Task<ApiResponse<DeskResponse>> GetDeskByIdAsync(Guid id, Guid tenantId);
    Task<ApiResponse<PagedResponse<DeskResponse>>> SearchDesksAsync(Guid tenantId, DeskSearchRequest request);
    Task<ApiResponse<DeskResponse>> UpdateDeskAsync(Guid id, Guid tenantId, UpdateDeskRequest request);
    Task<ApiResponse<bool>> DeleteDeskAsync(Guid id, Guid tenantId);
}
