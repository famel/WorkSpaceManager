using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WorkSpaceManager.Shared.Common;
using WorkSpaceManager.Shared.DTOs;

namespace WorkSpaceManager.BookingService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly Services.IBookingService _bookingService;
    private readonly ILogger<BookingsController> _logger;

    public BookingsController(Services.IBookingService bookingService, ILogger<BookingsController> logger)
    {
        _bookingService = bookingService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new booking
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(BookingResponse), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequest request)
    {
        var userId = GetUserId();
        var tenantId = GetTenantId();

        var result = await _bookingService.CreateBookingAsync(userId, tenantId, request);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get booking by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BookingResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetBooking(Guid id)
    {
        var tenantId = GetTenantId();
        var result = await _bookingService.GetBookingByIdAsync(id, tenantId);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get current user's bookings
    /// </summary>
    [HttpGet("my-bookings")]
    [ProducesResponseType(typeof(PagedResponse<BookingResponse>), 200)]
    public async Task<IActionResult> GetMyBookings([FromQuery] BookingSearchRequest request)
    {
        var userId = GetUserId();
        var tenantId = GetTenantId();

        var result = await _bookingService.GetUserBookingsAsync(userId, tenantId, request);

        return Ok(result);
    }

    /// <summary>
    /// Get upcoming bookings for current user
    /// </summary>
    [HttpGet("upcoming")]
    [ProducesResponseType(typeof(List<BookingResponse>), 200)]
    public async Task<IActionResult> GetUpcomingBookings([FromQuery] int days = 7)
    {
        var userId = GetUserId();
        var tenantId = GetTenantId();

        var result = await _bookingService.GetUpcomingBookingsAsync(userId, tenantId, days);

        return Ok(result);
    }

    /// <summary>
    /// Search bookings (admin/manager)
    /// </summary>
    [HttpPost("search")]
    [Authorize(Roles = "admin,manager")]
    [ProducesResponseType(typeof(PagedResponse<BookingResponse>), 200)]
    public async Task<IActionResult> SearchBookings([FromBody] BookingSearchRequest request)
    {
        var tenantId = GetTenantId();
        var result = await _bookingService.SearchBookingsAsync(tenantId, request);

        return Ok(result);
    }

    /// <summary>
    /// Update booking
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(BookingResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateBooking(Guid id, [FromBody] UpdateBookingRequest request)
    {
        var userId = GetUserId();
        var tenantId = GetTenantId();

        var result = await _bookingService.UpdateBookingAsync(id, userId, tenantId, request);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Cancel booking
    /// </summary>
    [HttpPost("{id}/cancel")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> CancelBooking(Guid id, [FromBody] CancelBookingRequest request)
    {
        var userId = GetUserId();
        var tenantId = GetTenantId();

        request.BookingId = id;
        var result = await _bookingService.CancelBookingAsync(id, userId, tenantId, request);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Check in to booking
    /// </summary>
    [HttpPost("{id}/checkin")]
    [ProducesResponseType(typeof(BookingResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> CheckIn(Guid id)
    {
        var userId = GetUserId();
        var tenantId = GetTenantId();

        var result = await _bookingService.CheckInAsync(id, userId, tenantId);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Check out from booking
    /// </summary>
    [HttpPost("{id}/checkout")]
    [ProducesResponseType(typeof(BookingResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> CheckOut(Guid id)
    {
        var userId = GetUserId();
        var tenantId = GetTenantId();

        var result = await _bookingService.CheckOutAsync(id, userId, tenantId);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Check availability
    /// </summary>
    [HttpPost("check-availability")]
    [ProducesResponseType(typeof(AvailabilityResponse), 200)]
    public async Task<IActionResult> CheckAvailability([FromBody] BookingAvailabilityRequest request)
    {
        var tenantId = GetTenantId();
        var result = await _bookingService.CheckAvailabilityAsync(tenantId, request);

        return Ok(result);
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                         ?? User.FindFirst("sub")?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User ID not found in token");
        }

        return userId;
    }

    private Guid GetTenantId()
    {
        var tenantIdClaim = User.FindFirst("tenant_id")?.Value;
        
        if (string.IsNullOrEmpty(tenantIdClaim) || !Guid.TryParse(tenantIdClaim, out var tenantId))
        {
            throw new UnauthorizedAccessException("Tenant ID not found in token");
        }

        return tenantId;
    }
}
