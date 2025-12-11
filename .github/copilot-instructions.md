# WorkSpaceManager Copilot Instructions

## Architecture Overview

WorkSpaceManager is a production-ready workspace booking and management system built with .NET 8 microservices architecture. The system enables employees to book desks and meeting rooms, manage check-in/out workflows, and provides admin features for space management.

### Technology Stack
- **Backend**: .NET 8, ASP.NET Core, Entity Framework Core 8
- **API Gateway**: Ocelot with JWT authentication
- **Database**: SQL Server 2019+
- **Authentication**: Keycloak (OAuth 2.0 / OpenID Connect)
- **Web Frontend**: React 18, TypeScript, React Router 6
- **Mobile App**: React Native with Expo
- **API Documentation**: Swagger/OpenAPI

### System Architecture
```
┌─────────────────────────────────────────────────────────┐
│                    CLIENT APPLICATIONS                   │
├──────────────────────────┬──────────────────────────────┤
│   React Web App          │   React Native Mobile App    │
│   (Port 3000)            │   (iOS & Android)            │
└──────────────┬───────────┴──────────────┬───────────────┘
               │                          │
               └──────────┬───────────────┘
                          ↓
         ┌────────────────────────────────────┐
         │      API Gateway (Port 5000)       │
         │      - Ocelot Routing              │
         │      - JWT Authentication          │
         │      - Rate Limiting               │
         └────┬───────────────────────┬───────┘
              │                       │
              ↓                       ↓
    ┌─────────────────┐    ┌─────────────────────┐
    │ Booking Service │    │ Space Management    │
    │ (Port 5001)     │    │ Service (Port 5002) │
    └────────┬────────┘    └────────┬────────────┘
             │                      │
             └──────────┬───────────┘
                        ↓
             ┌─────────────────────┐
             │  SQL Server + Keycloak │
             └─────────────────────┘
```

## Key Domain Concepts

### Core Entities

**Space Management:**
- `Building` - Physical buildings with address, total floors, active status
- `Floor` - Building floors with floor plan URLs, desk/room counts
- `Desk` - Individual workstations with amenities (monitor, docking station, accessibility)
- `MeetingRoom` - Conference rooms with capacity and equipment (projector, video conference)

**Booking Management:**
- `Booking` - Reservations linking users to desks or meeting rooms
- `BookingStatus` - Workflow states: `Pending`, `Confirmed`, `CheckedIn`, `CheckedOut`, `Cancelled`, `NoShow`

**Identity:**
- `User` - Employee profiles synced with Keycloak
- `TenantId` - Multi-tenant isolation identifier

### Business Rules
- Users can book desks or meeting rooms for specific time slots
- Check-in required within configured time window (e.g., 15 minutes)
- No-show detection and automatic cancellation
- Conflict detection prevents double bookings
- Role-based access: `user`, `admin`, `facility_manager`

## Project Structure

