using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WorkSpaceManager.SpaceManagementService.Data;
using WorkSpaceManager.Shared.Common;
using WorkSpaceManager.Shared.DTOs;
using WorkSpaceManager.Shared.Models;

namespace WorkSpaceManager.SpaceManagementService.Services;

public class FloorService : IFloorService
{
    private readonly SpaceDbContext _context;
    private readonly ILogger<FloorService> _logger;

    public FloorService(SpaceDbContext context, ILogger<FloorService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<FloorResponse>> CreateFloorAsync(Guid tenantId, CreateFloorRequest request)
    {
        try
        {
            // Verify building exists and belongs to tenant
            var building = await _context.Buildings
                .FirstOrDefaultAsync(b => b.Id == request.BuildingId && b.TenantId == tenantId);

            if (building == null)
            {
                return ApiResponse<FloorResponse>.ErrorResponse("Building not found");
            }

            var floor = new Floor
            {
                TenantId = tenantId,
                BuildingId = request.BuildingId,
                Name = request.Name,
                FloorNumber = request.FloorNumber,
                FloorPlanUrl = request.FloorPlanUrl,
                IsActive = true
            };

            _context.Floors.Add(floor);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Floor created: {FloorId} in building {BuildingId}", floor.Id, request.BuildingId);

            return ApiResponse<FloorResponse>.SuccessResponse(
                await MapToResponseAsync(floor),
                "Floor created successfully"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating floor");
            return ApiResponse<FloorResponse>.ErrorResponse("An error occurred while creating the floor");
        }
    }

    public async Task<ApiResponse<FloorResponse>> GetFloorByIdAsync(Guid id, Guid tenantId)
    {
        try
        {
            var floor = await _context.Floors
                .Include(f => f.Building)
                .Include(f => f.Desks)
                .Include(f => f.MeetingRooms)
                .FirstOrDefaultAsync(f => f.Id == id && f.TenantId == tenantId);

            if (floor == null)
            {
                return ApiResponse<FloorResponse>.ErrorResponse("Floor not found");
            }

            return ApiResponse<FloorResponse>.SuccessResponse(await MapToResponseAsync(floor));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving floor {FloorId}", id);
            return ApiResponse<FloorResponse>.ErrorResponse("An error occurred while retrieving the floor");
        }
    }

    public async Task<ApiResponse<PagedResponse<FloorResponse>>> GetFloorsAsync(
        Guid tenantId, Guid? buildingId = null, int pageNumber = 1, int pageSize = 20)
    {
        try
        {
            var query = _context.Floors
                .Include(f => f.Building)
                .Include(f => f.Desks)
                .Include(f => f.MeetingRooms)
                .Where(f => f.TenantId == tenantId);

            if (buildingId.HasValue)
            {
                query = query.Where(f => f.BuildingId == buildingId.Value);
            }

            var totalCount = await query.CountAsync();
            var floors = await query
                .OrderBy(f => f.Building.Name)
                .ThenBy(f => f.FloorNumber)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var responses = new List<FloorResponse>();
            foreach (var floor in floors)
            {
                responses.Add(await MapToResponseAsync(floor));
            }

            var pagedResponse = new PagedResponse<FloorResponse>(
                responses, totalCount, pageNumber, pageSize);

            return ApiResponse<PagedResponse<FloorResponse>>.SuccessResponse(pagedResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving floors");
            return ApiResponse<PagedResponse<FloorResponse>>.ErrorResponse(
                "An error occurred while retrieving floors");
        }
    }

    public async Task<ApiResponse<FloorResponse>> UpdateFloorAsync(
        Guid id, Guid tenantId, UpdateFloorRequest request)
    {
        try
        {
            var floor = await _context.Floors
                .FirstOrDefaultAsync(f => f.Id == id && f.TenantId == tenantId);

            if (floor == null)
            {
                return ApiResponse<FloorResponse>.ErrorResponse("Floor not found");
            }

            if (request.Name != null) floor.Name = request.Name;
            if (request.FloorNumber.HasValue) floor.FloorNumber = request.FloorNumber.Value;
            if (request.FloorPlanUrl != null) floor.FloorPlanUrl = request.FloorPlanUrl;
            if (request.IsActive.HasValue) floor.IsActive = request.IsActive.Value;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Floor updated: {FloorId}", id);

            return ApiResponse<FloorResponse>.SuccessResponse(
                await MapToResponseAsync(floor),
                "Floor updated successfully"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating floor {FloorId}", id);
            return ApiResponse<FloorResponse>.ErrorResponse("An error occurred while updating the floor");
        }
    }

    public async Task<ApiResponse<bool>> DeleteFloorAsync(Guid id, Guid tenantId)
    {
        try
        {
            var floor = await _context.Floors
                .Include(f => f.Desks)
                .Include(f => f.MeetingRooms)
                .FirstOrDefaultAsync(f => f.Id == id && f.TenantId == tenantId);

            if (floor == null)
            {
                return ApiResponse<bool>.ErrorResponse("Floor not found");
            }

            if (floor.Desks.Any() || floor.MeetingRooms.Any())
            {
                return ApiResponse<bool>.ErrorResponse("Cannot delete floor with existing desks or meeting rooms");
            }

            floor.IsDeleted = true;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Floor deleted: {FloorId}", id);

            return ApiResponse<bool>.SuccessResponse(true, "Floor deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting floor {FloorId}", id);
            return ApiResponse<bool>.ErrorResponse("An error occurred while deleting the floor");
        }
    }

    private async Task<FloorResponse> MapToResponseAsync(Floor floor)
    {
        if (floor.Building == null)
        {
            await _context.Entry(floor).Reference(f => f.Building).LoadAsync();
        }

        return new FloorResponse
        {
            Id = floor.Id,
            BuildingId = floor.BuildingId,
            Name = floor.Name,
            FloorNumber = floor.FloorNumber,
            TotalDesks = floor.Desks?.Count ?? 0,
            TotalMeetingRooms = floor.MeetingRooms?.Count ?? 0,
            FloorPlanUrl = floor.FloorPlanUrl,
            IsActive = floor.IsActive,
            BuildingName = floor.Building?.Name,
            CreatedAt = floor.CreatedAt
        };
    }
}
