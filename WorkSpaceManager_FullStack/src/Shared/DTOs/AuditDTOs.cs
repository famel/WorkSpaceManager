using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WorkSpaceManager.Shared.DTOs;

// ==================== AUDIT LOG DTOs ====================

public class AuditLogResponse
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public string? ResourceId { get; set; }
    public bool Success { get; set; }
    public string? Details { get; set; }
    public string? ErrorMessage { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public long? DurationMs { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateAuditLogRequest
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
    
    public Dictionary<string, object>? Metadata { get; set; }
}

public class AuditLogSearchRequest
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public string? Action { get; set; }
    public string? Resource { get; set; }
    public string? ResourceId { get; set; }
    public bool? Success { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public class AuditLogSearchResponse
{
    public List<AuditLogResponse> Logs { get; set; } = new();
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
}

public class AuditStatisticsResponse
{
    public int TotalLogs { get; set; }
    public int SuccessfulOperations { get; set; }
    public int FailedOperations { get; set; }
    public double SuccessRate { get; set; }
    public Dictionary<string, int> ActionCounts { get; set; } = new();
    public Dictionary<string, int> ResourceCounts { get; set; } = new();
    public Dictionary<string, int> UserActivityCounts { get; set; } = new();
    public Dictionary<string, int> HourlyDistribution { get; set; } = new();
    public DateTime? FirstLogDate { get; set; }
    public DateTime? LastLogDate { get; set; }
}

public class AuditExportRequest
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid? UserId { get; set; }
    public string? Action { get; set; }
    public string? Resource { get; set; }
    public bool? Success { get; set; }
    
    [Required]
    public string Format { get; set; } = "csv"; // csv, json, xlsx
}
