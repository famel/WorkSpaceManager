using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace WorkSpaceManager.Identity.Services;

/// <summary>
/// Service for accessing current user context from JWT claims
/// </summary>
public interface IUserContextService
{
    /// <summary>
    /// Gets the current user ID (subject claim)
    /// </summary>
    string? GetUserId();

    /// <summary>
    /// Gets the current username
    /// </summary>
    string? GetUsername();

    /// <summary>
    /// Gets the current user's email
    /// </summary>
    string? GetEmail();

    /// <summary>
    /// Gets the current user's full name
    /// </summary>
    string? GetFullName();

    /// <summary>
    /// Gets the current user's roles
    /// </summary>
    List<string> GetRoles();

    /// <summary>
    /// Gets a specific claim value
    /// </summary>
    string? GetClaim(string claimType);

    /// <summary>
    /// Gets all claims for the current user
    /// </summary>
    Dictionary<string, string> GetAllClaims();

    /// <summary>
    /// Checks if user has a specific role
    /// </summary>
    bool HasRole(string role);

    /// <summary>
    /// Checks if user is authenticated
    /// </summary>
    bool IsAuthenticated();

    /// <summary>
    /// Gets the tenant ID from claims
    /// </summary>
    Guid? GetTenantId();
}

public class UserContextService : IUserContextService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContextService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    /// <summary>
    /// Gets the current user ID from 'sub' claim
    /// </summary>
    public string? GetUserId()
    {
        return User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User?.FindFirst("sub")?.Value;
    }

    /// <summary>
    /// Gets the username from 'preferred_username' claim
    /// </summary>
    public string? GetUsername()
    {
        return User?.FindFirst("preferred_username")?.Value
            ?? User?.FindFirst(ClaimTypes.Name)?.Value;
    }

    /// <summary>
    /// Gets the email from 'email' claim
    /// </summary>
    public string? GetEmail()
    {
        return User?.FindFirst(ClaimTypes.Email)?.Value
            ?? User?.FindFirst("email")?.Value;
    }

    /// <summary>
    /// Gets the full name from 'name' claim
    /// </summary>
    public string? GetFullName()
    {
        return User?.FindFirst("name")?.Value;
    }

    /// <summary>
    /// Gets all roles from 'realm_access.roles' claim
    /// </summary>
    public List<string> GetRoles()
    {
        var roles = new List<string>();

        // Check standard role claims
        var roleClaims = User?.FindAll(ClaimTypes.Role);
        if (roleClaims != null)
        {
            roles.AddRange(roleClaims.Select(c => c.Value));
        }

        // Check Keycloak realm roles
        var realmRoles = User?.FindFirst("realm_access")?.Value;
        if (!string.IsNullOrEmpty(realmRoles))
        {
            try
            {
                var rolesObj = System.Text.Json.JsonSerializer.Deserialize<RealmAccess>(realmRoles);
                if (rolesObj?.Roles != null)
                {
                    roles.AddRange(rolesObj.Roles);
                }
            }
            catch
            {
                // Ignore JSON parsing errors
            }
        }

        return roles.Distinct().ToList();
    }

    /// <summary>
    /// Gets a specific claim value
    /// </summary>
    public string? GetClaim(string claimType)
    {
        return User?.FindFirst(claimType)?.Value;
    }

    /// <summary>
    /// Gets all claims as a dictionary
    /// </summary>
    public Dictionary<string, string> GetAllClaims()
    {
        var claims = new Dictionary<string, string>();

        if (User?.Claims == null)
        {
            return claims;
        }

        foreach (var claim in User.Claims)
        {
            // For duplicate claim types, keep the first value
            if (!claims.ContainsKey(claim.Type))
            {
                claims[claim.Type] = claim.Value;
            }
        }

        return claims;
    }

    /// <summary>
    /// Checks if user has a specific role
    /// </summary>
    public bool HasRole(string role)
    {
        return GetRoles().Contains(role, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks if user is authenticated
    /// </summary>
    public bool IsAuthenticated()
    {
        return User?.Identity?.IsAuthenticated == true;
    }

    /// <summary>
    /// Gets the tenant ID from custom claim
    /// </summary>
    public Guid? GetTenantId()
    {
        var tenantIdStr = GetClaim("tenant_id");
        if (Guid.TryParse(tenantIdStr, out var tenantId))
        {
            return tenantId;
        }
        return null;
    }

    private class RealmAccess
    {
        public List<string>? Roles { get; set; }
    }
}

/// <summary>
/// Extension methods for ClaimsPrincipal
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Gets the user ID from claims
    /// </summary>
    public static string? GetUserId(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? principal.FindFirst("sub")?.Value;
    }

    /// <summary>
    /// Gets the username from claims
    /// </summary>
    public static string? GetUsername(this ClaimsPrincipal principal)
    {
        return principal.FindFirst("preferred_username")?.Value
            ?? principal.FindFirst(ClaimTypes.Name)?.Value;
    }

    /// <summary>
    /// Gets the email from claims
    /// </summary>
    public static string? GetEmail(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.Email)?.Value
            ?? principal.FindFirst("email")?.Value;
    }

    /// <summary>
    /// Gets the tenant ID from claims
    /// </summary>
    public static Guid? GetTenantId(this ClaimsPrincipal principal)
    {
        var tenantIdStr = principal.FindFirst("tenant_id")?.Value;
        if (Guid.TryParse(tenantIdStr, out var tenantId))
        {
            return tenantId;
        }
        return null;
    }

    /// <summary>
    /// Checks if principal has a specific role
    /// </summary>
    public static bool HasRole(this ClaimsPrincipal principal, string role)
    {
        return principal.IsInRole(role);
    }
}