```
WorkSpaceManager/
├── WorkSpaceManager.sln                    # Solution file
├── WorkSpaceManager_FullStack/
│   └── src/
│       ├── Services/
│       │   ├── ApiGateway/                 # Ocelot API Gateway (Port 5000)
│       │   │   ├── Program.cs
│       │   │   ├── ocelot.json             # Route configuration
│       │   │   └── ocelot.Development.json
│       │   ├── BookingService/             # Booking Management (Port 5001)
│       │   │   ├── Controllers/
│       │   │   │   └── BookingsController.cs
│       │   │   ├── Services/
│       │   │   │   ├── IBookingService.cs
│       │   │   │   └── BookingService.cs
│       │   │   ├── Data/
│       │   │   │   └── BookingDbContext.cs
│       │   │   └── Program.cs
│       │   └── SpaceManagementService/     # Space Management (Port 5002)
│       │       ├── Controllers/
│       │       │   ├── BuildingsController.cs
│       │       │   ├── FloorsController.cs
│       │       │   ├── DesksController.cs
│       │       │   └── MeetingRoomsController.cs
│       │       ├── Services/
│       │       │   ├── IBuildingService.cs, BuildingService.cs
│       │       │   ├── IFloorService.cs, FloorService.cs
│       │       │   ├── IDeskService.cs, DeskService.cs
│       │       │   └── IMeetingRoomService.cs, MeetingRoomService.cs
│       │       └── Data/
│       │           └── SpaceDbContext.cs
│       ├── Shared/                         # Shared Libraries
│       │   ├── Common/                     # ApiResponse, PagedResponse utilities
│       │   │   ├── ApiResponse.cs
│       │   │   └── Common.csproj
│       │   ├── DTOs/                       # Data Transfer Objects
│       │   │   ├── BookingDTOs.cs
│       │   │   ├── SpaceDTOs.cs
│       │   │   └── DTOs.csproj
│       │   └── Models/                     # Entity Models
│       │       ├── Entities.cs
│       │       └── Models.csproj
│       ├── WebApp/                         # React Web Application
│       │   └── src/
│       │       ├── components/
│       │       │   ├── Auth/               # Login, ProtectedRoute
│       │       │   ├── Layout/             # DashboardLayout
│       │       │   ├── Dashboard/          # Dashboard
│       │       │   ├── Bookings/           # MyBookings, BookSpace
│       │       │   └── Admin/              # Buildings, Desks, MeetingRooms
│       │       └── services/
│       │           ├── api.ts              # Axios client configuration
│       │           ├── authService.ts      # Authentication
│       │           ├── bookingService.ts   # Booking API client
│       │           └── spaceService.ts     # Space management API client
│       └── MobileApp/                      # React Native Mobile App
│           └── src/
│               ├── screens/                # LoginScreen, DashboardScreen, etc.
│               ├── services/               # API clients
│               ├── navigation/             # AppNavigator
│               └── config/                 # Environment configuration
├── Keycloak_Implementation/                # Authentication configuration
│   ├── Configuration/
│   │   ├── alpha-bank-realm.json          # Keycloak realm export
│   │   ├── docker-compose.yml             # Keycloak + PostgreSQL
│   │   └── roles.json, sample-users.json
│   ├── Services/
│   │   ├── KeycloakService.cs
│   │   └── JitProvisioningService.cs
│   └── Middleware/
│       └── JwtAuthenticationMiddleware.cs
├── docs/                                   # Documentation
└── WorkSpaceManager_Unified_Database_Schema.sql
```

## Development Workflows

### Service Startup

```powershell
# Start all backend services
# Terminal 1 - API Gateway
cd WorkSpaceManager_FullStack/src/Services/ApiGateway
dotnet run  # Runs on port 5000

# Terminal 2 - Booking Service
cd WorkSpaceManager_FullStack/src/Services/BookingService
dotnet run  # Runs on port 5001

# Terminal 3 - Space Management Service
cd WorkSpaceManager_FullStack/src/Services/SpaceManagementService
dotnet run  # Runs on port 5002

# Terminal 4 - Web Application
cd WorkSpaceManager_FullStack/src/WebApp
npm install
npm start   # Runs on port 3000
```

### Build Commands

```powershell
# Restore and build entire solution
cd C:\My Implementations\WorkSpaceManager
dotnet restore WorkSpaceManager.sln
dotnet build WorkSpaceManager.sln --configuration Release

# Build specific service
dotnet build WorkSpaceManager_FullStack/src/Services/BookingService/BookingService.csproj
```

### Service Architecture Patterns

Each microservice follows this pattern:
- `Program.cs` - DI configuration, middleware setup, database connection
- `Controllers/` - REST API endpoints with `[Authorize]` attributes
- `Services/` - Business logic with interface/implementation pattern
- `Data/` - Entity Framework DbContext with fluent configuration

## Code Conventions

### DTOs & Serialization
```csharp
// Request DTOs
public class CreateBookingRequest
{
    [Required]
    public Guid? DeskId { get; set; }
    public Guid? MeetingRoomId { get; set; }
    [Required]
    public DateTime BookingDate { get; set; }
    // ...
}

// Response DTOs
public class BookingResponse
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    // Include related entity names for display
    public string? DeskNumber { get; set; }
    public string? BuildingName { get; set; }
}
```

