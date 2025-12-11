# WorkSpaceManager: Deployment Guide

This guide provides step-by-step instructions for deploying the WorkSpaceManager system in development, staging, and production environments.

---

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Infrastructure Setup](#infrastructure-setup)
3. [Database Deployment](#database-deployment)
4. [Keycloak Configuration](#keycloak-configuration)
5. [Microservices Deployment](#microservices-deployment)
6. [Frontend Deployment](#frontend-deployment)
7. [Mobile App Deployment](#mobile-app-deployment)
8. [Monitoring & Logging](#monitoring--logging)
9. [Security Hardening](#security-hardening)
10. [Troubleshooting](#troubleshooting)

---

## Prerequisites

### Development Environment

The following software must be installed on the development machine:

**Required Software:**
- **.NET 8 SDK** (version 8.0 or higher)
- **Node.js** (version 18 or higher) with npm
- **Docker Desktop** (for containerized services)
- **SQL Server 2019+** or Docker container
- **Visual Studio 2022** or **Visual Studio Code** with C# extensions
- **Git** for version control

**Optional Tools:**
- **Postman** or **Insomnia** for API testing
- **SQL Server Management Studio (SSMS)** for database management
- **Azure Data Studio** as a cross-platform alternative

### Production Environment

**Server Requirements:**

| Component | CPU | RAM | Storage | OS |
|-----------|-----|-----|---------|-----|
| API Gateway | 2 cores | 4 GB | 20 GB | Linux/Windows Server |
| Identity Service | 2 cores | 4 GB | 20 GB | Linux/Windows Server |
| Resource Service | 2 cores | 4 GB | 20 GB | Linux/Windows Server |
| Booking Service | 4 cores | 8 GB | 40 GB | Linux/Windows Server |
| Rules Service | 2 cores | 4 GB | 20 GB | Linux/Windows Server |
| Notification Service | 2 cores | 4 GB | 20 GB | Linux/Windows Server |
| SQL Server | 4 cores | 16 GB | 200 GB SSD | Windows Server 2019+ |
| Keycloak | 2 cores | 4 GB | 20 GB | Linux |
| RabbitMQ | 2 cores | 4 GB | 40 GB | Linux |

**Network Requirements:**
- Internal network connectivity between all services
- HTTPS/TLS certificates for external endpoints
- Firewall rules configured for service-to-service communication
- Load balancer for high availability (recommended)

---

## Infrastructure Setup

### Step 1: Start Infrastructure Services with Docker Compose

The `docker-compose.yml` file provides all infrastructure dependencies.

```bash
# Navigate to project root
cd /path/to/WorkSpaceManager

# Start all infrastructure services
docker-compose up -d

# Verify services are running
docker-compose ps
```

**Expected Output:**
```
NAME                COMMAND                  SERVICE             STATUS
sqlserver           /opt/mssql/bin/sqlse…   sqlserver           Up 2 minutes
keycloak            /opt/keycloak/bin/kc…   keycloak            Up 2 minutes
postgres            postgres                 postgres            Up 2 minutes
rabbitmq            docker-entrypoint.sh…   rabbitmq            Up 2 minutes
mailhog             MailHog                  mailhog             Up 2 minutes
```

### Step 2: Verify Service Health

**SQL Server:**
```bash
docker exec -it sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P "YourStrong@Passw0rd" \
  -Q "SELECT @@VERSION"
```

**Keycloak:**
```bash
curl http://localhost:8080/health
```

**RabbitMQ:**
```bash
curl http://localhost:15672/api/overview -u guest:guest
```

---

## Database Deployment

### Step 1: Create Database

```bash
# Connect to SQL Server
docker exec -it sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P "YourStrong@Passw0rd"
```

```sql
-- Create database
CREATE DATABASE WorkSpaceManager;
GO

-- Verify creation
SELECT name FROM sys.databases WHERE name = 'WorkSpaceManager';
GO
```

### Step 2: Execute Schema Script

```bash
# Copy schema file to container
docker cp WorkSpaceManager_Unified_Database_Schema.sql sqlserver:/tmp/

# Execute schema
docker exec -it sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P "YourStrong@Passw0rd" \
  -d WorkSpaceManager \
  -i /tmp/WorkSpaceManager_Unified_Database_Schema.sql
```

### Step 3: Load Seed Data

```bash
# Copy seed data file
docker cp WorkSpaceManager_Seed_Data.sql sqlserver:/tmp/

# Execute seed data
docker exec -it sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P "YourStrong@Passw0rd" \
  -d WorkSpaceManager \
  -i /tmp/WorkSpaceManager_Seed_Data.sql
```

### Step 4: Verify Database

```sql
-- Check table counts
USE WorkSpaceManager;
GO

SELECT 
    'Tenants' AS Entity, COUNT(*) AS Count FROM Identity_Tenants
UNION ALL
SELECT 'Users', COUNT(*) FROM Identity_UserMetadata
UNION ALL
SELECT 'Buildings', COUNT(*) FROM Resource_Buildings
UNION ALL
SELECT 'Desks', COUNT(*) FROM Resource_Desks
UNION ALL
SELECT 'Bookings', COUNT(*) FROM Booking_Bookings;
GO
```

---

## Keycloak Configuration

### Step 1: Access Keycloak Admin Console

Navigate to `http://localhost:8080` and log in with:
- **Username:** admin
- **Password:** admin

### Step 2: Create Realm

1. Click **"Add realm"** in the top-left dropdown
2. Enter realm name: `alpha-bank-realm`
3. Click **"Create"**

### Step 3: Create Client

1. Navigate to **Clients** → **Create**
2. Configure client settings:

| Field | Value |
|-------|-------|
| Client ID | workspace-manager-web |
| Client Protocol | openid-connect |
| Access Type | public |
| Valid Redirect URIs | http://localhost:3000/* |
| Web Origins | http://localhost:3000 |

3. Click **"Save"**

### Step 4: Create Test Users

1. Navigate to **Users** → **Add user**
2. Create user with:
   - **Username:** john.doe@alphabank.gr
   - **Email:** john.doe@alphabank.gr
   - **First Name:** John
   - **Last Name:** Doe
3. Navigate to **Credentials** tab
4. Set password: `password123`
5. Disable **"Temporary"** toggle
6. Click **"Set Password"**

### Step 5: Configure User Attributes

Add custom attributes for JIT provisioning:

1. Select user → **Attributes** tab
2. Add attributes:

| Key | Value |
|-----|-------|
| employeeId | EMP001 |
| department | IT |
| jobTitle | Senior Developer |

---

## Microservices Deployment

### Step 1: Build All Services

```bash
# Build Identity Service
cd src/Services/Identity
dotnet build -c Release

# Build Resource Service
cd ../Resource
dotnet build -c Release

# Build Booking Service
cd ../Booking
dotnet build -c Release

# Build Rules Service
cd ../Rules
dotnet build -c Release

# Build Notification Service
cd ../Notification
dotnet build -c Release

# Build API Gateway
cd ../../ApiGateway
dotnet build -c Release
```

### Step 2: Update Configuration Files

Each service requires an `appsettings.Production.json` file with production settings.

**Example for Identity Service:**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=production-sql-server;Database=WorkSpaceManager;User Id=app_user;Password=SecurePassword123!;TrustServerCertificate=true"
  },
  "Keycloak": {
    "Authority": "https://keycloak.production.com/realms/alpha-bank-realm",
    "Audience": "workspace-manager-web",
    "RequireHttpsMetadata": true
  },
  "RabbitMQ": {
    "Host": "production-rabbitmq.internal",
    "Port": 5672,
    "Username": "app_user",
    "Password": "SecureRabbitPassword!"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  }
}
```

### Step 3: Publish Services

```bash
# Publish Identity Service
cd src/Services/Identity
dotnet publish -c Release -o /publish/identity

# Publish Resource Service
cd ../Resource
dotnet publish -c Release -o /publish/resource

# Publish Booking Service
cd ../Booking
dotnet publish -c Release -o /publish/booking

# Publish Rules Service
cd ../Rules
dotnet publish -c Release -o /publish/rules

# Publish Notification Service
cd ../Notification
dotnet publish -c Release -o /publish/notification

# Publish API Gateway
cd ../../ApiGateway
dotnet publish -c Release -o /publish/gateway
```

### Step 4: Deploy as Windows Services (Windows Server)

```powershell
# Install Identity Service
New-Service -Name "WorkSpaceManager.Identity" `
  -BinaryPathName "C:\Services\Identity\WorkSpaceManager.Identity.exe" `
  -DisplayName "WorkSpaceManager Identity Service" `
  -StartupType Automatic

# Start service
Start-Service -Name "WorkSpaceManager.Identity"

# Repeat for other services...
```

### Step 5: Deploy as Systemd Services (Linux)

Create service file: `/etc/systemd/system/workspacemanager-identity.service`

```ini
[Unit]
Description=WorkSpaceManager Identity Service
After=network.target

[Service]
Type=notify
User=www-data
WorkingDirectory=/opt/workspacemanager/identity
ExecStart=/usr/bin/dotnet /opt/workspacemanager/identity/WorkSpaceManager.Identity.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=workspacemanager-identity
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target
```

Enable and start service:

```bash
sudo systemctl daemon-reload
sudo systemctl enable workspacemanager-identity
sudo systemctl start workspacemanager-identity
sudo systemctl status workspacemanager-identity
```

### Step 6: Configure Reverse Proxy (Nginx)

Create Nginx configuration: `/etc/nginx/sites-available/workspacemanager`

```nginx
upstream api_gateway {
    server localhost:5000;
}

server {
    listen 80;
    server_name api.workspacemanager.com;

    location / {
        return 301 https://$host$request_uri;
    }
}

server {
    listen 443 ssl http2;
    server_name api.workspacemanager.com;

    ssl_certificate /etc/ssl/certs/workspacemanager.crt;
    ssl_certificate_key /etc/ssl/private/workspacemanager.key;
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers HIGH:!aNULL:!MD5;

    location / {
        proxy_pass http://api_gateway;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

Enable site:

```bash
sudo ln -s /etc/nginx/sites-available/workspacemanager /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl reload nginx
```

---

## Frontend Deployment

### Step 1: Build React Web Application

```bash
cd src/Web/workspace-manager-web

# Install dependencies
npm install

# Build for production
npm run build
```

### Step 2: Deploy to Web Server

**Option A: Nginx Static Hosting**

```bash
# Copy build files
sudo cp -r build/* /var/www/workspacemanager/

# Configure Nginx
sudo nano /etc/nginx/sites-available/workspacemanager-web
```

```nginx
server {
    listen 80;
    server_name app.workspacemanager.com;
    root /var/www/workspacemanager;
    index index.html;

    location / {
        try_files $uri $uri/ /index.html;
    }

    location /api {
        proxy_pass http://api.workspacemanager.com;
    }
}
```

**Option B: Azure Static Web Apps**

```bash
# Install Azure Static Web Apps CLI
npm install -g @azure/static-web-apps-cli

# Deploy
swa deploy --app-location ./build --api-location ./api
```

---

## Mobile App Deployment

### iOS Deployment

**Step 1: Configure App in Xcode**

```bash
cd src/Mobile/workspace-manager-mobile

# Install dependencies
npm install

# Generate iOS project
npx expo prebuild --platform ios
```

**Step 2: Update Configuration**

Edit `ios/WorkSpaceManager/Info.plist`:

```xml
<key>CFBundleDisplayName</key>
<string>WorkSpace Manager</string>
<key>CFBundleIdentifier</key>
<string>com.alphabank.workspacemanager</string>
<key>CFBundleVersion</key>
<string>1.0.0</string>
```

**Step 3: Build and Submit to App Store**

```bash
# Build archive
xcodebuild -workspace ios/WorkSpaceManager.xcworkspace \
  -scheme WorkSpaceManager \
  -configuration Release \
  -archivePath build/WorkSpaceManager.xcarchive \
  archive

# Upload to App Store Connect
xcodebuild -exportArchive \
  -archivePath build/WorkSpaceManager.xcarchive \
  -exportPath build \
  -exportOptionsPlist ExportOptions.plist
```

### Android Deployment

**Step 1: Configure App**

```bash
# Generate Android project
npx expo prebuild --platform android
```

**Step 2: Update Configuration**

Edit `android/app/build.gradle`:

```gradle
android {
    defaultConfig {
        applicationId "com.alphabank.workspacemanager"
        versionCode 1
        versionName "1.0.0"
    }
    
    signingConfigs {
        release {
            storeFile file("release.keystore")
            storePassword "your_keystore_password"
            keyAlias "workspace-manager"
            keyPassword "your_key_password"
        }
    }
}
```

**Step 3: Build APK/AAB**

```bash
cd android

# Build release APK
./gradlew assembleRelease

# Build Android App Bundle (for Google Play)
./gradlew bundleRelease
```

**Step 4: Upload to Google Play Console**

Upload the generated AAB file at `android/app/build/outputs/bundle/release/app-release.aab` to Google Play Console.

---

## Monitoring & Logging

### Application Insights Integration

Add Application Insights to each service:

```csharp
// Program.cs
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
});
```

**appsettings.Production.json:**

```json
{
  "ApplicationInsights": {
    "ConnectionString": "InstrumentationKey=your-key;IngestionEndpoint=https://..."
  }
}
```

### Serilog Configuration

```csharp
// Program.cs
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.ApplicationInsights(
        services.GetRequiredService<TelemetryConfiguration>(),
        TelemetryConverter.Traces));
```

### Health Checks

Each service exposes health check endpoints:

```
GET /health
GET /health/ready
GET /health/live
```

Configure monitoring tools (e.g., Prometheus, Grafana) to poll these endpoints.

---

## Security Hardening

### SSL/TLS Configuration

**Generate Self-Signed Certificate (Development):**

```bash
dotnet dev-certs https --trust
```

**Production Certificate:**

Use Let's Encrypt or purchase from a Certificate Authority.

```bash
# Install Certbot
sudo apt install certbot python3-certbot-nginx

# Obtain certificate
sudo certbot --nginx -d api.workspacemanager.com
```

### Environment Variables

Store sensitive configuration in environment variables or Azure Key Vault:

```bash
export ConnectionStrings__DefaultConnection="Server=...;Database=...;User Id=...;Password=..."
export Keycloak__ClientSecret="your-client-secret"
export RabbitMQ__Password="your-rabbitmq-password"
```

### Database Security

**Create Application User:**

```sql
USE WorkSpaceManager;
GO

CREATE LOGIN app_user WITH PASSWORD = 'SecurePassword123!';
CREATE USER app_user FOR LOGIN app_user;

-- Grant minimal permissions
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::dbo TO app_user;
GO
```

**Enable Encryption:**

```sql
-- Enable Transparent Data Encryption (TDE)
USE master;
GO

CREATE MASTER KEY ENCRYPTION BY PASSWORD = 'MasterKeyPassword123!';
CREATE CERTIFICATE TDECert WITH SUBJECT = 'TDE Certificate';

USE WorkSpaceManager;
GO

CREATE DATABASE ENCRYPTION KEY
WITH ALGORITHM = AES_256
ENCRYPTION BY SERVER CERTIFICATE TDECert;

ALTER DATABASE WorkSpaceManager SET ENCRYPTION ON;
GO
```

---

## Troubleshooting

### Common Issues

**Issue 1: Service Cannot Connect to SQL Server**

**Symptoms:**
```
Microsoft.Data.SqlClient.SqlException: A network-related or instance-specific error occurred
```

**Solution:**
1. Verify SQL Server is running: `docker ps | grep sqlserver`
2. Check connection string in `appsettings.json`
3. Ensure firewall allows port 1433
4. Test connection: `telnet sql-server-host 1433`

**Issue 2: Keycloak Authentication Fails**

**Symptoms:**
```
401 Unauthorized - Invalid token
```

**Solution:**
1. Verify Keycloak is running: `curl http://keycloak-host:8080/health`
2. Check realm name matches configuration
3. Verify client ID and secret
4. Ensure token is not expired

**Issue 3: RabbitMQ Connection Refused**

**Symptoms:**
```
RabbitMQ.Client.Exceptions.BrokerUnreachableException
```

**Solution:**
1. Verify RabbitMQ is running: `docker ps | grep rabbitmq`
2. Check credentials in configuration
3. Verify network connectivity: `telnet rabbitmq-host 5672`
4. Check RabbitMQ logs: `docker logs rabbitmq`

### Logs Location

**Windows Services:**
- Event Viewer → Windows Logs → Application
- Service-specific logs: `C:\Services\{ServiceName}\logs\`

**Linux Systemd Services:**
```bash
# View service logs
sudo journalctl -u workspacemanager-identity -f

# View all WorkSpaceManager services
sudo journalctl -u workspacemanager-* -f
```

**Docker Containers:**
```bash
# View SQL Server logs
docker logs sqlserver

# View Keycloak logs
docker logs keycloak

# Follow logs in real-time
docker logs -f rabbitmq
```

---

## Production Checklist

Before deploying to production, verify the following:

**Infrastructure:**
- [ ] SQL Server is configured with high availability (Always On, clustering)
- [ ] Keycloak is deployed in HA mode with PostgreSQL replication
- [ ] RabbitMQ is configured with clustering and mirrored queues
- [ ] Load balancer is configured for API Gateway
- [ ] SSL/TLS certificates are installed and valid
- [ ] Firewall rules are configured correctly

**Security:**
- [ ] All default passwords have been changed
- [ ] Database user has minimal required permissions
- [ ] Secrets are stored in Azure Key Vault or equivalent
- [ ] HTTPS is enforced for all endpoints
- [ ] CORS policies are configured correctly
- [ ] Rate limiting is enabled on API Gateway

**Monitoring:**
- [ ] Application Insights is configured
- [ ] Health check endpoints are monitored
- [ ] Alerts are configured for critical errors
- [ ] Log retention policies are defined
- [ ] Backup procedures are tested

**Testing:**
- [ ] Integration tests pass
- [ ] Load testing completed
- [ ] Security scanning completed
- [ ] Penetration testing completed (if required)

---

**Document Version:** 1.0  
**Last Updated:** December 2025  
**Author:** Manus AI
