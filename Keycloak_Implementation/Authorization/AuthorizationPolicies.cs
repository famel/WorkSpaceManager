using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace WorkSpaceManager.Identity.Authorization;

/// <summary>
/// Authorization policy names
/// </summary>
public static class Policies
{
    public const string RequireAuthenticatedUser = "RequireAuthenticatedUser";
    public const string RequireAdminRole = "RequireAdminRole";
    public const string RequireManagerRole = "RequireManagerRole";
    public const string RequireUserRole = "RequireUserRole";
    public const string RequireFacilityManagerRole = "RequireFacilityManagerRole";
    public const string RequireHRRole = "RequireHRRole";
}

/// <summary>
/// Role names matching Keycloak realm roles
/// </summary>
public static class Roles
{
    public const string Admin = "admin";
    public const string Manager = "manager";
    public const string User = "user";
    public const string FacilityManager = "facility_manager";
    public const string HR = "hr";
}

/// <summary>
/// Extension methods for configuring authorization policies
/// </summary>
public static class AuthorizationPolicyExtensions
{
    /// <summary>
    /// Adds WorkSpaceManager authorization policies
    /// </summary>
    public static IServiceCollection AddWorkSpaceManagerAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            // Require authenticated user
            options.AddPolicy(Policies.RequireAuthenticatedUser, policy =>
            {
                policy.RequireAuthenticatedUser();
            });

            // Require Admin role
            options.AddPolicy(Policies.RequireAdminRole, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole(Roles.Admin);
            });

            // Require Manager role
            options.AddPolicy(Policies.RequireManagerRole, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole(Roles.Manager, Roles.Admin); // Admin can also act as manager
            });

            // Require User role (default for all authenticated users)
            options.AddPolicy(Policies.RequireUserRole, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole(Roles.User, Roles.Manager, Roles.Admin);
            });

            // Require Facility Manager role
            options.AddPolicy(Policies.RequireFacilityManagerRole, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole(Roles.FacilityManager, Roles.Admin);
            });

            // Require HR role
            options.AddPolicy(Policies.RequireHRRole, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole(Roles.HR, Roles.Admin);
            });
        });

        // Register custom authorization handlers
        services.AddScoped<IAuthorizationHandler, ResourceOwnerAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, SameTenantAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, DepartmentAuthorizationHandler>();

        return services;
    }
}

/// <summary>
/// Custom authorization requirements
/// </summary>
public static class Requirements
{
    /// <summary>
    /// Requirement for resource ownership
    /// </summary>
    public class ResourceOwnerRequirement : IAuthorizationRequirement
    {
        public ResourceOwnerRequirement(string resourceUserIdProperty = "UserId")
        {
            ResourceUserIdProperty = resourceUserIdProperty;
        }

        public string ResourceUserIdProperty { get; }
    }

    /// <summary>
    /// Requirement for same tenant access
    /// </summary>
    public class SameTenantRequirement : IAuthorizationRequirement
    {
        public SameTenantRequirement(string resourceTenantIdProperty = "TenantId")
        {
            ResourceTenantIdProperty = resourceTenantIdProperty;
        }

        public string ResourceTenantIdProperty { get; }
    }

    /// <summary>
    /// Requirement for same department access
    /// </summary>
    public class SameDepartmentRequirement : IAuthorizationRequirement
    {
        public SameDepartmentRequirement(string resourceDepartmentProperty = "Department")
        {
            ResourceDepartmentProperty = resourceDepartmentProperty;
        }

        public string ResourceDepartmentProperty { get; }
    }
}
