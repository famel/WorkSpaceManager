# WorkSpaceManager - Complete Implementation Guide

This guide provides all the code and instructions needed to complete the full-stack WorkSpaceManager application.

## 📦 What's Already Implemented

### ✅ Booking Service (100% Complete)
- ✅ Entity models and database context
- ✅ Complete business logic service (1000+ lines)
- ✅ REST API controller with all endpoints
- ✅ Authentication and authorization
- ✅ Configuration and project files

**Location:** `/src/Services/BookingService/`

**Endpoints:**
- `POST /api/bookings` - Create booking
- `GET /api/bookings/{id}` - Get booking
- `GET /api/bookings/my-bookings` - Get user's bookings
- `GET /api/bookings/upcoming` - Get upcoming bookings
- `POST /api/bookings/search` - Search bookings (admin)
- `PUT /api/bookings/{id}` - Update booking
- `POST /api/bookings/{id}/cancel` - Cancel booking
- `POST /api/bookings/{id}/checkin` - Check in
- `POST /api/bookings/{id}/checkout` - Check out
- `POST /api/bookings/check-availability` - Check availability

### ✅ Shared Libraries (100% Complete)
- ✅ Entity models (Booking, Desk, MeetingRoom, Floor, Building, User)
- ✅ DTOs for all operations
- ✅ Common utilities (ApiResponse, PagedResponse)

**Location:** `/src/Shared/`

## 🚧 Implementation Tasks

### Task 1: Space Management Service

Create the following files following the **exact same pattern** as Booking Service:

#### 1.1 Database Context
**File:** `/src/Services/SpaceManagementService/Data/SpaceDbContext.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using WorkSpaceManager.Shared.Models;

namespace WorkSpaceManager.SpaceManagementService.Data;

public class SpaceDbContext : DbContext
{
    public SpaceDbContext(DbContextOptions<SpaceDbContext> options) : base(options) { }

    public DbSet<Building> Buildings { get; set; }
    public DbSet<Floor> Floors { get; set; }
    public DbSet<Desk> Desks { get; set; }
    public DbSet<MeetingRoom> MeetingRooms { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Building
        modelBuilder.Entity<Building>(entity =>
        {
            entity.HasIndex(e => e.TenantId);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Configure Floor
        modelBuilder.Entity<Floor>(entity =>
        {
            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => e.BuildingId);
            entity.HasQueryFilter(e => !e.IsDeleted);
            
            entity.HasOne(e => e.Building)
                .WithMany(b => b.Floors)
                .HasForeignKey(e => e.BuildingId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Desk
        modelBuilder.Entity<Desk>(entity =>
        {
            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => e.FloorId);
            entity.HasQueryFilter(e => !e.IsDeleted);
            
            entity.HasOne(e => e.Floor)
                .WithMany(f => f.Desks)
                .HasForeignKey(e => e.FloorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure MeetingRoom
        modelBuilder.Entity<MeetingRoom>(entity =>
        {
            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => e.FloorId);
            entity.HasQueryFilter(e => !e.IsDeleted);
            
            entity.HasOne(e => e.Floor)
                .WithMany(f => f.MeetingRooms)
                .HasForeignKey(e => e.FloorId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is BaseEntity && 
                   (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var entity = (BaseEntity)entry.Entity;
            entity.UpdatedAt = DateTime.UtcNow;

            if (entry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTime.UtcNow;
            }
        }
    }
}
```

#### 1.2 Building Service Interface
**File:** `/src/Services/SpaceManagementService/Services/IBuildingService.cs`

```csharp
using WorkSpaceManager.Shared.Common;
using WorkSpaceManager.Shared.DTOs;

namespace WorkSpaceManager.SpaceManagementService.Services;

public interface IBuildingService
{
    Task<ApiResponse<BuildingResponse>> CreateBuildingAsync(Guid tenantId, CreateBuildingRequest request);
    Task<ApiResponse<BuildingResponse>> GetBuildingByIdAsync(Guid id, Guid tenantId);
    Task<ApiResponse<PagedResponse<BuildingResponse>>> GetBuildingsAsync(Guid tenantId, int pageNumber = 1, int pageSize = 20);
    Task<ApiResponse<BuildingResponse>> UpdateBuildingAsync(Guid id, Guid tenantId, UpdateBuildingRequest request);
    Task<ApiResponse<bool>> DeleteBuildingAsync(Guid id, Guid tenantId);
}
```

