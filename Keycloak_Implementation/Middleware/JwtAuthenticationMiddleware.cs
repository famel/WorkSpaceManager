using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WorkSpaceManager.Identity.Models;

namespace WorkSpaceManager.Identity.Middleware;

/// <summary>
/// Middleware for JWT token validation and user context injection
/// </summary>
public class JwtAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<JwtAuthenticationMiddleware> _logger;

    public JwtAuthenticationMiddleware(
        RequestDelegate next,
        ILogger<JwtAuthenticationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IOptions<KeycloakOptions> keycloakOptions)
    {
        var token = ExtractToken(context);

        if (!string.IsNullOrEmpty(token))
        {
            try
            {
                var principal = await ValidateTokenAsync(token, keycloakOptions.Value);
                if (principal != null)
                {
                    context.User = principal;
                    _logger.LogDebug("User authenticated: {UserId}", principal.FindFirst("sub")?.Value);
                }
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogWarning(ex, "Token validation failed");
                // Don't set user context, but continue processing
                // The authorization middleware will handle unauthorized access
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during token validation");
            }
        }

        await _next(context);
    }

    /// <summary>
    /// Extract JWT token from Authorization header
    /// </summary>
    private string? ExtractToken(HttpContext context)
    {
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        
        if (string.IsNullOrEmpty(authHeader))
        {
            return null;
        }

        if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return authHeader.Substring("Bearer ".Length).Trim();
        }

        return null;
    }

    /// <summary>
    /// Validate JWT token and return ClaimsPrincipal
    /// </summary>
    private async Task<ClaimsPrincipal?> ValidateTokenAsync(
        string token,
        KeycloakOptions options)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        // Get signing keys from Keycloak
        var configurationManager = new Microsoft.IdentityModel.Protocols.OpenIdConnect.ConfigurationManager<Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectConfiguration>(
            $"{options.GetRealmUrl()}/.well-known/openid-configuration",
            new Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectConfigurationRetriever(),
            new HttpDocumentRetriever());

        var discoveryDocument = await configurationManager.GetConfigurationAsync(CancellationToken.None);

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = options.TokenValidation.ValidateIssuer,
            ValidIssuer = options.GetRealmUrl(),

            ValidateAudience = options.TokenValidation.ValidateAudience,
            ValidAudience = options.Audience,

            ValidateLifetime = options.TokenValidation.ValidateLifetime,
            ClockSkew = TimeSpan.FromSeconds(options.TokenValidation.ClockSkewSeconds),

            ValidateIssuerSigningKey = options.TokenValidation.ValidateIssuerSigningKey,
            IssuerSigningKeys = discoveryDocument.SigningKeys,

            RequireExpirationTime = options.TokenValidation.RequireExpirationTime,
            RequireSignedTokens = options.TokenValidation.RequireSignedTokens
        };

        try
        {
            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            return principal;
        }
        catch (SecurityTokenException)
        {
            return null;
        }
    }
}

/// <summary>
/// Extension methods for adding JWT authentication middleware
/// </summary>
public static class JwtAuthenticationMiddlewareExtensions
{
    public static IApplicationBuilder UseJwtAuthentication(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<JwtAuthenticationMiddleware>();
    }
}
