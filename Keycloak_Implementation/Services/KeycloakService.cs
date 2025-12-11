using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WorkSpaceManager.Identity.Models;

namespace WorkSpaceManager.Identity.Services;

/// <summary>
/// Service for interacting with Keycloak API
/// </summary>
public interface IKeycloakService
{
    Task<KeycloakTokenResponse> GetTokenAsync(string username, string password, CancellationToken cancellationToken = default);
    Task<KeycloakTokenResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<KeycloakUserInfo> GetUserInfoAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<KeycloakUser?> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<KeycloakUser?> GetUserByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<KeycloakUser?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<string> CreateUserAsync(CreateKeycloakUserRequest request, CancellationToken cancellationToken = default);
    Task UpdateUserAsync(string userId, KeycloakUser user, CancellationToken cancellationToken = default);
    Task DeleteUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<List<KeycloakRole>> GetUserRolesAsync(string userId, CancellationToken cancellationToken = default);
    Task AssignRoleToUserAsync(string userId, string roleName, CancellationToken cancellationToken = default);
    Task RemoveRoleFromUserAsync(string userId, string roleName, CancellationToken cancellationToken = default);
}

public class KeycloakService : IKeycloakService
{
    private readonly HttpClient _httpClient;
    private readonly KeycloakOptions _options;
    private readonly ILogger<KeycloakService> _logger;
    private string? _adminToken;
    private DateTime _adminTokenExpiry = DateTime.MinValue;

