# WorkSpaceManager: Πλήρης Οδηγός Υλοποίησης

**Έκδοση:** 1.0  
**Ημερομηνία:** Δεκέμβριος 2025  
**Συγγραφέας:** Manus AI

---

## Περιεχόμενα

1. [Επισκόπηση Αρχιτεκτονικής](#επισκόπηση-αρχιτεκτονικής)
2. [Δομή Project](#δομή-project)
3. [Backend Services Implementation](#backend-services-implementation)
4. [API Gateway Configuration](#api-gateway-configuration)
5. [React Web Client Implementation](#react-web-client-implementation)
6. [React Native Mobile App](#react-native-mobile-app)
7. [Database Schemas](#database-schemas)
8. [Deployment Guide](#deployment-guide)
9. [Testing Strategy](#testing-strategy)

---

## Επισκόπηση Αρχιτεκτονικής

Το σύστημα WorkSpaceManager αποτελείται από:

### Microservices
- **Identity Service** (Port 5001): Διαχείριση χρηστών μέσω Keycloak
- **Resource Service** (Port 5002): Κατάλογος πόρων (Buildings, Floors, Desks, Rooms)
- **Booking Service** (Port 5003): Λογική κρατήσεων με CQRS pattern
- **Rules Service** (Port 5004): Επιχειρησιακοί κανόνες με NRules
- **Notification Service** (Port 5005): Email/Push notifications

### Infrastructure
- **API Gateway** (Port 5000): YARP reverse proxy
- **Keycloak** (Port 8080): Identity & Access Management
- **RabbitMQ** (Port 5672): Message broker
- **SQL Server** (Port 1433): Βάση δεδομένων

### Clients
- **React Web App** (Port 3000): Admin panel & user portal
- **React Native Mobile** (iOS/Android): Mobile booking app

---

## Δομή Project

```
WorkSpaceManager/
├── backend/
│   ├── src/
│   │   ├── WorkSpaceManager.Shared.Kernel/
│   │   │   ├── BuildingBlocks/
│   │   │   │   ├── Entity.cs
│   │   │   │   ├── IDomainEvent.cs
│   │   │   │   ├── ValueObject.cs
│   │   │   │   └── Result.cs
│   │   │   ├── Interfaces/
│   │   │   │   ├── IRepository.cs
│   │   │   │   └── IUnitOfWork.cs
│   │   │   └── Extensions/
│   │   │       └── ServiceCollectionExtensions.cs
│   │   │
│   │   ├── Services/
│   │   │   ├── Identity/
│   │   │   │   └── WorkSpaceManager.Identity.Api/
│   │   │   │       ├── Controllers/
│   │   │   │       │   ├── UsersController.cs
│   │   │   │       │   ├── RolesController.cs
│   │   │   │       │   └── AuthController.cs
│   │   │   │       ├── Services/
│   │   │   │       │   ├── IKeycloakService.cs
│   │   │   │       │   └── KeycloakService.cs
│   │   │   │       ├── Models/
│   │   │   │       │   └── UserModels.cs
│   │   │   │       ├── Program.cs
│   │   │   │       └── appsettings.json
│   │   │   │
│   │   │   ├── Resource/
│   │   │   │   └── WorkSpaceManager.Resource.Api/
│   │   │   │       ├── Domain/
│   │   │   │       │   ├── Entities/
│   │   │   │       │   │   ├── Building.cs
│   │   │   │       │   │   ├── Floor.cs
│   │   │   │       │   │   ├── Desk.cs
│   │   │   │       │   │   ├── MeetingRoom.cs
│   │   │   │       │   │   └── FloorPlan.cs
│   │   │   │       │   └── ValueObjects/
│   │   │   │       │       ├── Address.cs
│   │   │   │       │       └── Coordinates.cs
│   │   │   │       ├── Infrastructure/
│   │   │   │       │   ├── Data/
│   │   │   │       │   │   ├── ResourceDbContext.cs
│   │   │   │       │   │   └── Repositories/
│   │   │   │       │   │       ├── BuildingRepository.cs
│   │   │   │       │   │       └── DeskRepository.cs
│   │   │   │       │   └── Migrations/
│   │   │   │       ├── Application/
│   │   │   │       │   ├── Commands/
│   │   │   │       │   │   ├── CreateBuildingCommand.cs
│   │   │   │       │   │   └── UploadFloorPlanCommand.cs
│   │   │   │       │   └── Queries/
│   │   │   │       │       ├── GetAvailableDesksQuery.cs
│   │   │   │       │       └── GetFloorPlanQuery.cs
│   │   │   │       ├── Controllers/
│   │   │   │       │   ├── BuildingsController.cs
│   │   │   │       │   ├── DesksController.cs
│   │   │   │       │   └── FloorsController.cs
│   │   │   │       ├── Program.cs
│   │   │   │       └── appsettings.json
│   │   │   │
│   │   │   ├── Rules/
│   │   │   │   └── WorkSpaceManager.Rules.Api/
│   │   │   │       ├── Rules/
│   │   │   │       │   ├── MaxDaysPerWeekRule.cs
│   │   │   │       │   ├── DepartmentRestrictionRule.cs
│   │   │   │       │   └── AdvanceBookingRule.cs
│   │   │   │       ├── Models/
│   │   │   │       │   ├── BookingContext.cs
│   │   │   │       │   ├── RuleResult.cs
│   │   │   │       │   └── PolicyConfiguration.cs
│   │   │   │       ├── Services/
│   │   │   │       │   ├── IRulesEngine.cs
│   │   │   │       │   └── RulesEngineService.cs
│   │   │   │       ├── Protos/
│   │   │   │       │   └── rules.proto
│   │   │   │       ├── Controllers/
│   │   │   │       │   └── RulesController.cs
│   │   │   │       ├── Program.cs
│   │   │   │       └── appsettings.json
│   │   │   │
│   │   │   ├── Booking/
│   │   │   │   └── WorkSpaceManager.Booking.Api/
│   │   │   │       ├── Domain/
│   │   │   │       │   ├── Entities/
│   │   │   │       │   │   ├── Booking.cs
│   │   │   │       │   │   └── CheckIn.cs
│   │   │   │       │   ├── Events/
│   │   │   │       │   │   ├── BookingCreatedEvent.cs
│   │   │   │       │   │   ├── BookingCancelledEvent.cs
│   │   │   │       │   │   └── CheckInCompletedEvent.cs
│   │   │   │       │   └── Aggregates/
│   │   │   │       │       └── BookingAggregate.cs
│   │   │   │       ├── Infrastructure/
│   │   │   │       │   ├── Data/
│   │   │   │       │   │   ├── BookingDbContext.cs
│   │   │   │       │   │   └── Repositories/
│   │   │   │       │   │       └── BookingRepository.cs
│   │   │   │       │   ├── Messaging/
│   │   │   │       │   │   ├── RabbitMqPublisher.cs
│   │   │   │       │   │   └── EventBus.cs
│   │   │   │       │   └── gRPC/
│   │   │   │       │       └── RulesServiceClient.cs
│   │   │   │       ├── Application/
│   │   │   │       │   ├── Commands/
│   │   │   │       │   │   ├── CreateBookingCommand.cs
│   │   │   │       │   │   ├── CancelBookingCommand.cs
│   │   │   │       │   │   └── CheckInCommand.cs
│   │   │   │       │   ├── Queries/
│   │   │   │       │   │   ├── GetUserBookingsQuery.cs
│   │   │   │       │   │   └── GetBookingByIdQuery.cs
│   │   │   │       │   └── Handlers/
│   │   │   │       │       ├── CreateBookingHandler.cs
│   │   │   │       │       └── CancelBookingHandler.cs
│   │   │   │       ├── Controllers/
│   │   │   │       │   └── BookingsController.cs
│   │   │   │       ├── Program.cs
│   │   │   │       └── appsettings.json
│   │   │   │
│   │   │   └── Notification/
│   │   │       └── WorkSpaceManager.Notification.Api/
│   │   │           ├── Services/
│   │   │           │   ├── IEmailService.cs
│   │   │           │   ├── EmailService.cs
│   │   │           │   ├── IPushNotificationService.cs
│   │   │           │   └── PushNotificationService.cs
│   │   │           ├── Consumers/
│   │   │           │   ├── BookingCreatedConsumer.cs
│   │   │           │   └── BookingCancelledConsumer.cs
│   │   │           ├── Templates/
│   │   │           │   ├── booking-confirmation.html
│   │   │           │   └── booking-reminder.html
│   │   │           ├── Program.cs
│   │   │           └── appsettings.json
│   │   │
│   │   └── WorkSpaceManager.Gateway/
│   │       ├── Program.cs
│   │       └── appsettings.json
│   │
│   └── tests/
│       ├── WorkSpaceManager.Booking.Tests/
│       ├── WorkSpaceManager.Rules.Tests/
│       └── WorkSpaceManager.Integration.Tests/
│
├── web-client/
│   ├── public/
│   ├── src/
│   │   ├── components/
│   │   │   ├── Layout/
│   │   │   │   ├── Header.tsx
│   │   │   │   ├── Sidebar.tsx
│   │   │   │   └── Footer.tsx
│   │   │   ├── Booking/
│   │   │   │   ├── DeskSelector.tsx
│   │   │   │   ├── FloorPlanViewer.tsx
│   │   │   │   ├── BookingForm.tsx
│   │   │   │   └── BookingList.tsx
│   │   │   ├── Admin/
│   │   │   │   ├── UserManagement.tsx
│   │   │   │   ├── ResourceManagement.tsx
│   │   │   │   └── ReportsPanel.tsx
│   │   │   └── Common/
│   │   │       ├── DatePicker.tsx
│   │   │       ├── TimeSlotSelector.tsx
│   │   │       └── LanguageSwitcher.tsx
│   │   ├── pages/
│   │   │   ├── Dashboard.tsx
│   │   │   ├── BookDesk.tsx
│   │   │   ├── BookRoom.tsx
│   │   │   ├── MyBookings.tsx
│   │   │   └── Admin/
│   │   │       ├── Users.tsx
│   │   │       ├── Resources.tsx
│   │   │       └── Analytics.tsx
│   │   ├── services/
│   │   │   ├── api.ts
│   │   │   ├── authService.ts
│   │   │   ├── bookingService.ts
│   │   │   └── resourceService.ts
│   │   ├── store/
│   │   │   ├── authSlice.ts
│   │   │   ├── bookingSlice.ts
│   │   │   └── store.ts
│   │   ├── i18n/
│   │   │   ├── el.json
│   │   │   ├── en.json
│   │   │   └── i18n.ts
│   │   ├── App.tsx
│   │   └── main.tsx
│   ├── package.json
│   └── vite.config.ts
│
├── mobile-app/
│   ├── android/
│   ├── ios/
│   ├── src/
│   │   ├── screens/
│   │   │   ├── HomeScreen.tsx
│   │   │   ├── BookingScreen.tsx
│   │   │   ├── MapScreen.tsx
│   │   │   └── ProfileScreen.tsx
│   │   ├── components/
│   │   │   ├── DeskCard.tsx
│   │   │   ├── BookingCard.tsx
│   │   │   └── MapView.tsx
│   │   ├── navigation/
│   │   │   └── AppNavigator.tsx
│   │   ├── services/
│   │   │   ├── api.ts
│   │   │   ├── authService.ts
│   │   │   └── storageService.ts
│   │   ├── store/
│   │   │   └── store.ts
│   │   └── App.tsx
│   ├── package.json
│   └── app.json
│
├── docker-compose.yml
└── README.md
```

---

## Backend Services Implementation

### 1. Identity Service

#### Program.cs
```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using WorkSpaceManager.Identity.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Keycloak Integration
builder.Services.AddHttpClient<IKeycloakService, KeycloakService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Keycloak:BaseUrl"]!);
});

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Keycloak:Authority"];
        options.Audience = builder.Configuration["Keycloak:Audience"];
        options.RequireHttpsMetadata = false;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
```

#### KeycloakService.cs
```csharp
using System.Net.Http.Headers;
using System.Net.Http.Json;
using WorkSpaceManager.Identity.Api.Models;

namespace WorkSpaceManager.Identity.Api.Services;

public interface IKeycloakService
{
    Task<List<UserDto>> GetUsersAsync(string? tenantId);
    Task<UserDto?> GetUserByIdAsync(string userId);
    Task<UserDto> CreateUserAsync(CreateUserRequest request);
    Task UpdateUserAsync(string userId, UpdateUserRequest request);
    Task DeleteUserAsync(string userId);
    Task AssignRoleToUserAsync(string userId, string roleName);
    Task<LoginResponse> AuthenticateAsync(LoginRequest request);
}

public class KeycloakService : IKeycloakService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<KeycloakService> _logger;

    public KeycloakService(HttpClient httpClient, IConfiguration configuration, ILogger<KeycloakService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    private async Task<string> GetAdminTokenAsync()
    {
        var tokenEndpoint = $"{_configuration["Keycloak:BaseUrl"]}/realms/master/protocol/openid-connect/token";
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("client_id", _configuration["Keycloak:ClientId"]!),
            new KeyValuePair<string, string>("client_secret", _configuration["Keycloak:ClientSecret"]!)
        });

        var response = await _httpClient.PostAsync(tokenEndpoint, content);
        response.EnsureSuccessStatusCode();
        
        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();
        return tokenResponse!.AccessToken;
    }

    public async Task<List<UserDto>> GetUsersAsync(string? tenantId)
    {
        var token = await GetAdminTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var realm = tenantId ?? "master";
        var response = await _httpClient.GetAsync($"/admin/realms/{realm}/users");
        response.EnsureSuccessStatusCode();

        var users = await response.Content.ReadFromJsonAsync<List<KeycloakUser>>();
        return users!.Select(u => new UserDto
        {
            Id = u.Id,
            Username = u.Username,
            Email = u.Email,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Enabled = u.Enabled,
            TenantId = realm
        }).ToList();
    }

    public async Task<UserDto?> GetUserByIdAsync(string userId)
    {
        var token = await GetAdminTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.GetAsync($"/admin/realms/master/users/{userId}");
        if (!response.IsSuccessStatusCode)
            return null;

        var user = await response.Content.ReadFromJsonAsync<KeycloakUser>();
        return new UserDto
        {
            Id = user!.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Enabled = user.Enabled
        };
    }

    public async Task<UserDto> CreateUserAsync(CreateUserRequest request)
    {
        var token = await GetAdminTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var keycloakUser = new
        {
            username = request.Username,
            email = request.Email,
            firstName = request.FirstName,
            lastName = request.LastName,
            enabled = request.Enabled,
            credentials = new[]
            {
                new
                {
                    type = "password",
                    value = request.Password,
                    temporary = false
                }
            },
            attributes = new Dictionary<string, string[]>
            {
                ["tenantId"] = new[] { request.TenantId }
            }
        };

        var response = await _httpClient.PostAsJsonAsync($"/admin/realms/master/users", keycloakUser);
        response.EnsureSuccessStatusCode();

        var locationHeader = response.Headers.Location!.ToString();
        var userId = locationHeader.Split('/').Last();

        return await GetUserByIdAsync(userId) ?? throw new Exception("User created but not found");
    }

    public async Task UpdateUserAsync(string userId, UpdateUserRequest request)
    {
        var token = await GetAdminTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var updateData = new Dictionary<string, object?>();
        if (request.Email != null) updateData["email"] = request.Email;
        if (request.FirstName != null) updateData["firstName"] = request.FirstName;
        if (request.LastName != null) updateData["lastName"] = request.LastName;
        if (request.Enabled.HasValue) updateData["enabled"] = request.Enabled.Value;

        var response = await _httpClient.PutAsJsonAsync($"/admin/realms/master/users/{userId}", updateData);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteUserAsync(string userId)
    {
        var token = await GetAdminTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.DeleteAsync($"/admin/realms/master/users/{userId}");
        response.EnsureSuccessStatusCode();
    }

    public async Task AssignRoleToUserAsync(string userId, string roleName)
    {
        var token = await GetAdminTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Get role ID
        var rolesResponse = await _httpClient.GetAsync($"/admin/realms/master/roles");
        rolesResponse.EnsureSuccessStatusCode();
        var roles = await rolesResponse.Content.ReadFromJsonAsync<List<KeycloakRole>>();
        var role = roles!.FirstOrDefault(r => r.Name == roleName);

        if (role == null)
            throw new Exception($"Role '{roleName}' not found");

        // Assign role to user
        var assignResponse = await _httpClient.PostAsJsonAsync(
            $"/admin/realms/master/users/{userId}/role-mappings/realm",
            new[] { role }
        );
        assignResponse.EnsureSuccessStatusCode();
    }

    public async Task<LoginResponse> AuthenticateAsync(LoginRequest request)
    {
        var realm = request.TenantId;
        var tokenEndpoint = $"{_configuration["Keycloak:BaseUrl"]}/realms/{realm}/protocol/openid-connect/token";
        
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "password"),
            new KeyValuePair<string, string>("client_id", _configuration["Keycloak:ClientId"]!),
            new KeyValuePair<string, string>("client_secret", _configuration["Keycloak:ClientSecret"]!),
            new KeyValuePair<string, string>("username", request.Username),
            new KeyValuePair<string, string>("password", request.Password)
        });

        var response = await _httpClient.PostAsync(tokenEndpoint, content);
        response.EnsureSuccessStatusCode();

        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();
        var user = await GetUserByIdAsync(tokenResponse!.Sub);

        return new LoginResponse
        {
            AccessToken = tokenResponse.AccessToken,
            RefreshToken = tokenResponse.RefreshToken,
            ExpiresIn = tokenResponse.ExpiresIn,
            User = user!
        };
    }
}

// Internal models
internal class KeycloakUser
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool Enabled { get; set; }
}

internal class KeycloakRole
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

internal class TokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
    public string Sub { get; set; } = string.Empty;
}
```

#### appsettings.json
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Keycloak": {
    "BaseUrl": "http://localhost:8080",
    "Authority": "http://localhost:8080/realms/workspace-manager",
    "Audience": "workspace-api",
    "ClientId": "workspace-admin",
    "ClientSecret": "your-client-secret-here"
  }
}
```

---

### 2. Resource Service

#### Domain Entities

**Building.cs**
```csharp
using WorkSpaceManager.Shared.Kernel.BuildingBlocks;

namespace WorkSpaceManager.Resource.Api.Domain.Entities;

public class Building : Entity
{
    public string Name { get; private set; }
    public Address Address { get; private set; }
    public bool IsActive { get; private set; }
    
    private readonly List<Floor> _floors = new();
    public IReadOnlyCollection<Floor> Floors => _floors.AsReadOnly();

    public Building(Guid tenantId, string name, Address address)
    {
        TenantId = tenantId;
        Name = name;
        Address = address;
        IsActive = true;
    }

    public void AddFloor(Floor floor)
    {
        _floors.Add(floor);
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
```

**Floor.cs**
```csharp
using WorkSpaceManager.Shared.Kernel.BuildingBlocks;

namespace WorkSpaceManager.Resource.Api.Domain.Entities;

public class Floor : Entity
{
    public Guid BuildingId { get; private set; }
    public int FloorNumber { get; private set; }
    public string Name { get; private set; }
    public FloorPlan? FloorPlan { get; private set; }
    
    private readonly List<Desk> _desks = new();
    public IReadOnlyCollection<Desk> Desks => _desks.AsReadOnly();
    
    private readonly List<MeetingRoom> _meetingRooms = new();
    public IReadOnlyCollection<MeetingRoom> MeetingRooms => _meetingRooms.AsReadOnly();

    public Floor(Guid tenantId, Guid buildingId, int floorNumber, string name)
    {
        TenantId = tenantId;
        BuildingId = buildingId;
        FloorNumber = floorNumber;
        Name = name;
    }

    public void AttachFloorPlan(FloorPlan floorPlan)
    {
        FloorPlan = floorPlan;
    }

    public void AddDesk(Desk desk)
    {
        _desks.Add(desk);
    }

    public void AddMeetingRoom(MeetingRoom room)
    {
        _meetingRooms.Add(room);
    }
}
```

**Desk.cs**
```csharp
using WorkSpaceManager.Shared.Kernel.BuildingBlocks;

namespace WorkSpaceManager.Resource.Api.Domain.Entities;

public class Desk : Entity
{
    public Guid FloorId { get; private set; }
    public string DeskNumber { get; private set; }
    public string? Zone { get; private set; }
    public Coordinates Coordinates { get; private set; }
    public bool IsAvailable { get; private set; }
    public DeskType Type { get; private set; }
    public List<string> Amenities { get; private set; }

    public Desk(Guid tenantId, Guid floorId, string deskNumber, Coordinates coordinates)
    {
        TenantId = tenantId;
        FloorId = floorId;
        DeskNumber = deskNumber;
        Coordinates = coordinates;
        IsAvailable = true;
        Type = DeskType.Standard;
        Amenities = new List<string>();
    }

    public void MarkAsUnavailable()
    {
        IsAvailable = false;
    }

    public void MarkAsAvailable()
    {
        IsAvailable = true;
    }
}

public enum DeskType
{
    Standard,
    Standing,
    Ergonomic,
    Quiet
}
```

**MeetingRoom.cs**
```csharp
using WorkSpaceManager.Shared.Kernel.BuildingBlocks;

namespace WorkSpaceManager.Resource.Api.Domain.Entities;

public class MeetingRoom : Entity
{
    public Guid FloorId { get; private set; }
    public string RoomNumber { get; private set; }
    public string Name { get; private set; }
    public int Capacity { get; private set; }
    public Coordinates Coordinates { get; private set; }
    public List<string> Equipment { get; private set; }
    public bool IsAvailable { get; private set; }

    public MeetingRoom(Guid tenantId, Guid floorId, string roomNumber, string name, int capacity)
    {
        TenantId = tenantId;
        FloorId = floorId;
        RoomNumber = roomNumber;
        Name = name;
        Capacity = capacity;
        IsAvailable = true;
        Equipment = new List<string>();
        Coordinates = new Coordinates(0, 0);
    }

    public void SetCoordinates(Coordinates coordinates)
    {
        Coordinates = coordinates;
    }

    public void AddEquipment(string equipment)
    {
        if (!Equipment.Contains(equipment))
            Equipment.Add(equipment);
    }
}
```

**FloorPlan.cs**
```csharp
using WorkSpaceManager.Shared.Kernel.BuildingBlocks;

namespace WorkSpaceManager.Resource.Api.Domain.Entities;

public class FloorPlan : Entity
{
    public Guid FloorId { get; private set; }
    public string ImageUrl { get; private set; }
    public string MappingJson { get; private set; } // JSON with desk/room coordinates
    public DateTime UploadedAt { get; private set; }
    public string UploadedBy { get; private set; }

    public FloorPlan(Guid tenantId, Guid floorId, string imageUrl, string mappingJson, string uploadedBy)
    {
        TenantId = tenantId;
        FloorId = floorId;
        ImageUrl = imageUrl;
        MappingJson = mappingJson;
        UploadedAt = DateTime.UtcNow;
        UploadedBy = uploadedBy;
    }

    public void UpdateMapping(string mappingJson)
    {
        MappingJson = mappingJson;
    }
}
```

#### Value Objects

**Address.cs**
```csharp
using WorkSpaceManager.Shared.Kernel.BuildingBlocks;

namespace WorkSpaceManager.Resource.Api.Domain.ValueObjects;

public class Address : ValueObject
{
    public string Street { get; private set; }
    public string City { get; private set; }
    public string PostalCode { get; private set; }
    public string Country { get; private set; }

    public Address(string street, string city, string postalCode, string country)
    {
        Street = street;
        City = city;
        PostalCode = postalCode;
        Country = country;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return PostalCode;
        yield return Country;
    }
}
```

**Coordinates.cs**
```csharp
using WorkSpaceManager.Shared.Kernel.BuildingBlocks;

namespace WorkSpaceManager.Resource.Api.Domain.ValueObjects;

public class Coordinates : ValueObject
{
    public double X { get; private set; }
    public double Y { get; private set; }

    public Coordinates(double x, double y)
    {
        X = x;
        Y = y;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return X;
        yield return Y;
    }
}
```

#### DbContext

**ResourceDbContext.cs**
```csharp
using Microsoft.EntityFrameworkCore;
using WorkSpaceManager.Resource.Api.Domain.Entities;

namespace WorkSpaceManager.Resource.Api.Infrastructure.Data;

public class ResourceDbContext : DbContext
{
    public DbSet<Building> Buildings { get; set; }
    public DbSet<Floor> Floors { get; set; }
    public DbSet<Desk> Desks { get; set; }
    public DbSet<MeetingRoom> MeetingRooms { get; set; }
    public DbSet<FloorPlan> FloorPlans { get; set; }

    public ResourceDbContext(DbContextOptions<ResourceDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Building>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.OwnsOne(e => e.Address, address =>
            {
                address.Property(a => a.Street).HasColumnName("Street");
                address.Property(a => a.City).HasColumnName("City");
                address.Property(a => a.PostalCode).HasColumnName("PostalCode");
                address.Property(a => a.Country).HasColumnName("Country");
            });
            entity.HasMany(e => e.Floors).WithOne().HasForeignKey(f => f.BuildingId);
        });

        modelBuilder.Entity<Floor>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasMany(e => e.Desks).WithOne().HasForeignKey(d => d.FloorId);
            entity.HasMany(e => e.MeetingRooms).WithOne().HasForeignKey(r => r.FloorId);
            entity.HasOne(e => e.FloorPlan).WithOne().HasForeignKey<FloorPlan>(fp => fp.FloorId);
        });

        modelBuilder.Entity<Desk>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DeskNumber).IsRequired().HasMaxLength(50);
            entity.OwnsOne(e => e.Coordinates, coords =>
            {
                coords.Property(c => c.X).HasColumnName("CoordinateX");
                coords.Property(c => c.Y).HasColumnName("CoordinateY");
            });
            entity.Property(e => e.Amenities).HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
            );
        });

        modelBuilder.Entity<MeetingRoom>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RoomNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.OwnsOne(e => e.Coordinates, coords =>
            {
                coords.Property(c => c.X).HasColumnName("CoordinateX");
                coords.Property(c => c.Y).HasColumnName("CoordinateY");
            });
            entity.Property(e => e.Equipment).HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
            );
        });

        modelBuilder.Entity<FloorPlan>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ImageUrl).IsRequired();
            entity.Property(e => e.MappingJson).IsRequired();
        });
    }
}
```

#### Controllers

**BuildingsController.cs**
```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkSpaceManager.Resource.Api.Domain.Entities;
using WorkSpaceManager.Resource.Api.Infrastructure.Data;

