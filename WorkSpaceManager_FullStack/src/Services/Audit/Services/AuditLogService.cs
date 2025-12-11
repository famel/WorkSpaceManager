using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using WorkSpaceManager.AuditService.Data;
using WorkSpaceManager.Shared.DTOs;
using WorkSpaceManager.Shared.Common;
using WorkSpaceManager.Shared.Models;

namespace WorkSpaceManager.AuditService.Services;

public class AuditLogService : IAuditLogService
{
    private readonly AuditDbContext _context;
    private readonly ILogger<AuditLogService> _logger;

    public AuditLogService(AuditDbContext context, ILogger<AuditLogService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<AuditLogResponse>> CreateAuditLogAsync(Guid tenantId, CreateAuditLogRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var auditLog = new AuditLog
            {
                TenantId = tenantId,
                UserId = request.UserId,
                UserName = request.UserName,
                Action = request.Action,
                Resource = request.Resource,
                ResourceId = request.ResourceId,
                Success = request.Success,
                Details = request.Details,
                ErrorMessage = request.ErrorMessage,
                IpAddress = request.IpAddress,
                UserAgent = request.UserAgent,
                DurationMs = request.DurationMs,
                Metadata = request.Metadata != null ? JsonSerializer.Serialize(request.Metadata) : null
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Audit log created: {Action} on {Resource} by user {UserId}", 
                request.Action, request.Resource, request.UserId);

            return ApiResponse<AuditLogResponse>.SuccessResponse(MapToResponse(auditLog), "Audit log created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating audit log");
            return ApiResponse<AuditLogResponse>.ErrorResponse("Failed to create audit log", ex.Message);
        }
    }

    public async Task<ApiResponse<AuditLogResponse>> GetAuditLogByIdAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var auditLog = await _context.AuditLogs
                .FirstOrDefaultAsync(a => a.Id == id && a.TenantId == tenantId, cancellationToken);

            if (auditLog == null)
            {
                return ApiResponse<AuditLogResponse>.ErrorResponse("Audit log not found");
            }

            return ApiResponse<AuditLogResponse>.SuccessResponse(MapToResponse(auditLog));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit log {Id}", id);
            return ApiResponse<AuditLogResponse>.ErrorResponse("Failed to retrieve audit log", ex.Message);
        }
    }

    public async Task<AuditLogSearchResponse> SearchAuditLogsAsync(Guid tenantId, AuditLogSearchRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.AuditLogs.Where(a => a.TenantId == tenantId);

            // Apply filters
            if (request.StartDate.HasValue)
                query = query.Where(a => a.CreatedAt >= request.StartDate.Value);

            if (request.EndDate.HasValue)
                query = query.Where(a => a.CreatedAt <= request.EndDate.Value);

            if (request.UserId.HasValue)
                query = query.Where(a => a.UserId == request.UserId.Value);

            if (!string.IsNullOrWhiteSpace(request.UserName))
                query = query.Where(a => a.UserName != null && a.UserName.Contains(request.UserName));

            if (!string.IsNullOrWhiteSpace(request.Action))
                query = query.Where(a => a.Action == request.Action);

            if (!string.IsNullOrWhiteSpace(request.Resource))
                query = query.Where(a => a.Resource.Contains(request.Resource));

            if (!string.IsNullOrWhiteSpace(request.ResourceId))
                query = query.Where(a => a.ResourceId == request.ResourceId);

            if (request.Success.HasValue)
                query = query.Where(a => a.Success == request.Success.Value);

            // Get total count
            var totalRecords = await query.CountAsync(cancellationToken);

            // Apply pagination
            var pageSize = Math.Min(Math.Max(request.PageSize, 1), 1000);
            var pageNumber = Math.Max(request.PageNumber, 1);
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            var logs = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new AuditLogSearchResponse
            {
                Logs = logs.Select(MapToResponse).ToList(),
                TotalRecords = totalRecords,
                TotalPages = totalPages,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                HasNextPage = pageNumber < totalPages,
                HasPreviousPage = pageNumber > 1
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching audit logs");
            return new AuditLogSearchResponse();
        }
    }

    public async Task<ApiResponse<AuditStatisticsResponse>> GetStatisticsAsync(Guid tenantId, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.AuditLogs.Where(a => a.TenantId == tenantId);

            if (startDate.HasValue)
                query = query.Where(a => a.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(a => a.CreatedAt <= endDate.Value);

            var logs = await query.ToListAsync(cancellationToken);

            var stats = new AuditStatisticsResponse
            {
                TotalLogs = logs.Count,
                SuccessfulOperations = logs.Count(l => l.Success),
                FailedOperations = logs.Count(l => !l.Success),
                SuccessRate = logs.Count > 0 ? Math.Round(logs.Count(l => l.Success) * 100.0 / logs.Count, 2) : 0,
                ActionCounts = logs.GroupBy(l => l.Action).ToDictionary(g => g.Key, g => g.Count()),
                ResourceCounts = logs.GroupBy(l => l.Resource).ToDictionary(g => g.Key, g => g.Count()),
                UserActivityCounts = logs.Where(l => l.UserName != null).GroupBy(l => l.UserName!).ToDictionary(g => g.Key, g => g.Count()),
                HourlyDistribution = logs.GroupBy(l => l.CreatedAt.Hour.ToString("00")).ToDictionary(g => g.Key, g => g.Count()),
                FirstLogDate = logs.Min(l => (DateTime?)l.CreatedAt),
                LastLogDate = logs.Max(l => (DateTime?)l.CreatedAt)
            };

            return ApiResponse<AuditStatisticsResponse>.SuccessResponse(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audit statistics");
            return ApiResponse<AuditStatisticsResponse>.ErrorResponse("Failed to get statistics", ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> DeleteAuditLogsAsync(Guid tenantId, DateTime beforeDate, CancellationToken cancellationToken = default)
    {
        try
        {
            var logsToDelete = await _context.AuditLogs
                .Where(a => a.TenantId == tenantId && a.CreatedAt < beforeDate)
                .ToListAsync(cancellationToken);

            var count = logsToDelete.Count;
            
            foreach (var log in logsToDelete)
            {
                log.IsDeleted = true;
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Deleted {Count} audit logs before {Date}", count, beforeDate);

            return ApiResponse<bool>.SuccessResponse(true, $"Deleted {count} audit logs");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting audit logs");
            return ApiResponse<bool>.ErrorResponse("Failed to delete audit logs", ex.Message);
        }
    }

    private static AuditLogResponse MapToResponse(AuditLog auditLog)
    {
        return new AuditLogResponse
        {
            Id = auditLog.Id,
            UserId = auditLog.UserId,
            UserName = auditLog.UserName,
            Action = auditLog.Action,
            Resource = auditLog.Resource,
            ResourceId = auditLog.ResourceId,
            Success = auditLog.Success,
            Details = auditLog.Details,
            ErrorMessage = auditLog.ErrorMessage,
            IpAddress = auditLog.IpAddress,
            UserAgent = auditLog.UserAgent,
            DurationMs = auditLog.DurationMs,
            Metadata = !string.IsNullOrEmpty(auditLog.Metadata) 
                ? JsonSerializer.Deserialize<Dictionary<string, object>>(auditLog.Metadata) 
                : null,
            CreatedAt = auditLog.CreatedAt
        };
    }
}