#### 1.3 Building Service Implementation
**File:** `/src/Services/SpaceManagementService/Services/BuildingService.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WorkSpaceManager.SpaceManagementService.Data;
using WorkSpaceManager.Shared.Common;
using WorkSpaceManager.Shared.DTOs;
using WorkSpaceManager.Shared.Models;

namespace WorkSpaceManager.SpaceManagementService.Services;

public class BuildingService : IBuildingService
{
    private readonly SpaceDbContext _context;
    private readonly ILogger<BuildingService> _logger;

    public BuildingService(SpaceDbContext context, ILogger<BuildingService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<BuildingResponse>> CreateBuildingAsync(Guid tenantId, CreateBuildingRequest request)
    {
        try
        {
            var building = new Building
            {
                TenantId = tenantId,
                Name = request.Name,
                Address = request.Address,
                TotalFloors = request.TotalFloors,
                IsActive = true
            };

            _context.Buildings.Add(building);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Building created: {BuildingId}", building.Id);

            return ApiResponse<BuildingResponse>.SuccessResponse(
                MapToResponse(building),
                "Building created successfully"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating building");
            return ApiResponse<BuildingResponse>.ErrorResponse("An error occurred while creating the building");
        }
    }

    public async Task<ApiResponse<BuildingResponse>> GetBuildingByIdAsync(Guid id, Guid tenantId)
    {
        try
        {
            var building = await _context.Buildings
                .Include(b => b.Floors)
                .FirstOrDefaultAsync(b => b.Id == id && b.TenantId == tenantId);

            if (building == null)
            {
                return ApiResponse<BuildingResponse>.ErrorResponse("Building not found");
            }

            return ApiResponse<BuildingResponse>.SuccessResponse(MapToResponse(building));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving building {BuildingId}", id);
            return ApiResponse<BuildingResponse>.ErrorResponse("An error occurred while retrieving the building");
        }
    }

    public async Task<ApiResponse<PagedResponse<BuildingResponse>>> GetBuildingsAsync(
        Guid tenantId, int pageNumber = 1, int pageSize = 20)
    {
        try
        {
            var query = _context.Buildings
                .Include(b => b.Floors)
                .Where(b => b.TenantId == tenantId);

            var totalCount = await query.CountAsync();
            var buildings = await query
                .OrderBy(b => b.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var responses = buildings.Select(MapToResponse).ToList();
            var pagedResponse = new PagedResponse<BuildingResponse>(
                responses, totalCount, pageNumber, pageSize);

            return ApiResponse<PagedResponse<BuildingResponse>>.SuccessResponse(pagedResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving buildings");
            return ApiResponse<PagedResponse<BuildingResponse>>.ErrorResponse(
                "An error occurred while retrieving buildings");
        }
    }

    public async Task<ApiResponse<BuildingResponse>> UpdateBuildingAsync(
        Guid id, Guid tenantId, UpdateBuildingRequest request)
    {
        try
        {
            var building = await _context.Buildings
                .FirstOrDefaultAsync(b => b.Id == id && b.TenantId == tenantId);

            if (building == null)
            {
                return ApiResponse<BuildingResponse>.ErrorResponse("Building not found");
            }

            if (request.Name != null) building.Name = request.Name;
            if (request.Address != null) building.Address = request.Address;
            if (request.TotalFloors.HasValue) building.TotalFloors = request.TotalFloors.Value;
            if (request.IsActive.HasValue) building.IsActive = request.IsActive.Value;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Building updated: {BuildingId}", id);

            return ApiResponse<BuildingResponse>.SuccessResponse(
                MapToResponse(building),
                "Building updated successfully"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating building {BuildingId}", id);
            return ApiResponse<BuildingResponse>.ErrorResponse("An error occurred while updating the building");
        }
    }

    public async Task<ApiResponse<bool>> DeleteBuildingAsync(Guid id, Guid tenantId)
    {
        try
        {
            var building = await _context.Buildings
                .FirstOrDefaultAsync(b => b.Id == id && b.TenantId == tenantId);

            if (building == null)
            {
                return ApiResponse<bool>.ErrorResponse("Building not found");
            }

            building.IsDeleted = true;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Building deleted: {BuildingId}", id);

            return ApiResponse<bool>.SuccessResponse(true, "Building deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting building {BuildingId}", id);
            return ApiResponse<bool>.ErrorResponse("An error occurred while deleting the building");
        }
    }

    private BuildingResponse MapToResponse(Building building)
    {
        return new BuildingResponse
        {
            Id = building.Id,
            Name = building.Name,
            Address = building.Address,
            TotalFloors = building.TotalFloors,
            IsActive = building.IsActive,
            FloorsCount = building.Floors?.Count ?? 0,
            TotalDesks = building.Floors?.SelectMany(f => f.Desks).Count() ?? 0,
            TotalMeetingRooms = building.Floors?.SelectMany(f => f.MeetingRooms).Count() ?? 0,
            CreatedAt = building.CreatedAt
        };
    }
}
```

