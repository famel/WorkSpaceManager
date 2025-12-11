# WorkSpaceManager

A production-ready workspace booking and management system built with .NET 8 microservices architecture.

## 🏗️ Architecture

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

## 📦 Project Structure

```
WorkSpaceManager/
├── WorkSpaceManager.sln
├── WorkSpaceManager_FullStack/
│   └── src/
│       ├── Services/
│       │   ├── ApiGateway/          # Ocelot API Gateway
│       │   ├── BookingService/      # Booking management
│       │   └── SpaceManagementService/ # Space & resource management
│       ├── Shared/
│       │   ├── Common/              # Shared utilities (ApiResponse, PagedResponse)
│       │   ├── DTOs/                # Data Transfer Objects
│       │   └── Models/              # Entity models
│       ├── WebApp/                  # React Web Application
│       └── MobileApp/               # React Native Mobile App
├── Keycloak_Implementation/         # Authentication configuration
└── docs/                            # Documentation
```

## 🚀 Features

### Backend Services
- **API Gateway**: Ocelot-based routing, JWT authentication, CORS, rate limiting
- **Booking Service**: Complete booking lifecycle with check-in/out workflow
- **Space Management Service**: Buildings, floors, desks, meeting rooms CRUD

### User Features
- OAuth 2.0 / SSO login with Keycloak
- Dashboard with booking statistics
- Book desks and meeting rooms
- Check-in/Check-out functionality
- Biometric authentication (mobile)

### Admin Features
- Building and floor management
- Desk and meeting room configuration
- Booking reports and analytics
- No-show policy management

## 🛠️ Tech Stack

| Component | Technology |
|-----------|------------|
| Backend | .NET 8, ASP.NET Core, Entity Framework Core |
| API Gateway | Ocelot |
| Database | SQL Server |
| Authentication | Keycloak, JWT |
| Web Frontend | React 18, TypeScript |
| Mobile | React Native, Expo |

## 📋 Prerequisites

- .NET 8 SDK
- Node.js 18+
- SQL Server 2019+
- Docker (optional)
- Keycloak 23+

## 🏃 Quick Start

### 1. Clone the repository
```bash
git clone https://github.com/YOUR_USERNAME/WorkSpaceManager.git
cd WorkSpaceManager
```

### 2. Restore and build
```bash
dotnet restore
dotnet build --configuration Release
```

### 3. Configure database
Update connection strings in `appsettings.json` for each service.

### 4. Run services
```bash
# Terminal 1 - API Gateway
cd WorkSpaceManager_FullStack/src/Services/ApiGateway
dotnet run

# Terminal 2 - Booking Service
cd WorkSpaceManager_FullStack/src/Services/BookingService
dotnet run

# Terminal 3 - Space Management Service
cd WorkSpaceManager_FullStack/src/Services/SpaceManagementService
dotnet run
```

## 📚 API Documentation

Once running, Swagger documentation is available at:
- Booking Service: `http://localhost:5001/swagger`
- Space Management Service: `http://localhost:5002/swagger`

## 🔐 Authentication

The system uses Keycloak for authentication with the following default configuration:
- Realm: `alpha-bank-realm`
- Client IDs: `workspace-manager-web`, `workspace-manager-mobile`, `workspace-manager-api`

## 📄 License

This project is licensed under the MIT License.

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request
