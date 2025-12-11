# Keycloak Configuration Import Guide

This guide provides step-by-step instructions for importing and configuring Keycloak for the WorkSpaceManager system.

---

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Quick Start](#quick-start)
3. [Manual Import Steps](#manual-import-steps)
4. [Configuration Files Overview](#configuration-files-overview)
5. [Post-Import Configuration](#post-import-configuration)
6. [Verification](#verification)
7. [Troubleshooting](#troubleshooting)

---

## Prerequisites

**Required:**
- Keycloak 22.0 or later installed and running
- Admin access to Keycloak Admin Console
- Docker (optional, for containerized deployment)

**Optional:**
- Azure AD tenant (for SSO integration)
- SMTP server credentials (for email notifications)

---

## Quick Start

### Option 1: Docker Compose (Recommended for Development)

```bash
# Start Keycloak with PostgreSQL
docker-compose up -d

# Wait for Keycloak to start (check logs)
docker-compose logs -f keycloak

# Access Keycloak Admin Console
# URL: http://localhost:8080
# Username: admin
# Password: admin
```

### Option 2: Import via Admin Console

1. **Access Keycloak Admin Console**
   - URL: `http://localhost:8080`
   - Login with admin credentials

2. **Import Realm**
   - Navigate to: Master realm → Create Realm
   - Click "Browse" and select `alpha-bank-realm.json`
   - Click "Create"

3. **Import Clients**
   - Navigate to: Clients → Import client
   - Import each client JSON file:
     - `client-workspace-manager-api.json`
     - `client-workspace-manager-web.json`
     - `client-workspace-manager-mobile.json`

4. **Configure Roles**
   - Navigate to: Realm roles → Create role
   - Create roles from `roles.json`

---

## Manual Import Steps

### Step 1: Create Realm

**Via Admin Console:**
1. Go to Master realm dropdown → Create Realm
2. Upload `alpha-bank-realm.json`
3. Click "Create"

**Via CLI:**
```bash
# Using Keycloak CLI
/opt/keycloak/bin/kc.sh import \
  --file=/path/to/alpha-bank-realm.json \
  --override true
```

**Via REST API:**
```bash
# Get admin token
TOKEN=$(curl -X POST "http://localhost:8080/realms/master/protocol/openid-connect/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "username=admin" \
  -d "password=admin" \
  -d "grant_type=password" \
  -d "client_id=admin-cli" | jq -r '.access_token')

# Import realm
curl -X POST "http://localhost:8080/admin/realms" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d @alpha-bank-realm.json
```

### Step 2: Import Clients

**For each client (API, Web, Mobile):**

1. Navigate to: Clients → Import client
2. Select the client JSON file
3. Click "Save"

**Important:** Update the client secret for `workspace-manager-api`:
1. Go to: Clients → workspace-manager-api → Credentials
2. Click "Regenerate Secret"
3. Copy the secret and update your backend `appsettings.json`

### Step 3: Create Roles

**Via Admin Console:**
1. Navigate to: Realm roles
2. Click "Create role"
3. For each role in `roles.json`:
   - Name: (e.g., `admin`, `manager`, `user`)
   - Description: (copy from JSON)
   - Click "Save"

**Create Composite Roles:**
1. Edit `admin` role
2. Go to "Composite roles" tab
3. Add: `manager`, `user`, `facility_manager`, `hr`
4. Save

### Step 4: Configure User Attributes

**Via Admin Console:**
1. Navigate to: Realm settings → User profile
2. Click "Create attribute" for each custom attribute:
   - `employeeId`
   - `department`
   - `jobTitle`
   - `managerId`
   - `tenant_id`
   - `preferredLanguage`

**Attribute Configuration Example (employeeId):**
- Attribute name: `employeeId`
- Display name: `Employee ID`
- Required for: `user` role
- Permissions: View (admin, user), Edit (admin)

### Step 5: Configure Azure AD Integration (Optional)

**Prerequisites:**
1. Azure AD application registration
2. Client ID and Client Secret
3. Tenant ID

**Steps:**
1. Navigate to: Identity providers → Add provider → OpenID Connect v1.0
2. Fill in the configuration:
   - Alias: `azure-ad`
   - Display name: `Alpha Bank Azure AD`
   - Authorization URL: `https://login.microsoftonline.com/{TENANT_ID}/oauth2/v2.0/authorize`
   - Token URL: `https://login.microsoftonline.com/{TENANT_ID}/oauth2/v2.0/token`
   - Client ID: `{YOUR_CLIENT_ID}`
   - Client Secret: `{YOUR_CLIENT_SECRET}`
   - Default Scopes: `openid profile email`

3. Configure Mappers:
   - Navigate to: Identity providers → azure-ad → Mappers
   - Import mappers from `identity-provider-mappers-azure-ad.json`

### Step 6: Configure SMTP (Optional)

1. Navigate to: Realm settings → Email
2. Fill in SMTP configuration:
   - Host: `smtp.office365.com`
   - Port: `587`
   - From: `noreply@alphabank.gr`
   - Enable StartTLS: `ON`
   - Enable Authentication: `ON`
   - Username: `{SMTP_USERNAME}`
   - Password: `{SMTP_PASSWORD}`

3. Test email:
   - Click "Test connection"

---

## Configuration Files Overview

### Realm Configuration
**File:** `alpha-bank-realm.json`

**Contains:**
- Realm settings and policies
- Token lifespans
- Session timeouts
- Security settings
- Internationalization (English, Greek)
- Event logging configuration

### Client Configurations

**1. API Client** (`client-workspace-manager-api.json`)
- **Type:** Confidential
- **Purpose:** Backend API authentication
- **Features:**
  - Service accounts enabled
  - Authorization services enabled
  - Direct access grants enabled
  - Custom protocol mappers for user attributes

**2. Web Client** (`client-workspace-manager-web.json`)
- **Type:** Public
- **Purpose:** React web application
- **Features:**
  - Standard flow (Authorization Code)
  - PKCE enabled
  - Direct access grants for development
  - Web origins configured

**3. Mobile Client** (`client-workspace-manager-mobile.json`)
- **Type:** Public
- **Purpose:** React Native mobile app
- **Features:**
  - Standard flow with PKCE
  - Custom redirect URIs for mobile
  - Refresh token support
  - Longer access token lifespan

### Roles Configuration
**File:** `roles.json`

**Roles:**
- `admin` - Full system access (composite)
- `manager` - Team oversight (composite)
- `user` - Standard user (default)
- `facility_manager` - Space management
- `hr` - HR and policy management
- `guest` - Limited guest access

### Identity Provider Configuration
**Files:**
- `identity-provider-azure-ad.json`
- `identity-provider-mappers-azure-ad.json`

**Features:**
- Azure AD SSO integration
- Automatic user attribute mapping
- Role synchronization

---

## Post-Import Configuration

### 1. Update Client Secrets

**API Client:**
```bash
# Get the client secret from Keycloak
# Update appsettings.json:
{
  "Keycloak": {
    "ClientSecret": "YOUR_GENERATED_SECRET"
  }
}
```

### 2. Update Redirect URIs

**Production URLs:**
1. Navigate to each client
2. Update redirect URIs:
   - Web: `https://app.workspacemanager.com/*`
   - Mobile: Update with production scheme

### 3. Create Test Users

**Via Admin Console:**
1. Navigate to: Users → Add user
2. Fill in user details:
   - Username: `test.user@alphabank.gr`
   - Email: `test.user@alphabank.gr`
   - First name: `Test`
   - Last name: `User`
   - Email verified: `ON`

3. Set attributes:
   - employeeId: `EMP001`
   - department: `IT`
   - jobTitle: `Software Engineer`
   - tenant_id: `{GUID}`
   - preferredLanguage: `en`

4. Set password:
   - Go to: Credentials tab
   - Set password
   - Temporary: `OFF`

5. Assign roles:
   - Go to: Role mapping tab
   - Assign: `user` role

**Via REST API:**
```bash
# Create user
curl -X POST "http://localhost:8080/admin/realms/alpha-bank-realm/users" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "test.user@alphabank.gr",
    "email": "test.user@alphabank.gr",
    "firstName": "Test",
    "lastName": "User",
    "enabled": true,
    "emailVerified": true,
    "attributes": {
      "employeeId": ["EMP001"],
      "department": ["IT"],
      "tenant_id": ["YOUR_TENANT_GUID"]
    }
  }'
```

### 4. Enable Required Features

**Brute Force Protection:**
- Already enabled in realm configuration
- Verify: Realm settings → Security defenses

**Event Logging:**
- Already enabled in realm configuration
- Verify: Realm settings → Events

---

## Verification

### 1. Test Token Endpoint

```bash
# Get access token
curl -X POST "http://localhost:8080/realms/alpha-bank-realm/protocol/openid-connect/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "client_id=workspace-manager-web" \
  -d "grant_type=password" \
  -d "username=test.user@alphabank.gr" \
  -d "password=yourpassword"
```

**Expected Response:**
```json
{
  "access_token": "eyJhbGc...",
  "expires_in": 900,
  "refresh_expires_in": 1800,
  "refresh_token": "eyJhbGc...",
  "token_type": "Bearer"
}
```

### 2. Decode and Verify Token

Use [jwt.io](https://jwt.io) to decode the access token and verify:
- ✅ Issuer: `http://localhost:8080/realms/alpha-bank-realm`
- ✅ Audience: `workspace-manager-api`
- ✅ Custom claims present: `employeeId`, `department`, `tenant_id`
- ✅ Roles present in `realm_access.roles`

### 3. Test User Info Endpoint

```bash
curl -X GET "http://localhost:8080/realms/alpha-bank-realm/protocol/openid-connect/userinfo" \
  -H "Authorization: Bearer {ACCESS_TOKEN}"
```

### 4. Test Backend Integration

```bash
# Call your API with the token
curl -X GET "http://localhost:5000/api/bookings/my" \
  -H "Authorization: Bearer {ACCESS_TOKEN}"
```

---

## Troubleshooting

### Issue 1: Import Fails

**Symptoms:**
```
Error importing realm: Invalid JSON
```

**Solutions:**
1. Validate JSON files: `jq . alpha-bank-realm.json`
2. Check Keycloak version compatibility
3. Try importing via CLI instead of Admin Console

### Issue 2: Client Secret Not Working

**Symptoms:**
```
401 Unauthorized - Invalid client credentials
```

**Solutions:**
1. Regenerate client secret in Keycloak
2. Update `appsettings.json` with new secret
3. Verify client ID matches exactly

### Issue 3: Token Missing Custom Claims

**Symptoms:**
```
Token doesn't contain employeeId, department, etc.
```

**Solutions:**
1. Verify protocol mappers are configured
2. Check user attributes are set
3. Ensure mappers are enabled for the client

### Issue 4: Azure AD Integration Not Working

**Symptoms:**
```
Error: Invalid redirect URI
```

**Solutions:**
1. Verify Azure AD app registration redirect URIs
2. Check tenant ID is correct
3. Verify client secret hasn't expired
4. Test Azure AD endpoints manually

### Issue 5: CORS Errors

**Symptoms:**
```
Access blocked by CORS policy
```

**Solutions:**
1. Add web origins to client configuration
2. Verify redirect URIs include the origin
3. Check backend CORS policy

---

## Docker Compose Configuration

Create `docker-compose.yml`:

```yaml
version: '3.8'

services:
  postgres:
    image: postgres:15-alpine
    environment:
      POSTGRES_DB: keycloak
      POSTGRES_USER: keycloak
      POSTGRES_PASSWORD: keycloak
    volumes:
      - postgres_data:/var/lib/postgresql/data
    networks:
      - keycloak-network

  keycloak:
    image: quay.io/keycloak/keycloak:22.0
    command: start-dev --import-realm
    environment:
      KC_DB: postgres
      KC_DB_URL: jdbc:postgresql://postgres:5432/keycloak
      KC_DB_USERNAME: keycloak
      KC_DB_PASSWORD: keycloak
      KEYCLOAK_ADMIN: admin
      KEYCLOAK_ADMIN_PASSWORD: admin
      KC_HEALTH_ENABLED: true
      KC_METRICS_ENABLED: true
    ports:
      - "8080:8080"
    volumes:
      - ./Configuration:/opt/keycloak/data/import
    depends_on:
      - postgres
    networks:
      - keycloak-network

volumes:
  postgres_data:

networks:
  keycloak-network:
    driver: bridge
```

**Start:**
```bash
docker-compose up -d
```

**Stop:**
```bash
docker-compose down
```

**View Logs:**
```bash
docker-compose logs -f keycloak
```

---

## Production Deployment Checklist

- [ ] Use PostgreSQL or MySQL (not H2)
- [ ] Enable HTTPS/TLS
- [ ] Set strong admin password
- [ ] Configure production redirect URIs
- [ ] Enable brute force protection
- [ ] Configure SMTP for email
- [ ] Set up backup strategy
- [ ] Enable audit logging
- [ ] Configure high availability (if needed)
- [ ] Set appropriate token lifespans
- [ ] Review and test all security settings
- [ ] Document client secrets securely
- [ ] Set up monitoring and alerting

---

## Additional Resources

- [Keycloak Documentation](https://www.keycloak.org/documentation)
- [Keycloak Server Administration Guide](https://www.keycloak.org/docs/latest/server_admin/)
- [Keycloak REST API](https://www.keycloak.org/docs-api/latest/rest-api/)
- [Azure AD Integration Guide](https://docs.microsoft.com/en-us/azure/active-directory/develop/)

---

**Document Version:** 1.0  
**Last Updated:** December 2025  
**Author:** Manus AI
