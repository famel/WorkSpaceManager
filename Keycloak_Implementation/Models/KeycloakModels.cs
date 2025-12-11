using System.Text.Json.Serialization;

namespace WorkSpaceManager.Identity.Models;

/// <summary>
/// Keycloak user representation
/// </summary>
public class KeycloakUser
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("firstName")]
    public string? FirstName { get; set; }

    [JsonPropertyName("lastName")]
    public string? LastName { get; set; }

    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    [JsonPropertyName("emailVerified")]
    public bool EmailVerified { get; set; }

    [JsonPropertyName("attributes")]
    public Dictionary<string, List<string>>? Attributes { get; set; }

    [JsonPropertyName("createdTimestamp")]
    public long? CreatedTimestamp { get; set; }

    /// <summary>
    /// Gets a single attribute value
    /// </summary>
    public string? GetAttribute(string key)
    {
        if (Attributes?.TryGetValue(key, out var values) == true && values.Count > 0)
        {
            return values[0];
        }
        return null;
    }

    /// <summary>
    /// Sets a single attribute value
    /// </summary>
    public void SetAttribute(string key, string value)
    {
        Attributes ??= new Dictionary<string, List<string>>();
        Attributes[key] = new List<string> { value };
    }
}

/// <summary>
/// Token response from Keycloak
/// </summary>
public class KeycloakTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("refresh_expires_in")]
    public int RefreshExpiresIn { get; set; }

    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = "Bearer";

    [JsonPropertyName("not-before-policy")]
    public int NotBeforePolicy { get; set; }

    [JsonPropertyName("session_state")]
    public string? SessionState { get; set; }

    [JsonPropertyName("scope")]
    public string? Scope { get; set; }
}

/// <summary>
/// User info response from Keycloak
/// </summary>
public class KeycloakUserInfo
{
    [JsonPropertyName("sub")]
    public string Subject { get; set; } = string.Empty;

    [JsonPropertyName("email_verified")]
    public bool EmailVerified { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("preferred_username")]
    public string? PreferredUsername { get; set; }

    [JsonPropertyName("given_name")]
    public string? GivenName { get; set; }

    [JsonPropertyName("family_name")]
    public string? FamilyName { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }
}

/// <summary>
/// Role representation in Keycloak
/// </summary>
public class KeycloakRole
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("composite")]
    public bool Composite { get; set; }

    [JsonPropertyName("clientRole")]
    public bool ClientRole { get; set; }

    [JsonPropertyName("containerId")]
    public string? ContainerId { get; set; }
}

/// <summary>
/// Request to create a new user in Keycloak
/// </summary>
public class CreateKeycloakUserRequest
{
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("firstName")]
    public string FirstName { get; set; } = string.Empty;

    [JsonPropertyName("lastName")]
    public string LastName { get; set; } = string.Empty;

    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    [JsonPropertyName("emailVerified")]
    public bool EmailVerified { get; set; } = false;

    [JsonPropertyName("attributes")]
    public Dictionary<string, List<string>>? Attributes { get; set; }

    [JsonPropertyName("credentials")]
    public List<KeycloakCredential>? Credentials { get; set; }
}

/// <summary>
/// Credential for Keycloak user
/// </summary>
public class KeycloakCredential
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "password";

    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;

    [JsonPropertyName("temporary")]
    public bool Temporary { get; set; } = false;
}

/// <summary>
/// Error response from Keycloak
/// </summary>
public class KeycloakErrorResponse
{
    [JsonPropertyName("error")]
    public string? Error { get; set; }

    [JsonPropertyName("error_description")]
    public string? ErrorDescription { get; set; }

    [JsonPropertyName("errorMessage")]
    public string? ErrorMessage { get; set; }
}
