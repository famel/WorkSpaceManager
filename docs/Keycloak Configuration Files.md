# Keycloak Configuration Files

This directory contains all necessary configuration files to set up Keycloak for the WorkSpaceManager system.

---

## 📁 Files Overview

### Core Configuration Files

| File | Description | Usage |
|------|-------------|-------|
| `alpha-bank-realm.json` | Complete realm configuration | Import to create the realm |
| `client-workspace-manager-api.json` | Backend API client | Import after realm creation |
| `client-workspace-manager-web.json` | React web client | Import after realm creation |
| `client-workspace-manager-mobile.json` | React Native mobile client | Import after realm creation |
| `roles.json` | Realm and client roles | Reference for manual role creation |
| `user-attributes.json` | Custom user attributes | Reference for user profile configuration |

### Identity Provider Configuration

| File | Description | Usage |
|------|-------------|-------|
| `identity-provider-azure-ad.json` | Azure AD SSO integration | Import for Azure AD authentication |
| `identity-provider-mappers-azure-ad.json` | Azure AD attribute mappers | Import after identity provider |

### Deployment Files

| File | Description | Usage |
|------|-------------|-------|
| `docker-compose.yml` | Docker Compose configuration | Run `docker-compose up -d` |
| `sample-users.json` | Test users for development | Import for testing |
| `validate-keycloak.sh` | Validation script | Run to verify configuration |
| `IMPORT_GUIDE.md` | Detailed import instructions | Follow for step-by-step setup |

---

## 🚀 Quick Start

### Option 1: Docker Compose (Recommended)

```bash
# Start Keycloak with PostgreSQL
docker-compose up -d

# Wait for Keycloak to be ready (check logs)
docker-compose logs -f keycloak

# Access Keycloak Admin Console
# URL: http://localhost:8080
# Username: admin
# Password: admin

# Validate configuration
./validate-keycloak.sh
```

### Option 2: Manual Import

1. **Import Realm**
   ```bash
   # Via Admin Console: Create Realm → Browse → alpha-bank-realm.json
   # Or via CLI:
   /opt/keycloak/bin/kc.sh import --file=alpha-bank-realm.json
   ```

2. **Import Clients**
   ```bash
   # Via Admin Console: Clients → Import client
   # Import each client JSON file
   ```

3. **Create Roles**
   ```bash
   # Via Admin Console: Realm roles → Create role
   # Create roles from roles.json
   ```

4. **Import Test Users** (Optional)
   ```bash
   # Via Admin Console: Users → Add user
   # Or import sample-users.json
   ```

5. **Validate Configuration**
   ```bash
   ./validate-keycloak.sh
   ```

---

## 🔧 Configuration Details

### Realm Settings

**Realm Name:** `alpha-bank-realm`

**Key Features:**
- ✅ Internationalization (English, Greek)
- ✅ Brute force protection enabled
- ✅ Event logging enabled
- ✅ Email verification enabled
- ✅ Remember me enabled
- ✅ Password reset enabled

**Token Lifespans:**
- Access Token: 900 seconds (15 minutes)
- SSO Session Idle: 1800 seconds (30 minutes)
- SSO Session Max: 36000 seconds (10 hours)
- Offline Session Idle: 2592000 seconds (30 days)

### Client Configurations

#### 1. workspace-manager-api (Backend)

**Type:** Confidential  
**Purpose:** Backend API service

**Settings:**
- Service Accounts: ✅ Enabled
- Authorization: ✅ Enabled
- Direct Access Grants: ✅ Enabled
- Standard Flow: ❌ Disabled

**Protocol Mappers:**
- tenant_id
- employeeId
- department
- jobTitle
- managerId
- preferredLanguage
- realm_access.roles

#### 2. workspace-manager-web (React Web)

**Type:** Public  
**Purpose:** React web application

**Settings:**
- Standard Flow: ✅ Enabled
- Direct Access Grants: ✅ Enabled (for development)
- PKCE: ✅ Enabled (S256)

