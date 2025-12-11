using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkSpaceManager.Shared.Models;

/// <summary>
/// Base entity with common properties
/// </summary>
public abstract class BaseEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsDeleted { get; set; } = false;
}

/// <summary>
/// Multi-tenant base entity
/// </summary>
public abstract class TenantEntity : BaseEntity
{
    [Required]
    public Guid TenantId { get; set; }
}

// ==================== SPACE MANAGEMENT ENTITIES ====================

[Table("Buildings")]
public class Building : TenantEntity
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Address { get; set; }
    
    public int TotalFloors { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public virtual ICollection<Floor> Floors { get; set; } = new List<Floor>();
}

[Table("Floors")]
public class Floor : TenantEntity
{
    [Required]
    public Guid BuildingId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    public int FloorNumber { get; set; }
    
    public int TotalDesks { get; set; }
    
    public int TotalMeetingRooms { get; set; }
    
    [MaxLength(1000)]
    public string? FloorPlanUrl { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    [ForeignKey(nameof(BuildingId))]
    public virtual Building Building { get; set; } = null!;
    
    public virtual ICollection<Desk> Desks { get; set; } = new List<Desk>();
    public virtual ICollection<MeetingRoom> MeetingRooms { get; set; } = new List<MeetingRoom>();
}

[Table("Desks")]
public class Desk : TenantEntity
{
    [Required]
    public Guid FloorId { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string DeskNumber { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? Location { get; set; }
    
    public bool HasMonitor { get; set; }
    
    public bool HasDockingStation { get; set; }
    
    public bool IsNearWindow { get; set; }
    
    public bool IsAccessible { get; set; }
    
    public bool IsAvailable { get; set; } = true;
    
    [MaxLength(500)]
    public string? Notes { get; set; }
    
    // Navigation properties
    [ForeignKey(nameof(FloorId))]
    public virtual Floor Floor { get; set; } = null!;
    
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}

[Table("MeetingRooms")]
public class MeetingRoom : TenantEntity
{
    [Required]
    public Guid FloorId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string RoomNumber { get; set; } = string.Empty;
    
    public int Capacity { get; set; }
    
    public bool HasProjector { get; set; }
    
    public bool HasWhiteboard { get; set; }
    
    public bool HasVideoConference { get; set; }
    
    public bool HasTelephone { get; set; }
    
    public bool IsAccessible { get; set; }
    
    public bool IsAvailable { get; set; } = true;
    
    [MaxLength(500)]
    public string? Equipment { get; set; }
    
    [MaxLength(500)]
    public string? Notes { get; set; }
    
    // Navigation properties
    [ForeignKey(nameof(FloorId))]
    public virtual Floor Floor { get; set; } = null!;
    
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}

// ==================== BOOKING ENTITIES ====================

[Table("Bookings")]
public class Booking : TenantEntity
{
    [Required]
    public Guid UserId { get; set; }
    
    public Guid? DeskId { get; set; }
    
    public Guid? MeetingRoomId { get; set; }
    
    [Required]
    public DateTime BookingDate { get; set; }
    
    [Required]
    public TimeSpan StartTime { get; set; }
    
    [Required]
    public TimeSpan EndTime { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = BookingStatus.Pending;
    
    [MaxLength(500)]
    public string? Purpose
 { get; set; }
    
    public DateTime? CheckInTime { get; set; }
    
    public DateTime? CheckOutTime { get; set; }
    
    public bool IsNoShow { get; set; } = false;
    
    [MaxLength(500)]
    public string? CancellationReason { get; set; }
    
    public DateTime? CancelledAt { get; set; }
    
    // Navigation properties
    [ForeignKey(nameof(DeskId))]
    public virtual Desk? Desk { get; set; }
    
    [ForeignKey(nameof(MeetingRoomId))]
    public virtual MeetingRoom? MeetingRoom { get; set; }
}

// ==================== USER ENTITIES ====================

[Table("Users")]
public class User : TenantEntity
{
    [Required]
    [MaxLength(100)]
    public string EmployeeId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? Department { get; set; }
    
    [MaxLength(100)]
    public string? JobTitle { get; set; }
    
    public Guid? ManagerId { get; set; }
    
    [MaxLength(10)]
    public string PreferredLanguage { get; set; } = "en";
    
    public bool IsActive { get; set; } = true;
    
    [Required]
    [MaxLength(100)]
    public string KeycloakUserId { get; set; } = string.Empty;
    
    // Navigation properties
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}

// ==================== CONSTANTS ====================

public static class BookingStatus
{
    public const string Pending = "Pending";
    public const string Confirmed = "Confirmed";
    public const string CheckedIn = "CheckedIn";
    public const string CheckedOut = "CheckedOut";
    public const string Cancelled = "Cancelled";
    public const string NoShow = "NoShow";
}

public static class ResourceType
{
    public const string Desk = "Desk";
    public const string MeetingRoom = "MeetingRoom";
}
