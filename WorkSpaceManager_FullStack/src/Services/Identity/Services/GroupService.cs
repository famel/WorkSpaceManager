using Microsoft.EntityFrameworkCore;
using WorkSpaceManager.IdentityService.Data;
using WorkSpaceManager.Shared.DTOs;
using WorkSpaceManager.Shared.Common;
using WorkSpaceManager.Shared.Models;

namespace WorkSpaceManager.IdentityService.Services;

public class GroupService : IGroupService
{
    private readonly IdentityDbContext _context;
    private readonly ILogger<GroupService> _logger;

    public GroupService(IdentityDbContext context, ILogger<GroupService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<GroupResponse>> GetGroupByIdAsync(Guid tenantId, Guid groupId, CancellationToken cancellationToken = default)
    {
        try
        {
            var group = await _context.Groups
                .Include(g => g.ParentGroup)
                .Include(g => g.Members)
                .Include(g => g.ChildGroups)
                .FirstOrDefaultAsync(g => g.Id == groupId && g.TenantId == tenantId, cancellationToken);

            if (group == null)
            {
                return ApiResponse<GroupResponse>.ErrorResponse("Group not found");
            }

            return ApiResponse<GroupResponse>.SuccessResponse(MapToResponse(group));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting group {GroupId}", groupId);
            return ApiResponse<GroupResponse>.ErrorResponse("Failed to get group", ex.Message);
        }
    }

    public async Task<PagedResponse<GroupResponse>> GetAllGroupsAsync(Guid tenantId, int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.Groups
                .Include(g => g.ParentGroup)
                .Include(g => g.Members)
                .Include(g => g.ChildGroups)
                .Where(g => g.TenantId == tenantId);

            var totalCount = await query.CountAsync(cancellationToken);

            var groups = await query
                .OrderBy(g => g.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PagedResponse<GroupResponse>(
                groups.Select(MapToResponse).ToList(),
                totalCount,
                pageNumber,
                pageSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all groups");
            return new PagedResponse<GroupResponse>();
        }
    }

    public async Task<ApiResponse<GroupResponse>> CreateGroupAsync(Guid tenantId, CreateGroupRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if group name already exists
            var existingGroup = await _context.Groups
                .FirstOrDefaultAsync(g => g.Name == request.Name && g.TenantId == tenantId, cancellationToken);

            if (existingGroup != null)
            {
                return ApiResponse<GroupResponse>.ErrorResponse("Group with this name already exists");
            }

            var group = new Group
            {
                TenantId = tenantId,
                Name = request.Name,
                Description = request.Description,
                ParentGroupId = request.ParentGroupId,
                IsActive = true
            };

            _context.Groups.Add(group);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Group created: {GroupId} - {GroupName}", group.Id, group.Name);

            return await GetGroupByIdAsync(tenantId, group.Id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating group");
            return ApiResponse<GroupResponse>.ErrorResponse("Failed to create group", ex.Message);
        }
    }

    public async Task<ApiResponse<GroupResponse>> UpdateGroupAsync(Guid tenantId, Guid groupId, UpdateGroupRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var group = await _context.Groups
                .FirstOrDefaultAsync(g => g.Id == groupId && g.TenantId == tenantId, cancellationToken);

            if (group == null)
            {
                return ApiResponse<GroupResponse>.ErrorResponse("Group not found");
            }

            if (!string.IsNullOrEmpty(request.Name))
            {
                var existingGroup = await _context.Groups
                    .FirstOrDefaultAsync(g => g.Name == request.Name && g.Id != groupId && g.TenantId == tenantId, cancellationToken);
                if (existingGroup != null)
                {
                    return ApiResponse<GroupResponse>.ErrorResponse("Group with this name already exists");
                }
                group.Name = request.Name;
            }

            if (request.Description != null)
                group.Description = request.Description;

            if (request.ParentGroupId.HasValue)
            {
                // Prevent circular references
                if (request.ParentGroupId == groupId)
                {
                    return ApiResponse<GroupResponse>.ErrorResponse("A group cannot be its own parent");
                }
                group.ParentGroupId = request.ParentGroupId;
            }

            if (request.IsActive.HasValue)
                group.IsActive = request.IsActive.Value;

            group.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Group updated: {GroupId}", groupId);

            return await GetGroupByIdAsync(tenantId, groupId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating group {GroupId}", groupId);
            return ApiResponse<GroupResponse>.ErrorResponse("Failed to update group", ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> DeleteGroupAsync(Guid tenantId, Guid groupId, CancellationToken cancellationToken = default)
    {
        try
        {
            var group = await _context.Groups
                .Include(g => g.ChildGroups)
                .FirstOrDefaultAsync(g => g.Id == groupId && g.TenantId == tenantId, cancellationToken);

            if (group == null)
            {
                return ApiResponse<bool>.ErrorResponse("Group not found");
            }

            if (group.ChildGroups.Any(cg => !cg.IsDeleted))
            {
                return ApiResponse<bool>.ErrorResponse("Cannot delete group with child groups");
            }

            group.IsDeleted = true;
            group.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Group deleted: {GroupId}", groupId);

            return ApiResponse<bool>.SuccessResponse(true, "Group deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting group {GroupId}", groupId);
            return ApiResponse<bool>.ErrorResponse("Failed to delete group", ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> AddMemberAsync(Guid tenantId, AddGroupMemberRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var existingMembership = await _context.GroupMembers
                .FirstOrDefaultAsync(gm => gm.GroupId == request.GroupId && gm.UserId == request.UserId, cancellationToken);

            if (existingMembership != null)
            {
                if (existingMembership.IsActive)
                {
                    return ApiResponse<bool>.ErrorResponse("User is already a member of this group");
                }
                existingMembership.IsActive = true;
                existingMembership.ExpiresAt = request.ExpiresAt;
                existingMembership.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                var groupMember = new GroupMember
                {
                    TenantId = tenantId,
                    GroupId = request.GroupId,
                    UserId = request.UserId,
                    IsActive = true,
                    ExpiresAt = request.ExpiresAt
                };
                _context.GroupMembers.Add(groupMember);
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User {UserId} added to group {GroupId}", request.UserId, request.GroupId);

            return ApiResponse<bool>.SuccessResponse(true, "Member added to group");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding user {UserId} to group {GroupId}", request.UserId, request.GroupId);
            return ApiResponse<bool>.ErrorResponse("Failed to add member to group", ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> RemoveMemberAsync(Guid tenantId, RemoveGroupMemberRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var groupMember = await _context.GroupMembers
                .FirstOrDefaultAsync(gm => gm.GroupId == request.GroupId && gm.UserId == request.UserId, cancellationToken);

            if (groupMember == null)
            {
                return ApiResponse<bool>.ErrorResponse("Membership not found");
            }

            groupMember.IsActive = false;
            groupMember.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User {UserId} removed from group {GroupId}", request.UserId, request.GroupId);

            return ApiResponse<bool>.SuccessResponse(true, "Member removed from group");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing user {UserId} from group {GroupId}", request.UserId, request.GroupId);
            return ApiResponse<bool>.ErrorResponse("Failed to remove member from group", ex.Message);
        }
    }

    public async Task<PagedResponse<GroupMemberResponse>> GetGroupMembersAsync(Guid tenantId, Guid groupId, int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.GroupMembers
                .Include(gm => gm.User)
                .Include(gm => gm.Group)
                .Where(gm => gm.GroupId == groupId && gm.TenantId == tenantId && gm.IsActive);

            var totalCount = await query.CountAsync(cancellationToken);

            var members = await query
                .OrderBy(gm => gm.User.LastName).ThenBy(gm => gm.User.FirstName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PagedResponse<GroupMemberResponse>(
                members.Select(gm => new GroupMemberResponse
                {
                    Id = gm.Id,
                    GroupId = gm.GroupId,
                    GroupName = gm.Group.Name,
                    UserId = gm.UserId,
                    UserName = $"{gm.User.FirstName} {gm.User.LastName}",
                    UserEmail = gm.User.Email,
                    IsActive = gm.IsActive,
                    ExpiresAt = gm.ExpiresAt,
                    CreatedAt = gm.CreatedAt
                }).ToList(),
                totalCount,
                pageNumber,
                pageSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting members for group {GroupId}", groupId);
            return new PagedResponse<GroupMemberResponse>();
        }
    }

    private static GroupResponse MapToResponse(Group group)
    {
        return new GroupResponse
        {
            Id = group.Id,
            Name = group.Name,
            Description = group.Description,
            ParentGroupId = group.ParentGroupId,
            ParentGroupName = group.ParentGroup?.Name,
            IsActive = group.IsActive,
            MemberCount = group.Members?.Count(m => m.IsActive) ?? 0,
            ChildGroupCount = group.ChildGroups?.Count(cg => !cg.IsDeleted) ?? 0,
            CreatedAt = group.CreatedAt,
            UpdatedAt = group.UpdatedAt
        };
    }
}
