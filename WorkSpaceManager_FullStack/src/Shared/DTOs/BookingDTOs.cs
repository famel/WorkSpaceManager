using System;
using System.ComponentModel.DataAnnotations;

namespace WorkSpaceManager.Shared.DTOs;

// ==================== BOOKING DTOs ====================

public class CreateBookingRequest
{
    [Required]
    public Guid? DeskId { get; set; }
    
    public Guid? MeetingRoomId { get; set; }
    
    [Required]
    public DateTime BookingDate { get; set; }
    
    [Required]
    public TimeSpan StartTime { get; set; }
    
    [Required]
    public TimeSpan EndTime { get; set; }
    
    [MaxLength(500)]
    public string? Purpose { get; set; }
}

public class UpdateBookingRequest
{
    public DateTime? BookingDate { get; set; }
    
    public TimeSpan? StartTime { get; set; }
    
    public TimeSpan? EndTime { get; set; }
    
    [MaxLength(500)]
    public string? Purpose { get; set; }
}

public class BookingResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? DeskId { get; set; }
    public Guid? MeetingRoomId { get; set; }
    public DateTime BookingDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Purpose { get; set; }
    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public bool IsNoShow { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Related data
    public string? DeskNumber { get; set; }
    public string? MeetingRoomName { get; set; }
    public string? FloorName { get; set; }
    public string? BuildingName { get; set; }
    public string? UserName { get; set; }
    public string? UserEmail { get; set; }
}

public class CheckInRequest
{
    [Required]
    public Guid BookingId { get; set; }
}

public class CheckOutRequest
{
    [Required]
    public Guid BookingId { get; set; }
}

public class CancelBookingRequest
{
    [Required]
    public Guid BookingId { get; set; }
    
    [MaxLength(500)]
    public string? Reason { get; set; }
}

public class BookingSearchRequest
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public Guid? DeskId { get; set; }
    public Guid? MeetingRoomId { get; set; }
    public Guid? UserId { get; set; }
    public string? Status { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class BookingAvailabilityRequest
{
    [Required]
    public DateTime Date { get; set; }
    
    [Required]
    public TimeSpan StartTime { get; set; }
    
    [Required]
    public TimeSpan EndTime { get; set; }
    
    public Guid? DeskId { get; set; }
    public Guid? MeetingRoomId { get; set; }
    public Guid? FloorId { get; set; }
}

public class AvailabilityResponse
{
    public bool IsAvailable { get; set; }
    public string? Message { get; set; }
    public List<Guid> AvailableResourceIds { get; set; } = new();
}
