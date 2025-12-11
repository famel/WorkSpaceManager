using Microsoft.EntityFrameworkCore;
using WorkSpaceManager.IdentityService.Data;
using WorkSpaceManager.Shared.DTOs;
using WorkSpaceManager.Shared.Common;
using WorkSpaceManager.Shared.Models;

namespace WorkSpaceManager.IdentityService.Services;

public class RoleService : IRoleService
{
    private readonly IdentityDbContext _context;
    private readonly ILogger<RoleService> _logger;

    public RoleService(IdentityDbContext context, ILogger<RoleService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<RoleResponse>> GetRoleByIdAsync(Guid tenantId, Guid roleId, CancellationToken cancellationToken = default)
    {
        try
        {
            var role = await _context.Roles
                .Include(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
                .Include(r => r.UserRoles)
                .FirstOrDefaultAsync(r => r.Id == roleId && r.TenantId == tenantId, cancellationToken);

            if (role == null)
            {
                return ApiResponse<RoleResponse>.ErrorResponse("Role not found");
            }

            return ApiResponse<RoleResponse>.SuccessResponse(MapToResponse(role));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting role {RoleId}", roleId);
            return ApiResponse<RoleResponse>.ErrorResponse("Failed to get role", ex.Message);
        }
    }

    public async Task<PagedResponse<RoleResponse>> GetAllRolesAsync(Guid tenantId, int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.Roles
                .Include(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
                .Include(r => r.UserRoles)
                .Where(r => r.TenantId == tenantId);

            var totalCount = await query.CountAsync(cancellationToken);

            var roles = await query
                .OrderBy(r => r.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PagedResponse<RoleResponse>(
                roles.Select(MapToResponse).ToList(),
                totalCount,
                pageNumber,
                pageSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all roles");
            return new PagedResponse<RoleResponse>();
        }
    }

    public async Task<ApiResponse<RoleResponse>> CreateRoleAsync(Guid tenantId, CreateRoleRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if role name already exists
            var existingRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == request.Name && r.TenantId == tenantId, cancellationToken);

            if (existingRole != null)
            {
                return ApiResponse<RoleResponse>.ErrorResponse("Role with this name already exists");
            }

            var role = new Role
            {
                TenantId = tenantId,
                Name = request.Name,
                Description = request.Description,
                IsActive = true,
                IsSystemRole = false
            };

            _context.Roles.Add(role);
            await _context.SaveChangesAsync(cancellationToken);

            // Add permissions if specified
            if (request.PermissionIds?.Any() == true)
            {
                foreach (var permissionId in request.PermissionIds)
                {
                    var rolePermission = new RolePermission
                    {
                        TenantId = tenantId,
                        RoleId = role.Id,
                        PermissionId = permissionId,
                        IsActive = true
                    };
                    _context.RolePermissions.Add(rolePermission);
                }
                await _context.SaveChangesAsync(cancellationToken);
            }

            _logger.LogInformation("Role created: {RoleId} - {RoleName}", role.Id, role.Name);

            return await GetRoleByIdAsync(tenantId, role.Id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating role");
            return ApiResponse<RoleResponse>.ErrorResponse("Failed to create role", ex.Message);
        }
    }

    public async Task<ApiResponse<RoleResponse>> UpdateRoleAsync(Guid tenantId, Guid roleId, UpdateRoleRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var role = await _context.Roles
                .FirstOrDefaultAsync(r => r.Id == roleId && r.TenantId == tenantId, cancellationToken);

            if (role == null)
            {
                return ApiResponse<RoleResponse>.ErrorResponse("Role not found");
            }

            if (role.IsSystemRole)
            {
                return ApiResponse<RoleResponse>.ErrorResponse("Cannot modify system roles");
            }

            if (!string.IsNullOrEmpty(request.Name))
            {
                // Check if new name conflicts
                var existingRole = await _context.Roles
                    .FirstOrDefaultAsync(r => r.Name == request.Name && r.Id != roleId && r.TenantId == tenantId, cancellationToken);
                if (existingRole != null)
                {
                    return ApiResponse<RoleResponse>.ErrorResponse("Role with this name already exists");
                }
                role.Name = request.Name;
            }

            if (request.Description != null)
                role.Description = request.Description;

            if (request.IsActive.HasValue)
                role.IsActive = request.IsActive.Value;

            role.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Role updated: {RoleId}", roleId);

            return await GetRoleByIdAsync(tenantId, roleId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role {RoleId}", roleId);
            return ApiResponse<RoleResponse>.ErrorResponse("Failed to update role", ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> DeleteRoleAsync(Guid tenantId, Guid roleId, CancellationToken cancellationToken = default)
    {
        try
        {
            var role = await _context.Roles
                .FirstOrDefaultAsync(r => r.Id == roleId && r.TenantId == tenantId, cancellationToken);

            if (role == null)
            {
                return ApiResponse<bool>.ErrorResponse("Role not found");
            }

            if (role.IsSystemRole)
            {
                return ApiResponse<bool>.ErrorResponse("Cannot delete system roles");
            }

            role.IsDeleted = true;
            role.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Role deleted: {RoleId}", roleId);

            return ApiResponse<bool>.SuccessResponse(true, "Role deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting role {RoleId}", roleId);
            return ApiResponse<bool>.ErrorResponse("Failed to delete role", ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> AddPermissionToRoleAsync(Guid tenantId, Guid roleId, Guid permissionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var existingRolePermission = await _context.RolePermissions
                .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId, cancellationToken);

            if (existingRolePermission != null)
            {
                if (existingRolePermission.IsActive)
                {
                    return ApiResponse<bool>.ErrorResponse("Permission already assigned to role");
                }
                existingRolePermission.IsActive = true;
                existingRolePermission.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                var rolePermission = new RolePermission
                {
                    TenantId = tenantId,
                    RoleId = roleId,
                    PermissionId = permissionId,
                    IsActive = true
                };
                _context.RolePermissions.Add(rolePermission);
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Permission {PermissionId} added to role {RoleId}", permissionId, roleId);

            return ApiResponse<bool>.SuccessResponse(true, "Permission added to role");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding permission {PermissionId} to role {RoleId}", permissionId, roleId);
            return ApiResponse<bool>.ErrorResponse("Failed to add permission to role", ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> RemovePermissionFromRoleAsync(Guid tenantId, Guid roleId, Guid permissionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var rolePermission = await _context.RolePermissions
                .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId, cancellationToken);

            if (rolePermission == null)
            {
                return ApiResponse<bool>.ErrorResponse("Permission not assigned to role");
            }

            rolePermission.IsActive = false;
            rolePermission.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Permission {PermissionId} removed from role {RoleId}", permissionId, roleId);

            return ApiResponse<bool>.SuccessResponse(true, "Permission removed from role");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing permission {PermissionId} from role {RoleId}", permissionId, roleId);
            return ApiResponse<bool>.ErrorResponse("Failed to remove permission from role", ex.Message);
        }
    }

    private static RoleResponse MapToResponse(Role role)
    {
        return new RoleResponse
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            IsActive = role.IsActive,
            IsSystemRole = role.IsSystemRole,
            UserCount = role.UserRoles?.Count(ur => ur.IsActive) ?? 0,
            PermissionCount = role.RolePermissions?.Count(rp => rp.IsActive) ?? 0,
            CreatedAt = role.CreatedAt,
            UpdatedAt = role.UpdatedAt,
            Permissions = role.RolePermissions?.Where(rp => rp.IsActive).Select(rp => new PermissionResponse
            {
                Id = rp.Permission.Id,
                Name = rp.Permission.Name,
                Description = rp.Permission.Description,
                Resource = rp.Permission.Resource,
                Action = rp.Permission.Action,
                IsActive = rp.Permission.IsActive,
                CreatedAt = rp.Permission.CreatedAt
            }).ToList() ?? new List<PermissionResponse>()
        };
    }
}
