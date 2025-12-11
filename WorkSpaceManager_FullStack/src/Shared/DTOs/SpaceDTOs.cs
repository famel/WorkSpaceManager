using System;
using System.ComponentModel.DataAnnotations;

namespace WorkSpaceManager.Shared.DTOs;

// ==================== BUILDING DTOs ====================

public class CreateBuildingRequest
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Address { get; set; }
    
    public int TotalFloors { get; set; }
}

public class UpdateBuildingRequest
{
    [MaxLength(200)]
    public string? Name { get; set; }
    
    [MaxLength(500)]
    public string? Address { get; set; }
    
    public int? TotalFloors { get; set; }
    
    public bool? IsActive { get; set; }
}

public class BuildingResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public int TotalFloors { get; set; }
    public bool IsActive { get; set; }
    public int FloorsCount { get; set; }
    public int TotalDesks { get; set; }
    public int TotalMeetingRooms { get; set; }
    public DateTime CreatedAt { get; set; }
}

// ==================== FLOOR DTOs ====================

public class CreateFloorRequest
{
    [Required]
    public Guid BuildingId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    public int FloorNumber { get; set; }
    
    [MaxLength(1000)]
    public string? FloorPlanUrl { get; set; }
}

public class UpdateFloorRequest
{
    [MaxLength(100)]
    public string? Name { get; set; }
    
    public int? FloorNumber { get; set; }
    
    [MaxLength(1000)]
    public string? FloorPlanUrl { get; set; }
    
    public bool? IsActive { get; set; }
}

public class FloorResponse
{
    public Guid Id { get; set; }
    public Guid BuildingId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int FloorNumber { get; set; }
    public int TotalDesks { get; set; }
    public int TotalMeetingRooms { get; set; }
    public string? FloorPlanUrl { get; set; }
    public bool IsActive { get; set; }
    public string? BuildingName { get; set; }
    public DateTime CreatedAt { get; set; }
}

// ==================== DESK DTOs ====================

public class CreateDeskRequest
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
    
    [MaxLength(500)]
    public string? Notes { get; set; }
}

public class UpdateDeskRequest
{
    [MaxLength(50)]
    public string? DeskNumber { get; set; }
    
    [MaxLength(100)]
    public string? Location { get; set; }
    
    public bool? HasMonitor { get; set; }
    public bool? HasDockingStation { get; set; }
    public bool? IsNearWindow { get; set; }
    public bool? IsAccessible { get; set; }
    public bool? IsAvailable { get; set; }
    
    [MaxLength(500)]
    public string? Notes { get; set; }
}

public class DeskResponse
{
    public Guid Id { get; set; }
    public Guid FloorId { get; set; }
    public string DeskNumber { get; set; } = string.Empty;
    public string? Location { get; set; }
    public bool HasMonitor { get; set; }
    public bool HasDockingStation { get; set; }
    public bool IsNearWindow { get; set; }
    public bool IsAccessible { get; set; }
    public bool IsAvailable { get; set; }
    public string? Notes { get; set; }
    public string? FloorName { get; set; }
    public string? BuildingName { get; set; }
    public DateTime CreatedAt { get; set; }
}

// ==================== MEETING ROOM DTOs ====================

public class CreateMeetingRoomRequest
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
    
    [MaxLength(500)]
    public string? Equipment { get; set; }
    
    [MaxLength(500)]
    public string? Notes { get; set; }
}

public class UpdateMeetingRoomRequest
{
    [MaxLength(100)]
    public string? Name { get; set; }
    
    [MaxLength(50)]
    public string? RoomNumber { get; set; }
    
    public int? Capacity { get; set; }
    
    public bool? HasProjector { get; set; }
    public bool? HasWhiteboard { get; set; }
    public bool? HasVideoConference { get; set; }
    public bool? HasTelephone { get; set; }
    public bool? IsAccessible { get; set; }
    public bool? IsAvailable { get; set; }
    
    [MaxLength(500)]
    public string? Equipment { get; set; }
    
    [MaxLength(500)]
    public string? Notes { get; set; }
}

public class MeetingRoomResponse
{
    public Guid Id { get; set; }
    public Guid FloorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string RoomNumber { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public bool HasProjector { get; set; }
    public bool HasWhiteboard { get; set; }
    public bool HasVideoConference { get; set; }
    public bool HasTelephone { get; set; }
    public bool IsAccessible { get; set; }
    public bool IsAvailable { get; set; }
    public string? Equipment { get; set; }
    public string? Notes { get; set; }
    public string? FloorName { get; set; }
    public string? BuildingName { get; set; }
    public DateTime CreatedAt { get; set; }
}

// ==================== SEARCH DTOs ====================

public class SpaceSearchRequest
{
    public Guid? BuildingId { get; set; }
    public Guid? FloorId { get; set; }
    public bool? IsAvailable { get; set; }
    public bool? IsAccessible { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class DeskSearchRequest : SpaceSearchRequest
{
    public bool? HasMonitor { get; set; }
    public bool? HasDockingStation { get; set; }
    public bool? IsNearWindow { get; set; }
}

public class MeetingRoomSearchRequest : SpaceSearchRequest
{
    public int? MinCapacity { get; set; }
    public bool? HasProjector { get; set; }
    public bool? HasWhiteboard { get; set; }
    public bool? HasVideoConference { get; set; }
}
