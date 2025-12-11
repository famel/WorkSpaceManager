using WorkSpaceManager.Shared.Models;

namespace WorkSpaceManager.IdentityService.Services;

public interface IJwtService
{
    Task<string> GenerateAccessTokenAsync(User user, List<string> roles);
    Task<string> GenerateRefreshTokenAsync();
    Task<(bool IsValid, Guid UserId)> ValidateRefreshTokenAsync(string token);
}
