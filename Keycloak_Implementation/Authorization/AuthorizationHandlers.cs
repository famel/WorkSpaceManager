using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WorkSpaceManager.Identity.Data;
using WorkSpaceManager.Identity.Services;
using static WorkSpaceManager.Identity.Authorization.Requirements;

namespace WorkSpaceManager.Identity.Authorization;

/// <summary>
/// Authorization handler for resource ownership
/// </summary>
public class ResourceOwnerAuthorizationHandler : AuthorizationHandler<ResourceOwnerRequirement>
{
    private readonly IUserContextService _userContext;
    private readonly ILogger<ResourceOwnerAuthorizationHandler> _logger;

    public ResourceOwnerAuthorizationHandler(
        IUserContextService userContext,
        ILogger<ResourceOwnerAuthorizationHandler> logger)
    {
        _userContext = userContext;
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ResourceOwnerRequirement requirement)
    {
        var currentUserId = _userContext.GetUserId();
        if (string.IsNullOrEmpty(currentUserId))
        {
            _logger.LogWarning("User ID not found in context");
            return Task.CompletedTask;
        }

        // Check if resource has the specified user ID property
        var resource = context.Resource;
        if (resource == null)
        {
            _logger.LogWarning("Resource not provided for authorization");
            return Task.CompletedTask;
        }

        var resourceType = resource.GetType();
        var userIdProperty = resourceType.GetProperty(requirement.ResourceUserIdProperty);

        if (userIdProperty == null)
        {
            _logger.LogWarning(
                "Property {PropertyName} not found on resource type {ResourceType}",
                requirement.ResourceUserIdProperty,
                resourceType.Name);
            return Task.CompletedTask;
        }

        var resourceUserId = userIdProperty.GetValue(resource)?.ToString();

        // Allow access if user owns the resource or is admin
        if (resourceUserId == currentUserId || _userContext.HasRole(Roles.Admin))
        {
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogWarning(
                "User {UserId} attempted to access resource owned by {ResourceUserId}",
                currentUserId,
                resourceUserId);
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Authorization handler for same tenant access
/// </summary>
public class SameTenantAuthorizationHandler : AuthorizationHandler<SameTenantRequirement>
{
    private readonly IUserContextService _userContext;
    private readonly ILogger<SameTenantAuthorizationHandler> _logger;

    public SameTenantAuthorizationHandler(
        IUserContextService userContext,
        ILogger<SameTenantAuthorizationHandler> logger)
    {
        _userContext = userContext;
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        SameTenantRequirement requirement)
    {
        var currentTenantId = _userContext.GetTenantId();
        if (!currentTenantId.HasValue)
        {
            _logger.LogWarning("Tenant ID not found in context");
            return Task.CompletedTask;
        }

        var resource = context.Resource;
        if (resource == null)
        {
            _logger.LogWarning("Resource not provided for authorization");
            return Task.CompletedTask;
        }

        var resourceType = resource.GetType();
        var tenantIdProperty = resourceType.GetProperty(requirement.ResourceTenantIdProperty);

        if (tenantIdProperty == null)
        {
            _logger.LogWarning(
                "Property {PropertyName} not found on resource type {ResourceType}",
                requirement.ResourceTenantIdProperty,
                resourceType.Name);
            return Task.CompletedTask;
        }

        var resourceTenantId = tenantIdProperty.GetValue(resource);

        // Convert to Guid for comparison
        Guid? resourceTenantGuid = null;
        if (resourceTenantId is Guid guid)
        {
            resourceTenantGuid = guid;
        }
        else if (resourceTenantId is string str && Guid.TryParse(str, out var parsed))
        {
            resourceTenantGuid = parsed;
        }

        // Allow access if same tenant or admin
        if (resourceTenantGuid == currentTenantId || _userContext.HasRole(Roles.Admin))
        {
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogWarning(
                "User from tenant {UserTenantId} attempted to access resource from tenant {ResourceTenantId}",
                currentTenantId,
                resourceTenantGuid);
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Authorization handler for same department access
/// </summary>
public class DepartmentAuthorizationHandler : AuthorizationHandler<SameDepartmentRequirement>
{
    private readonly IUserContextService _userContext;
    private readonly IdentityDbContext _dbContext;
    private readonly ILogger<DepartmentAuthorizationHandler> _logger;

    public DepartmentAuthorizationHandler(
        IUserContextService userContext,
        IdentityDbContext dbContext,
        ILogger<DepartmentAuthorizationHandler> logger)
    {
        _userContext = userContext;
        _dbContext = dbContext;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        SameDepartmentRequirement requirement)
    {
        var currentUserId = _userContext.GetUserId();
        if (string.IsNullOrEmpty(currentUserId))
        {
            _logger.LogWarning("User ID not found in context");
            return;
        }

        // Get current user's department
        var currentUser = await _dbContext.UserMetadata
            .FirstOrDefaultAsync(u => u.KeycloakUserId == currentUserId);

        if (currentUser == null)
        {
            _logger.LogWarning("User metadata not found for {UserId}", currentUserId);
            return;
        }

        var resource = context.Resource;
        if (resource == null)
        {
            _logger.LogWarning("Resource not provided for authorization");
            return;
        }

        var resourceType = resource.GetType();
        var departmentProperty = resourceType.GetProperty(requirement.ResourceDepartmentProperty);

        if (departmentProperty == null)
        {
            _logger.LogWarning(
                "Property {PropertyName} not found on resource type {ResourceType}",
                requirement.ResourceDepartmentProperty,
                resourceType.Name);
            return;
        }

        var resourceDepartment = departmentProperty.GetValue(resource)?.ToString();

        // Allow access if same department, manager, or admin
        if (resourceDepartment == currentUser.Department ||
            _userContext.HasRole(Roles.Manager) ||
            _userContext.HasRole(Roles.Admin))
        {
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogWarning(
                "User from department {UserDepartment} attempted to access resource from department {ResourceDepartment}",
                currentUser.Department,
                resourceDepartment);
        }
    }
}

/// <summary>
/// Helper extension methods for authorization
/// </summary>
public static class AuthorizationExtensions
{
    /// <summary>
    /// Checks if user can access resource based on ownership
    /// </summary>
    public static async Task<bool> CanAccessResourceAsync(
        this IAuthorizationService authorizationService,
        System.Security.Claims.ClaimsPrincipal user,
        object resource,
        string policy)
    {
        var result = await authorizationService.AuthorizeAsync(user, resource, policy);
        return result.Succeeded;
    }

    /// <summary>
    /// Ensures user can access resource, throws if not authorized
    /// </summary>
    public static async Task EnsureCanAccessResourceAsync(
        this IAuthorizationService authorizationService,
        System.Security.Claims.ClaimsPrincipal user,
        object resource,
        string policy)
    {
        var result = await authorizationService.AuthorizeAsync(user, resource, policy);
        if (!result.Succeeded)
        {
            throw new UnauthorizedAccessException($"User is not authorized to access this resource");
        }
    }
}
