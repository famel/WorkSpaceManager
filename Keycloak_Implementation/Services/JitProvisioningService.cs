using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WorkSpaceManager.Identity.Data;
using WorkSpaceManager.Identity.Data.Entities;
using WorkSpaceManager.Identity.Models;

namespace WorkSpaceManager.Identity.Services;

/// <summary>
/// Service for Just-In-Time (JIT) user provisioning from Keycloak to local database
/// </summary>
public interface IJitProvisioningService
{
    /// <summary>
    /// Provisions a user from Keycloak to local database if not exists
    /// </summary>
    Task<UserMetadata> ProvisionUserAsync(
        string keycloakUserId,
        Guid tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Syncs user data from Keycloak to local database
    /// </summary>
    Task<UserMetadata> SyncUserAsync(
        string keycloakUserId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates last login timestamp for user
    /// </summary>
    Task UpdateLastLoginAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}

public class JitProvisioningService : IJitProvisioningService
{
    private readonly IdentityDbContext _dbContext;
    private readonly IKeycloakService _keycloakService;
    private readonly ILogger<JitProvisioningService> _logger;

    public JitProvisioningService(
        IdentityDbContext dbContext,
        IKeycloakService keycloakService,
        ILogger<JitProvisioningService> logger)
    {
        _dbContext = dbContext;
        _keycloakService = keycloakService;
        _logger = logger;
    }

    /// <summary>
    /// Provisions a user from Keycloak if not already in local database
    /// </summary>
    public async Task<UserMetadata> ProvisionUserAsync(
        string keycloakUserId,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        // Check if user already exists in local database
        var existingUser = await _dbContext.UserMetadata
            .FirstOrDefaultAsync(
                u => u.KeycloakUserId == keycloakUserId && u.TenantId == tenantId,
                cancellationToken);

        if (existingUser != null)
        {
            _logger.LogDebug("User already exists: {KeycloakUserId}", keycloakUserId);
            
            // Update last login
            existingUser.LastLoginAt = DateTime.UtcNow;
            existingUser.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
            
            return existingUser;
        }

        // Fetch user from Keycloak
        var keycloakUser = await _keycloakService.GetUserByIdAsync(keycloakUserId, cancellationToken);
        if (keycloakUser == null)
        {
            throw new InvalidOperationException($"User not found in Keycloak: {keycloakUserId}");
        }

        // Create new user metadata
        var userMetadata = new UserMetadata
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            KeycloakUserId = keycloakUserId,
            EmployeeId = keycloakUser.GetAttribute("employeeId") ?? string.Empty,
            Department = keycloakUser.GetAttribute("department"),
            JobTitle = keycloakUser.GetAttribute("jobTitle"),
            ManagerId = ParseGuidAttribute(keycloakUser.GetAttribute("managerId")),
            PreferredLanguage = keycloakUser.GetAttribute("preferredLanguage") ?? "en",
            NoShowCount = 0,
            LastLoginAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.UserMetadata.Add(userMetadata);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Provisioned new user: {KeycloakUserId}, EmployeeId: {EmployeeId}",
            keycloakUserId,
            userMetadata.EmployeeId);

        return userMetadata;
    }

    /// <summary>
    /// Syncs user data from Keycloak to local database
    /// </summary>
    public async Task<UserMetadata> SyncUserAsync(
        string keycloakUserId,
        CancellationToken cancellationToken = default)
    {
        // Get user from local database
        var userMetadata = await _dbContext.UserMetadata
            .FirstOrDefaultAsync(u => u.KeycloakUserId == keycloakUserId, cancellationToken);

        if (userMetadata == null)
        {
            throw new InvalidOperationException($"User not found in local database: {keycloakUserId}");
        }

        // Fetch latest data from Keycloak
        var keycloakUser = await _keycloakService.GetUserByIdAsync(keycloakUserId, cancellationToken);
        if (keycloakUser == null)
        {
            throw new InvalidOperationException($"User not found in Keycloak: {keycloakUserId}");
        }

        // Update user metadata
        userMetadata.EmployeeId = keycloakUser.GetAttribute("employeeId") ?? userMetadata.EmployeeId;
        userMetadata.Department = keycloakUser.GetAttribute("department");
        userMetadata.JobTitle = keycloakUser.GetAttribute("jobTitle");
        userMetadata.ManagerId = ParseGuidAttribute(keycloakUser.GetAttribute("managerId"));
        userMetadata.PreferredLanguage = keycloakUser.GetAttribute("preferredLanguage") ?? userMetadata.PreferredLanguage;
        userMetadata.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Synced user data from Keycloak: {KeycloakUserId}", keycloakUserId);

        return userMetadata;
    }

    /// <summary>
    /// Updates last login timestamp for user
    /// </summary>
    public async Task UpdateLastLoginAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.UserMetadata.FindAsync(new object[] { userId }, cancellationToken);
        if (user != null)
        {
            user.LastLoginAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Helper to parse GUID from string attribute
    /// </summary>
    private Guid? ParseGuidAttribute(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        if (Guid.TryParse(value, out var guid))
        {
            return guid;
        }

        return null;
    }
}

/// <summary>
/// Middleware for automatic JIT provisioning on authentication
/// </summary>
public class JitProvisioningMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<JitProvisioningMiddleware> _logger;

    public JitProvisioningMiddleware(
        RequestDelegate next,
        ILogger<JitProvisioningMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IJitProvisioningService jitService,
        IUserContextService userContext)
    {
        // Check if user is authenticated
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            var keycloakUserId = userContext.GetUserId();
            var tenantId = userContext.GetTenantId();

            if (!string.IsNullOrEmpty(keycloakUserId) && tenantId.HasValue)
            {
                try
                {
                    // Provision user if not exists
                    await jitService.ProvisionUserAsync(
                        keycloakUserId,
                        tenantId.Value,
                        context.RequestAborted);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to provision user: {KeycloakUserId}", keycloakUserId);
                    // Don't fail the request, just log the error
                }
            }
        }

        await _next(context);
    }
}

/// <summary>
/// Extension methods for JIT provisioning middleware
/// </summary>
public static class JitProvisioningMiddlewareExtensions
{
    public static IApplicationBuilder UseJitProvisioning(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<JitProvisioningMiddleware>();
    }
}
