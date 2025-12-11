#!/bin/bash

# Keycloak Configuration Validation Script
# This script validates that Keycloak is properly configured for WorkSpaceManager

set -e

# Configuration
KEYCLOAK_URL="${KEYCLOAK_URL:-http://localhost:8080}"
REALM="${REALM:-alpha-bank-realm}"
ADMIN_USER="${ADMIN_USER:-admin}"
ADMIN_PASSWORD="${ADMIN_PASSWORD:-admin}"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Counters
PASSED=0
FAILED=0

echo "=========================================="
echo "Keycloak Configuration Validation"
echo "=========================================="
echo "Keycloak URL: $KEYCLOAK_URL"
echo "Realm: $REALM"
echo ""

# Function to print success
success() {
    echo -e "${GREEN}✓${NC} $1"
    ((PASSED++))
}

# Function to print failure
failure() {
    echo -e "${RED}✗${NC} $1"
    ((FAILED++))
}

# Function to print warning
warning() {
    echo -e "${YELLOW}⚠${NC} $1"
}

# Function to print info
info() {
    echo -e "ℹ $1"
}

# Get admin token
echo "Step 1: Authenticating with Keycloak..."
TOKEN_RESPONSE=$(curl -s -X POST "$KEYCLOAK_URL/realms/master/protocol/openid-connect/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "username=$ADMIN_USER" \
  -d "password=$ADMIN_PASSWORD" \
  -d "grant_type=password" \
  -d "client_id=admin-cli")

if [ $? -ne 0 ]; then
    failure "Failed to connect to Keycloak"
    exit 1
fi

TOKEN=$(echo "$TOKEN_RESPONSE" | jq -r '.access_token')

if [ "$TOKEN" == "null" ] || [ -z "$TOKEN" ]; then
    failure "Failed to get admin token"
    echo "Response: $TOKEN_RESPONSE"
    exit 1
fi

success "Authenticated successfully"
echo ""

# Check if realm exists
echo "Step 2: Checking realm configuration..."
REALM_RESPONSE=$(curl -s -X GET "$KEYCLOAK_URL/admin/realms/$REALM" \
  -H "Authorization: Bearer $TOKEN")

if echo "$REALM_RESPONSE" | jq -e '.realm' > /dev/null 2>&1; then
    success "Realm '$REALM' exists"
    
    # Check realm settings
    REALM_ENABLED=$(echo "$REALM_RESPONSE" | jq -r '.enabled')
    if [ "$REALM_ENABLED" == "true" ]; then
        success "Realm is enabled"
    else
        failure "Realm is disabled"
    fi
    
    # Check internationalization
    I18N_ENABLED=$(echo "$REALM_RESPONSE" | jq -r '.internationalizationEnabled')
    if [ "$I18N_ENABLED" == "true" ]; then
        success "Internationalization is enabled"
    else
        warning "Internationalization is disabled"
    fi
    
    # Check brute force protection
    BRUTE_FORCE=$(echo "$REALM_RESPONSE" | jq -r '.bruteForceProtected')
    if [ "$BRUTE_FORCE" == "true" ]; then
        success "Brute force protection is enabled"
    else
        warning "Brute force protection is disabled"
    fi
else
    failure "Realm '$REALM' does not exist"
fi
echo ""

# Check clients
echo "Step 3: Checking client configurations..."
CLIENTS=("workspace-manager-api" "workspace-manager-web" "workspace-manager-mobile")

for CLIENT_ID in "${CLIENTS[@]}"; do
    CLIENT_RESPONSE=$(curl -s -X GET "$KEYCLOAK_URL/admin/realms/$REALM/clients?clientId=$CLIENT_ID" \
      -H "Authorization: Bearer $TOKEN")
    
    if echo "$CLIENT_RESPONSE" | jq -e '.[0].clientId' > /dev/null 2>&1; then
        success "Client '$CLIENT_ID' exists"
        
        # Get client details
        CLIENT_UUID=$(echo "$CLIENT_RESPONSE" | jq -r '.[0].id')
        CLIENT_DETAILS=$(curl -s -X GET "$KEYCLOAK_URL/admin/realms/$REALM/clients/$CLIENT_UUID" \
          -H "Authorization: Bearer $TOKEN")
        
        # Check if enabled
        CLIENT_ENABLED=$(echo "$CLIENT_DETAILS" | jq -r '.enabled')
        if [ "$CLIENT_ENABLED" == "true" ]; then
            success "  Client is enabled"
        else
            failure "  Client is disabled"
        fi
        
        # Check protocol mappers
        MAPPERS_COUNT=$(echo "$CLIENT_DETAILS" | jq '.protocolMappers | length')
        if [ "$MAPPERS_COUNT" -gt 0 ]; then
            success "  Protocol mappers configured ($MAPPERS_COUNT mappers)"
        else
            warning "  No protocol mappers configured"
        fi
    else
        failure "Client '$CLIENT_ID' does not exist"
    fi
done
echo ""

# Check roles
echo "Step 4: Checking role configurations..."
ROLES=("admin" "manager" "user" "facility_manager" "hr")

for ROLE_NAME in "${ROLES[@]}"; do
    ROLE_RESPONSE=$(curl -s -X GET "$KEYCLOAK_URL/admin/realms/$REALM/roles/$ROLE_NAME" \
      -H "Authorization: Bearer $TOKEN")
    
    if echo "$ROLE_RESPONSE" | jq -e '.name' > /dev/null 2>&1; then
        success "Role '$ROLE_NAME' exists"
        
        # Check if composite (for admin and manager)
        if [ "$ROLE_NAME" == "admin" ] || [ "$ROLE_NAME" == "manager" ]; then
            IS_COMPOSITE=$(echo "$ROLE_RESPONSE" | jq -r '.composite')
            if [ "$IS_COMPOSITE" == "true" ]; then
                success "  Role is composite"
            else
                warning "  Role should be composite"
            fi
        fi
    else
        failure "Role '$ROLE_NAME' does not exist"
    fi
done
echo ""

# Check user attributes configuration
echo "Step 5: Checking user attributes..."
USER_PROFILE=$(curl -s -X GET "$KEYCLOAK_URL/admin/realms/$REALM/users/profile" \
  -H "Authorization: Bearer $TOKEN")

CUSTOM_ATTRIBUTES=("employeeId" "department" "jobTitle" "managerId" "tenant_id" "preferredLanguage")

for ATTR in "${CUSTOM_ATTRIBUTES[@]}"; do
    if echo "$USER_PROFILE" | jq -e ".attributes[] | select(.name == \"$ATTR\")" > /dev/null 2>&1; then
        success "Attribute '$ATTR' is configured"
    else
        warning "Attribute '$ATTR' is not configured"
    fi
done
echo ""

# Test token endpoint
echo "Step 6: Testing token endpoint..."
TEST_TOKEN_RESPONSE=$(curl -s -X POST "$KEYCLOAK_URL/realms/$REALM/protocol/openid-connect/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "client_id=workspace-manager-web" \
  -d "grant_type=client_credentials" 2>&1)

if echo "$TEST_TOKEN_RESPONSE" | grep -q "unsupported_grant_type"; then
    success "Token endpoint is accessible (client_credentials not enabled for public client - expected)"
elif echo "$TEST_TOKEN_RESPONSE" | grep -q "access_token"; then
    success "Token endpoint is working"
else
    warning "Token endpoint response unexpected"
fi
echo ""

# Check OpenID configuration
echo "Step 7: Checking OpenID Connect configuration..."
OIDC_CONFIG=$(curl -s -X GET "$KEYCLOAK_URL/realms/$REALM/.well-known/openid-configuration")

if echo "$OIDC_CONFIG" | jq -e '.issuer' > /dev/null 2>&1; then
    success "OpenID Connect discovery endpoint is accessible"
    
    ISSUER=$(echo "$OIDC_CONFIG" | jq -r '.issuer')
    info "  Issuer: $ISSUER"
    
    TOKEN_ENDPOINT=$(echo "$OIDC_CONFIG" | jq -r '.token_endpoint')
    info "  Token endpoint: $TOKEN_ENDPOINT"
    
    USERINFO_ENDPOINT=$(echo "$OIDC_CONFIG" | jq -r '.userinfo_endpoint')
    info "  UserInfo endpoint: $USERINFO_ENDPOINT"
else
    failure "OpenID Connect discovery endpoint is not accessible"
fi
echo ""

# Check identity providers
echo "Step 8: Checking identity providers..."
IDP_RESPONSE=$(curl -s -X GET "$KEYCLOAK_URL/admin/realms/$REALM/identity-provider/instances" \
  -H "Authorization: Bearer $TOKEN")

IDP_COUNT=$(echo "$IDP_RESPONSE" | jq 'length')
if [ "$IDP_COUNT" -gt 0 ]; then
    success "Identity providers configured ($IDP_COUNT providers)"
    echo "$IDP_RESPONSE" | jq -r '.[].alias' | while read -r IDP_ALIAS; do
        info "  - $IDP_ALIAS"
    done
else
    info "No identity providers configured (optional)"
fi
echo ""

# Summary
echo "=========================================="
echo "Validation Summary"
echo "=========================================="
echo -e "${GREEN}Passed:${NC} $PASSED"
echo -e "${RED}Failed:${NC} $FAILED"
echo ""

if [ $FAILED -eq 0 ]; then
    echo -e "${GREEN}✓ All critical checks passed!${NC}"
    echo ""
    echo "Next steps:"
    echo "1. Create test users"
    echo "2. Test authentication flow"
    echo "3. Integrate with backend API"
    echo "4. Test frontend applications"
    exit 0
else
    echo -e "${RED}✗ Some checks failed. Please review the configuration.${NC}"
    exit 1
fi