**Continue this pattern for:**
- `IFloorService` and `FloorService`
- `IDeskService` and `DeskService`
- `IMeetingRoomService` and `MeetingRoomService`

#### 1.4 Controllers
**File:** `/src/Services/SpaceManagementService/Controllers/BuildingsController.cs`

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WorkSpaceManager.Shared.DTOs;
using WorkSpaceManager.SpaceManagementService.Services;

namespace WorkSpaceManager.SpaceManagementService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BuildingsController : ControllerBase
{
    private readonly IBuildingService _buildingService;

    public BuildingsController(IBuildingService buildingService)
    {
        _buildingService = buildingService;
    }

    [HttpPost]
    [Authorize(Roles = "admin,facility_manager")]
    public async Task<IActionResult> CreateBuilding([FromBody] CreateBuildingRequest request)
    {
        var tenantId = GetTenantId();
        var result = await _buildingService.CreateBuildingAsync(tenantId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetBuilding(Guid id)
    {
        var tenantId = GetTenantId();
        var result = await _buildingService.GetBuildingByIdAsync(id, tenantId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetBuildings([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var tenantId = GetTenantId();
        var result = await _buildingService.GetBuildingsAsync(tenantId, pageNumber, pageSize);
        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "admin,facility_manager")]
    public async Task<IActionResult> UpdateBuilding(Guid id, [FromBody] UpdateBuildingRequest request)
    {
        var tenantId = GetTenantId();
        var result = await _buildingService.UpdateBuildingAsync(id, tenantId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> DeleteBuilding(Guid id)
    {
        var tenantId = GetTenantId();
        var result = await _buildingService.DeleteBuildingAsync(id, tenantId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    private Guid GetTenantId()
    {
        var tenantIdClaim = User.FindFirst("tenant_id")?.Value;
        if (string.IsNullOrEmpty(tenantIdClaim) || !Guid.TryParse(tenantIdClaim, out var tenantId))
        {
            throw new UnauthorizedAccessException("Tenant ID not found in token");
        }
        return tenantId;
    }
}
```

**Create similar controllers for:**
- `FloorsController`
- `DesksController`
- `MeetingRoomsController`

#### 1.5 Program.cs
**File:** `/src/Services/SpaceManagementService/Program.cs`

Copy from Booking Service `Program.cs` and replace:
- `BookingDbContext` → `SpaceDbContext`
- `IBookingService, BookingService` → Register all space services

#### 1.6 Project File
**File:** `/src/Services/SpaceManagementService/SpaceManagementService.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <RootNamespace>WorkSpaceManager.SpaceManagementService</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.0" />
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\Shared\Models\Models.csproj" />
    <ProjectReference Include="..\..\Shared\DTOs\DTOs.csproj" />
    <ProjectReference Include="..\..\Shared\Common\Common.csproj" />
  </ItemGroup>
</Project>
```

### Task 2: API Gateway

#### 2.1 Ocelot Configuration
**File:** `/src/Services/ApiGateway/ocelot.json`

```json
{
  "Routes": [
    {
      "DownstreamPathTemplate": "/api/bookings/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        {
          "Host": "booking-service",
          "Port": 80
        }
      ],
      "UpstreamPathTemplate": "/api/bookings/{everything}",
      "UpstreamHttpMethod": [ "GET", "POST", "PUT", "DELETE" ],
      "AuthenticationOptions": {
        "AuthenticationProviderKey": "Bearer",
        "AllowedScopes": []
      }
    },
    {
      "DownstreamPathTemplate": "/api/buildings/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        {
          "Host": "space-service",
          "Port": 80
        }
      ],
      "UpstreamPathTemplate": "/api/buildings/{everything}",
      "UpstreamHttpMethod": [ "GET", "POST", "PUT", "DELETE" ],
      "AuthenticationOptions": {
        "AuthenticationProviderKey": "Bearer"
      }
    },
    {
      "DownstreamPathTemplate": "/api/floors/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        {
          "Host": "space-service",
          "Port": 80
        }
      ],
      "UpstreamPathTemplate": "/api/floors/{everything}",
      "UpstreamHttpMethod": [ "GET", "POST", "PUT", "DELETE" ],
      "AuthenticationOptions": {
        "AuthenticationProviderKey": "Bearer"
      }
    },
    {
      "DownstreamPathTemplate": "/api/desks/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        {
          "Host": "space-service",
          "Port": 80
        }
      ],
      "UpstreamPathTemplate": "/api/desks/{everything}",
      "UpstreamHttpMethod": [ "GET", "POST", "PUT", "DELETE" ],
      "AuthenticationOptions": {
        "AuthenticationProviderKey": "Bearer"
      }
    },
    {
      "DownstreamPathTemplate": "/api/meetingrooms/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        {
          "Host": "space-service",
          "Port": 80
        }
      ],
      "UpstreamPathTemplate": "/api/meetingrooms/{everything}",
      "UpstreamHttpMethod": [ "GET", "POST", "PUT", "DELETE" ],
      "AuthenticationOptions": {
        "AuthenticationProviderKey": "Bearer"
      }
    }
  ],
  "GlobalConfiguration": {
    "BaseUrl": "http://localhost:5000"
  }
}
```

#### 2.2 API Gateway Program.cs
**File:** `/src/Services/ApiGateway/Program.cs`

```csharp
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = builder.Configuration["Keycloak:Authority"];
        options.Audience = builder.Configuration["Keycloak:Audience"];
        options.RequireHttpsMetadata = false;
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

