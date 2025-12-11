using WorkSpaceManager.Shared.Common;
using WorkSpaceManager.Shared.DTOs;

namespace WorkSpaceManager.SpaceManagementService.Services;

public interface IBuildingService
{
    Task<ApiResponse<BuildingResponse>> CreateBuildingAsync(Guid tenantId, CreateBuildingRequest request);
    Task<ApiResponse<BuildingResponse>> GetBuildingByIdAsync(Guid id, Guid tenantId);
    Task<ApiResponse<PagedResponse<BuildingResponse>>> GetBuildingsAsync(Guid tenantId, int pageNumber = 1, int pageSize = 20);
    Task<ApiResponse<BuildingResponse>> UpdateBuildingAsync(Guid id, Guid tenantId, UpdateBuildingRequest request);
    Task<ApiResponse<bool>> DeleteBuildingAsync(Guid id, Guid tenantId);
}
