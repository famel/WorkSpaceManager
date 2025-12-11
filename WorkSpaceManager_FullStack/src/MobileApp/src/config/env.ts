// Environment Configuration
export const ENV = {
  // API Configuration
  API_URL: 'http://localhost:5000', // Change to your API Gateway URL
  
  // Keycloak Configuration
  KEYCLOAK_URL: 'http://localhost:8080',
  KEYCLOAK_REALM: 'alpha-bank-realm',
  KEYCLOAK_CLIENT_ID: 'workspace-manager-mobile',
  KEYCLOAK_REDIRECT_URI: 'workspacemanager://oauth',
  
  // App Configuration
  APP_NAME: 'WorkSpace Manager',
  APP_VERSION: '1.0.0',
};

export default ENV;
