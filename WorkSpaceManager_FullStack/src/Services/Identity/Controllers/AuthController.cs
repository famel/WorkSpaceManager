using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WorkSpaceManager.IdentityService.Data;
using WorkSpaceManager.IdentityService.Services;
using WorkSpaceManager.Shared.DTOs;
using WorkSpaceManager.Shared.Common;
using WorkSpaceManager.Shared.Models;

namespace WorkSpaceManager.IdentityService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IdentityDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly IUserService _userService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IdentityDbContext context,
        IJwtService jwtService,
        IUserService userService,
        IConfiguration configuration,
        ILogger<AuthController> logger)
    {
        _context = context;
        _jwtService = jwtService;
        _userService = userService;
        _configuration = configuration;
        _logger = logger;
    }

    private Guid GetTenantId()
    {
        var tenantClaim = User.FindFirst("tenant_id")?.Value;
        return Guid.TryParse(tenantClaim, out var tenantId) ? tenantId : Guid.Empty;
    }

    /// <summary>
    /// User login
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 401)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<LoginResponse>.ErrorResponse("Invalid request"));
        }

        try
        {
            _logger.LogInformation("Login attempt for user: {Email}", request.Email);

            // Find user by email (ignoring tenant filter for login)
            var user = await _context.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Email == request.Email && !u.IsDeleted, cancellationToken);

            if (user == null)
            {
                _logger.LogWarning("Login failed: User not found - {Email}", request.Email);
                return Unauthorized(ApiResponse<LoginResponse>.ErrorResponse("Invalid credentials"));
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Login failed: User is inactive - {Email}", request.Email);
                return Unauthorized(ApiResponse<LoginResponse>.ErrorResponse("Account is disabled"));
            }

            // Check lockout
            if (user.LockoutEndDate.HasValue && user.LockoutEndDate > DateTime.UtcNow)
            {
                _logger.LogWarning("Login failed: User is locked out - {Email}", request.Email);
                return Unauthorized(ApiResponse<LoginResponse>.ErrorResponse($"Account is locked until {user.LockoutEndDate:g}"));
            }

            // Verify password
            if (string.IsNullOrEmpty(user.PasswordHash) || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                // Increment failed login attempts
                user.FailedLoginAttempts++;
                if (user.FailedLoginAttempts >= 5)
                {
                    user.LockoutEndDate = DateTime.UtcNow.AddMinutes(15);
                    _logger.LogWarning("User locked out due to too many failed attempts: {Email}", request.Email);
                }
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogWarning("Login failed: Invalid password - {Email}", request.Email);
                return Unauthorized(ApiResponse<LoginResponse>.ErrorResponse("Invalid credentials"));
            }

            // Get user roles
            var roles = await _userService.GetUserRolesAsync(user.Id, cancellationToken);

            // Generate tokens
            var accessToken = await _jwtService.GenerateAccessTokenAsync(user, roles);
            var refreshToken = await _jwtService.GenerateRefreshTokenAsync();

            // Save refresh token
            var refreshTokenExpiration = int.Parse(_configuration["JWT:RefreshTokenExpirationDays"] ?? "7");
            var refreshTokenEntity = new RefreshToken
            {
                TenantId = user.TenantId,
                UserId = user.Id,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenExpiration),
                CreatedByIp = HttpContext.Connection.RemoteIpAddress?.ToString()
            };
            _context.RefreshTokens.Add(refreshTokenEntity);

            // Reset failed login attempts and update last login
            user.FailedLoginAttempts = 0;
            user.LockoutEndDate = null;
            user.LastLoginDate = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            var accessTokenExpiration = int.Parse(_configuration["JWT:AccessTokenExpirationMinutes"] ?? "60");

            var response = new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(accessTokenExpiration),
                User = new UserInfoResponse
                {
                    Id = user.Id,
                    EmployeeId = user.EmployeeId,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Department = user.Department,
                    JobTitle = user.JobTitle,
                    IsActive = user.IsActive,
                    LastLoginDate = user.LastLoginDate,
                    Roles = roles
                }
            };

            _logger.LogInformation("User logged in successfully: {Email}", request.Email);

            return Ok(ApiResponse<LoginResponse>.SuccessResponse(response, "Login successful"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for {Email}", request.Email);
            return StatusCode(500, ApiResponse<LoginResponse>.ErrorResponse("An error occurred during login"));
        }
    }

    /// <summary>
    /// User registration
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<UserInfoResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<UserInfoResponse>), 400)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<UserInfoResponse>.ErrorResponse("Invalid request"));
        }

        try
        {
            _logger.LogInformation("Registration attempt for: {Email}", request.Email);

            // Check if email already exists
            var existingUser = await _context.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Email == request.Email && !u.IsDeleted, cancellationToken);

            if (existingUser != null)
            {
                _logger.LogWarning("Registration failed: Email already exists - {Email}", request.Email);
                return BadRequest(ApiResponse<UserInfoResponse>.ErrorResponse("An account with this email already exists"));
            }

            // Use default tenant for self-registration
            var defaultTenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");

            // Create new user
            var user = new User
            {
                TenantId = defaultTenantId,
                EmployeeId = $"EMP-{DateTime.UtcNow:yyyyMMddHHmmss}",
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Department = request.Department,
                JobTitle = request.JobTitle,
                PhoneNumber = request.PhoneNumber,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);

            // Assign default User role
            var userRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == "User" && r.TenantId == defaultTenantId, cancellationToken);

            if (userRole != null)
            {
                var userRoleAssignment = new UserRole
                {
                    TenantId = defaultTenantId,
                    UserId = user.Id,
                    RoleId = userRole.Id,
                    IsActive = true
                };
                _context.UserRoles.Add(userRoleAssignment);
                await _context.SaveChangesAsync(cancellationToken);
            }

            var roles = userRole != null ? new List<string> { userRole.Name } : new List<string>();

            var response = new UserInfoResponse
            {
                Id = user.Id,
                EmployeeId = user.EmployeeId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Department = user.Department,
                JobTitle = user.JobTitle,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive,
                Roles = roles
            };

            _logger.LogInformation("User registered successfully: {Email}", request.Email);

            return Ok(ApiResponse<UserInfoResponse>.SuccessResponse(response, "Registration successful"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for {Email}", request.Email);
            return StatusCode(500, ApiResponse<UserInfoResponse>.ErrorResponse("An error occurred during registration"));
        }
    }

    /// <summary>
    /// Refresh access token
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 401)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var (isValid, userId) = await _jwtService.ValidateRefreshTokenAsync(request.RefreshToken);

            if (!isValid)
            {
                return Unauthorized(ApiResponse<LoginResponse>.ErrorResponse("Invalid or expired refresh token"));
            }

            var user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);
            if (user == null || !user.IsActive)
            {
                return Unauthorized(ApiResponse<LoginResponse>.ErrorResponse("User not found or inactive"));
            }

            // Get user roles
            var roles = await _userService.GetUserRolesAsync(user.Id, cancellationToken);

            // Generate new tokens
            var accessToken = await _jwtService.GenerateAccessTokenAsync(user, roles);
            var newRefreshToken = await _jwtService.GenerateRefreshTokenAsync();

            // Revoke old refresh token
            var oldToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, cancellationToken);
            if (oldToken != null)
            {
                oldToken.IsRevoked = true;
                oldToken.RevokedAt = DateTime.UtcNow;
                oldToken.RevokedByIp = HttpContext.Connection.RemoteIpAddress?.ToString();
                oldToken.ReplacedByToken = newRefreshToken;
            }

            // Save new refresh token
            var refreshTokenExpiration = int.Parse(_configuration["JWT:RefreshTokenExpirationDays"] ?? "7");
            var refreshTokenEntity = new RefreshToken
            {
                TenantId = user.TenantId,
                UserId = user.Id,
                Token = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenExpiration),
                CreatedByIp = HttpContext.Connection.RemoteIpAddress?.ToString()
            };
            _context.RefreshTokens.Add(refreshTokenEntity);

            await _context.SaveChangesAsync(cancellationToken);

            var accessTokenExpiration = int.Parse(_configuration["JWT:AccessTokenExpirationMinutes"] ?? "60");

            var response = new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(accessTokenExpiration),
                User = new UserInfoResponse
                {
                    Id = user.Id,
                    EmployeeId = user.EmployeeId,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Department = user.Department,
                    JobTitle = user.JobTitle,
                    IsActive = user.IsActive,
                    LastLoginDate = user.LastLoginDate,
                    Roles = roles
                }
            };

            return Ok(ApiResponse<LoginResponse>.SuccessResponse(response, "Token refreshed successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return StatusCode(500, ApiResponse<LoginResponse>.ErrorResponse("An error occurred"));
        }
    }

    /// <summary>
    /// Logout (revoke refresh token)
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var token = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, cancellationToken);

            if (token != null)
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
                token.RevokedByIp = HttpContext.Connection.RemoteIpAddress?.ToString();
                await _context.SaveChangesAsync(cancellationToken);
            }

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Logged out successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("An error occurred"));
        }
    }

    /// <summary>
    /// Change password
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 400)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse("Invalid request"));
        }

        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(ApiResponse<bool>.ErrorResponse("Invalid user"));
            }

            var user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);
            if (user == null)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse("User not found"));
            }

            // Verify current password
            if (string.IsNullOrEmpty(user.PasswordHash) || !BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            {
                return BadRequest(ApiResponse<bool>.ErrorResponse("Current password is incorrect"));
            }

            // Update password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            // Revoke all refresh tokens
            var refreshTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && !rt.IsRevoked)
                .ToListAsync(cancellationToken);
            foreach (var token in refreshTokens)
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Password changed for user: {UserId}", userId);

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Password changed successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password");
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("An error occurred"));
        }
    }

    /// <summary>
    /// Get current user info
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UserInfoResponse>), 200)]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(ApiResponse<UserInfoResponse>.ErrorResponse("Invalid user"));
            }

            var user = await _context.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .Include(u => u.GroupMemberships).ThenInclude(gm => gm.Group)
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (user == null)
            {
                return NotFound(ApiResponse<UserInfoResponse>.ErrorResponse("User not found"));
            }

            var roles = user.UserRoles?.Where(ur => ur.IsActive).Select(ur => ur.Role.Name).ToList() ?? new List<string>();
            var groups = user.GroupMemberships?.Where(gm => gm.IsActive).Select(gm => gm.Group.Name).ToList() ?? new List<string>();

            var response = new UserInfoResponse
            {
                Id = user.Id,
                EmployeeId = user.EmployeeId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Department = user.Department,
                JobTitle = user.JobTitle,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive,
                LastLoginDate = user.LastLoginDate,
                Roles = roles,
                Groups = groups
            };

            return Ok(ApiResponse<UserInfoResponse>.SuccessResponse(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user");
            return StatusCode(500, ApiResponse<UserInfoResponse>.ErrorResponse("An error occurred"));
        }
    }
}