**Redirect URIs:**
- `http://localhost:3000/*`
- `https://app.workspacemanager.com/*`

**Web Origins:**
- `http://localhost:3000`
- `https://app.workspacemanager.com`

#### 3. workspace-manager-mobile (React Native)

**Type:** Public  
**Purpose:** React Native mobile app

**Settings:**
- Standard Flow: ✅ Enabled
- PKCE: ✅ Enabled (S256)
- Direct Access Grants: ❌ Disabled

**Redirect URIs:**
- `workspacemanager://*`
- `com.alphabank.workspacemanager://*`
- `exp://localhost:19000/*` (Expo development)

**Token Lifespan:**
- Access Token: 1800 seconds (30 minutes)

### Roles

| Role | Description | Composite | Default |
|------|-------------|-----------|---------|
| `admin` | Full system access | ✅ (includes all roles) | ❌ |
| `manager` | Team oversight | ✅ (includes user) | ❌ |
| `user` | Standard user | ❌ | ✅ |
| `facility_manager` | Space management | ❌ | ❌ |
| `hr` | HR and policy management | ❌ | ❌ |
| `guest` | Limited guest access | ❌ | ❌ |

### User Attributes

| Attribute | Type | Required | Editable By |
|-----------|------|----------|-------------|
| `employeeId` | String | ✅ | Admin |
| `department` | String | ❌ | Admin |
| `jobTitle` | String | ❌ | Admin |
| `managerId` | String (GUID) | ❌ | Admin |
| `tenant_id` | String (GUID) | ✅ | Admin |
| `preferredLanguage` | String (en/el) | ❌ | Admin, User |

---

## 🔐 Security Configuration

### Brute Force Protection

**Settings:**
- ✅ Enabled
- Max Failures: 5
- Wait Increment: 60 seconds
- Max Wait: 900 seconds (15 minutes)
- Failure Reset: 43200 seconds (12 hours)

### Password Policy

**Default Policy:**
- Minimum length: 8 characters
- At least 1 uppercase letter
- At least 1 lowercase letter
- At least 1 digit
- At least 1 special character

**To Configure:**
```
Realm Settings → Security Defenses → Password Policy
```

### Session Management

**Settings:**
- SSO Session Idle Timeout: 30 minutes
- SSO Session Max Lifespan: 10 hours
- Offline Session Idle Timeout: 30 days
- Client Session Idle Timeout: Inherited
- Client Session Max Lifespan: Inherited

---

## 🌐 Azure AD Integration

### Prerequisites

1. Azure AD tenant
2. App registration in Azure AD
3. Client ID and Client Secret
4. Tenant ID

### Configuration Steps

1. **Import Identity Provider**
   ```bash
   # Update identity-provider-azure-ad.json with your values:
   # - YOUR_AZURE_AD_CLIENT_ID
   # - YOUR_TENANT_ID
   # - YOUR_AZURE_AD_CLIENT_SECRET
   
   # Import via Admin Console:
   # Identity Providers → Add provider → OpenID Connect v1.0
   ```

2. **Import Mappers**
   ```bash
   # Import identity-provider-mappers-azure-ad.json
   # Identity Providers → azure-ad → Mappers → Import
   ```

3. **Test SSO**
   ```bash
   # Navigate to login page
   # Click "Alpha Bank Azure AD" button
   # Authenticate with Azure AD credentials
   ```

### Azure AD Custom Attributes

To sync custom attributes from Azure AD, configure extension attributes:

```json
{
  "claim": "extension_employeeId",
  "user.attribute": "employeeId"
}
```

---

## 📧 Email Configuration

### SMTP Settings

**For Office 365:**
```
Host: smtp.office365.com
Port: 587
From: noreply@alphabank.gr
Enable StartTLS: ON
Enable Authentication: ON
Username: noreply@alphabank.gr
Password: [Your password]
```

**For Gmail:**
```
Host: smtp.gmail.com
Port: 587
From: noreply@example.com
Enable StartTLS: ON
Enable Authentication: ON
Username: noreply@example.com
Password: [App password]
```

