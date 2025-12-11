using Microsoft.EntityFrameworkCore;
using WorkSpaceManager.IdentityService.Data;
using WorkSpaceManager.Shared.DTOs;
using WorkSpaceManager.Shared.Common;
using WorkSpaceManager.Shared.Models;

namespace WorkSpaceManager.IdentityService.Services;

public class UserService : IUserService
{
    private readonly IdentityDbContext _context;
    private readonly ILogger<UserService> _logger;

    public UserService(IdentityDbContext context, ILogger<UserService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<UserResponse>> GetUserByIdAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .Include(u => u.GroupMemberships).ThenInclude(gm => gm.Group)
                .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId, cancellationToken);

            if (user == null)
            {
                return ApiResponse<UserResponse>.ErrorResponse("User not found");
            }

            return ApiResponse<UserResponse>.SuccessResponse(MapToResponse(user));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user {UserId}", userId);
            return ApiResponse<UserResponse>.ErrorResponse("Failed to get user", ex.Message);
        }
    }

    public async Task<ApiResponse<UserResponse>> GetUserByEmailAsync(Guid tenantId, string email, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .Include(u => u.GroupMemberships).ThenInclude(gm => gm.Group)
                .FirstOrDefaultAsync(u => u.Email == email && u.TenantId == tenantId, cancellationToken);

            if (user == null)
            {
                return ApiResponse<UserResponse>.ErrorResponse("User not found");
            }

            return ApiResponse<UserResponse>.SuccessResponse(MapToResponse(user));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by email {Email}", email);
            return ApiResponse<UserResponse>.ErrorResponse("Failed to get user", ex.Message);
        }
    }

    public async Task<PagedResponse<UserResponse>> SearchUsersAsync(Guid tenantId, UserSearchRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .Include(u => u.GroupMemberships).ThenInclude(gm => gm.Group)
                .Where(u => u.TenantId == tenantId);

            // Apply filters
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.ToLower();
                query = query.Where(u => 
                    u.FirstName.ToLower().Contains(term) ||
                    u.LastName.ToLower().Contains(term) ||
                    u.Email.ToLower().Contains(term) ||
                    u.EmployeeId.ToLower().Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(request.Department))
            {
                query = query.Where(u => u.Department == request.Department);
            }

            if (request.IsActive.HasValue)
            {
                query = query.Where(u => u.IsActive == request.IsActive.Value);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var users = await query
                .OrderBy(u => u.LastName).ThenBy(u => u.FirstName)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedResponse<UserResponse>(
                users.Select(MapToResponse).ToList(),
                totalCount,
                request.PageNumber,
                request.PageSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching users");
            return new PagedResponse<UserResponse>();
        }
    }

    public async Task<ApiResponse<UserResponse>> CreateUserAsync(Guid tenantId, CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if email already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email && u.TenantId == tenantId, cancellationToken);

            if (existingUser != null)
            {
                return ApiResponse<UserResponse>.ErrorResponse("User with this email already exists");
            }

            var user = new User
            {
                TenantId = tenantId,
                EmployeeId = request.EmployeeId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PasswordHash = !string.IsNullOrEmpty(request.Password) 
                    ? BCrypt.Net.BCrypt.HashPassword(request.Password) 
                    : null,
                Department = request.Department,
                JobTitle = request.JobTitle,
                PhoneNumber = request.PhoneNumber,
                ManagerId = request.ManagerId,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);

            // Assign roles if specified
            if (request.RoleIds?.Any() == true)
            {
                foreach (var roleId in request.RoleIds)
                {
                    var userRole = new UserRole
                    {
                        TenantId = tenantId,
                        UserId = user.Id,
                        RoleId = roleId,
                        IsActive = true
                    };
                    _context.UserRoles.Add(userRole);
                }
                await _context.SaveChangesAsync(cancellationToken);
            }

            // Add to groups if specified
            if (request.GroupIds?.Any() == true)
            {
                foreach (var groupId in request.GroupIds)
                {
                    var groupMember = new GroupMember
                    {
                        TenantId = tenantId,
                        GroupId = groupId,
                        UserId = user.Id,
                        IsActive = true
                    };
                    _context.GroupMembers.Add(groupMember);
                }
                await _context.SaveChangesAsync(cancellationToken);
            }

            _logger.LogInformation("User created: {UserId} - {Email}", user.Id, user.Email);

            // Reload with includes
            var result = await GetUserByIdAsync(tenantId, user.Id, cancellationToken);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return ApiResponse<UserResponse>.ErrorResponse("Failed to create user", ex.Message);
        }
    }

    public async Task<ApiResponse<UserResponse>> UpdateUserAsync(Guid tenantId, Guid userId, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId, cancellationToken);

            if (user == null)
            {
                return ApiResponse<UserResponse>.ErrorResponse("User not found");
            }

            // Update fields if provided
            if (!string.IsNullOrEmpty(request.FirstName))
                user.FirstName = request.FirstName;
            
            if (!string.IsNullOrEmpty(request.LastName))
                user.LastName = request.LastName;
            
            if (!string.IsNullOrEmpty(request.Email))
            {
                // Check if email is already taken
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == request.Email && u.Id != userId && u.TenantId == tenantId, cancellationToken);
                if (existingUser != null)
                {
                    return ApiResponse<UserResponse>.ErrorResponse("Email is already in use");
                }
                user.Email = request.Email;
            }

            if (request.Department != null)
                user.Department = request.Department;
            
            if (request.JobTitle != null)
                user.JobTitle = request.JobTitle;
            
            if (request.PhoneNumber != null)
                user.PhoneNumber = request.PhoneNumber;
            
            if (request.ManagerId.HasValue)
                user.ManagerId = request.ManagerId;
            
            if (!string.IsNullOrEmpty(request.PreferredLanguage))
                user.PreferredLanguage = request.PreferredLanguage;
            
            if (request.IsActive.HasValue)
                user.IsActive = request.IsActive.Value;

            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User updated: {UserId}", userId);

            return await GetUserByIdAsync(tenantId, userId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", userId);
            return ApiResponse<UserResponse>.ErrorResponse("Failed to update user", ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> DeleteUserAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId, cancellationToken);

            if (user == null)
            {
                return ApiResponse<bool>.ErrorResponse("User not found");
            }

            user.IsDeleted = true;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User deleted: {UserId}", userId);

            return ApiResponse<bool>.SuccessResponse(true, "User deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", userId);
            return ApiResponse<bool>.ErrorResponse("Failed to delete user", ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> AssignRoleAsync(Guid tenantId, AssignRoleRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if role assignment already exists
            var existingAssignment = await _context.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == request.UserId && ur.RoleId == request.RoleId && ur.TenantId == tenantId, cancellationToken);

            if (existingAssignment != null)
            {
                if (existingAssignment.IsActive)
                {
                    return ApiResponse<bool>.ErrorResponse("Role is already assigned to user");
                }
                
                // Reactivate existing assignment
                existingAssignment.IsActive = true;
                existingAssignment.ExpiresAt = request.ExpiresAt;
                existingAssignment.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                var userRole = new UserRole
                {
                    TenantId = tenantId,
                    UserId = request.UserId,
                    RoleId = request.RoleId,
                    IsActive = true,
                    ExpiresAt = request.ExpiresAt
                };
                _context.UserRoles.Add(userRole);
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Role {RoleId} assigned to user {UserId}", request.RoleId, request.UserId);

            return ApiResponse<bool>.SuccessResponse(true, "Role assigned successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning role {RoleId} to user {UserId}", request.RoleId, request.UserId);
            return ApiResponse<bool>.ErrorResponse("Failed to assign role", ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> RevokeRoleAsync(Guid tenantId, RevokeRoleRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var userRole = await _context.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == request.UserId && ur.RoleId == request.RoleId && ur.TenantId == tenantId, cancellationToken);

            if (userRole == null)
            {
                return ApiResponse<bool>.ErrorResponse("Role assignment not found");
            }

            userRole.IsActive = false;
            userRole.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Role {RoleId} revoked from user {UserId}", request.RoleId, request.UserId);

            return ApiResponse<bool>.SuccessResponse(true, "Role revoked successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking role {RoleId} from user {UserId}", request.RoleId, request.UserId);
            return ApiResponse<bool>.ErrorResponse("Failed to revoke role", ex.Message);
        }
    }

    public async Task<List<string>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var roles = await _context.UserRoles
                .Include(ur => ur.Role)
                .Where(ur => ur.UserId == userId && ur.IsActive && (ur.ExpiresAt == null || ur.ExpiresAt > DateTime.UtcNow))
                .Select(ur => ur.Role.Name)
                .ToListAsync(cancellationToken);

            return roles.Any() ? roles : new List<string> { SystemRoles.User };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting roles for user {UserId}", userId);
            return new List<string> { SystemRoles.User };
        }
    }

    private static UserResponse MapToResponse(User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            EmployeeId = user.EmployeeId,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Department = user.Department,
            JobTitle = user.JobTitle,
            PhoneNumber = user.PhoneNumber,
            ManagerId = user.ManagerId,
            PreferredLanguage = user.PreferredLanguage,
            IsActive = user.IsActive,
            LastLoginDate = user.LastLoginDate,
            FailedLoginAttempts = user.FailedLoginAttempts,
            LockoutEndDate = user.LockoutEndDate,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            Roles = user.UserRoles?.Where(ur => ur.IsActive).Select(ur => new RoleResponse
            {
                Id = ur.Role.Id,
                Name = ur.Role.Name,
                Description = ur.Role.Description,
                IsActive = ur.Role.IsActive,
                IsSystemRole = ur.Role.IsSystemRole
            }).ToList() ?? new List<RoleResponse>(),
            Groups = user.GroupMemberships?.Where(gm => gm.IsActive).Select(gm => new GroupResponse
            {
                Id = gm.Group.Id,
                Name = gm.Group.Name,
                Description = gm.Group.Description,
                IsActive = gm.Group.IsActive
            }).ToList() ?? new List<GroupResponse>()
        };
    }
}
