using WorkSpaceManager.Shared.Common;
using WorkSpaceManager.Shared.DTOs;

namespace WorkSpaceManager.SpaceManagementService.Services;

public interface IFloorService
{
    Task<ApiResponse<FloorResponse>> CreateFloorAsync(Guid tenantId, CreateFloorRequest request);
    Task<ApiResponse<FloorResponse>> GetFloorByIdAsync(Guid id, Guid tenantId);
    Task<ApiResponse<PagedResponse<FloorResponse>>> GetFloorsAsync(Guid tenantId, Guid? buildingId = null, int pageNumber = 1, int pageSize = 20);
    Task<ApiResponse<FloorResponse>> UpdateFloorAsync(Guid id, Guid tenantId, UpdateFloorRequest request);
    Task<ApiResponse<bool>> DeleteFloorAsync(Guid id, Guid tenantId);
}
