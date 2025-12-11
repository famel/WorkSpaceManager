using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WorkSpaceManager.IdentityService.Data;
using WorkSpaceManager.Shared.Models;

namespace WorkSpaceManager.IdentityService.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;
    private readonly IdentityDbContext _context;
    private readonly ILogger<JwtService> _logger;

    public JwtService(IConfiguration configuration, IdentityDbContext context, ILogger<JwtService> logger)
    {
        _configuration = configuration;
        _context = context;
        _logger = logger;
    }

    public Task<string> GenerateAccessTokenAsync(User user, List<string> roles)
    {
        var jwtSettings = _configuration.GetSection("JWT");
        var secretKey = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret is required");
        var issuer = jwtSettings["Issuer"] ?? "WorkSpaceManager";
        var audience = jwtSettings["Audience"] ?? "WorkSpaceManager-API";
        var expirationMinutes = int.Parse(jwtSettings["AccessTokenExpirationMinutes"] ?? "60");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("tenant_id", user.TenantId.ToString()),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new("employee_id", user.EmployeeId)
        };

        // Add role claims
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        return Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
    }

    public async Task<string> GenerateRefreshTokenAsync()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return await Task.FromResult(Convert.ToBase64String(randomBytes));
    }

    public async Task<(bool IsValid, Guid UserId)> ValidateRefreshTokenAsync(string token)
    {
        try
        {
            var refreshToken = await _context.RefreshTokens
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Token == token && !r.IsRevoked && r.ExpiresAt > DateTime.UtcNow);

            if (refreshToken == null)
            {
                return (false, Guid.Empty);
            }

            return (true, refreshToken.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating refresh token");
            return (false, Guid.Empty);
        }
    }
}
