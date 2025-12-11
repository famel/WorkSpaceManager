using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WorkSpaceManager.SpaceManagementService.Data;
using WorkSpaceManager.Shared.Common;
using WorkSpaceManager.Shared.DTOs;
using WorkSpaceManager.Shared.Models;

namespace WorkSpaceManager.SpaceManagementService.Services;

public class DeskService : IDeskService
{
    private readonly SpaceDbContext _context;
    private readonly ILogger<DeskService> _logger;

    public DeskService(SpaceDbContext context, ILogger<DeskService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<DeskResponse>> CreateDeskAsync(Guid tenantId, CreateDeskRequest request)
    {
        try
        {
            // Verify floor exists and belongs to tenant
            var floor = await _context.Floors
                .FirstOrDefaultAsync(f => f.Id == request.FloorId && f.TenantId == tenantId);

            if (floor == null)
            {
                return ApiResponse<DeskResponse>.ErrorResponse("Floor not found");
            }

            // Check for duplicate desk number on the same floor
            var existingDesk = await _context.Desks
                .FirstOrDefaultAsync(d => d.FloorId == request.FloorId && 
                                         d.DeskNumber == request.DeskNumber && 
                                         d.TenantId == tenantId);

            if (existingDesk != null)
            {
                return ApiResponse<DeskResponse>.ErrorResponse($"Desk number {request.DeskNumber} already exists on this floor");
            }

            var desk = new Desk
            {
                TenantId = tenantId,
                FloorId = request.FloorId,
                DeskNumber = request.DeskNumber,
                Location = request.Location,
                HasMonitor = request.HasMonitor,
                HasDockingStation = request.HasDockingStation,
                IsNearWindow = request.IsNearWindow,
                IsAccessible = request.IsAccessible,
                Notes = request.Notes,
                IsAvailable = true
            };

            _context.Desks.Add(desk);
            
            // Update floor desk count
            floor.TotalDesks++;
            
            await _context.SaveChangesAsync();

            _logger.LogInformation("Desk created: {DeskId} on floor {FloorId}", desk.Id, request.FloorId);

            return ApiResponse<DeskResponse>.SuccessResponse(
                await MapToResponseAsync(desk),
                "Desk created successfully"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating desk");
            return ApiResponse<DeskResponse>.ErrorResponse("An error occurred while creating the desk");
        }
    }

    public async Task<ApiResponse<DeskResponse>> GetDeskByIdAsync(Guid id, Guid tenantId)
    {
        try
        {
            var desk = await _context.Desks
                .Include(d => d.Floor)
                    .ThenInclude(f => f.Building)
                .FirstOrDefaultAsync(d => d.Id == id && d.TenantId == tenantId);

            if (desk == null)
            {
                return ApiResponse<DeskResponse>.ErrorResponse("Desk not found");
            }

            return ApiResponse<DeskResponse>.SuccessResponse(await MapToResponseAsync(desk));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving desk {DeskId}", id);
            return ApiResponse<DeskResponse>.ErrorResponse("An error occurred while retrieving the desk");
        }
    }

    public async Task<ApiResponse<PagedResponse<DeskResponse>>> SearchDesksAsync(
        Guid tenantId, DeskSearchRequest request)
    {
        try
        {
            var query = _context.Desks
                .Include(d => d.Floor)
                    .ThenInclude(f => f.Building)
                .Where(d => d.TenantId == tenantId);

            // Apply filters
            if (request.BuildingId.HasValue)
            {
                query = query.Where(d => d.Floor.BuildingId == request.BuildingId.Value);
            }

            if (request.FloorId.HasValue)
            {
                query = query.Where(d => d.FloorId == request.FloorId.Value);
            }

            if (request.IsAvailable.HasValue)
            {
                query = query.Where(d => d.IsAvailable == request.IsAvailable.Value);
            }

            if (request.IsAccessible.HasValue)
            {
                query = query.Where(d => d.IsAccessible == request.IsAccessible.Value);
            }

            if (request.HasMonitor.HasValue)
            {
                query = query.Where(d => d.HasMonitor == request.HasMonitor.Value);
            }

            if (request.HasDockingStation.HasValue)
            {
                query = query.Where(d => d.HasDockingStation == request.HasDockingStation.Value);
            }

            if (request.IsNearWindow.HasValue)
            {
                query = query.Where(d => d.IsNearWindow == request.IsNearWindow.Value);
            }

            var totalCount = await query.CountAsync();
            var desks = await query
                .OrderBy(d => d.Floor.Building.Name)
                .ThenBy(d => d.Floor.FloorNumber)
                .ThenBy(d => d.DeskNumber)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var responses = new List<DeskResponse>();
            foreach (var desk in desks)
            {
                responses.Add(await MapToResponseAsync(desk));
            }

            var pagedResponse = new PagedResponse<DeskResponse>(
                responses, totalCount, request.PageNumber, request.PageSize);

            return ApiResponse<PagedResponse<DeskResponse>>.SuccessResponse(pagedResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching desks");
            return ApiResponse<PagedResponse<DeskResponse>>.ErrorResponse(
                "An error occurred while searching desks");
        }
    }

    public async Task<ApiResponse<DeskResponse>> UpdateDeskAsync(
        Guid id, Guid tenantId, UpdateDeskRequest request)
    {
        try
        {
            var desk = await _context.Desks
                .FirstOrDefaultAsync(d => d.Id == id && d.TenantId == tenantId);

            if (desk == null)
            {
                return ApiResponse<DeskResponse>.ErrorResponse("Desk not found");
            }

            // Check for duplicate desk number if changing
            if (request.DeskNumber != null && request.DeskNumber != desk.DeskNumber)
            {
                var existingDesk = await _context.Desks
                    .FirstOrDefaultAsync(d => d.FloorId == desk.FloorId && 
                                             d.DeskNumber == request.DeskNumber && 
                                             d.TenantId == tenantId);

                if (existingDesk != null)
                {
                    return ApiResponse<DeskResponse>.ErrorResponse($"Desk number {request.DeskNumber} already exists on this floor");
                }

                desk.DeskNumber = request.DeskNumber;
            }

            if (request.Location != null) desk.Location = request.Location;
            if (request.HasMonitor.HasValue) desk.HasMonitor = request.HasMonitor.Value;
            if (request.HasDockingStation.HasValue) desk.HasDockingStation = request.HasDockingStation.Value;
            if (request.IsNearWindow.HasValue) desk.IsNearWindow = request.IsNearWindow.Value;
            if (request.IsAccessible.HasValue) desk.IsAccessible = request.IsAccessible.Value;
            if (request.IsAvailable.HasValue) desk.IsAvailable = request.IsAvailable.Value;
            if (request.Notes != null) desk.Notes = request.Notes;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Desk updated: {DeskId}", id);

            return ApiResponse<DeskResponse>.SuccessResponse(
                await MapToResponseAsync(desk),
                "Desk updated successfully"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating desk {DeskId}", id);
            return ApiResponse<DeskResponse>.ErrorResponse("An error occurred while updating the desk");
        }
    }

    public async Task<ApiResponse<bool>> DeleteDeskAsync(Guid id, Guid tenantId)
    {
        try
        {
            var desk = await _context.Desks
                .Include(d => d.Floor)
                .FirstOrDefaultAsync(d => d.Id == id && d.TenantId == tenantId);

            if (desk == null)
            {
                return ApiResponse<bool>.ErrorResponse("Desk not found");
            }

            desk.IsDeleted = true;
            
            // Update floor desk count
            if (desk.Floor != null)
            {
                desk.Floor.TotalDesks = Math.Max(0, desk.Floor.TotalDesks - 1);
            }
            
            await _context.SaveChangesAsync();

            _logger.LogInformation("Desk deleted: {DeskId}", id);

            return ApiResponse<bool>.SuccessResponse(true, "Desk deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting desk {DeskId}", id);
            return ApiResponse<bool>.ErrorResponse("An error occurred while deleting the desk");
        }
    }

    private async Task<DeskResponse> MapToResponseAsync(Desk desk)
    {
        if (desk.Floor == null)
        {
            await _context.Entry(desk).Reference(d => d.Floor).LoadAsync();
        }

        if (desk.Floor?.Building == null && desk.Floor != null)
        {
            await _context.Entry(desk.Floor).Reference(f => f.Building).LoadAsync();
        }

        return new DeskResponse
        {
            Id = desk.Id,
            FloorId = desk.FloorId,
            DeskNumber = desk.DeskNumber,
            Location = desk.Location,
            HasMonitor = desk.HasMonitor,
            HasDockingStation = desk.HasDockingStation,
            IsNearWindow = desk.IsNearWindow,
            IsAccessible = desk.IsAccessible,
            IsAvailable = desk.IsAvailable,
            Notes = desk.Notes,
            FloorName = desk.Floor?.Name,
            BuildingName = desk.Floor?.Building?.Name,
            CreatedAt = desk.CreatedAt
        };
    }
}