    public KeycloakService(
        HttpClient httpClient,
        IOptions<KeycloakOptions> options,
        ILogger<KeycloakService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Get access token for user credentials
    /// </summary>
    public async Task<KeycloakTokenResponse> GetTokenAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = _options.ClientId,
            ["username"] = username,
            ["password"] = password
        });

        if (!string.IsNullOrEmpty(_options.ClientSecret))
        {
            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            var credentials = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{_options.ClientId}:{_options.ClientSecret}"));
            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Basic", credentials);
        }

        var response = await _httpClient.PostAsync(
            _options.GetTokenEndpoint(),
            content,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadFromJsonAsync<KeycloakErrorResponse>(cancellationToken);
            _logger.LogError("Failed to get token: {Error}", error?.ErrorDescription ?? "Unknown error");
            throw new KeycloakException($"Failed to get token: {error?.ErrorDescription ?? "Unknown error"}");
        }

        var tokenResponse = await response.Content.ReadFromJsonAsync<KeycloakTokenResponse>(cancellationToken);
        return tokenResponse ?? throw new KeycloakException("Empty token response");
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    public async Task<KeycloakTokenResponse> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["client_id"] = _options.ClientId,
            ["refresh_token"] = refreshToken
        });

        if (!string.IsNullOrEmpty(_options.ClientSecret))
        {
            content.Headers.Add("client_secret", _options.ClientSecret);
        }

        var response = await _httpClient.PostAsync(
            _options.GetTokenEndpoint(),
            content,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadFromJsonAsync<KeycloakErrorResponse>(cancellationToken);
            throw new KeycloakException($"Failed to refresh token: {error?.ErrorDescription ?? "Unknown error"}");
        }

        var tokenResponse = await response.Content.ReadFromJsonAsync<KeycloakTokenResponse>(cancellationToken);
        return tokenResponse ?? throw new KeycloakException("Empty token response");
    }

    /// <summary>
    /// Get user info from access token
    /// </summary>
    public async Task<KeycloakUserInfo> GetUserInfoAsync(
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, _options.GetUserInfoEndpoint());
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new KeycloakException("Failed to get user info");
        }

        var userInfo = await response.Content.ReadFromJsonAsync<KeycloakUserInfo>(cancellationToken);
        return userInfo ?? throw new KeycloakException("Empty user info response");
    }

    /// <summary>
    /// Get user by ID from Keycloak admin API
    /// </summary>
    public async Task<KeycloakUser?> GetUserByIdAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        await EnsureAdminTokenAsync(cancellationToken);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{_options.GetAdminApiUrl()}/users/{userId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<KeycloakUser>(cancellationToken);
    }

    /// <summary>
    /// Get user by username
    /// </summary>
    public async Task<KeycloakUser?> GetUserByUsernameAsync(
        string username,
        CancellationToken cancellationToken = default)
    {
        await EnsureAdminTokenAsync(cancellationToken);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{_options.GetAdminApiUrl()}/users?username={Uri.EscapeDataString(username)}&exact=true");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var users = await response.Content.ReadFromJsonAsync<List<KeycloakUser>>(cancellationToken);
        return users?.FirstOrDefault();
    }

    /// <summary>
    /// Get user by email
    /// </summary>
    public async Task<KeycloakUser?> GetUserByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        await EnsureAdminTokenAsync(cancellationToken);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{_options.GetAdminApiUrl()}/users?email={Uri.EscapeDataString(email)}&exact=true");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var users = await response.Content.ReadFromJsonAsync<List<KeycloakUser>>(cancellationToken);
        return users?.FirstOrDefault();
    }

    /// <summary>
    /// Create a new user in Keycloak
    /// </summary>
    public async Task<string> CreateUserAsync(
        CreateKeycloakUserRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsureAdminTokenAsync(cancellationToken);

        var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"{_options.GetAdminApiUrl()}/users");
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
        httpRequest.Content = JsonContent.Create(request);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Failed to create user: {Error}", error);
            throw new KeycloakException($"Failed to create user: {error}");
        }

        // Extract user ID from Location header
        var location = response.Headers.Location?.ToString();
        if (string.IsNullOrEmpty(location))
        {
            throw new KeycloakException("User created but no location header returned");
        }

        var userId = location.Split('/').Last();
        _logger.LogInformation("Created user with ID: {UserId}", userId);
        return userId;
    }

    /// <summary>
    /// Update existing user in Keycloak
    /// </summary>
    public async Task UpdateUserAsync(
        string userId,
        KeycloakUser user,
        CancellationToken cancellationToken = default)
    {
        await EnsureAdminTokenAsync(cancellationToken);

        var request = new HttpRequestMessage(
            HttpMethod.Put,
            $"{_options.GetAdminApiUrl()}/users/{userId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
        request.Content = JsonContent.Create(user);

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new KeycloakException($"Failed to update user: {error}");
        }

        _logger.LogInformation("Updated user: {UserId}", userId);
    }

    /// <summary>
    /// Delete user from Keycloak
    /// </summary>
    public async Task DeleteUserAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        await EnsureAdminTokenAsync(cancellationToken);

        var request = new HttpRequestMessage(
            HttpMethod.Delete,
            $"{_options.GetAdminApiUrl()}/users/{userId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        _logger.LogInformation("Deleted user: {UserId}", userId);
    }

    /// <summary>
    /// Get roles assigned to user
    /// </summary>
    public async Task<List<KeycloakRole>> GetUserRolesAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        await EnsureAdminTokenAsync(cancellationToken);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{_options.GetAdminApiUrl()}/users/{userId}/role-mappings/realm");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<List<KeycloakRole>>(cancellationToken)
            ?? new List<KeycloakRole>();
    }

    /// <summary>
    /// Assign role to user
    /// </summary>
    public async Task AssignRoleToUserAsync(
        string userId,
        string roleName,
        CancellationToken cancellationToken = default)
    {
        await EnsureAdminTokenAsync(cancellationToken);

        // First, get the role by name
        var roleRequest = new HttpRequestMessage(
            HttpMethod.Get,
            $"{_options.GetAdminApiUrl()}/roles/{roleName}");
        roleRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);

        var roleResponse = await _httpClient.SendAsync(roleRequest, cancellationToken);
        roleResponse.EnsureSuccessStatusCode();

        var role = await roleResponse.Content.ReadFromJsonAsync<KeycloakRole>(cancellationToken);
        if (role == null)
        {
            throw new KeycloakException($"Role not found: {roleName}");
        }

        // Assign the role to user
        var assignRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"{_options.GetAdminApiUrl()}/users/{userId}/role-mappings/realm");
        assignRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
        assignRequest.Content = JsonContent.Create(new[] { role });

        var assignResponse = await _httpClient.SendAsync(assignRequest, cancellationToken);
        assignResponse.EnsureSuccessStatusCode();

        _logger.LogInformation("Assigned role {RoleName} to user {UserId}", roleName, userId);
    }

    /// <summary>
    /// Remove role from user
    /// </summary>
    public async Task RemoveRoleFromUserAsync(
        string userId,
        string roleName,
        CancellationToken cancellationToken = default)
    {
        await EnsureAdminTokenAsync(cancellationToken);

        // Get the role
        var roleRequest = new HttpRequestMessage(
            HttpMethod.Get,
            $"{_options.GetAdminApiUrl()}/roles/{roleName}");
        roleRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);

        var roleResponse = await _httpClient.SendAsync(roleRequest, cancellationToken);
        roleResponse.EnsureSuccessStatusCode();

        var role = await roleResponse.Content.ReadFromJsonAsync<KeycloakRole>(cancellationToken);
        if (role == null)
        {
            throw new KeycloakException($"Role not found: {roleName}");
        }

        // Remove the role from user
        var removeRequest = new HttpRequestMessage(
            HttpMethod.Delete,
            $"{_options.GetAdminApiUrl()}/users/{userId}/role-mappings/realm");
        removeRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
        removeRequest.Content = JsonContent.Create(new[] { role });

        var removeResponse = await _httpClient.SendAsync(removeRequest, cancellationToken);
        removeResponse.EnsureSuccessStatusCode();

        _logger.LogInformation("Removed role {RoleName} from user {UserId}", roleName, userId);
    }

    /// <summary>
    /// Ensure admin token is valid and refresh if needed
    /// </summary>
    private async Task EnsureAdminTokenAsync(CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(_adminToken) && DateTime.UtcNow < _adminTokenExpiry)
        {
            return;
        }

        _logger.LogDebug("Obtaining admin token");

        var tokenResponse = await GetTokenAsync(
            _options.AdminUsername,
            _options.AdminPassword,
            cancellationToken);

        _adminToken = tokenResponse.AccessToken;
        _adminTokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 60); // Refresh 1 minute early

        _logger.LogDebug("Admin token obtained, expires at {Expiry}", _adminTokenExpiry);
    }
}

/// <summary>
/// Exception thrown by Keycloak operations
/// </summary>
public class KeycloakException : Exception
{
    public KeycloakException(string message) : base(message) { }
    public KeycloakException(string message, Exception innerException) : base(message, innerException) { }
}
