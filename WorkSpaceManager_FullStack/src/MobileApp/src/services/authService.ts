import * as AuthSession from 'expo-auth-session';
import * as SecureStore from 'expo-secure-store';
import * as LocalAuthentication from 'expo-local-authentication';
import { jwtDecode } from 'jwt-decode';
import ENV from '../config/env';

const TOKEN_KEY = 'access_token';
const REFRESH_TOKEN_KEY = 'refresh_token';
const BIOMETRIC_ENABLED_KEY = 'biometric_enabled';

interface TokenResponse {
  access_token: string;
  refresh_token: string;
  expires_in: number;
  refresh_expires_in: number;
  token_type: string;
}

interface DecodedToken {
  sub: string;
  email?: string;
  name?: string;
  preferred_username?: string;
  tenant_id?: string;
  roles?: string[];
  exp: number;
}

class AuthService {
  private discovery = {
    authorizationEndpoint: `${ENV.KEYCLOAK_URL}/realms/${ENV.KEYCLOAK_REALM}/protocol/openid-connect/auth`,
    tokenEndpoint: `${ENV.KEYCLOAK_URL}/realms/${ENV.KEYCLOAK_REALM}/protocol/openid-connect/token`,
  };

  // OAuth Login
  async login(): Promise<boolean> {
    try {
      const redirectUri = AuthSession.makeRedirectUri({
        scheme: 'workspacemanager',
      });

      const result = await AuthSession.startAsync({
        authUrl: `${this.discovery.authorizationEndpoint}?${new URLSearchParams({
          client_id: ENV.KEYCLOAK_CLIENT_ID,
          redirect_uri: redirectUri,
          response_type: 'code',
          scope: 'openid profile email',
        })}`,
        returnUrl: redirectUri,
      });

      if (result.type === 'success' && result.params.code) {
        const tokenResponse = await this.exchangeCodeForToken(result.params.code, redirectUri);
        if (tokenResponse) {
          await this.storeTokens(tokenResponse);
          return true;
        }
      }

      return false;
    } catch (error) {
      console.error('Login failed:', error);
      return false;
    }
  }

  // Exchange authorization code for tokens
  private async exchangeCodeForToken(code: string, redirectUri: string): Promise<TokenResponse | null> {
    try {
      const response = await fetch(this.discovery.tokenEndpoint, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/x-www-form-urlencoded',
        },
        body: new URLSearchParams({
          grant_type: 'authorization_code',
          client_id: ENV.KEYCLOAK_CLIENT_ID,
          code,
          redirect_uri: redirectUri,
        }).toString(),
      });

      if (response.ok) {
        return await response.json();
      }

      return null;
    } catch (error) {
      console.error('Token exchange failed:', error);
      return null;
    }
  }

  // Refresh token
  async refreshToken(): Promise<boolean> {
    try {
      const refreshToken = await SecureStore.getItemAsync(REFRESH_TOKEN_KEY);
      if (!refreshToken) return false;

      const response = await fetch(this.discovery.tokenEndpoint, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/x-www-form-urlencoded',
        },
        body: new URLSearchParams({
          grant_type: 'refresh_token',
          client_id: ENV.KEYCLOAK_CLIENT_ID,
          refresh_token: refreshToken,
        }).toString(),
      });

      if (response.ok) {
        const tokenResponse: TokenResponse = await response.json();
        await this.storeTokens(tokenResponse);
        return true;
      }

      return false;
    } catch (error) {
      console.error('Token refresh failed:', error);
      return false;
    }
  }

  // Store tokens securely
  private async storeTokens(tokenResponse: TokenResponse): Promise<void> {
    await SecureStore.setItemAsync(TOKEN_KEY, tokenResponse.access_token);
    await SecureStore.setItemAsync(REFRESH_TOKEN_KEY, tokenResponse.refresh_token);
  }

  // Get access token
  async getAccessToken(): Promise<string | null> {
    return await SecureStore.getItemAsync(TOKEN_KEY);
  }

  // Logout
  async logout(): Promise<void> {
    await SecureStore.deleteItemAsync(TOKEN_KEY);
    await SecureStore.deleteItemAsync(REFRESH_TOKEN_KEY);
  }

  // Check if authenticated
  async isAuthenticated(): Promise<boolean> {
    const token = await this.getAccessToken();
    if (!token) return false;

    try {
      const decoded = jwtDecode<DecodedToken>(token);
      return decoded.exp * 1000 > Date.now();
    } catch {
      return false;
    }
  }

  // Get current user
  async getCurrentUser(): Promise<DecodedToken | null> {
    const token = await this.getAccessToken();
    if (!token) return null;

    try {
      return jwtDecode<DecodedToken>(token);
    } catch {
      return null;
    }
  }

  // Get user roles
  async getUserRoles(): Promise<string[]> {
    const user = await this.getCurrentUser();
    return user?.roles || [];
  }

  // Check if user has role
  async hasRole(role: string): Promise<boolean> {
    const roles = await this.getUserRoles();
    return roles.includes(role);
  }

  // Biometric authentication
  async isBiometricAvailable(): Promise<boolean> {
    const compatible = await LocalAuthentication.hasHardwareAsync();
    const enrolled = await LocalAuthentication.isEnrolledAsync();
    return compatible && enrolled;
  }

  async isBiometricEnabled(): Promise<boolean> {
    const enabled = await SecureStore.getItemAsync(BIOMETRIC_ENABLED_KEY);
    return enabled === 'true';
  }

  async enableBiometric(): Promise<void> {
    await SecureStore.setItemAsync(BIOMETRIC_ENABLED_KEY, 'true');
  }

  async disableBiometric(): Promise<void> {
    await SecureStore.deleteItemAsync(BIOMETRIC_ENABLED_KEY);
  }

  async authenticateWithBiometric(): Promise<boolean> {
    try {
      const result = await LocalAuthentication.authenticateAsync({
        promptMessage: 'Authenticate to access WorkSpace Manager',
        fallbackLabel: 'Use passcode',
      });

      return result.success;
    } catch (error) {
      console.error('Biometric authentication failed:', error);
      return false;
    }
  }
}

export default new AuthService();
