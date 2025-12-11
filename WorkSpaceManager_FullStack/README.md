# WorkSpaceManager - Full Stack Implementation

Enterprise workspace booking system with microservices architecture.

## 🏗️ Architecture

```
WorkSpaceManager/
├── src/
│   ├── Services/
│   │   ├── BookingService/          # Booking management microservice
│   │   ├── SpaceManagementService/  # Space and resource management
│   │   └── ApiGateway/              # API Gateway with Ocelot
│   ├── Shared/
│   │   ├── Common/                  # Shared utilities and extensions
│   │   ├── Models/                  # Shared domain models
│   │   └── DTOs/                    # Data transfer objects
│   └── Web/
│       └── ReactApp/                # React web application
├── docker-compose.yml
└── README.md
```

## 🚀 Quick Start

### Prerequisites
- .NET 8 SDK
- Node.js 18+
- Docker & Docker Compose
- SQL Server 2019+

### Run with Docker Compose

```bash
docker-compose up -d
```

### Access Applications
- **Web App**: http://localhost:3000
- **API Gateway**: http://localhost:5000
- **Booking Service**: http://localhost:5001
- **Space Management Service**: http://localhost:5002

## 📦 Services

### Booking Service (Port 5001)
- Create, read, update, delete bookings
- Check-in/check-out functionality
- Booking validation and conflict detection
- No-show tracking

### Space Management Service (Port 5002)
- Manage buildings, floors, desks, meeting rooms
- Resource availability checking
- Capacity management
- Maintenance scheduling

### API Gateway (Port 5000)
- Request routing and aggregation
- Authentication and authorization
- Rate limiting
- Load balancing

## 🔧 Development

### Backend (.NET 8)

```bash
# Restore dependencies
dotnet restore

# Run Booking Service
cd src/Services/BookingService
dotnet run

# Run Space Management Service
cd src/Services/SpaceManagementService
dotnet run

# Run API Gateway
cd src/Services/ApiGateway
dotnet run
```

### Frontend (React)

```bash
cd src/Web/ReactApp
npm install
npm start
```

## 📊 Database

SQL Server with multi-tenant support. Connection strings in `appsettings.json`.

### Migrations

```bash
# Apply migrations
dotnet ef database update --project src/Services/BookingService

# Create new migration
dotnet ef migrations add MigrationName --project src/Services/BookingService
```

## 🔐 Authentication

Integrated with Keycloak for authentication and authorization.

Configure in `appsettings.json`:
```json
{
  "Keycloak": {
    "Authority": "http://localhost:8080",
    "Realm": "alpha-bank-realm",
    "ClientId": "workspace-manager-api"
  }
}
```

## 📝 API Documentation

Swagger UI available at:
- Booking Service: http://localhost:5001/swagger
- Space Management Service: http://localhost:5002/swagger
- API Gateway: http://localhost:5000/swagger

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Run specific service tests
dotnet test src/Services/BookingService.Tests
```

## 📦 Deployment

### Docker

```bash
# Build images
docker-compose build

# Deploy to production
docker-compose -f docker-compose.prod.yml up -d
```

## 📄 License

Proprietary - Alpha Bank

## 👥 Team

Developed by Manus AI for Alpha Bank