builder.Services.AddOcelot();

var app = builder.Build();

app.UseCors("AllowAll");
await app.UseOcelot();

app.Run();
```

#### 2.3 API Gateway Project File
**File:** `/src/Services/ApiGateway/ApiGateway.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Ocelot" Version="20.0.0" />
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
  </ItemGroup>
</Project>
```

### Task 3: Docker Compose

**File:** `/docker-compose.yml`

```yaml
version: '3.8'

services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2019-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrong@Passw0rd
    ports:
      - "1433:1433"
    volumes:
      - sqldata:/var/opt/mssql

  keycloak:
    image: quay.io/keycloak/keycloak:22.0
    command: start-dev
    environment:
      - KEYCLOAK_ADMIN=admin
      - KEYCLOAK_ADMIN_PASSWORD=admin
    ports:
      - "8080:8080"

  booking-service:
    build:
      context: .
      dockerfile: src/Services/BookingService/Dockerfile
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=WorkSpaceManager;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;
      - Keycloak__Authority=http://keycloak:8080
    ports:
      - "5001:80"
    depends_on:
      - sqlserver
      - keycloak

  space-service:
    build:
      context: .
      dockerfile: src/Services/SpaceManagementService/Dockerfile
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=WorkSpaceManager;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;
      - Keycloak__Authority=http://keycloak:8080
    ports:
      - "5002:80"
    depends_on:
      - sqlserver
      - keycloak

  api-gateway:
    build:
      context: .
      dockerfile: src/Services/ApiGateway/Dockerfile
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - Keycloak__Authority=http://keycloak:8080
    ports:
      - "5000:80"
    depends_on:
      - booking-service
      - space-service

  webapp:
    build:
      context: ./src/Web/ReactApp
      dockerfile: Dockerfile
    ports:
      - "3000:80"
    depends_on:
      - api-gateway

volumes:
  sqldata:
```

### Task 4: React Web App

#### 4.1 API Client
**File:** `/src/Web/ReactApp/src/services/api.ts`

```typescript
import axios from 'axios';

