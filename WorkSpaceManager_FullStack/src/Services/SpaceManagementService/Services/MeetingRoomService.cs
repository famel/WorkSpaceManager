using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WorkSpaceManager.SpaceManagementService.Data;
using WorkSpaceManager.Shared.Common;
using WorkSpaceManager.Shared.DTOs;
using WorkSpaceManager.Shared.Models;

namespace WorkSpaceManager.SpaceManagementService.Services;

public class MeetingRoomService : IMeetingRoomService
{
    private readonly SpaceDbContext _context;
    private readonly ILogger<MeetingRoomService> _logger;

    public MeetingRoomService(SpaceDbContext context, ILogger<MeetingRoomService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<MeetingRoomResponse>> CreateMeetingRoomAsync(Guid tenantId, CreateMeetingRoomRequest request)
    {
        try
        {
            // Verify floor exists and belongs to tenant
            var floor = await _context.Floors
                .FirstOrDefaultAsync(f => f.Id == request.FloorId && f.TenantId == tenantId);

            if (floor == null)
            {
                return ApiResponse<MeetingRoomResponse>.ErrorResponse("Floor not found");
            }

            // Check for duplicate room number on the same floor
            var existingRoom = await _context.MeetingRooms
                .FirstOrDefaultAsync(m => m.FloorId == request.FloorId && 
                                         m.RoomNumber == request.RoomNumber && 
                                         m.TenantId == tenantId);

            if (existingRoom != null)
            {
                return ApiResponse<MeetingRoomResponse>.ErrorResponse($"Meeting room {request.RoomNumber} already exists on this floor");
            }

            var meetingRoom = new MeetingRoom
            {
                TenantId = tenantId,
                FloorId = request.FloorId,
                Name = request.Name,
                RoomNumber = request.RoomNumber,
                Capacity = request.Capacity,
                HasProjector = request.HasProjector,
                HasWhiteboard = request.HasWhiteboard,
                HasVideoConference = request.HasVideoConference,
                HasTelephone = request.HasTelephone,
                IsAccessible = request.IsAccessible,
                Equipment = request.Equipment,
                Notes = request.Notes,
                IsAvailable = true
            };

            _context.MeetingRooms.Add(meetingRoom);
            
            // Update floor meeting room count
            floor.TotalMeetingRooms++;
            
            await _context.SaveChangesAsync();

            _logger.LogInformation("Meeting room created: {MeetingRoomId} on floor {FloorId}", meetingRoom.Id, request.FloorId);

            return ApiResponse<MeetingRoomResponse>.SuccessResponse(
                await MapToResponseAsync(meetingRoom),
                "Meeting room created successfully"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating meeting room");
            return ApiResponse<MeetingRoomResponse>.ErrorResponse("An error occurred while creating the meeting room");
        }
    }

    public async Task<ApiResponse<MeetingRoomResponse>> GetMeetingRoomByIdAsync(Guid id, Guid tenantId)
    {
        try
        {
            var meetingRoom = await _context.MeetingRooms
                .Include(m => m.Floor)
                    .ThenInclude(f => f.Building)
                .FirstOrDefaultAsync(m => m.Id == id && m.TenantId == tenantId);

            if (meetingRoom == null)
            {
                return ApiResponse<MeetingRoomResponse>.ErrorResponse("Meeting room not found");
            }

            return ApiResponse<MeetingRoomResponse>.SuccessResponse(await MapToResponseAsync(meetingRoom));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving meeting room {MeetingRoomId}", id);
            return ApiResponse<MeetingRoomResponse>.ErrorResponse("An error occurred while retrieving the meeting room");
        }
    }

    public async Task<ApiResponse<PagedResponse<MeetingRoomResponse>>> SearchMeetingRoomsAsync(
        Guid tenantId, MeetingRoomSearchRequest request)
    {
        try
        {
            var query = _context.MeetingRooms
                .Include(m => m.Floor)
                    .ThenInclude(f => f.Building)
                .Where(m => m.TenantId == tenantId);

            // Apply filters
            if (request.BuildingId.HasValue)
            {
                query = query.Where(m => m.Floor.BuildingId == request.BuildingId.Value);
            }

            if (request.FloorId.HasValue)
            {
                query = query.Where(m => m.FloorId == request.FloorId.Value);
            }

            if (request.IsAvailable.HasValue)
            {
                query = query.Where(m => m.IsAvailable == request.IsAvailable.Value);
            }

            if (request.IsAccessible.HasValue)
            {
                query = query.Where(m => m.IsAccessible == request.IsAccessible.Value);
            }

            if (request.MinCapacity.HasValue)
            {
                query = query.Where(m => m.Capacity >= request.MinCapacity.Value);
            }

            if (request.HasProjector.HasValue)
            {
                query = query.Where(m => m.HasProjector == request.HasProjector.Value);
            }

            if (request.HasWhiteboard.HasValue)
            {
                query = query.Where(m => m.HasWhiteboard == request.HasWhiteboard.Value);
            }

            if (request.HasVideoConference.HasValue)
            {
                query = query.Where(m => m.HasVideoConference == request.HasVideoConference.Value);
            }

            var totalCount = await query.CountAsync();
            var meetingRooms = await query
                .OrderBy(m => m.Floor.Building.Name)
                .ThenBy(m => m.Floor.FloorNumber)
                .ThenBy(m => m.RoomNumber)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var responses = new List<MeetingRoomResponse>();
            foreach (var room in meetingRooms)
            {
                responses.Add(await MapToResponseAsync(room));
            }

            var pagedResponse = new PagedResponse<MeetingRoomResponse>(
                responses, totalCount, request.PageNumber, request.PageSize);

            return ApiResponse<PagedResponse<MeetingRoomResponse>>.SuccessResponse(pagedResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching meeting rooms");
            return ApiResponse<PagedResponse<MeetingRoomResponse>>.ErrorResponse(
                "An error occurred while searching meeting rooms");
        }
    }

    public async Task<ApiResponse<MeetingRoomResponse>> UpdateMeetingRoomAsync(
        Guid id, Guid tenantId, UpdateMeetingRoomRequest request)
    {
        try
        {
            var meetingRoom = await _context.MeetingRooms
                .FirstOrDefaultAsync(m => m.Id == id && m.TenantId == tenantId);

            if (meetingRoom == null)
            {
                return ApiResponse<MeetingRoomResponse>.ErrorResponse("Meeting room not found");
            }

            // Check for duplicate room number if changing
            if (request.RoomNumber != null && request.RoomNumber != meetingRoom.RoomNumber)
            {
                var existingRoom = await _context.MeetingRooms
                    .FirstOrDefaultAsync(m => m.FloorId == meetingRoom.FloorId && 
                                             m.RoomNumber == request.RoomNumber && 
                                             m.TenantId == tenantId);

                if (existingRoom != null)
                {
                    return ApiResponse<MeetingRoomResponse>.ErrorResponse($"Meeting room {request.RoomNumber} already exists on this floor");
                }

                meetingRoom.RoomNumber = request.RoomNumber;
            }

            if (request.Name != null) meetingRoom.Name = request.Name;
            if (request.Capacity.HasValue) meetingRoom.Capacity = request.Capacity.Value;
            if (request.HasProjector.HasValue) meetingRoom.HasProjector = request.HasProjector.Value;
            if (request.HasWhiteboard.HasValue) meetingRoom.HasWhiteboard = request.HasWhiteboard.Value;
            if (request.HasVideoConference.HasValue) meetingRoom.HasVideoConference = request.HasVideoConference.Value;
            if (request.HasTelephone.HasValue) meetingRoom.HasTelephone = request.HasTelephone.Value;
            if (request.IsAccessible.HasValue) meetingRoom.IsAccessible = request.IsAccessible.Value;
            if (request.IsAvailable.HasValue) meetingRoom.IsAvailable = request.IsAvailable.Value;
            if (request.Equipment != null) meetingRoom.Equipment = request.Equipment;
            if (request.Notes != null) meetingRoom.Notes = request.Notes;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Meeting room updated: {MeetingRoomId}", id);

            return ApiResponse<MeetingRoomResponse>.SuccessResponse(
                await MapToResponseAsync(meetingRoom),
                "Meeting room updated successfully"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating meeting room {MeetingRoomId}", id);
            return ApiResponse<MeetingRoomResponse>.ErrorResponse("An error occurred while updating the meeting room");
        }
    }

    public async Task<ApiResponse<bool>> DeleteMeetingRoomAsync(Guid id, Guid tenantId)
    {
        try
        {
            var meetingRoom = await _context.MeetingRooms
                .Include(m => m.Floor)
                .FirstOrDefaultAsync(m => m.Id == id && m.TenantId == tenantId);

            if (meetingRoom == null)
            {
                return ApiResponse<bool>.ErrorResponse("Meeting room not found");
            }

            meetingRoom.IsDeleted = true;
            
            // Update floor meeting room count
            if (meetingRoom.Floor != null)
            {
                meetingRoom.Floor.TotalMeetingRooms = Math.Max(0, meetingRoom.Floor.TotalMeetingRooms - 1);
            }
            
            await _context.SaveChangesAsync();

            _logger.LogInformation("Meeting room deleted: {MeetingRoomId}", id);

            return ApiResponse<bool>.SuccessResponse(true, "Meeting room deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting meeting room {MeetingRoomId}", id);
            return ApiResponse<bool>.ErrorResponse("An error occurred while deleting the meeting room");
        }
    }

    private async Task<MeetingRoomResponse> MapToResponseAsync(MeetingRoom meetingRoom)
    {
        if (meetingRoom.Floor == null)
        {
            await _context.Entry(meetingRoom).Reference(m => m.Floor).LoadAsync();
        }

        if (meetingRoom.Floor?.Building == null && meetingRoom.Floor != null)
        {
            await _context.Entry(meetingRoom.Floor).Reference(f => f.Building).LoadAsync();
        }

        return new MeetingRoomResponse
        {
            Id = meetingRoom.Id,
            FloorId = meetingRoom.FloorId,
            Name = meetingRoom.Name,
            RoomNumber = meetingRoom.RoomNumber,
            Capacity = meetingRoom.Capacity,
            HasProjector = meetingRoom.HasProjector,
            HasWhiteboard = meetingRoom.HasWhiteboard,
            HasVideoConference = meetingRoom.HasVideoConference,
            HasTelephone = meetingRoom.HasTelephone,
            IsAccessible = meetingRoom.IsAccessible,
            IsAvailable = meetingRoom.IsAvailable,
            Equipment = meetingRoom.Equipment,
            Notes = meetingRoom.Notes,
            FloorName = meetingRoom.Floor?.Name,
            BuildingName = meetingRoom.Floor?.Building?.Name,
            CreatedAt = meetingRoom.CreatedAt
        };
    }
}
