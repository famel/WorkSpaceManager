using WorkSpaceManager.Shared.Common;
using WorkSpaceManager.Shared.DTOs;

namespace WorkSpaceManager.BookingService.Services;

public interface IBookingService
{
    Task<ApiResponse<BookingResponse>> CreateBookingAsync(Guid userId, Guid tenantId, CreateBookingRequest request);
    Task<ApiResponse<BookingResponse>> GetBookingByIdAsync(Guid id, Guid tenantId);
    Task<ApiResponse<PagedResponse<BookingResponse>>> GetUserBookingsAsync(Guid userId, Guid tenantId, BookingSearchRequest request);
    Task<ApiResponse<PagedResponse<BookingResponse>>> SearchBookingsAsync(Guid tenantId, BookingSearchRequest request);
    Task<ApiResponse<BookingResponse>> UpdateBookingAsync(Guid id, Guid userId, Guid tenantId, UpdateBookingRequest request);
    Task<ApiResponse<bool>> CancelBookingAsync(Guid id, Guid userId, Guid tenantId, CancelBookingRequest request);
    Task<ApiResponse<BookingResponse>> CheckInAsync(Guid id, Guid userId, Guid tenantId);
    Task<ApiResponse<BookingResponse>> CheckOutAsync(Guid id, Guid userId, Guid tenantId);
    Task<ApiResponse<AvailabilityResponse>> CheckAvailabilityAsync(Guid tenantId, BookingAvailabilityRequest request);
    Task<ApiResponse<List<BookingResponse>>> GetUpcomingBookingsAsync(Guid userId, Guid tenantId, int days = 7);
    Task MarkNoShowBookingsAsync();
}
