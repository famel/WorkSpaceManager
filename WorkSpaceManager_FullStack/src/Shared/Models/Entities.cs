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
    
    [MaxLength(500)]
    public string? PasswordHash { get; set; }
    
    [MaxLength(100)]
    public string? Department { get; set; }
    
    [MaxLength(100)]
    public string? JobTitle { get; set; }
    
    public Guid? ManagerId { get; set; }
    
    [MaxLength(10)]
    public string PreferredLanguage { get; set; } = "en";
    
    public bool IsActive { get; set; } = true;
    
    [MaxLength(100)]
    public string? KeycloakUserId { get; set; }
    
    public DateTime? LastLoginDate { get; set; }
    
    public int FailedLoginAttempts { get; set; } = 0;
    
    public DateTime? LockoutEndDate { get; set; }
    
    [MaxLength(50)]
    public string? PhoneNumber { get; set; }
    
    // Navigation properties
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public virtual ICollection<GroupMember> GroupMemberships { get; set; } = new List<GroupMember>();
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}

// ==================== IDENTITY ENTITIES ====================

[Table("Roles")]
public class Role : TenantEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public bool IsSystemRole { get; set; } = false;
    
    // Navigation properties
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

[Table("UserRoles")]
public class UserRole : TenantEntity
{
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    public Guid RoleId { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime? ExpiresAt { get; set; }
    
    // Navigation properties
    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;
    
    [ForeignKey(nameof(RoleId))]
    public virtual Role Role { get; set; } = null!;
}

[Table("Permissions")]
public class Permission : TenantEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Resource { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string Action { get; set; } = string.Empty;
    
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

[Table("RolePermissions")]
public class RolePermission : TenantEntity
{
    [Required]
    public Guid RoleId { get; set; }
    
    [Required]
    public Guid PermissionId { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    [ForeignKey(nameof(RoleId))]
    public virtual Role Role { get; set; } = null!;
    
    [ForeignKey(nameof(PermissionId))]
    public virtual Permission Permission { get; set; } = null!;
}

[Table("Groups")]
public class Group : TenantEntity
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public Guid? ParentGroupId { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    [ForeignKey(nameof(ParentGroupId))]
    public virtual Group? ParentGroup { get; set; }
    
    public virtual ICollection<Group> ChildGroups { get; set; } = new List<Group>();
    public virtual ICollection<GroupMember> Members { get; set; } = new List<GroupMember>();
}

[Table("GroupMembers")]
public class GroupMember : TenantEntity
{
    [Required]
    public Guid GroupId { get; set; }
    
    [Required]
    public Guid UserId { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime? ExpiresAt { get; set; }
    
    // Navigation properties
    [ForeignKey(nameof(GroupId))]
    public virtual Group Group { get; set; } = null!;
    
    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;
}

[Table("RefreshTokens")]
public class RefreshToken : TenantEntity
{
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string Token { get; set; } = string.Empty;
    
    [Required]
    public DateTime ExpiresAt { get; set; }
    
    public bool IsRevoked { get; set; } = false;
    
    public DateTime? RevokedAt { get; set; }
    
    [MaxLength(500)]
    public string? ReplacedByToken { get; set; }
    
    [MaxLength(50)]
    public string? CreatedByIp { get; set; }
    
    [MaxLength(50)]
    public string? RevokedByIp { get; set; }
    
    // Navigation properties
    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;
}

// ==================== AUDIT ENTITIES ====================

[Table("AuditLogs")]
public class AuditLog : TenantEntity
{
    public Guid? UserId { get; set; }
    
    [MaxLength(255)]
    public string? UserName { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Action { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string Resource { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? ResourceId { get; set; }
    
    public bool Success { get; set; } = true;
    
    [MaxLength(500)]
    public string? Details { get; set; }
    
    [MaxLength(500)]
    public string? ErrorMessage { get; set; }
    
    [MaxLength(50)]
    public string? IpAddress { get; set; }
    
    [MaxLength(500)]
    public string? UserAgent { get; set; }
    
    public long? DurationMs { get; set; }
    
    public string? Metadata { get; set; } // JSON serialized additional data
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

public static class AuditActions
{
    public const string Create = "Create";
    public const string Read = "Read";
    public const string Update = "Update";
    public const string Delete = "Delete";
    public const string Login = "Login";
    public const string Logout = "Logout";
    public const string PasswordChange = "PasswordChange";
    public const string PasswordReset = "PasswordReset";
    public const string RoleAssign = "RoleAssign";
    public const string RoleRevoke = "RoleRevoke";
    public const string CheckIn = "CheckIn";
    public const string CheckOut = "CheckOut";
}

public static class SystemRoles
{
    public const string Admin = "Admin";
    public const string FacilityManager = "FacilityManager";
    public const string User = "User";
}