const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:5000';

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add auth token to requests
apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('access_token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export default apiClient;
```

#### 4.2 Booking Service
**File:** `/src/Web/ReactApp/src/services/bookingService.ts`

```typescript
import apiClient from './api';

export interface CreateBookingRequest {
  deskId?: string;
  meetingRoomId?: string;
  bookingDate: string;
  startTime: string;
  endTime: string;
  purpose?: string;
}

export interface Booking {
  id: string;
  userId: string;
  deskId?: string;
  meetingRoomId?: string;
  bookingDate: string;
  startTime: string;
  endTime: string;
  status: string;
  purpose?: string;
  deskNumber?: string;
  meetingRoomName?: string;
  floorName?: string;
  buildingName?: string;
}

export const bookingService = {
  async createBooking(request: CreateBookingRequest) {
    const response = await apiClient.post('/api/bookings', request);
    return response.data;
  },

  async getMyBookings(pageNumber = 1, pageSize = 20) {
    const response = await apiClient.get('/api/bookings/my-bookings', {
      params: { pageNumber, pageSize },
    });
    return response.data;
  },

  async getUpcomingBookings(days = 7) {
    const response = await apiClient.get('/api/bookings/upcoming', {
      params: { days },
    });
    return response.data;
  },

  async checkIn(bookingId: string) {
    const response = await apiClient.post(`/api/bookings/${bookingId}/checkin`);
    return response.data;
  },

  async checkOut(bookingId: string) {
    const response = await apiClient.post(`/api/bookings/${bookingId}/checkout`);
    return response.data;
  },

  async cancelBooking(bookingId: string, reason?: string) {
    const response = await apiClient.post(`/api/bookings/${bookingId}/cancel`, { reason });
    return response.data;
  },

  async checkAvailability(request: any) {
    const response = await apiClient.post('/api/bookings/check-availability', request);
    return response.data;
  },
};
```

#### 4.3 Space Service
**File:** `/src/Web/ReactApp/src/services/spaceService.ts`

```typescript
import apiClient from './api';

export const spaceService = {
  async getBuildings(pageNumber = 1, pageSize = 20) {
    const response = await apiClient.get('/api/buildings', {
      params: { pageNumber, pageSize },
    });
    return response.data;
  },

  async getFloors(buildingId: string) {
    const response = await apiClient.get(`/api/buildings/${buildingId}/floors`);
    return response.data;
  },

  async getDesks(floorId: string) {
    const response = await apiClient.get(`/api/floors/${floorId}/desks`);
    return response.data;
  },

  async getMeetingRooms(floorId: string) {
    const response = await apiClient.get(`/api/floors/${floorId}/meetingrooms`);
    return response.data;
  },
};
```

#### 4.4 Booking Form Component
**File:** `/src/Web/ReactApp/src/components/BookingForm.tsx`

```typescript
import React, { useState } from 'react';
import { bookingService } from '../services/bookingService';

export const BookingForm: React.FC = () => {
  const [formData, setFormData] = useState({
    deskId: '',
    bookingDate: '',
    startTime: '09:00',
    endTime: '17:00',
    purpose: '',
  });

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await bookingService.createBooking(formData);
      alert('Booking created successfully!');
    } catch (error) {
      alert('Error creating booking');
    }
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      <div>
        <label className="block text-sm font-medium">Booking Date</label>
        <input
          type="date"
          value={formData.bookingDate}
          onChange={(e) => setFormData({ ...formData, bookingDate: e.target.value })}
          className="mt-1 block w-full rounded-md border-gray-300 shadow-sm"
          required
        />
      </div>

      <div className="grid grid-cols-2 gap-4">
        <div>
          <label className="block text-sm font-medium">Start Time</label>
          <input
            type="time"
            value={formData.startTime}
            onChange={(e) => setFormData({ ...formData, startTime: e.target.value })}
            className="mt-1 block w-full rounded-md border-gray-300 shadow-sm"
            required
          />
        </div>

        <div>
          <label className="block text-sm font-medium">End Time</label>
          <input
            type="time"
            value={formData.endTime}
            onChange={(e) => setFormData({ ...formData, endTime: e.target.value })}
            className="mt-1 block w-full rounded-md border-gray-300 shadow-sm"
            required
          />
        </div>
      </div>

      <div>
        <label className="block text-sm font-medium">Purpose (Optional)</label>
        <textarea
          value={formData.purpose}
          onChange={(e) => setFormData({ ...formData, purpose: e.target.value })}
          className="mt-1 block w-full rounded-md border-gray-300 shadow-sm"
          rows={3}
        />
      </div>

      <button
        type="submit"
        className="w-full bg-blue-600 text-white py-2 px-4 rounded-md hover:bg-blue-700"
      >
        Create Booking
      </button>
    </form>
  );
};
```

#### 4.5 My Bookings Component
**File:** `/src/Web/ReactApp/src/components/MyBookings.tsx`

```typescript
import React, { useEffect, useState } from 'react';
import { bookingService, Booking } from '../services/bookingService';