**For Development (MailHog):**
```
Host: mailhog
Port: 1025
From: noreply@localhost
Enable StartTLS: OFF
Enable Authentication: OFF
```

### Email Templates

Email templates can be customized at:
```
Realm Settings → Themes → Email Theme
```

---

## 🧪 Testing

### 1. Test Authentication

```bash
# Get access token
curl -X POST "http://localhost:8080/realms/alpha-bank-realm/protocol/openid-connect/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "client_id=workspace-manager-web" \
  -d "grant_type=password" \
  -d "username=user1@alphabank.gr" \
  -d "password=User@123"
```

### 2. Validate Token

```bash
# Decode token at https://jwt.io
# Verify claims:
# - iss: http://localhost:8080/realms/alpha-bank-realm
# - aud: workspace-manager-api
# - employeeId, department, tenant_id present
# - realm_access.roles contains assigned roles
```

### 3. Test User Info

```bash
curl -X GET "http://localhost:8080/realms/alpha-bank-realm/protocol/openid-connect/userinfo" \
  -H "Authorization: Bearer {ACCESS_TOKEN}"
```

### 4. Run Validation Script

```bash
./validate-keycloak.sh
```

---

## 🔄 Backup and Restore

### Backup

```bash
# Export realm
docker exec keycloak /opt/keycloak/bin/kc.sh export \
  --dir /tmp/export \
  --realm alpha-bank-realm

# Copy from container
docker cp keycloak:/tmp/export ./backup/
```

### Restore

```bash
# Copy to container
docker cp ./backup/alpha-bank-realm.json keycloak:/tmp/

# Import realm
docker exec keycloak /opt/keycloak/bin/kc.sh import \
  --file /tmp/alpha-bank-realm.json \
  --override true
```

---

## 📊 Monitoring

### Health Check

```bash
curl http://localhost:8080/health
```

### Metrics

```bash
curl http://localhost:8080/metrics
```

### Event Logs

View in Admin Console:
```
Realm Settings → Events → Login events
Realm Settings → Events → Admin events
```

---

## 🚀 Production Deployment

### Checklist

- [ ] Use PostgreSQL or MySQL (not H2)
- [ ] Enable HTTPS/TLS
- [ ] Set strong admin password
- [ ] Configure production redirect URIs
- [ ] Update client secrets
- [ ] Enable brute force protection
- [ ] Configure SMTP for email
- [ ] Set up database backups
- [ ] Enable audit logging
- [ ] Configure high availability
- [ ] Set appropriate token lifespans
- [ ] Review security settings
- [ ] Set up monitoring
- [ ] Configure reverse proxy (Nginx/Apache)
- [ ] Enable rate limiting

### Environment Variables

Create `.env` file:

```env
# Database
DB_NAME=keycloak
DB_USER=keycloak
DB_PASSWORD=strong_password_here

# Keycloak Admin
ADMIN_USER=admin
ADMIN_PASSWORD=strong_admin_password_here

# Hostname
KEYCLOAK_HOSTNAME=auth.workspacemanager.com

# SMTP
SMTP_HOST=smtp.office365.com
SMTP_PORT=587
SMTP_USER=noreply@alphabank.gr
SMTP_PASSWORD=smtp_password_here
```

---

## 📚 Additional Resources

- [Keycloak Documentation](https://www.keycloak.org/documentation)
- [Server Administration Guide](https://www.keycloak.org/docs/latest/server_admin/)
- [Securing Applications](https://www.keycloak.org/docs/latest/securing_apps/)
- [REST API Documentation](https://www.keycloak.org/docs-api/latest/rest-api/)

---

## 🆘 Support

For issues or questions:
1. Check `IMPORT_GUIDE.md` for detailed instructions
2. Run `./validate-keycloak.sh` to diagnose issues
3. Review Keycloak logs: `docker-compose logs keycloak`
4. Consult the main implementation README

---

**Version:** 1.0  
**Last Updated:** December 2025  
**Author:** Manus AI
