using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WorkSpaceManager.BookingService.Data;
using WorkSpaceManager.Shared.Common;
using WorkSpaceManager.Shared.DTOs;
using WorkSpaceManager.Shared.Models;

namespace WorkSpaceManager.BookingService.Services;

public class BookingService : IBookingService
{
    private readonly BookingDbContext _context;
    private readonly ILogger<BookingService> _logger;

    public BookingService(BookingDbContext context, ILogger<BookingService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<BookingResponse>> CreateBookingAsync(Guid userId, Guid tenantId, CreateBookingRequest request)
    {
        try
        {
            // Validation
            if (request.DeskId == null && request.MeetingRoomId == null)
            {
                return ApiResponse<BookingResponse>.ErrorResponse("Either DeskId or MeetingRoomId must be provided");
            }

            if (request.DeskId != null && request.MeetingRoomId != null)
            {
                return ApiResponse<BookingResponse>.ErrorResponse("Cannot book both desk and meeting room");
            }

            if (request.EndTime <= request.StartTime)
            {
                return ApiResponse<BookingResponse>.ErrorResponse("End time must be after start time");
            }

            // Check if resource exists and is available
            if (request.DeskId != null)
            {
                var desk = await _context.Desks
                    .FirstOrDefaultAsync(d => d.Id == request.DeskId && d.TenantId == tenantId);

                if (desk == null)
                {
                    return ApiResponse<BookingResponse>.ErrorResponse("Desk not found");
                }

                if (!desk.IsAvailable)
                {
                    return ApiResponse<BookingResponse>.ErrorResponse("Desk is not available");
                }
            }

            if (request.MeetingRoomId != null)
            {
                var meetingRoom = await _context.MeetingRooms
                    .FirstOrDefaultAsync(m => m.Id == request.MeetingRoomId && m.TenantId == tenantId);

                if (meetingRoom == null)
                {
                    return ApiResponse<BookingResponse>.ErrorResponse("Meeting room not found");
                }

                if (!meetingRoom.IsAvailable)
                {
                    return ApiResponse<BookingResponse>.ErrorResponse("Meeting room is not available");
                }
            }

            // Check for conflicts
            var hasConflict = await _context.Bookings
                .Where(b => b.TenantId == tenantId &&
                           b.BookingDate.Date == request.BookingDate.Date &&
                           b.Status != BookingStatus.Cancelled &&
                           b.Status != BookingStatus.NoShow &&
                           ((request.DeskId != null && b.DeskId == request.DeskId) ||
                            (request.MeetingRoomId != null && b.MeetingRoomId == request.MeetingRoomId)) &&
                           ((b.StartTime < request.EndTime && b.EndTime > request.StartTime)))
                .AnyAsync();

            if (hasConflict)
            {
                return ApiResponse<BookingResponse>.ErrorResponse("Resource is already booked for the selected time slot");
            }

            // Create booking
            var booking = new Booking
            {
                UserId = userId,
                TenantId = tenantId,
                DeskId = request.DeskId,
                MeetingRoomId = request.MeetingRoomId,
                BookingDate = request.BookingDate.Date,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                Purpose = request.Purpose,
                Status = BookingStatus.Confirmed
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Booking created: {BookingId} for user {UserId}", booking.Id, userId);

            return ApiResponse<BookingResponse>.SuccessResponse(
                await MapToResponseAsync(booking),
                "Booking created successfully"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating booking for user {UserId}", userId);
            return ApiResponse<BookingResponse>.ErrorResponse("An error occurred while creating the booking");
        }
    }

    public async Task<ApiResponse<BookingResponse>> GetBookingByIdAsync(Guid id, Guid tenantId)
    {
        try
        {
            var booking = await _context.Bookings
                .Include(b => b.Desk).ThenInclude(d => d!.Floor).ThenInclude(f => f!.Building)
                .Include(b => b.MeetingRoom).ThenInclude(m => m!.Floor).ThenInclude(f => f!.Building)
                .FirstOrDefaultAsync(b => b.Id == id && b.TenantId == tenantId);

            if (booking == null)
            {
                return ApiResponse<BookingResponse>.ErrorResponse("Booking not found");
            }

            return ApiResponse<BookingResponse>.SuccessResponse(await MapToResponseAsync(booking));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving booking {BookingId}", id);
            return ApiResponse<BookingResponse>.ErrorResponse("An error occurred while retrieving the booking");
        }
    }

    public async Task<ApiResponse<PagedResponse<BookingResponse>>> GetUserBookingsAsync(
        Guid userId, Guid tenantId, BookingSearchRequest request)
    {
        try
        {
            var query = _context.Bookings
                .Include(b => b.Desk).ThenInclude(d => d!.Floor).ThenInclude(f => f!.Building)
                .Include(b => b.MeetingRoom).ThenInclude(m => m!.Floor).ThenInclude(f => f!.Building)
                .Where(b => b.UserId == userId && b.TenantId == tenantId);

            query = ApplyFilters(query, request);

            var totalCount = await query.CountAsync();
            var bookings = await query
                .OrderByDescending(b => b.BookingDate)
                .ThenBy(b => b.StartTime)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var responses = new List<BookingResponse>();
            foreach (var booking in bookings)
            {
                responses.Add(await MapToResponseAsync(booking));
            }

            var pagedResponse = new PagedResponse<BookingResponse>(
                responses, totalCount, request.PageNumber, request.PageSize);

            return ApiResponse<PagedResponse<BookingResponse>>.SuccessResponse(pagedResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving bookings for user {UserId}", userId);
            return ApiResponse<PagedResponse<BookingResponse>>.ErrorResponse(
                "An error occurred while retrieving bookings");
        }
    }

    public async Task<ApiResponse<PagedResponse<BookingResponse>>> SearchBookingsAsync(
        Guid tenantId, BookingSearchRequest request)
    {
        try
        {
            var query = _context.Bookings
                .Include(b => b.Desk).ThenInclude(d => d!.Floor).ThenInclude(f => f!.Building)
                .Include(b => b.MeetingRoom).ThenInclude(m => m!.Floor).ThenInclude(f => f!.Building)
                .Where(b => b.TenantId == tenantId);

            query = ApplyFilters(query, request);

            var totalCount = await query.CountAsync();
            var bookings = await query
                .OrderByDescending(b => b.BookingDate)
                .ThenBy(b => b.StartTime)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var responses = new List<BookingResponse>();
            foreach (var booking in bookings)
            {
                responses.Add(await MapToResponseAsync(booking));
            }

            var pagedResponse = new PagedResponse<BookingResponse>(
                responses, totalCount, request.PageNumber, request.PageSize);

            return ApiResponse<PagedResponse<BookingResponse>>.SuccessResponse(pagedResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching bookings");
            return ApiResponse<PagedResponse<BookingResponse>>.ErrorResponse(
                "An error occurred while searching bookings");
        }
    }

    public async Task<ApiResponse<BookingResponse>> UpdateBookingAsync(
        Guid id, Guid userId, Guid tenantId, UpdateBookingRequest request)
    {
        try
        {
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId && b.TenantId == tenantId);

            if (booking == null)
            {
                return ApiResponse<BookingResponse>.ErrorResponse("Booking not found");
            }

            if (booking.Status == BookingStatus.Cancelled)
            {
                return ApiResponse<BookingResponse>.ErrorResponse("Cannot update cancelled booking");
            }

            if (booking.Status == BookingStatus.CheckedOut)
            {
                return ApiResponse<BookingResponse>.ErrorResponse("Cannot update completed booking");
            }

            // Update fields
            if (request.BookingDate.HasValue)
            {
                booking.BookingDate = request.BookingDate.Value.Date;
            }

            if (request.StartTime.HasValue)
            {
                booking.StartTime = request.StartTime.Value;
            }

            if (request.EndTime.HasValue)
            {
                booking.EndTime = request.EndTime.Value;
            }

            if (request.Purpose != null)
            {
                booking.Purpose = request.Purpose;
            }

            // Validate time
            if (booking.EndTime <= booking.StartTime)
            {
                return ApiResponse<BookingResponse>.ErrorResponse("End time must be after start time");
            }

            // Check for conflicts
            var hasConflict = await _context.Bookings
                .Where(b => b.Id != id &&
                           b.TenantId == tenantId &&
                           b.BookingDate.Date == booking.BookingDate.Date &&
                           b.Status != BookingStatus.Cancelled &&
                           b.Status != BookingStatus.NoShow &&
                           ((booking.DeskId != null && b.DeskId == booking.DeskId) ||
                            (booking.MeetingRoomId != null && b.MeetingRoomId == booking.MeetingRoomId)) &&
                           ((b.StartTime < booking.EndTime && b.EndTime > booking.StartTime)))
                .AnyAsync();

            if (hasConflict)
            {
                return ApiResponse<BookingResponse>.ErrorResponse(
                    "Resource is already booked for the selected time slot");
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Booking updated: {BookingId}", id);

            return ApiResponse<BookingResponse>.SuccessResponse(
                await MapToResponseAsync(booking),
                "Booking updated successfully"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating booking {BookingId}", id);
            return ApiResponse<BookingResponse>.ErrorResponse("An error occurred while updating the booking");
        }
    }

    public async Task<ApiResponse<bool>> CancelBookingAsync(
        Guid id, Guid userId, Guid tenantId, CancelBookingRequest request)
    {
        try
        {
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId && b.TenantId == tenantId);

            if (booking == null)
            {
                return ApiResponse<bool>.ErrorResponse("Booking not found");
            }

            if (booking.Status == BookingStatus.Cancelled)
            {
                return ApiResponse<bool>.ErrorResponse("Booking is already cancelled");
            }

            if (booking.Status == BookingStatus.CheckedOut)
            {
                return ApiResponse<bool>.ErrorResponse("Cannot cancel completed booking");
            }

            booking.Status = BookingStatus.Cancelled;
            booking.CancellationReason = request.Reason;
            booking.CancelledAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Booking cancelled: {BookingId}", id);

            return ApiResponse<bool>.SuccessResponse(true, "Booking cancelled successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling booking {BookingId}", id);
            return ApiResponse<bool>.ErrorResponse("An error occurred while cancelling the booking");
        }
    }

    public async Task<ApiResponse<BookingResponse>> CheckInAsync(Guid id, Guid userId, Guid tenantId)
    {
        try
        {
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId && b.TenantId == tenantId);

            if (booking == null)
            {
                return ApiResponse<BookingResponse>.ErrorResponse("Booking not found");
            }

            if (booking.Status != BookingStatus.Confirmed)
            {
                return ApiResponse<BookingResponse>.ErrorResponse(
                    $"Cannot check in. Booking status is {booking.Status}");
            }

            if (booking.BookingDate.Date != DateTime.UtcNow.Date)
            {
                return ApiResponse<BookingResponse>.ErrorResponse("Can only check in on the booking date");
            }

            booking.Status = BookingStatus.CheckedIn;
            booking.CheckInTime = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("User checked in: {BookingId}", id);

            return ApiResponse<BookingResponse>.SuccessResponse(
                await MapToResponseAsync(booking),
                "Checked in successfully"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking in booking {BookingId}", id);
            return ApiResponse<BookingResponse>.ErrorResponse("An error occurred during check-in");
        }
    }

    public async Task<ApiResponse<BookingResponse>> CheckOutAsync(Guid id, Guid userId, Guid tenantId)
    {
        try
        {
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId && b.TenantId == tenantId);

            if (booking == null)
            {
                return ApiResponse<BookingResponse>.ErrorResponse("Booking not found");
            }

            if (booking.Status != BookingStatus.CheckedIn)
            {
                return ApiResponse<BookingResponse>.ErrorResponse(
                    "Can only check out after checking in");
            }

            booking.Status = BookingStatus.CheckedOut;
            booking.CheckOutTime = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("User checked out: {BookingId}", id);

            return ApiResponse<BookingResponse>.SuccessResponse(
                await MapToResponseAsync(booking),
                "Checked out successfully"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking out booking {BookingId}", id);
            return ApiResponse<BookingResponse>.ErrorResponse("An error occurred during check-out");
        }
    }

    public async Task<ApiResponse<AvailabilityResponse>> CheckAvailabilityAsync(
        Guid tenantId, BookingAvailabilityRequest request)
    {
        try
        {
            var query = _context.Bookings
                .Where(b => b.TenantId == tenantId &&
                           b.BookingDate.Date == request.Date.Date &&
                           b.Status != BookingStatus.Cancelled &&
                           b.Status != BookingStatus.NoShow &&
                           b.StartTime < request.EndTime &&
                           b.EndTime > request.StartTime);

            if (request.DeskId.HasValue)
            {
                var isAvailable = !await query.AnyAsync(b => b.DeskId == request.DeskId);
                return ApiResponse<AvailabilityResponse>.SuccessResponse(new AvailabilityResponse
                {
                    IsAvailable = isAvailable,
                    Message = isAvailable ? "Desk is available" : "Desk is not available for the selected time"
                });
            }

            if (request.MeetingRoomId.HasValue)
            {
                var isAvailable = !await query.AnyAsync(b => b.MeetingRoomId == request.MeetingRoomId);
                return ApiResponse<AvailabilityResponse>.SuccessResponse(new AvailabilityResponse
                {
                    IsAvailable = isAvailable,
                    Message = isAvailable ? "Meeting room is available" : "Meeting room is not available for the selected time"
                });
            }

            // Find available resources on floor
            if (request.FloorId.HasValue)
            {
                var bookedDeskIds = await query.Where(b => b.DeskId != null).Select(b => b.DeskId!.Value).ToListAsync();
                var availableDesks = await _context.Desks
                    .Where(d => d.FloorId == request.FloorId && d.IsAvailable && !bookedDeskIds.Contains(d.Id))
                    .Select(d => d.Id)
                    .ToListAsync();

                return ApiResponse<AvailabilityResponse>.SuccessResponse(new AvailabilityResponse
                {
                    IsAvailable = availableDesks.Any(),
                    AvailableResourceIds = availableDesks,
                    Message = $"{availableDesks.Count} desks available"
                });
            }

            return ApiResponse<AvailabilityResponse>.ErrorResponse("Must specify DeskId, MeetingRoomId, or FloorId");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking availability");
            return ApiResponse<AvailabilityResponse>.ErrorResponse("An error occurred while checking availability");
        }
    }

    public async Task<ApiResponse<List<BookingResponse>>> GetUpcomingBookingsAsync(
        Guid userId, Guid tenantId, int days = 7)
    {
        try
        {
            var startDate = DateTime.UtcNow.Date;
            var endDate = startDate.AddDays(days);

            var bookings = await _context.Bookings
                .Include(b => b.Desk).ThenInclude(d => d!.Floor).ThenInclude(f => f!.Building)
                .Include(b => b.MeetingRoom).ThenInclude(m => m!.Floor).ThenInclude(f => f!.Building)
                .Where(b => b.UserId == userId &&
                           b.TenantId == tenantId &&
                           b.BookingDate >= startDate &&
                           b.BookingDate < endDate &&
                           b.Status != BookingStatus.Cancelled &&
                           b.Status != BookingStatus.NoShow)
                .OrderBy(b => b.BookingDate)
                .ThenBy(b => b.StartTime)
                .ToListAsync();

            var responses = new List<BookingResponse>();
            foreach (var booking in bookings)
            {
                responses.Add(await MapToResponseAsync(booking));
            }

            return ApiResponse<List<BookingResponse>>.SuccessResponse(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving upcoming bookings for user {UserId}", userId);
            return ApiResponse<List<BookingResponse>>.ErrorResponse(
                "An error occurred while retrieving upcoming bookings");
        }
    }

    public async Task MarkNoShowBookingsAsync()
    {
        try
        {
            var cutoffTime = DateTime.UtcNow.AddHours(-2);
            var today = DateTime.UtcNow.Date;

            var noShowBookings = await _context.Bookings
                .Where(b => b.BookingDate.Date == today &&
                           b.Status == BookingStatus.Confirmed &&
                           b.CheckInTime == null &&
                           DateTime.UtcNow.TimeOfDay > b.StartTime.Add(TimeSpan.FromHours(2)))
                .ToListAsync();

            foreach (var booking in noShowBookings)
            {
                booking.Status = BookingStatus.NoShow;
                booking.IsNoShow = true;
            }

            if (noShowBookings.Any())
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Marked {Count} bookings as no-show", noShowBookings.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking no-show bookings");
        }
    }

    private IQueryable<Booking> ApplyFilters(IQueryable<Booking> query, BookingSearchRequest request)
    {
        if (request.FromDate.HasValue)
        {
            query = query.Where(b => b.BookingDate >= request.FromDate.Value.Date);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(b => b.BookingDate <= request.ToDate.Value.Date);
        }

        if (request.DeskId.HasValue)
        {
            query = query.Where(b => b.DeskId == request.DeskId);
        }

        if (request.MeetingRoomId.HasValue)
        {
            query = query.Where(b => b.MeetingRoomId == request.MeetingRoomId);
        }

        if (request.UserId.HasValue)
        {
            query = query.Where(b => b.UserId == request.UserId);
        }

        if (!string.IsNullOrEmpty(request.Status))
        {
            query = query.Where(b => b.Status == request.Status);
        }

        return query;
    }

    private async Task<BookingResponse> MapToResponseAsync(Booking booking)
    {
        // Load related entities if not already loaded
        if (booking.Desk != null && booking.Desk.Floor == null)
        {
            await _context.Entry(booking.Desk).Reference(d => d.Floor).LoadAsync();
            if (booking.Desk.Floor != null)
            {
                await _context.Entry(booking.Desk.Floor).Reference(f => f.Building).LoadAsync();
            }
        }

        if (booking.MeetingRoom != null && booking.MeetingRoom.Floor == null)
        {
            await _context.Entry(booking.MeetingRoom).Reference(m => m.Floor).LoadAsync();
            if (booking.MeetingRoom.Floor != null)
            {
                await _context.Entry(booking.MeetingRoom.Floor).Reference(f => f.Building).LoadAsync();
            }
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == booking.UserId);

        return new BookingResponse
        {
            Id = booking.Id,
            UserId = booking.UserId,
            DeskId = booking.DeskId,
            MeetingRoomId = booking.MeetingRoomId,
            BookingDate = booking.BookingDate,
            StartTime = booking.StartTime,
            EndTime = booking.EndTime,
            Status = booking.Status,
            Purpose = booking.Purpose,
            CheckInTime = booking.CheckInTime,
            CheckOutTime = booking.CheckOutTime,
            IsNoShow = booking.IsNoShow,
            CreatedAt = booking.CreatedAt,
            UpdatedAt = booking.UpdatedAt,
            DeskNumber = booking.Desk?.DeskNumber,
            MeetingRoomName = booking.MeetingRoom?.Name,
            FloorName = booking.Desk?.Floor?.Name ?? booking.MeetingRoom?.Floor?.Name,
            BuildingName = booking.Desk?.Floor?.Building?.Name ?? booking.MeetingRoom?.Floor?.Building?.Name,
            UserName = user != null ? $"{user.FirstName} {user.LastName}" : null,
            UserEmail = user?.Email
        };
    }
}
