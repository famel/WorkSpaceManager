# Keycloak Authentication & Authorization Implementation

This document provides complete implementation details for integrating Keycloak authentication and authorization in the WorkSpaceManager system.

---

## Table of Contents

1. [Overview](#overview)
2. [Backend Implementation](#backend-implementation)
3. [React Web Client](#react-web-client)
4. [React Native Mobile App](#react-native-mobile-app)
5. [Configuration](#configuration)
6. [Usage Examples](#usage-examples)
7. [Testing](#testing)
8. [Troubleshooting](#troubleshooting)

---

## Overview

The WorkSpaceManager system uses **Keycloak** as the identity provider with the following features:

**Authentication Features:**
- OAuth 2.0 / OpenID Connect authentication
- JWT token-based authorization
- Single Sign-On (SSO) with Azure AD integration
- Just-In-Time (JIT) user provisioning
- Automatic token refresh
- Biometric authentication (mobile app)

**Authorization Features:**
- Role-based access control (RBAC)
- Resource-based authorization
- Tenant isolation
- Department-based access control
- Custom authorization policies

---

## Backend Implementation

### Step 1: Add NuGet Packages

```bash
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package Microsoft.IdentityModel.Protocols.OpenIdConnect
dotnet add package System.IdentityModel.Tokens.Jwt
```

### Step 2: Configure Services

Add the following to `Program.cs`:

```csharp
using WorkSpaceManager.Identity.Services;
using WorkSpaceManager.Identity.Middleware;
using WorkSpaceManager.Identity.Authorization;
using WorkSpaceManager.Identity.Models;

var builder = WebApplication.CreateBuilder(args);

// Configure Keycloak options
builder.Services.Configure<KeycloakOptions>(
    builder.Configuration.GetSection(KeycloakOptions.SectionName));

// Register HTTP client for Keycloak service
builder.Services.AddHttpClient<IKeycloakService, KeycloakService>();

// Register services
builder.Services.AddScoped<IKeycloakService, KeycloakService>();
builder.Services.AddScoped<IJitProvisioningService, JitProvisioningService>();
builder.Services.AddScoped<IUserContextService, UserContextService>();

// Add HTTP context accessor
builder.Services.AddHttpContextAccessor();

// Configure JWT authentication
var keycloakOptions = builder.Configuration
    .GetSection(KeycloakOptions.SectionName)
    .Get<KeycloakOptions>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Authority = keycloakOptions.GetRealmUrl();
    options.Audience = keycloakOptions.Audience;
    options.RequireHttpsMetadata = keycloakOptions.RequireHttpsMetadata;
    
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = keycloakOptions.TokenValidation.ValidateIssuer,
        ValidateAudience = keycloakOptions.TokenValidation.ValidateAudience,
        ValidateLifetime = keycloakOptions.TokenValidation.ValidateLifetime,
        ValidateIssuerSigningKey = keycloakOptions.TokenValidation.ValidateIssuerSigningKey,
        ClockSkew = TimeSpan.FromSeconds(keycloakOptions.TokenValidation.ClockSkewSeconds)
    };
});

// Add authorization policies
builder.Services.AddWorkSpaceManagerAuthorization();

var app = builder.Build();

// Add middleware
app.UseAuthentication();
app.UseJwtAuthentication(); // Custom JWT middleware
app.UseJitProvisioning(); // Automatic user provisioning
app.UseAuthorization();

app.MapControllers();

app.Run();
```

### Step 3: Configure appsettings.json

```json
{
  "Keycloak": {
    "Authority": "http://localhost:8080",
    "Realm": "alpha-bank-realm",
    "ClientId": "workspace-manager-api",
    "ClientSecret": "your-client-secret",
    "AdminUsername": "admin",
    "AdminPassword": "admin",
    "Audience": "workspace-manager-api",
    "RequireHttpsMetadata": false,
    "TokenValidation": {
      "ValidateIssuer": true,
      "ValidateAudience": true,
      "ValidateLifetime": true,
      "ValidateIssuerSigningKey": true,
      "ClockSkewSeconds": 300,
      "RequireExpirationTime": true,
      "RequireSignedTokens": true
    }
  }
}
```

### Step 4: Protect API Endpoints

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkSpaceManager.Identity.Authorization;
using WorkSpaceManager.Identity.Services;

namespace WorkSpaceManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly IUserContextService _userContext;
    private readonly IAuthorizationService _authorizationService;

    public BookingsController(
        IUserContextService userContext,
        IAuthorizationService authorizationService)
    {
        _userContext = userContext;
        _authorizationService = authorizationService;
    }

    // Require authenticated user
    [Authorize(Policy = Policies.RequireAuthenticatedUser)]
    [HttpGet("my")]
    public IActionResult GetMyBookings()
    {
        var userId = _userContext.GetUserId();
        var tenantId = _userContext.GetTenantId();
        
        // Get bookings for current user
        return Ok(new { userId, tenantId });
    }

    // Require admin role
    [Authorize(Policy = Policies.RequireAdminRole)]
    [HttpGet("all")]
    public IActionResult GetAllBookings()
    {
        // Only admins can access
        return Ok();
    }

    // Resource-based authorization
    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> CancelBooking(Guid id)
    {
        // Get booking from database
        var booking = await GetBookingById(id);
        
        if (booking == null)
        {
            return NotFound();
        }

        // Check if user can cancel this booking
        var authResult = await _authorizationService.AuthorizeAsync(
            User,
            booking,
            new Requirements.ResourceOwnerRequirement());

        if (!authResult.Succeeded)
        {
            return Forbid();
        }

        // Cancel booking
        return Ok();
    }

    private Task<object?> GetBookingById(Guid id)
    {
        // Implementation
        return Task.FromResult<object?>(null);
    }
}
```

---

## React Web Client

### Step 1: Install Dependencies

```bash
npm install
```

### Step 2: Configure Environment Variables

Create `.env` file:

```env
REACT_APP_KEYCLOAK_URL=http://localhost:8080
REACT_APP_KEYCLOAK_REALM=alpha-bank-realm
REACT_APP_KEYCLOAK_CLIENT_ID=workspace-manager-web
REACT_APP_REDIRECT_URI=http://localhost:3000/callback
REACT_APP_API_URL=http://localhost:5000/api
```

### Step 3: Setup Authentication Provider

In `App.tsx`:

```tsx
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { AuthProvider, AuthCallback } from './auth/useAuth';
import { HomePage } from './pages/HomePage';
import { DashboardPage } from './pages/DashboardPage';
import { ProtectedRoute } from './components/ProtectedRoute';

function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          <Route path="/" element={<HomePage />} />
          <Route path="/callback" element={<AuthCallback />} />
          <Route
            path="/dashboard"
            element={
              <ProtectedRoute>
                <DashboardPage />
              </ProtectedRoute>
            }
          />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  );
}

export default App;
```

### Step 4: Create Protected Route Component

```tsx
import { ReactNode } from 'react';
import { Navigate } from 'react-router-dom';
import { useAuth } from '../auth/useAuth';

interface ProtectedRouteProps {
  children: ReactNode;
}

export function ProtectedRoute({ children }: ProtectedRouteProps) {
  const { isAuthenticated, isLoading } = useAuth();

  if (isLoading) {
    return <div>Loading...</div>;
  }

  if (!isAuthenticated) {
    return <Navigate to="/" replace />;
  }

  return <>{children}</>;
}
```

### Step 5: Use Authentication in Components

```tsx
import { useAuth } from '../auth/useAuth';

export function HomePage() {
  const { user, isAuthenticated, login, logout } = useAuth();

  return (
    <div>
      <h1>Welcome to WorkSpace Manager</h1>
      
      {!isAuthenticated ? (
        <button onClick={login}>Sign In</button>
      ) : (
        <div>
          <p>Welcome, {user?.name}!</p>
          <button onClick={logout}>Sign Out</button>
        </div>
      )}
    </div>
  );
}
```

### Step 6: Make Authenticated API Calls

```tsx
import { useAuth } from '../auth/useAuth';

export function BookingsList() {
  const { getAccessToken } = useAuth();
  const [bookings, setBookings] = useState([]);

  useEffect(() => {
    const fetchBookings = async () => {
      const token = getAccessToken();
      
      const response = await fetch(
        `${process.env.REACT_APP_API_URL}/bookings/my`,
        {
          headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json',
          },
        }
      );

      if (response.ok) {
        const data = await response.json();
        setBookings(data);
      }
    };

    fetchBookings();
  }, [getAccessToken]);

  return (
    <div>
      {bookings.map(booking => (
        <div key={booking.id}>{booking.resourceName}</div>
      ))}
    </div>
  );
}
```

---

## React Native Mobile App

### Step 1: Install Dependencies

```bash
npx expo install expo-auth-session expo-secure-store expo-local-authentication
```

### Step 2: Configure app.json

```json
{
  "expo": {
    "scheme": "workspacemanager",
    "ios": {
      "bundleIdentifier": "com.alphabank.workspacemanager",
      "infoPlist": {
        "NSFaceIDUsageDescription": "We use Face ID to securely authenticate you"
      }
    },
    "android": {
      "package": "com.alphabank.workspacemanager",
      "permissions": [
        "USE_BIOMETRIC",
        "USE_FINGERPRINT"
      ]
    }
  }
}
```

### Step 3: Setup Authentication Provider

In `App.tsx`:

```tsx
import { NavigationContainer } from '@react-navigation/native';
import { createNativeStackNavigator } from '@react-navigation/native-stack';
import { AuthProvider, useAuth } from './auth/useAuth';
import { LoginScreen } from './auth/LoginScreen';
import { HomeScreen } from './screens/HomeScreen';

const Stack = createNativeStackNavigator();

function Navigation() {
  const { isAuthenticated, isLoading } = useAuth();

  if (isLoading) {
    return <LoadingScreen />;
  }

  return (
    <Stack.Navigator>
      {!isAuthenticated ? (
        <Stack.Screen name="Login" component={LoginScreen} />
      ) : (
        <Stack.Screen name="Home" component={HomeScreen} />
      )}
    </Stack.Navigator>
  );
}

export default function App() {
  return (
    <AuthProvider>
      <NavigationContainer>
        <Navigation />
      </NavigationContainer>
    </AuthProvider>
  );
}
```

### Step 4: Make Authenticated API Calls

```tsx
import { useAuth } from '../auth/useAuth';
import { useState, useEffect } from 'react';

export function BookingsScreen() {
  const { getAccessToken } = useAuth();
  const [bookings, setBookings] = useState([]);

  useEffect(() => {
    const fetchBookings = async () => {
      const token = await getAccessToken();
      
      const response = await fetch(
        `${process.env.EXPO_PUBLIC_API_URL}/bookings/my`,
        {
          headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json',
          },
        }
      );

      if (response.ok) {
        const data = await response.json();
        setBookings(data);
      }
    };

    fetchBookings();
  }, [getAccessToken]);

  return (
    <View>
      {bookings.map(booking => (
        <Text key={booking.id}>{booking.resourceName}</Text>
      ))}
    </View>
  );
}
```

---

## Configuration

### Keycloak Realm Configuration

**1. Create Realm:**
- Name: `alpha-bank-realm`

**2. Create Clients:**

**API Client (Backend):**
- Client ID: `workspace-manager-api`
- Access Type: `confidential`
- Service Accounts Enabled: `ON`
- Authorization Enabled: `ON`

**Web Client:**
- Client ID: `workspace-manager-web`
- Access Type: `public`
- Valid Redirect URIs: `http://localhost:3000/*`
- Web Origins: `http://localhost:3000`

**Mobile Client:**
- Client ID: `workspace-manager-mobile`
- Access Type: `public`
- Valid Redirect URIs: `workspacemanager://*`

**3. Create Roles:**
- `admin`
- `manager`
- `user`
- `facility_manager`
- `hr`

**4. Create User Attributes:**
- `employeeId`
- `department`
- `jobTitle`
- `managerId`
- `preferredLanguage`
- `tenant_id`

---

## Usage Examples

### Example 1: Create User with JIT Provisioning

When a user logs in for the first time, they are automatically provisioned:

```csharp
// Middleware automatically calls this
var userMetadata = await jitProvisioningService.ProvisionUserAsync(
    keycloakUserId: "keycloak-user-001",
    tenantId: tenantGuid);
```

### Example 2: Check User Permissions

```csharp
// In controller
var userId = _userContext.GetUserId();
var tenantId = _userContext.GetTenantId();
var isAdmin = _userContext.HasRole(Roles.Admin);

if (!isAdmin)
{
    return Forbid();
}
```

### Example 3: Resource-Based Authorization

```csharp
// Check if user can access a specific booking
var booking = await _dbContext.Bookings.FindAsync(bookingId);

var authResult = await _authorizationService.AuthorizeAsync(
    User,
    booking,
    new Requirements.ResourceOwnerRequirement());

if (!authResult.Succeeded)
{
    return Forbid();
}
```

### Example 4: React Login Flow

```tsx
// User clicks login button
const handleLogin = () => {
  const { login } = useAuth();
  login(); // Redirects to Keycloak
};

// After successful login, user is redirected to /callback
// AuthCallback component handles token exchange
// User is then redirected to dashboard
```

### Example 5: React Native Biometric Login

```tsx
const handleBiometricLogin = async () => {
  try {
    await loginWithBiometric();
    // Navigate to home screen
  } catch (error) {
    Alert.alert('Authentication Failed', error.message);
  }
};
```

---

## Testing

### Unit Tests

```csharp
using Xunit;
using Moq;
using WorkSpaceManager.Identity.Services;

public class JitProvisioningServiceTests
{
    [Fact]
    public async Task ProvisionUserAsync_CreatesNewUser_WhenNotExists()
    {
        // Arrange
        var mockKeycloakService = new Mock<IKeycloakService>();
        var mockDbContext = new Mock<IdentityDbContext>();
        
        var service = new JitProvisioningService(
            mockDbContext.Object,
            mockKeycloakService.Object,
            Mock.Of<ILogger<JitProvisioningService>>());

        // Act
        var result = await service.ProvisionUserAsync(
            "keycloak-user-001",
            Guid.NewGuid());

        // Assert
        Assert.NotNull(result);
        Assert.Equal("keycloak-user-001", result.KeycloakUserId);
    }
}
```

### Integration Tests

```csharp
[Fact]
public async Task Login_ReturnsToken_WithValidCredentials()
{
    // Arrange
    var client = _factory.CreateClient();

    // Act
    var response = await client.PostAsync("/api/auth/login", new
    {
        username = "test@example.com",
        password = "password123"
    });

    // Assert
    response.EnsureSuccessStatusCode();
    var token = await response.Content.ReadAsStringAsync();
    Assert.NotEmpty(token);
}
```

---

## Troubleshooting

### Issue 1: Token Validation Fails

**Symptoms:**
```
401 Unauthorized - Invalid token
```

**Solutions:**
1. Verify Keycloak is running: `curl http://localhost:8080/health`
2. Check realm name in configuration
3. Verify client ID and audience
4. Ensure token is not expired
5. Check clock skew settings

### Issue 2: CORS Errors in Web Client

**Symptoms:**
```
Access to fetch has been blocked by CORS policy
```

**Solutions:**
1. Add CORS policy in backend:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebClient", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

app.UseCors("AllowWebClient");
```

2. Configure Web Origins in Keycloak client settings

### Issue 3: Biometric Authentication Not Available

**Symptoms:**
```
Biometric hardware not available
```

**Solutions:**
1. Check device has biometric hardware
2. Ensure biometric credentials are enrolled
3. Verify app permissions in `app.json`
4. Check iOS Info.plist for Face ID usage description

### Issue 4: JIT Provisioning Fails

**Symptoms:**
```
User not found in Keycloak
```

**Solutions:**
1. Verify Keycloak admin credentials
2. Check user exists in Keycloak
3. Ensure user attributes are configured
4. Verify tenant ID in token claims

---

## Security Best Practices

1. **Always use HTTPS in production**
2. **Store client secrets securely** (Azure Key Vault, environment variables)
3. **Implement token refresh** before expiration
4. **Use short-lived access tokens** (5-15 minutes)
5. **Rotate refresh tokens** after each use
6. **Implement rate limiting** on authentication endpoints
7. **Log authentication failures** for security monitoring
8. **Use secure storage** for tokens (SecureStore on mobile)
9. **Validate tokens on every request**
10. **Implement logout on all devices** when password changes

---

## Additional Resources

- [Keycloak Documentation](https://www.keycloak.org/documentation)
- [OAuth 2.0 Specification](https://oauth.net/2/)
- [OpenID Connect Specification](https://openid.net/connect/)
- [JWT.io](https://jwt.io/) - JWT Debugger
- [Expo AuthSession](https://docs.expo.dev/versions/latest/sdk/auth-session/)
- [React Navigation Authentication Flow](https://reactnavigation.org/docs/auth-flow/)

---

**Document Version:** 1.0  
**Last Updated:** December 2025  
**Author:** Manus AI