namespace WorkSpaceManager.Resource.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BuildingsController : ControllerBase
{
    private readonly ResourceDbContext _context;
    private readonly ILogger<BuildingsController> _logger;

    public BuildingsController(ResourceDbContext context, ILogger<BuildingsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Building>>> GetBuildings([FromQuery] Guid tenantId)
    {
        var buildings = await _context.Buildings
            .Where(b => b.TenantId == tenantId && b.IsActive)
            .Include(b => b.Floors)
            .ToListAsync();

        return Ok(buildings);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Building>> GetBuilding(Guid id, [FromQuery] Guid tenantId)
    {
        var building = await _context.Buildings
            .Include(b => b.Floors)
            .ThenInclude(f => f.Desks)
            .Include(b => b.Floors)
            .ThenInclude(f => f.MeetingRooms)
            .FirstOrDefaultAsync(b => b.Id == id && b.TenantId == tenantId);

        if (building == null)
            return NotFound();

        return Ok(building);
    }

    [HttpPost]
    public async Task<ActionResult<Building>> CreateBuilding([FromBody] CreateBuildingRequest request)
    {
        var building = new Building(
            request.TenantId,
            request.Name,
            new Address(request.Street, request.City, request.PostalCode, request.Country)
        );

        _context.Buildings.Add(building);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetBuilding), new { id = building.Id, tenantId = building.TenantId }, building);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBuilding(Guid id, [FromQuery] Guid tenantId)
    {
        var building = await _context.Buildings
            .FirstOrDefaultAsync(b => b.Id == id && b.TenantId == tenantId);

        if (building == null)
            return NotFound();

        building.Deactivate();
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public record CreateBuildingRequest(
    Guid TenantId,
    string Name,
    string Street,
    string City,
    string PostalCode,
    string Country
);
```

**DesksController.cs**
```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkSpaceManager.Resource.Api.Infrastructure.Data;

namespace WorkSpaceManager.Resource.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DesksController : ControllerBase
{
    private readonly ResourceDbContext _context;

    public DesksController(ResourceDbContext context)
    {
        _context = context;
    }

    [HttpGet("available")]
    public async Task<IActionResult> GetAvailableDesks(
        [FromQuery] Guid tenantId,
        [FromQuery] Guid floorId,
        [FromQuery] DateTime date)
    {
        var desks = await _context.Desks
            .Where(d => d.TenantId == tenantId && d.FloorId == floorId && d.IsAvailable)
            .ToListAsync();

        // TODO: Check against bookings for the specified date
        
        return Ok(desks);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDesk(Guid id, [FromQuery] Guid tenantId)
    {
        var desk = await _context.Desks
            .FirstOrDefaultAsync(d => d.Id == id && d.TenantId == tenantId);

        if (desk == null)
            return NotFound();

        return Ok(desk);
    }
}
```

---

### 3. Rules Service

#### NRules Implementation

**MaxDaysPerWeekRule.cs**
```csharp
using NRules.Fluent.Dsl;
using WorkSpaceManager.Rules.Api.Models;

namespace WorkSpaceManager.Rules.Api.Rules;

public class MaxDaysPerWeekRule : Rule
{
    public override void Define()
    {
        BookingContext context = null!;
        PolicyConfiguration policy = null!;

        When()
            .Match(() => context, c => c.UserBookingsThisWeek >= 2)
            .Match(() => policy, p => p.MaxDaysPerWeek == 2);

        Then()
            .Do(ctx => ctx.For(context).Insert(new RuleViolation
            {
                RuleName = "MaxDaysPerWeek",
                Message = $"User has already booked {context.UserBookingsThisWeek} days this week. Maximum allowed is {policy.MaxDaysPerWeek}.",
                Severity = ViolationSeverity.Error
            }));
    }
}
```

**DepartmentRestrictionRule.cs**
```csharp
using NRules.Fluent.Dsl;
using WorkSpaceManager.Rules.Api.Models;

namespace WorkSpaceManager.Rules.Api.Rules;

public class DepartmentRestrictionRule : Rule
{
    public override void Define()
    {
        BookingContext context = null!;
        PolicyConfiguration policy = null!;

        When()
            .Match(() => context, c => !string.IsNullOrEmpty(c.DeskZone))
            .Match(() => policy, p => p.RestrictToDepartment)
            .Match(() => context, c => c.UserDepartment != c.DeskZone);

        Then()
            .Do(ctx => ctx.For(context).Insert(new RuleViolation
            {
                RuleName = "DepartmentRestriction",
                Message = $"User from {context.UserDepartment} cannot book desks in {context.DeskZone} zone.",
                Severity = ViolationSeverity.Error
            }));
    }
}
```

**AdvanceBookingRule.cs**
```csharp
using NRules.Fluent.Dsl;
using WorkSpaceManager.Rules.Api.Models;

namespace WorkSpaceManager.Rules.Api.Rules;

public class AdvanceBookingRule : Rule
{
    public override void Define()
    {
        BookingContext context = null!;
        PolicyConfiguration policy = null!;

        When()
            .Match(() => context, c => (c.BookingDate - DateTime.UtcNow).TotalDays > 14)
            .Match(() => policy, p => p.MaxAdvanceBookingDays == 14);

        Then()
            .Do(ctx => ctx.For(context).Insert(new RuleViolation
            {
                RuleName = "AdvanceBooking",
                Message = $"Cannot book more than {policy.MaxAdvanceBookingDays} days in advance.",
                Severity = ViolationSeverity.Error
            }));
    }
}
```

#### Models

**BookingContext.cs**
```csharp
namespace WorkSpaceManager.Rules.Api.Models;

public class BookingContext
{
    public Guid UserId { get; set; }
    public Guid ResourceId { get; set; }
    public DateTime BookingDate { get; set; }
    public string UserDepartment { get; set; } = string.Empty;
    public string? DeskZone { get; set; }
    public int UserBookingsThisWeek { get; set; }
    public int UserBookingsThisMonth { get; set; }
}
```

**RuleResult.cs**
```csharp
namespace WorkSpaceManager.Rules.Api.Models;

public class RuleResult
{
    public bool IsValid { get; set; }
    public List<RuleViolation> Violations { get; set; } = new();
}

public class RuleViolation
{
    public string RuleName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public ViolationSeverity Severity { get; set; }
}

public enum ViolationSeverity
{
    Warning,
    Error
}
```

**PolicyConfiguration.cs**
```csharp
namespace WorkSpaceManager.Rules.Api.Models;

public class PolicyConfiguration
{
    public Guid TenantId { get; set; }
    public int MaxDaysPerWeek { get; set; } = 2;
    public int MaxAdvanceBookingDays { get; set; } = 14;
    public bool RestrictToDepartment { get; set; } = false;
    public bool AllowRecurringBookings { get; set; } = true;
    public int NoShowThreshold { get; set; } = 3;
}
```

#### RulesEngineService.cs
```csharp
using NRules;
using NRules.Fluent;
using WorkSpaceManager.Rules.Api.Models;

namespace WorkSpaceManager.Rules.Api.Services;

public interface IRulesEngine
{
    Task<RuleResult> ValidateBookingAsync(BookingContext context, PolicyConfiguration policy);
}

public class RulesEngineService : IRulesEngine
{
    private readonly ISession _session;

    public RulesEngineService()
    {
        var repository = new RuleRepository();
        repository.Load(x => x.From(typeof(RulesEngineService).Assembly));
        
        var factory = repository.Compile();
        _session = factory.CreateSession();
    }

    public Task<RuleResult> ValidateBookingAsync(BookingContext context, PolicyConfiguration policy)
    {
        _session.Insert(context);
        _session.Insert(policy);
        
        _session.Fire();

        var violations = _session.Query<RuleViolation>().ToList();
        
        var result = new RuleResult
        {
            IsValid = !violations.Any(v => v.Severity == ViolationSeverity.Error),
            Violations = violations
        };

        return Task.FromResult(result);
    }
}
```

#### gRPC Proto

**rules.proto**
```protobuf
syntax = "proto3";

option csharp_namespace = "WorkSpaceManager.Rules.Api.Protos";

package rules;

service RulesService {
  rpc ValidateBooking (BookingValidationRequest) returns (BookingValidationResponse);
}

message BookingValidationRequest {
  string user_id = 1;
  string resource_id = 2;
  string booking_date = 3;
  string user_department = 4;
  string desk_zone = 5;
  int32 user_bookings_this_week = 6;
  string tenant_id = 7;
}

message BookingValidationResponse {
  bool is_valid = 1;
  repeated RuleViolation violations = 2;
}

message RuleViolation {
  string rule_name = 1;
  string message = 2;
  int32 severity = 3;
}
```

---

### 4. Booking Service

#### Domain

**Booking.cs**
```csharp
using WorkSpaceManager.Shared.Kernel.BuildingBlocks;
using WorkSpaceManager.Booking.Api.Domain.Events;

namespace WorkSpaceManager.Booking.Api.Domain.Entities;

public class Booking : Entity
{
    public Guid UserId { get; private set; }
    public Guid ResourceId { get; private set; }
    public ResourceType ResourceType { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public BookingStatus Status { get; private set; }
    public string? CancellationReason { get; private set; }
    public DateTime? CheckInTime { get; private set; }

    public Booking(Guid tenantId, Guid userId, Guid resourceId, ResourceType resourceType, 
                   DateTime startTime, DateTime endTime)
    {
        TenantId = tenantId;
        UserId = userId;
        ResourceId = resourceId;
        ResourceType = resourceType;
        StartTime = startTime;
        EndTime = endTime;
        Status = BookingStatus.Confirmed;

        AddDomainEvent(new BookingCreatedEvent(Id, UserId, ResourceId, StartTime, EndTime));
    }

    public void Cancel(string reason)
    {
        if (Status == BookingStatus.Cancelled)
            throw new InvalidOperationException("Booking is already cancelled");

        Status = BookingStatus.Cancelled;
        CancellationReason = reason;

        AddDomainEvent(new BookingCancelledEvent(Id, UserId, ResourceId, reason));
    }

    public void CheckIn()
    {
        if (Status != BookingStatus.Confirmed)
            throw new InvalidOperationException("Cannot check in to a non-confirmed booking");

        Status = BookingStatus.CheckedIn;
        CheckInTime = DateTime.UtcNow;

        AddDomainEvent(new CheckInCompletedEvent(Id, UserId, ResourceId, CheckInTime.Value));
    }

    public void MarkAsNoShow()
    {
        if (Status != BookingStatus.Confirmed)
            return;

        Status = BookingStatus.NoShow;
        AddDomainEvent(new BookingNoShowEvent(Id, UserId, ResourceId));
    }
}

public enum BookingStatus
{
    Confirmed,
    CheckedIn,
    Cancelled,
    NoShow
}

public enum ResourceType
{
    Desk,
    MeetingRoom
}
```

#### Events

**BookingCreatedEvent.cs**
```csharp
using WorkSpaceManager.Shared.Kernel.BuildingBlocks;

namespace WorkSpaceManager.Booking.Api.Domain.Events;

public record BookingCreatedEvent(
    Guid BookingId,
    Guid UserId,
    Guid ResourceId,
    DateTime StartTime,
    DateTime EndTime
) : IDomainEvent;
```

**BookingCancelledEvent.cs**
```csharp
using WorkSpaceManager.Shared.Kernel.BuildingBlocks;

namespace WorkSpaceManager.Booking.Api.Domain.Events;

public record BookingCancelledEvent(
    Guid BookingId,
    Guid UserId,
    Guid ResourceId,
    string Reason
) : IDomainEvent;
```

**CheckInCompletedEvent.cs**
```csharp
using WorkSpaceManager.Shared.Kernel.BuildingBlocks;

namespace WorkSpaceManager.Booking.Api.Domain.Events;

public record CheckInCompletedEvent(
    Guid BookingId,
    Guid UserId,
    Guid ResourceId,
    DateTime CheckInTime
) : IDomainEvent;
```

#### CQRS Commands

**CreateBookingCommand.cs**
```csharp
namespace WorkSpaceManager.Booking.Api.Application.Commands;

public record CreateBookingCommand(
    Guid TenantId,
    Guid UserId,
    Guid ResourceId,
    ResourceType ResourceType,
    DateTime StartTime,
    DateTime EndTime
);

public record CreateBookingResult(
    bool Success,
    Guid? BookingId,
    List<string> Errors
);
```

**CreateBookingHandler.cs**
```csharp
using WorkSpaceManager.Booking.Api.Domain.Entities;
using WorkSpaceManager.Booking.Api.Infrastructure.Data;
using WorkSpaceManager.Booking.Api.Infrastructure.gRPC;

namespace WorkSpaceManager.Booking.Api.Application.Commands;

public class CreateBookingHandler
{
    private readonly BookingDbContext _context;
    private readonly RulesServiceClient _rulesClient;
    private readonly ILogger<CreateBookingHandler> _logger;

    public CreateBookingHandler(
        BookingDbContext context,
        RulesServiceClient rulesClient,
        ILogger<CreateBookingHandler> logger)
    {
        _context = context;
        _rulesClient = rulesClient;
        _logger = logger;
    }

    public async Task<CreateBookingResult> HandleAsync(CreateBookingCommand command)
    {
        try
        {
            // Validate with Rules Service
            var validationResult = await _rulesClient.ValidateBookingAsync(new()
            {
                UserId = command.UserId,
                ResourceId = command.ResourceId,
                BookingDate = command.StartTime,
                TenantId = command.TenantId
            });

            if (!validationResult.IsValid)
            {
                return new CreateBookingResult(
                    false,
                    null,
                    validationResult.Violations.Select(v => v.Message).ToList()
                );
            }

            // Create booking
            var booking = new Booking(
                command.TenantId,
                command.UserId,
                command.ResourceId,
                command.ResourceType,
                command.StartTime,
                command.EndTime
            );

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            // Domain events will be published automatically via DbContext

            return new CreateBookingResult(true, booking.Id, new List<string>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating booking");
            return new CreateBookingResult(false, null, new List<string> { "An error occurred while creating the booking" });
        }
    }
}
```

#### DbContext with Event Publishing

**BookingDbContext.cs**
```csharp
using Microsoft.EntityFrameworkCore;
using WorkSpaceManager.Booking.Api.Domain.Entities;
using WorkSpaceManager.Booking.Api.Infrastructure.Messaging;
using WorkSpaceManager.Shared.Kernel.BuildingBlocks;

namespace WorkSpaceManager.Booking.Api.Infrastructure.Data;

public class BookingDbContext : DbContext
{
    private readonly IEventBus _eventBus;

    public DbSet<Booking> Bookings { get; set; }

    public BookingDbContext(DbContextOptions<BookingDbContext> options, IEventBus eventBus) 
        : base(options)
    {
        _eventBus = eventBus;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.ResourceType).HasConversion<string>();
            entity.HasIndex(e => new { e.TenantId, e.UserId, e.StartTime });
            entity.HasIndex(e => new { e.TenantId, e.ResourceId, e.StartTime });
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entitiesWithEvents = ChangeTracker.Entries<Entity>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Any())
            .ToList();

        var result = await base.SaveChangesAsync(cancellationToken);

        // Publish domain events after successful save
        foreach (var entity in entitiesWithEvents)
        {
            var events = entity.DomainEvents.ToList();
            entity.ClearDomainEvents();

            foreach (var domainEvent in events)
            {
                await _eventBus.PublishAsync(domainEvent);
            }
        }

        return result;
    }
}
```

#### RabbitMQ Event Bus

**IEventBus.cs**
```csharp
using WorkSpaceManager.Shared.Kernel.BuildingBlocks;

namespace WorkSpaceManager.Booking.Api.Infrastructure.Messaging;

public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent @event) where TEvent : IDomainEvent;
}
```

**RabbitMqEventBus.cs**
```csharp
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using WorkSpaceManager.Shared.Kernel.BuildingBlocks;

namespace WorkSpaceManager.Booking.Api.Infrastructure.Messaging;

public class RabbitMqEventBus : IEventBus, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMqEventBus> _logger;

    public RabbitMqEventBus(IConfiguration configuration, ILogger<RabbitMqEventBus> logger)
    {
        _logger = logger;
        
        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMQ:Host"] ?? "localhost",
            Port = int.Parse(configuration["RabbitMQ:Port"] ?? "5672"),
            UserName = configuration["RabbitMQ:Username"] ?? "guest",
            Password = configuration["RabbitMQ:Password"] ?? "guest"
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare("workspace-events", ExchangeType.Topic, durable: true);
    }

    public Task PublishAsync<TEvent>(TEvent @event) where TEvent : IDomainEvent
    {
        var eventName = @event.GetType().Name;
        var message = JsonSerializer.Serialize(@event);
        var body = Encoding.UTF8.GetBytes(message);

        var properties = _channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.ContentType = "application/json";
        properties.Type = eventName;

        _channel.BasicPublish(
            exchange: "workspace-events",
            routingKey: eventName,
            basicProperties: properties,
            body: body
        );

        _logger.LogInformation("Published event {EventName}", eventName);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
    }
}
```

---

### 5. Notification Service

#### Email Service

**IEmailService.cs**
```csharp
namespace WorkSpaceManager.Notification.Api.Services;

public interface IEmailService
{
    Task SendBookingConfirmationAsync(string toEmail, BookingDetails booking);
    Task SendBookingReminderAsync(string toEmail, BookingDetails booking);
    Task SendCancellationNotificationAsync(string toEmail, BookingDetails booking);
}

public record BookingDetails(
    string BookingId,
    string ResourceName,
    DateTime StartTime,
    DateTime EndTime,
    string Location
);
```

**EmailService.cs**
```csharp
using System.Net;
using System.Net.Mail;

namespace WorkSpaceManager.Notification.Api.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly SmtpClient _smtpClient;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        _smtpClient = new SmtpClient
        {
            Host = _configuration["Email:SmtpHost"] ?? "localhost",
            Port = int.Parse(_configuration["Email:SmtpPort"] ?? "1025"),
            EnableSsl = bool.Parse(_configuration["Email:EnableSsl"] ?? "false"),
            Credentials = new NetworkCredential(
                _configuration["Email:Username"],
                _configuration["Email:Password"]
            )
        };
    }

    public async Task SendBookingConfirmationAsync(string toEmail, BookingDetails booking)
    {
        var subject = $"Booking Confirmation - {booking.ResourceName}";
        var body = $@"
            <html>
            <body>
                <h2>Your Booking is Confirmed</h2>
                <p>Dear User,</p>
                <p>Your booking has been successfully confirmed.</p>
                <ul>
                    <li><strong>Resource:</strong> {booking.ResourceName}</li>
                    <li><strong>Date:</strong> {booking.StartTime:dd/MM/yyyy}</li>
                    <li><strong>Time:</strong> {booking.StartTime:HH:mm} - {booking.EndTime:HH:mm}</li>
                    <li><strong>Location:</strong> {booking.Location}</li>
                </ul>
                <p>Please remember to check in on the day of your booking.</p>
            </body>
            </html>
        ";

        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendBookingReminderAsync(string toEmail, BookingDetails booking)
    {
        var subject = $"Reminder: Upcoming Booking - {booking.ResourceName}";
        var body = $@"
            <html>
            <body>
                <h2>Booking Reminder</h2>
                <p>This is a reminder about your upcoming booking tomorrow.</p>
                <ul>
                    <li><strong>Resource:</strong> {booking.ResourceName}</li>
                    <li><strong>Date:</strong> {booking.StartTime:dd/MM/yyyy}</li>
                    <li><strong>Time:</strong> {booking.StartTime:HH:mm} - {booking.EndTime:HH:mm}</li>
                    <li><strong>Location:</strong> {booking.Location}</li>
                </ul>
            </body>
            </html>
        ";

        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendCancellationNotificationAsync(string toEmail, BookingDetails booking)
    {
        var subject = $"Booking Cancelled - {booking.ResourceName}";
        var body = $@"
            <html>
            <body>
                <h2>Booking Cancellation</h2>
                <p>Your booking has been cancelled.</p>
                <ul>
                    <li><strong>Resource:</strong> {booking.ResourceName}</li>
                    <li><strong>Date:</strong> {booking.StartTime:dd/MM/yyyy}</li>
                    <li><strong>Time:</strong> {booking.StartTime:HH:mm} - {booking.EndTime:HH:mm}</li>
                </ul>
            </body>
            </html>
        ";

        await SendEmailAsync(toEmail, subject, body);
    }

    private async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        try
        {
            var message = new MailMessage
            {
                From = new MailAddress(_configuration["Email:FromAddress"] ?? "noreply@workspace.com"),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            message.To.Add(toEmail);

            await _smtpClient.SendMailAsync(message);
            _logger.LogInformation("Email sent to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
            throw;
        }
    }
}
```

#### RabbitMQ Consumers

**BookingCreatedConsumer.cs**
```csharp
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using WorkSpaceManager.Notification.Api.Services;

namespace WorkSpaceManager.Notification.Api.Consumers;

public class BookingCreatedConsumer : BackgroundService
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BookingCreatedConsumer> _logger;

    public BookingCreatedConsumer(
        IConfiguration configuration,
        IServiceProvider serviceProvider,
        ILogger<BookingCreatedConsumer> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;

        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMQ:Host"] ?? "localhost",
            Port = int.Parse(configuration["RabbitMQ:Port"] ?? "5672")
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare("workspace-events", ExchangeType.Topic, durable: true);
        _channel.QueueDeclare("booking-notifications", durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind("booking-notifications", "workspace-events", "BookingCreatedEvent");
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new EventingBasicConsumer(_channel);
        
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            try
            {
                var bookingEvent = JsonSerializer.Deserialize<BookingCreatedEventDto>(message);
                
                using var scope = _serviceProvider.CreateScope();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                // TODO: Fetch user email and resource details
                var bookingDetails = new BookingDetails(
                    bookingEvent!.BookingId.ToString(),
                    "Desk 101",
                    bookingEvent.StartTime,
                    bookingEvent.EndTime,
                    "Floor 3, Building A"
                );

                await emailService.SendBookingConfirmationAsync("user@example.com", bookingDetails);

                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing BookingCreatedEvent");
                _channel.BasicNack(ea.DeliveryTag, false, true);
            }
        };

        _channel.BasicConsume("booking-notifications", false, consumer);

        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}

internal record BookingCreatedEventDto(
    Guid BookingId,
    Guid UserId,
    Guid ResourceId,
    DateTime StartTime,
    DateTime EndTime
);
```

---

## API Gateway Configuration

### appsettings.json
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ReverseProxy": {
    "Routes": {
      "identity-route": {
        "ClusterId": "identity-cluster",
        "Match": {
          "Path": "/api/identity/{**catch-all}"
        },
        "Transforms": [
          { "PathPattern": "/api/{**catch-all}" }
        ]
      },
      "resource-route": {
        "ClusterId": "resource-cluster",
        "Match": {
          "Path": "/api/resources/{**catch-all}"
        },
        "Transforms": [
          { "PathPattern": "/api/{**catch-all}" }
        ]
      },
      "booking-route": {
        "ClusterId": "booking-cluster",
        "Match": {
          "Path": "/api/bookings/{**catch-all}"
        },
        "Transforms": [
          { "PathPattern": "/api/{**catch-all}" }
        ]
      }
    },
    "Clusters": {
      "identity-cluster": {
        "Destinations": {
          "destination1": {
            "Address": "http://localhost:5001"
          }
        }
      },
      "resource-cluster": {
        "Destinations": {
          "destination1": {
            "Address": "http://localhost:5002"
          }
        }
      },
      "booking-cluster": {
        "Destinations": {
          "destination1": {
            "Address": "http://localhost:5003"
          }
        }
      }
    }
  },
  "Jwt": {
    "Authority": "http://localhost:8080/realms/workspace-manager",
    "Audience": "workspace-api"
  }
}
```

### Program.cs
```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

// Add YARP
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Add JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Jwt:Authority"];
        options.Audience = builder.Configuration["Jwt:Audience"];
        options.RequireHttpsMetadata = false;
    });

builder.Services.AddAuthorization();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebClient", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

app.UseCors("AllowWebClient");
app.UseAuthentication();
app.UseAuthorization();
app.MapReverseProxy();

app.Run();
```

---

## Database Schemas

### Identity Service
```sql
-- Managed by Keycloak (PostgreSQL)
-- No custom schema required
```

### Resource Service
```sql
CREATE TABLE Buildings (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Street NVARCHAR(200),
    City NVARCHAR(100),
    PostalCode NVARCHAR(20),
    Country NVARCHAR(100),
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    INDEX IX_Buildings_TenantId (TenantId)
);

CREATE TABLE Floors (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    BuildingId UNIQUEIDENTIFIER NOT NULL,
    FloorNumber INT NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (BuildingId) REFERENCES Buildings(Id),
    INDEX IX_Floors_BuildingId (BuildingId)
);

CREATE TABLE Desks (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    FloorId UNIQUEIDENTIFIER NOT NULL,
    DeskNumber NVARCHAR(50) NOT NULL,
    Zone NVARCHAR(100),
    CoordinateX FLOAT NOT NULL,
    CoordinateY FLOAT NOT NULL,
    IsAvailable BIT NOT NULL DEFAULT 1,
    Type NVARCHAR(50) NOT NULL,
    Amenities NVARCHAR(500),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (FloorId) REFERENCES Floors(Id),
    INDEX IX_Desks_FloorId (FloorId),
    INDEX IX_Desks_TenantId_IsAvailable (TenantId, IsAvailable)
);

CREATE TABLE MeetingRooms (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    FloorId UNIQUEIDENTIFIER NOT NULL,
    RoomNumber NVARCHAR(50) NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Capacity INT NOT NULL,
    CoordinateX FLOAT NOT NULL,
    CoordinateY FLOAT NOT NULL,
    Equipment NVARCHAR(500),
    IsAvailable BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (FloorId) REFERENCES Floors(Id),
    INDEX IX_MeetingRooms_FloorId (FloorId)
);

CREATE TABLE FloorPlans (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    FloorId UNIQUEIDENTIFIER NOT NULL,
    ImageUrl NVARCHAR(500) NOT NULL,
    MappingJson NVARCHAR(MAX) NOT NULL,
    UploadedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UploadedBy NVARCHAR(100) NOT NULL,
    FOREIGN KEY (FloorId) REFERENCES Floors(Id)
);
```

### Booking Service
```sql
CREATE TABLE Bookings (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    UserId UNIQUEIDENTIFIER NOT NULL,
    ResourceId UNIQUEIDENTIFIER NOT NULL,
    ResourceType NVARCHAR(50) NOT NULL,
    StartTime DATETIME2 NOT NULL,
    EndTime DATETIME2 NOT NULL,
    Status NVARCHAR(50) NOT NULL,
    CancellationReason NVARCHAR(500),
    CheckInTime DATETIME2,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    INDEX IX_Bookings_TenantId_UserId_StartTime (TenantId, UserId, StartTime),
    INDEX IX_Bookings_TenantId_ResourceId_StartTime (TenantId, ResourceId, StartTime),
    INDEX IX_Bookings_Status (Status)
);
```

---

## React Web Client Implementation

### Key Files

#### src/services/api.ts
```typescript
import axios from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000';

export const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add auth token to requests
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('access_token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Handle token expiration
api.interceptors.response.use(
  (response) => response,
  async (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('access_token');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);
```

#### src/services/bookingService.ts
```typescript
import { api } from './api';

export interface Booking {
  id: string;
  userId: string;
  resourceId: string;
  resourceType: 'Desk' | 'MeetingRoom';
  startTime: string;
  endTime: string;
  status: 'Confirmed' | 'CheckedIn' | 'Cancelled' | 'NoShow';
}

export interface CreateBookingRequest {
  tenantId: string;
  userId: string;
  resourceId: string;
  resourceType: 'Desk' | 'MeetingRoom';
  startTime: string;
  endTime: string;
}

export const bookingService = {
  async getMyBookings(): Promise<Booking[]> {
    const response = await api.get('/api/bookings/my-bookings');
    return response.data;
  },

  async createBooking(request: CreateBookingRequest): Promise<Booking> {
    const response = await api.post('/api/bookings', request);
    return response.data;
  },

  async cancelBooking(bookingId: string, reason: string): Promise<void> {
    await api.post(`/api/bookings/${bookingId}/cancel`, { reason });
  },

  async checkIn(bookingId: string): Promise<void> {
    await api.post(`/api/bookings/${bookingId}/check-in`);
  },
};
```

#### src/components/Booking/DeskSelector.tsx
```typescript
import React, { useState, useEffect } from 'react';
import { resourceService } from '@/services/resourceService';

interface Desk {
  id: string;
  deskNumber: string;
  zone: string;
  isAvailable: boolean;
  type: string;
}

interface DeskSelectorProps {
  floorId: string;
  selectedDate: Date;
  onSelectDesk: (deskId: string) => void;
}

export const DeskSelector: React.FC<DeskSelectorProps> = ({
  floorId,
  selectedDate,
  onSelectDesk,
}) => {
  const [desks, setDesks] = useState<Desk[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchDesks = async () => {
      setLoading(true);
      try {
        const availableDesks = await resourceService.getAvailableDesks(
          floorId,
          selectedDate
        );
        setDesks(availableDesks);
      } catch (error) {
        console.error('Failed to fetch desks:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchDesks();
  }, [floorId, selectedDate]);

  if (loading) {
    return <div>Loading desks...</div>;
  }

  return (
    <div className="grid grid-cols-4 gap-4">
      {desks.map((desk) => (
        <button
          key={desk.id}
          onClick={() => onSelectDesk(desk.id)}
          className={`p-4 border rounded-lg ${
            desk.isAvailable
              ? 'bg-green-100 hover:bg-green-200'
              : 'bg-gray-200 cursor-not-allowed'
          }`}
          disabled={!desk.isAvailable}
        >
          <div className="font-bold">{desk.deskNumber}</div>
          <div className="text-sm text-gray-600">{desk.zone}</div>
          <div className="text-xs">{desk.type}</div>
        </button>
      ))}
    </div>
  );
};
```

#### src/pages/BookDesk.tsx
```typescript
import React, { useState } from 'react';
import { DeskSelector } from '@/components/Booking/DeskSelector';
import { bookingService } from '@/services/bookingService';
import { useAuth } from '@/hooks/useAuth';

export const BookDesk: React.FC = () => {
  const { user } = useAuth();
  const [selectedDate, setSelectedDate] = useState(new Date());
  const [selectedFloor, setSelectedFloor] = useState('');
  const [selectedDesk, setSelectedDesk] = useState('');
  const [loading, setLoading] = useState(false);

  const handleBooking = async () => {
    if (!selectedDesk || !user) return;

    setLoading(true);
    try {
      await bookingService.createBooking({
        tenantId: user.tenantId,
        userId: user.id,
        resourceId: selectedDesk,
        resourceType: 'Desk',
        startTime: selectedDate.toISOString(),
        endTime: new Date(selectedDate.getTime() + 8 * 60 * 60 * 1000).toISOString(),
      });

      alert('Booking successful!');
    } catch (error) {
      console.error('Booking failed:', error);
      alert('Booking failed. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="container mx-auto p-6">
      <h1 className="text-3xl font-bold mb-6">Book a Desk</h1>

      <div className="mb-6">
        <label className="block mb-2">Select Date</label>
        <input
          type="date"
          value={selectedDate.toISOString().split('T')[0]}
          onChange={(e) => setSelectedDate(new Date(e.target.value))}
          className="border p-2 rounded"
        />
      </div>

      {selectedFloor && (
        <DeskSelector
          floorId={selectedFloor}
          selectedDate={selectedDate}
          onSelectDesk={setSelectedDesk}
        />
      )}

      {selectedDesk && (
        <button
          onClick={handleBooking}
          disabled={loading}
          className="mt-6 bg-blue-600 text-white px-6 py-3 rounded-lg hover:bg-blue-700 disabled:bg-gray-400"
        >
          {loading ? 'Booking...' : 'Confirm Booking'}
        </button>
      )}
    </div>
  );
};
```

#### src/i18n/el.json
```json
{
  "common": {
    "welcome": "Καλώς ήρθατε",
    "logout": "Αποσύνδεση",
    "save": "Αποθήκευση",
    "cancel": "Ακύρωση"
  },
  "booking": {
    "title": "Κράτηση Θέσης",
    "selectDate": "Επιλέξτε Ημερομηνία",
    "selectDesk": "Επιλέξτε Θέση",
    "confirm": "Επιβεβαίωση Κράτησης",
    "success": "Η κράτηση ολοκληρώθηκε επιτυχώς",
    "error": "Η κράτηση απέτυχε"
  },
  "myBookings": {
    "title": "Οι Κρατήσεις μου",
    "noBookings": "Δεν έχετε κρατήσεις",
    "checkIn": "Check-in",
    "cancelBooking": "Ακύρωση"
  }
}
```

---

## React Native Mobile App

### Key Files

#### src/screens/HomeScreen.tsx
```typescript
import React, { useEffect, useState } from 'react';
import { View, Text, FlatList, TouchableOpacity, StyleSheet } from 'react-native';
import { useNavigation } from '@react-navigation/native';
import { bookingService } from '../services/bookingService';

export const HomeScreen = () => {
  const navigation = useNavigation();
  const [bookings, setBookings] = useState([]);

  useEffect(() => {
    loadBookings();
  }, []);

  const loadBookings = async () => {
    try {
      const data = await bookingService.getMyBookings();
      setBookings(data);
    } catch (error) {
      console.error('Failed to load bookings:', error);
    }
  };

  return (
    <View style={styles.container}>
      <Text style={styles.title}>My Bookings</Text>
      
      <FlatList
        data={bookings}
        keyExtractor={(item) => item.id}
        renderItem={({ item }) => (
          <View style={styles.bookingCard}>
            <Text style={styles.resourceName}>Desk {item.resourceId}</Text>
            <Text>{new Date(item.startTime).toLocaleDateString()}</Text>
            <Text style={styles.status}>{item.status}</Text>
          </View>
        )}
      />

      <TouchableOpacity
        style={styles.fab}
        onPress={() => navigation.navigate('Booking')}
      >
        <Text style={styles.fabText}>+</Text>
      </TouchableOpacity>
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    padding: 16,
    backgroundColor: '#f5f5f5',
  },
  title: {
    fontSize: 24,
    fontWeight: 'bold',
    marginBottom: 16,
  },
  bookingCard: {
    backgroundColor: 'white',
    padding: 16,
    borderRadius: 8,
    marginBottom: 12,
    elevation: 2,
  },
  resourceName: {
    fontSize: 18,
    fontWeight: '600',
  },
  status: {
    marginTop: 8,
    color: '#4CAF50',
    fontWeight: '500',
  },
  fab: {
    position: 'absolute',
    right: 20,
    bottom: 20,
    width: 60,
    height: 60,
    borderRadius: 30,
    backgroundColor: '#2196F3',
    justifyContent: 'center',
    alignItems: 'center',
    elevation: 5,
  },
  fabText: {
    fontSize: 30,
    color: 'white',
  },
});
```

#### src/services/storageService.ts
```typescript
import AsyncStorage from '@react-native-async-storage/async-storage';

export const storageService = {
  async saveToken(token: string): Promise<void> {
    await AsyncStorage.setItem('access_token', token);
  },

  async getToken(): Promise<string | null> {
    return await AsyncStorage.getItem('access_token');
  },

  async removeToken(): Promise<void> {
    await AsyncStorage.removeItem('access_token');
  },

  async saveBookingsCache(bookings: any[]): Promise<void> {
    await AsyncStorage.setItem('cached_bookings', JSON.stringify(bookings));
  },

  async getBookingsCache(): Promise<any[]> {
    const cached = await AsyncStorage.getItem('cached_bookings');
    return cached ? JSON.parse(cached) : [];
  },
};
```

---

## Deployment Guide

### Prerequisites
- Docker & Docker Compose
- .NET 8 SDK
- Node.js 20+
- SQL Server 2022

### Step 1: Infrastructure Setup
```bash
# Start infrastructure services
cd WorkSpaceManager
docker-compose up -d

# Wait for services to be healthy
docker-compose ps
```

### Step 2: Configure Keycloak
1. Access Keycloak Admin Console: `http://localhost:8080`
2. Create realm: `workspace-manager`
3. Create client: `workspace-api`
4. Configure client settings:
   - Access Type: confidential
   - Service Accounts Enabled: ON
   - Authorization Enabled: ON
5. Create roles: `Admin`, `User`, `Manager`
6. Copy client secret to appsettings.json

### Step 3: Database Migrations
```bash
# Resource Service
cd src/Services/Resource/WorkSpaceManager.Resource.Api
dotnet ef migrations add InitialCreate
dotnet ef database update

# Booking Service
cd ../../../Booking/WorkSpaceManager.Booking.Api
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Step 4: Build & Run Backend Services
```bash
# Build all services
dotnet build WorkSpaceManager.sln

# Run services (in separate terminals)
cd src/Services/Identity/WorkSpaceManager.Identity.Api
dotnet run --urls="http://localhost:5001"

cd src/Services/Resource/WorkSpaceManager.Resource.Api
dotnet run --urls="http://localhost:5002"

cd src/Services/Booking/WorkSpaceManager.Booking.Api
dotnet run --urls="http://localhost:5003"

cd src/Services/Rules/WorkSpaceManager.Rules.Api
dotnet run --urls="http://localhost:5004"

cd src/Services/Notification/WorkSpaceManager.Notification.Api
dotnet run --urls="http://localhost:5005"

# Run API Gateway
cd src/WorkSpaceManager.Gateway
dotnet run --urls="http://localhost:5000"
```

### Step 5: Run Web Client
```bash
cd web-client
npm install
npm run dev
```

### Step 6: Run Mobile App
```bash
cd mobile-app
npm install

# For iOS
npx pod-install
npx react-native run-ios

# For Android
npx react-native run-android
```

---

## Testing Strategy

### Unit Tests
```bash
# Run all unit tests
dotnet test WorkSpaceManager.sln
```

### Integration Tests
```csharp
// Example: Booking Integration Test
[Fact]
public async Task CreateBooking_ShouldValidateRules_AndPublishEvent()
{
    // Arrange
    var bookingCommand = new CreateBookingCommand(
        TenantId: Guid.NewGuid(),
        UserId: Guid.NewGuid(),
        ResourceId: Guid.NewGuid(),
        ResourceType: ResourceType.Desk,
        StartTime: DateTime.UtcNow.AddDays(1),
        EndTime: DateTime.UtcNow.AddDays(1).AddHours(8)
    );

    // Act
    var result = await _bookingHandler.HandleAsync(bookingCommand);

    // Assert
    Assert.True(result.Success);
    Assert.NotNull(result.BookingId);
    
    // Verify event was published
    _eventBusMock.Verify(x => x.PublishAsync(
        It.IsAny<BookingCreatedEvent>()), Times.Once);
}
```

### API Tests (Postman/REST Client)
```http
### Create Booking
POST http://localhost:5000/api/bookings
Authorization: Bearer {{access_token}}
Content-Type: application/json

{
  "tenantId": "{{tenantId}}",
  "userId": "{{userId}}",
  "resourceId": "{{deskId}}",
  "resourceType": "Desk",
  "startTime": "2025-12-11T09:00:00Z",
  "endTime": "2025-12-11T17:00:00Z"
}

### Get My Bookings
GET http://localhost:5000/api/bookings/my-bookings
Authorization: Bearer {{access_token}}
```

---

## Επόμενα Βήματα

1. **Phase 1 (Weeks 1-2):** Complete all backend microservices implementation
2. **Phase 2 (Weeks 3-4):** Develop React Web Client with all features
3. **Phase 3 (Weeks 5-6):** Build React Native Mobile App
4. **Phase 4 (Week 7):** Integration testing and bug fixes
5. **Phase 5 (Week 8):** Performance optimization and deployment

---

## Συμπέρασμα

Αυτός ο οδηγός παρέχει μια ολοκληρωμένη βάση για την υλοποίηση του συστήματος WorkSpaceManager. Κάθε service έχει σχεδιαστεί να είναι ανεξάρτητο, επεκτάσιμο και εύκολο στη συντήρηση, ακολουθώντας τις βέλτιστες πρακτικές της αρχιτεκτονικής microservices.

**Σημαντικές Σημειώσεις:**
- Όλα τα services χρησιμοποιούν JWT authentication μέσω Keycloak
- Η επικοινωνία μεταξύ των services γίνεται μέσω gRPC (synchronous) και RabbitMQ (asynchronous)
- Το Booking Service ακολουθεί CQRS pattern για καλύτερη απόδοση
- Το Rules Service χρησιμοποιεί NRules για ευέλικτη διαχείριση επιχειρησιακών κανόνων
- Όλα τα services υποστηρίζουν multitenancy μέσω του TenantId field

---

**Έκδοση:** 1.0  
**Ημερομηνία:** Δεκέμβριος 2025  
**Συγγραφέας:** Manus AI