export const MyBookings: React.FC = () => {
  const [bookings, setBookings] = useState<Booking[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadBookings();
  }, []);

  const loadBookings = async () => {
    try {
      const response = await bookingService.getMyBookings();
      setBookings(response.data.items);
    } catch (error) {
      console.error('Error loading bookings:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleCheckIn = async (bookingId: string) => {
    try {
      await bookingService.checkIn(bookingId);
      loadBookings();
      alert('Checked in successfully!');
    } catch (error) {
      alert('Error checking in');
    }
  };

  const handleCancel = async (bookingId: string) => {
    if (window.confirm('Are you sure you want to cancel this booking?')) {
      try {
        await bookingService.cancelBooking(bookingId);
        loadBookings();
        alert('Booking cancelled');
      } catch (error) {
        alert('Error cancelling booking');
      }
    }
  };

  if (loading) return <div>Loading...</div>;

  return (
    <div className="space-y-4">
      <h2 className="text-2xl font-bold">My Bookings</h2>
      
      {bookings.length === 0 ? (
        <p>No bookings found</p>
      ) : (
        <div className="grid gap-4">
          {bookings.map((booking) => (
            <div key={booking.id} className="border rounded-lg p-4 shadow">
              <div className="flex justify-between items-start">
                <div>
                  <h3 className="font-semibold">
                    {booking.deskNumber || booking.meetingRoomName}
                  </h3>
                  <p className="text-sm text-gray-600">
                    {booking.buildingName} - {booking.floorName}
                  </p>
                  <p className="text-sm">
                    {new Date(booking.bookingDate).toLocaleDateString()} | {booking.startTime} - {booking.endTime}
                  </p>
                  <span className={`inline-block px-2 py-1 text-xs rounded ${
                    booking.status === 'Confirmed' ? 'bg-green-100 text-green-800' :
                    booking.status === 'CheckedIn' ? 'bg-blue-100 text-blue-800' :
                    booking.status === 'Cancelled' ? 'bg-red-100 text-red-800' :
                    'bg-gray-100 text-gray-800'
                  }`}>
                    {booking.status}
                  </span>
                </div>

                <div className="flex gap-2">
                  {booking.status === 'Confirmed' && (
                    <>
                      <button
                        onClick={() => handleCheckIn(booking.id)}
                        className="px-3 py-1 bg-blue-600 text-white rounded hover:bg-blue-700"
                      >
                        Check In
                      </button>
                      <button
                        onClick={() => handleCancel(booking.id)}
                        className="px-3 py-1 bg-red-600 text-white rounded hover:bg-red-700"
                      >
                        Cancel
                      </button>
                    </>
                  )}
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};
```

### Task 5: Shared Project Files

#### 5.1 Models Project
**File:** `/src/Shared/Models/Models.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <RootNamespace>WorkSpaceManager.Shared.Models</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
  </ItemGroup>
</Project>
```

#### 5.2 DTOs Project
**File:** `/src/Shared/DTOs/DTOs.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <RootNamespace>WorkSpaceManager.Shared.DTOs</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="System.ComponentModel.Annotations" Version="5.0.0" />
  </ItemGroup>
</Project>
```

#### 5.3 Common Project
**File:** `/src/Shared/Common/Common.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <RootNamespace>WorkSpaceManager.Shared.Common</RootNamespace>
  </PropertyGroup>
</Project>
```

## 🚀 Getting Started

### 1. Build and Run

```bash
# Build all services
dotnet build

# Run with Docker Compose
docker-compose up -d

# Or run individually
cd src/Services/BookingService
dotnet run

cd src/Services/SpaceManagementService
dotnet run

cd src/Services/ApiGateway
dotnet run
```

### 2. Access Applications

- **API Gateway:** http://localhost:5000
- **Booking Service:** http://localhost:5001/swagger
- **Space Service:** http://localhost:5002/swagger
- **Web App:** http://localhost:3000

### 3. Database Setup

```bash
# Apply migrations
dotnet ef database update --project src/Services/BookingService
dotnet ef database update --project src/Services/SpaceManagementService
```

## 📝 Next Steps

1. Implement remaining Space Management Service methods (Floors, Desks, MeetingRooms)
2. Add more React components (Dashboard, Floor Plan Viewer, Admin Panel)
3. Implement real-time notifications with SignalR
4. Add comprehensive error handling and logging
5. Write unit and integration tests
6. Set up CI/CD pipeline

## 🎯 Key Patterns to Follow

- **Service Pattern:** Follow BookingService implementation
- **Controller Pattern:** Follow BookingsController implementation
- **React Components:** Use hooks, TypeScript, Tailwind CSS
- **API Responses:** Always use ApiResponse<T> wrapper
- **Authentication:** Extract userId and tenantId from JWT claims
- **Error Handling:** Try-catch with logging

This guide provides everything needed to complete the full-stack implementation!
