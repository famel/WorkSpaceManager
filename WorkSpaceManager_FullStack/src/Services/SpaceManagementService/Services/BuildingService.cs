using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WorkSpaceManager.SpaceManagementService.Data;
using WorkSpaceManager.Shared.Common;
using WorkSpaceManager.Shared.DTOs;
using WorkSpaceManager.Shared.Models;

namespace WorkSpaceManager.SpaceManagementService.Services;

public class BuildingService : IBuildingService
{
    private readonly SpaceDbContext _context;
    private readonly ILogger<BuildingService> _logger;

    public BuildingService(SpaceDbContext context, ILogger<BuildingService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<BuildingResponse>> CreateBuildingAsync(Guid tenantId, CreateBuildingRequest request)
    {
        try
        {
            var building = new Building
            {
                TenantId = tenantId,
                Name = request.Name,
                Address = request.Address,
                TotalFloors = request.TotalFloors,
                IsActive = true
            };

            _context.Buildings.Add(building);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Building created: {BuildingId} for tenant {TenantId}", building.Id, tenantId);

            return ApiResponse<BuildingResponse>.SuccessResponse(
                MapToResponse(building),
                "Building created successfully"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating building for tenant {TenantId}", tenantId);
            return ApiResponse<BuildingResponse>.ErrorResponse("An error occurred while creating the building");
        }
    }

    public async Task<ApiResponse<BuildingResponse>> GetBuildingByIdAsync(Guid id, Guid tenantId)
    {
        try
        {
            var building = await _context.Buildings
                .Include(b => b.Floors)
                    .ThenInclude(f => f.Desks)
                .Include(b => b.Floors)
                    .ThenInclude(f => f.MeetingRooms)
                .FirstOrDefaultAsync(b => b.Id == id && b.TenantId == tenantId);

            if (building == null)
            {
                return ApiResponse<BuildingResponse>.ErrorResponse("Building not found");
            }

            return ApiResponse<BuildingResponse>.SuccessResponse(MapToResponse(building));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving building {BuildingId}", id);
            return ApiResponse<BuildingResponse>.ErrorResponse("An error occurred while retrieving the building");
        }
    }

    public async Task<ApiResponse<PagedResponse<BuildingResponse>>> GetBuildingsAsync(
        Guid tenantId, int pageNumber = 1, int pageSize = 20)
    {
        try
        {
            var query = _context.Buildings
                .Include(b => b.Floors)
                    .ThenInclude(f => f.Desks)
                .Include(b => b.Floors)
                    .ThenInclude(f => f.MeetingRooms)
                .Where(b => b.TenantId == tenantId);

            var totalCount = await query.CountAsync();
            var buildings = await query
                .OrderBy(b => b.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var responses = buildings.Select(MapToResponse).ToList();
            var pagedResponse = new PagedResponse<BuildingResponse>(
                responses, totalCount, pageNumber, pageSize);

            return ApiResponse<PagedResponse<BuildingResponse>>.SuccessResponse(pagedResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving buildings for tenant {TenantId}", tenantId);
            return ApiResponse<PagedResponse<BuildingResponse>>.ErrorResponse(
                "An error occurred while retrieving buildings");
        }
    }

    public async Task<ApiResponse<BuildingResponse>> UpdateBuildingAsync(
        Guid id, Guid tenantId, UpdateBuildingRequest request)
    {
        try
        {
            var building = await _context.Buildings
                .FirstOrDefaultAsync(b => b.Id == id && b.TenantId == tenantId);

            if (building == null)
            {
                return ApiResponse<BuildingResponse>.ErrorResponse("Building not found");
            }

            if (request.Name != null) building.Name = request.Name;
            if (request.Address != null) building.Address = request.Address;
            if (request.TotalFloors.HasValue) building.TotalFloors = request.TotalFloors.Value;
            if (request.IsActive.HasValue) building.IsActive = request.IsActive.Value;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Building updated: {BuildingId}", id);

            return ApiResponse<BuildingResponse>.SuccessResponse(
                MapToResponse(building),
                "Building updated successfully"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating building {BuildingId}", id);
            return ApiResponse<BuildingResponse>.ErrorResponse("An error occurred while updating the building");
        }
    }

    public async Task<ApiResponse<bool>> DeleteBuildingAsync(Guid id, Guid tenantId)
    {
        try
        {
            var building = await _context.Buildings
                .Include(b => b.Floors)
                .FirstOrDefaultAsync(b => b.Id == id && b.TenantId == tenantId);

            if (building == null)
            {
                return ApiResponse<bool>.ErrorResponse("Building not found");
            }

            if (building.Floors.Any())
            {
                return ApiResponse<bool>.ErrorResponse("Cannot delete building with existing floors");
            }

            building.IsDeleted = true;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Building deleted: {BuildingId}", id);

            return ApiResponse<bool>.SuccessResponse(true, "Building deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting building {BuildingId}", id);
            return ApiResponse<bool>.ErrorResponse("An error occurred while deleting the building");
        }
    }

    private BuildingResponse MapToResponse(Building building)
    {
        return new BuildingResponse
        {
            Id = building.Id,
            Name = building.Name,
            Address = building.Address,
            TotalFloors = building.TotalFloors,
            IsActive = building.IsActive,
            FloorsCount = building.Floors?.Count ?? 0,
            TotalDesks = building.Floors?.SelectMany(f => f.Desks).Count() ?? 0,
            TotalMeetingRooms = building.Floors?.SelectMany(f => f.MeetingRooms).Count() ?? 0,
            CreatedAt = building.CreatedAt
        };
    }
}
