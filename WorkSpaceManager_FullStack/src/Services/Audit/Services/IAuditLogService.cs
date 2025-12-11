using WorkSpaceManager.Shared.DTOs;
using WorkSpaceManager.Shared.Common;

namespace WorkSpaceManager.AuditService.Services;

public interface IAuditLogService
{
    Task<ApiResponse<AuditLogResponse>> CreateAuditLogAsync(Guid tenantId, CreateAuditLogRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<AuditLogResponse>> GetAuditLogByIdAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default);
    Task<AuditLogSearchResponse> SearchAuditLogsAsync(Guid tenantId, AuditLogSearchRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<AuditStatisticsResponse>> GetStatisticsAsync(Guid tenantId, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteAuditLogsAsync(Guid tenantId, DateTime beforeDate, CancellationToken cancellationToken = default);
}
