namespace WorkSpaceManager.Identity.Models;

/// <summary>
/// Configuration options for Keycloak integration
/// </summary>
public class KeycloakOptions
{
    public const string SectionName = "Keycloak";

    /// <summary>
    /// Keycloak server base URL (e.g., http://localhost:8080)
    /// </summary>
    public string Authority { get; set; } = string.Empty;

    /// <summary>
    /// Realm name for the tenant
    /// </summary>
    public string Realm { get; set; } = string.Empty;

    /// <summary>
    /// Client ID for the application
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Client secret (for confidential clients)
    /// </summary>
    public string? ClientSecret { get; set; }

    /// <summary>
    /// Admin username for Keycloak management operations
    /// </summary>
    public string AdminUsername { get; set; } = string.Empty;

    /// <summary>
    /// Admin password for Keycloak management operations
    /// </summary>
    public string AdminPassword { get; set; } = string.Empty;

    /// <summary>
    /// Audience for JWT token validation
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Whether to require HTTPS metadata endpoints
    /// </summary>
    public bool RequireHttpsMetadata { get; set; } = true;

    /// <summary>
    /// Token validation parameters
    /// </summary>
    public TokenValidationOptions TokenValidation { get; set; } = new();

    /// <summary>
    /// Gets the full authority URL with realm
    /// </summary>
    public string GetRealmUrl() => $"{Authority}/realms/{Realm}";

    /// <summary>
    /// Gets the token endpoint URL
    /// </summary>
    public string GetTokenEndpoint() => $"{GetRealmUrl()}/protocol/openid-connect/token";

    /// <summary>
    /// Gets the user info endpoint URL
    /// </summary>
    public string GetUserInfoEndpoint() => $"{GetRealmUrl()}/protocol/openid-connect/userinfo";

    /// <summary>
    /// Gets the admin API base URL
    /// </summary>
    public string GetAdminApiUrl() => $"{Authority}/admin/realms/{Realm}";
}

/// <summary>
/// Token validation configuration options
/// </summary>
public class TokenValidationOptions
{
    /// <summary>
    /// Whether to validate the token issuer
    /// </summary>
    public bool ValidateIssuer { get; set; } = true;

    /// <summary>
    /// Whether to validate the token audience
    /// </summary>
    public bool ValidateAudience { get; set; } = true;

    /// <summary>
    /// Whether to validate the token lifetime
    /// </summary>
    public bool ValidateLifetime { get; set; } = true;

    /// <summary>
    /// Whether to validate the issuer signing key
    /// </summary>
    public bool ValidateIssuerSigningKey { get; set; } = true;

    /// <summary>
    /// Clock skew for token expiration validation (in seconds)
    /// </summary>
    public int ClockSkewSeconds { get; set; } = 300; // 5 minutes

    /// <summary>
    /// Whether to require expiration time in tokens
    /// </summary>
    public bool RequireExpirationTime { get; set; } = true;

    /// <summary>
    /// Whether to require signed tokens
    /// </summary>
    public bool RequireSignedTokens { get; set; } = true;
}