### API Response Wrapper
```csharp
// Always wrap responses in ApiResponse<T>
return ApiResponse<BookingResponse>.SuccessResponse(data, "Booking created successfully");
return ApiResponse<BookingResponse>.ErrorResponse("Booking not found");

// Paginated responses
return new PagedResponse<BookingResponse>(items, totalCount, pageNumber, pageSize);
```

### Controller Patterns
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;
    
    [HttpPost]
    [ProducesResponseType(typeof(BookingResponse), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequest request)
    {
        var userId = GetUserId();  // Extract from JWT claims
        var tenantId = GetTenantId();
        var result = await _bookingService.CreateBookingAsync(userId, tenantId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }
    
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BookingResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetBooking(Guid id)
    {
        // ...
    }
}
```

### Dependency Injection
```csharp
// Program.cs service registration
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IBuildingService, BuildingService>();
builder.Services.AddScoped<IFloorService, FloorService>();
builder.Services.AddScoped<IDeskService, DeskService>();
builder.Services.AddScoped<IMeetingRoomService, MeetingRoomService>();
```

### Entity Framework Patterns
```csharp
// DbContext configuration
public class BookingDbContext : DbContext
{
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<Desk> Desks { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Multi-tenant filtering
        modelBuilder.Entity<Booking>()
            .HasQueryFilter(e => !e.IsDeleted);
            
        // Indexes
        modelBuilder.Entity<Booking>()
            .HasIndex(e => e.TenantId);
    }
}

// Include related entities with null-forgiving for EF Core
var bookings = await _context.Bookings
    .Include(b => b.Desk).ThenInclude(d => d!.Floor).ThenInclude(f => f!.Building)
    .Include(b => b.MeetingRoom).ThenInclude(m => m!.Floor).ThenInclude(f => f!.Building)
    .Where(b => b.TenantId == tenantId)
    .ToListAsync();
```

## API Endpoints

### Booking Service (Port 5001)
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/api/bookings` | Create booking | User |
| GET | `/api/bookings/{id}` | Get booking by ID | User |
| GET | `/api/bookings/my-bookings` | Get user's bookings | User |
| GET | `/api/bookings/upcoming` | Get upcoming bookings | User |
| POST | `/api/bookings/search` | Search bookings | Admin/Manager |
| PUT | `/api/bookings/{id}` | Update booking | User |
| POST | `/api/bookings/{id}/cancel` | Cancel booking | User |
| POST | `/api/bookings/{id}/checkin` | Check in | User |
| POST | `/api/bookings/{id}/checkout` | Check out | User |
| POST | `/api/bookings/check-availability` | Check availability | User |

### Space Management Service (Port 5002)
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET/POST | `/api/buildings` | List/Create buildings | Admin |
| GET/PUT/DELETE | `/api/buildings/{id}` | CRUD building | Admin |
| GET/POST | `/api/floors` | List/Create floors | Admin |
| GET/PUT/DELETE | `/api/floors/{id}` | CRUD floor | Admin |
| GET/POST | `/api/desks` | List/Create desks | Admin |
| GET/PUT/DELETE | `/api/desks/{id}` | CRUD desk | Admin |
| GET | `/api/desks/search` | Search available desks | User |
| GET/POST | `/api/meetingrooms` | List/Create rooms | Admin |
| GET/PUT/DELETE | `/api/meetingrooms/{id}` | CRUD room | Admin |
| GET | `/api/meetingrooms/search` | Search available rooms | User |

## Authentication & Authorization

### Keycloak Configuration
- **Realm**: `alpha-bank-realm`
- **Clients**: `workspace-manager-web`, `workspace-manager-mobile`, `workspace-manager-api`
- **Roles**: `user`, `admin`, `facility_manager`

### JWT Token Flow
1. User authenticates via Keycloak login page
2. Keycloak issues JWT access token
3. Token sent in `Authorization: Bearer {token}` header
4. API Gateway validates token with Keycloak
5. Claims extracted: `sub` (userId), `tenant_id`, `roles`

### Authorization Patterns
```csharp
// Role-based authorization
[Authorize(Roles = "admin,facility_manager")]
public async Task<IActionResult> CreateBuilding([FromBody] CreateBuildingRequest request)

// User-specific resource access
var userId = GetUserId();
var booking = await _context.Bookings
    .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId && b.TenantId == tenantId);
```

## Database Configuration

### SQL Server Credentials
- **Server**: `localhost` (or `.` for local instance)
- **Username**: `sa`
- **Password**: `!Famel1965`
- **Database**: `WorkSpaceManager`

### Connection Strings
```json
// Development (appsettings.json)
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=WorkSpaceManager;User Id=sa;Password=!Famel1965;TrustServerCertificate=True;"
  }
}

// Docker environment
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=sqlserver;Database=WorkSpaceManager;User Id=sa;Password=!Famel1965;TrustServerCertificate=True;"
  }
}
```

### Database Setup
```powershell
# Create database using the unified schema script
sqlcmd -S localhost -U sa -P "!Famel1965" -i "WorkSpaceManager_Unified_Database_Schema.sql"

# Seed sample data
sqlcmd -S localhost -U sa -P "!Famel1965" -i "WorkSpaceManager_Seed_Data.sql"

# Verify database exists
sqlcmd -S localhost -U sa -P "!Famel1965" -Q "SELECT name FROM sys.databases WHERE name = 'WorkSpaceManager'"
```

### Database Schema
Key tables: `Buildings`, `Floors`, `Desks`, `MeetingRooms`, `Bookings`, `Users`
- All entities inherit from `TenantEntity` with `TenantId` for multi-tenancy
- Soft delete pattern with `IsDeleted` flag
- Automatic timestamps: `CreatedAt`, `UpdatedAt`

## Frontend Development

### React Web App Structure
```
src/
├── App.tsx                    # Routes configuration
├── components/
│   ├── Auth/
│   │   ├── Login.tsx          # Login form
│   │   └── ProtectedRoute.tsx # Route guard
│   ├── Layout/
│   │   └── DashboardLayout.tsx # Sidebar + main content
│   ├── Dashboard/
│   │   └── Dashboard.tsx      # Statistics and quick actions
│   ├── Bookings/
│   │   ├── MyBookings.tsx     # User's booking list
│   │   └── BookSpace.tsx      # Create booking form
│   └── Admin/
│       ├── Buildings.tsx      # Building management
│       ├── Desks.tsx          # Desk management
│       └── MeetingRooms.tsx   # Room management
└── services/
    ├── api.ts                 # Axios instance with interceptors
    ├── authService.ts         # Token management
    ├── bookingService.ts      # Booking API calls
    └── spaceService.ts        # Space management API calls
```

### API Client Pattern
```typescript
// services/api.ts
const apiClient = axios.create({
  baseURL: process.env.REACT_APP_API_URL || 'http://localhost:5000',
});

// Request interceptor for JWT
apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('accessToken');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Response wrapper type
export interface ApiResponse<T> {
  success: boolean;
  message?: string;
  data?: T;
  errors?: string[];
}
```

### Route Protection
```tsx
<Route
  path="/admin/buildings"
  element={
    <ProtectedRoute requiredRole="admin">
      <DashboardLayout>
        <Buildings />
      </DashboardLayout>
    </ProtectedRoute>
  }
/>
```

## Mobile App Development

### React Native + Expo Setup
```bash
cd WorkSpaceManager_FullStack/src/MobileApp
npm install
npx expo start
```

### Environment Configuration
```typescript
// src/config/env.ts
export const ENV = {
  API_URL: 'http://localhost:5000',      // iOS Simulator
  // API_URL: 'http://10.0.2.2:5000',    // Android Emulator
  KEYCLOAK_URL: 'http://localhost:8080',
  KEYCLOAK_REALM: 'alpha-bank-realm',
  KEYCLOAK_CLIENT_ID: 'workspace-manager-mobile',
};
```

### Mobile Features
- OAuth 2.0 login with Keycloak
- Biometric authentication (Face ID / Touch ID)
- Secure token storage (Expo SecureStore)
- Push notifications (APNS/FCM)
- Offline mode with local caching

## Docker & Infrastructure

### Docker Compose (Development)
```yaml
# Keycloak_Implementation/Configuration/docker-compose.yml
services:
  keycloak:
    image: quay.io/keycloak/keycloak:23.0
    ports:
      - "8080:8080"
    environment:
      KEYCLOAK_ADMIN: admin
      KEYCLOAK_ADMIN_PASSWORD: admin

  sqlserver:
    image: mcr.microsoft.com/mssql/server:2019-latest
    ports:
      - "1433:1433"
    environment:
      SA_PASSWORD: YourStrong@Passw0rd
      ACCEPT_EULA: "Y"
```

### Running with Docker
```powershell
# Start infrastructure
cd Keycloak_Implementation/Configuration
docker-compose up -d

# Verify services
docker-compose ps
```

## Testing

### Swagger Documentation
Access Swagger UI when services are running:
- Booking Service: `http://localhost:5001/swagger`
- Space Management Service: `http://localhost:5002/swagger`

### Manual Testing
```powershell
# Health check
Invoke-RestMethod -Uri "http://localhost:5001/health"

# Get token from Keycloak (example)
$tokenResponse = Invoke-RestMethod -Uri "http://localhost:8080/realms/alpha-bank-realm/protocol/openid-connect/token" `
  -Method POST -Body @{
    grant_type = "password"
    client_id = "workspace-manager-web"
    username = "admin@alphabank.gr"
    password = "Admin@123"
  }

# Use token for API calls
$headers = @{ Authorization = "Bearer $($tokenResponse.access_token)" }
Invoke-RestMethod -Uri "http://localhost:5000/api/bookings/my-bookings" -Headers $headers
```

## Common Issues & Solutions

### Port Conflicts
| Service | Port | Check Command |
|---------|------|---------------|
| API Gateway | 5000 | `netstat -an | findstr :5000` |
| Booking Service | 5001 | `netstat -an | findstr :5001` |
| Space Service | 5002 | `netstat -an | findstr :5002` |
| Web App | 3000 | `netstat -an | findstr :3000` |
| Keycloak | 8080 | `netstat -an | findstr :8080` |
| SQL Server | 1433 | `netstat -an | findstr :1433` |

### Database Migration Errors
```powershell
# Reset database
cd WorkSpaceManager_FullStack/src/Services/BookingService
dotnet ef database drop --force
dotnet ef database update
```

### CORS Issues
Ensure API Gateway has CORS configured:
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
```

### Nullable Reference Warnings
Use null-forgiving operator for EF Core ThenInclude chains:
```csharp
.Include(b => b.Desk).ThenInclude(d => d!.Floor).ThenInclude(f => f!.Building)
```

## Best Practices

### When Adding New Features
1. Create DTOs in `Shared/DTOs/`
2. Add entities to `Shared/Models/Entities.cs`
3. Update DbContext with new DbSet and configuration
4. Create service interface and implementation
5. Add controller with proper authorization
6. Update Ocelot routes if needed
7. Add frontend components and API client methods

### Code Quality Checklist
- [ ] All public methods have XML documentation
- [ ] DTOs use `[Required]` and `[MaxLength]` attributes
- [ ] Controllers return `ApiResponse<T>` wrapper
- [ ] Services handle exceptions and log errors
- [ ] Multi-tenancy enforced with `TenantId` filtering
- [ ] Soft delete pattern used (`IsDeleted` flag)
- [ ] No nullable reference warnings

### Git Workflow
```powershell
# Create feature branch
git checkout -b feature/new-feature

# Commit with conventional commits
git commit -m "feat(booking): add recurring booking support"

# Push and create PR
git push -u origin feature/new-feature
```

## Demo Credentials

| Role | Email | Password |
|------|-------|----------|
| Admin | admin@alphabank.gr | Admin@123 |
| User | user1@alphabank.gr | User@123 |
| Manager | manager@alphabank.gr | Manager@123 |

---

**Repository**: https://github.com/famel/WorkSpaceManager

**CRITICAL**: Always build and test after making changes:
```powershell
dotnet build WorkSpaceManager.sln --configuration Release
```
