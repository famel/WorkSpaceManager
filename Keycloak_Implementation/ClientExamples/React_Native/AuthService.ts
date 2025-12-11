/**
 * Authentication service for React Native mobile app
 * Handles Keycloak authentication with in-app browser and biometric support
 */

import * as AuthSession from 'expo-auth-session';
import * as SecureStore from 'expo-secure-store';
import * as LocalAuthentication from 'expo-local-authentication';

export interface KeycloakConfig {
  authority: string;
  realm: string;
  clientId: string;
  redirectUri: string;
}

export interface TokenResponse {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
  tokenType: string;
}

export interface UserInfo {
  sub: string;
  email?: string;
  name?: string;
  preferredUsername?: string;
  givenName?: string;
  familyName?: string;
  emailVerified?: boolean;
}

class MobileAuthService {
  private config: KeycloakConfig;
  private discovery: AuthSession.DiscoveryDocument | null = null;

  constructor(config: KeycloakConfig) {
    this.config = config;
  }

  /**
   * Initialize the service and load discovery document
   */
  async initialize(): Promise<void> {
    const realmUrl = `${this.config.authority}/realms/${this.config.realm}`;
    this.discovery = await AuthSession.fetchDiscoveryAsync(realmUrl);
  }

  /**
   * Login with in-app browser
   */
  async login(): Promise<TokenResponse> {
    if (!this.discovery) {
      await this.initialize();
    }

    if (!this.discovery) {
      throw new Error('Failed to load discovery document');
    }

    const redirectUri = AuthSession.makeRedirectUri({
      scheme: 'workspacemanager',
    });

    const request = new AuthSession.AuthRequest({
      clientId: this.config.clientId,
      redirectUri: redirectUri,
      scopes: ['openid', 'profile', 'email'],
      responseType: AuthSession.ResponseType.Code,
    });

    const result = await request.promptAsync(this.discovery);

    if (result.type !== 'success') {
      throw new Error('Authentication cancelled or failed');
    }

    // Exchange code for tokens
    const tokenResponse = await this.exchangeCodeForToken(
      result.params.code,
      redirectUri
    );

    // Save tokens securely
    await this.saveTokens(tokenResponse);

    return tokenResponse;
  }

  /**
   * Login with biometric authentication
   */
  async loginWithBiometric(): Promise<boolean> {
    // Check if biometric is available
    const hasHardware = await LocalAuthentication.hasHardwareAsync();
    if (!hasHardware) {
      throw new Error('Biometric hardware not available');
    }

    const isEnrolled = await LocalAuthentication.isEnrolledAsync();
    if (!isEnrolled) {
      throw new Error('No biometric credentials enrolled');
    }

    // Check if we have saved credentials
    const hasSavedCredentials = await this.hasSavedTokens();
    if (!hasSavedCredentials) {
      throw new Error('No saved credentials for biometric login');
    }

    // Authenticate with biometric
    const result = await LocalAuthentication.authenticateAsync({
      promptMessage: 'Authenticate to access WorkSpaceManager',
      fallbackLabel: 'Use passcode',
    });

    if (!result.success) {
      throw new Error('Biometric authentication failed');
    }

    // Try to refresh token
    try {
      await this.refreshAccessToken();
      return true;
    } catch (error) {
      // Refresh failed, need to login again
      throw new Error('Session expired, please login again');
    }
  }

  /**
   * Exchange authorization code for tokens
   */
  private async exchangeCodeForToken(
    code: string,
    redirectUri: string
  ): Promise<TokenResponse> {
    if (!this.discovery?.tokenEndpoint) {
      throw new Error('Token endpoint not available');
    }

    const body = new URLSearchParams({
      grant_type: 'authorization_code',
      client_id: this.config.clientId,
      redirect_uri: redirectUri,
      code: code,
    });

    const response = await fetch(this.discovery.tokenEndpoint, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/x-www-form-urlencoded',
      },
      body: body.toString(),
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(`Failed to exchange code: ${error.error_description || error.error}`);
    }

    const data = await response.json();

    return {
      accessToken: data.access_token,
      refreshToken: data.refresh_token,
      expiresIn: data.expires_in,
      tokenType: data.token_type,
    };
  }

  /**
   * Refresh access token
   */
  async refreshAccessToken(): Promise<TokenResponse> {
    const refreshToken = await SecureStore.getItemAsync('refresh_token');
    if (!refreshToken) {
      throw new Error('No refresh token available');
    }

    if (!this.discovery?.tokenEndpoint) {
      await this.initialize();
    }

    if (!this.discovery?.tokenEndpoint) {
      throw new Error('Token endpoint not available');
    }

    const body = new URLSearchParams({
      grant_type: 'refresh_token',
      client_id: this.config.clientId,
      refresh_token: refreshToken,
    });

    const response = await fetch(this.discovery.tokenEndpoint, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/x-www-form-urlencoded',
      },
      body: body.toString(),
    });

    if (!response.ok) {
      // Refresh failed, clear tokens
      await this.clearTokens();
      throw new Error('Failed to refresh token');
    }

    const data = await response.json();

    const tokenResponse: TokenResponse = {
      accessToken: data.access_token,
      refreshToken: data.refresh_token || refreshToken,
      expiresIn: data.expires_in,
      tokenType: data.token_type,
    };

    await this.saveTokens(tokenResponse);
    return tokenResponse;
  }

  /**
   * Get user info
   */
  async getUserInfo(): Promise<UserInfo> {
    const accessToken = await this.getAccessToken();
    if (!accessToken) {
      throw new Error('No access token available');
    }

    if (!this.discovery?.userInfoEndpoint) {
      await this.initialize();
    }

    if (!this.discovery?.userInfoEndpoint) {
      throw new Error('UserInfo endpoint not available');
    }

    const response = await fetch(this.discovery.userInfoEndpoint, {
      headers: {
        Authorization: `Bearer ${accessToken}`,
      },
    });

    if (!response.ok) {
      throw new Error('Failed to get user info');
    }

    return await response.json();
  }

  /**
   * Logout
   */
  async logout(): Promise<void> {
    await this.clearTokens();

    // Optional: Call Keycloak logout endpoint
    if (this.discovery?.endSessionEndpoint) {
      const accessToken = await this.getAccessToken();
      if (accessToken) {
        try {
          await fetch(this.discovery.endSessionEndpoint, {
            method: 'POST',
            headers: {
              Authorization: `Bearer ${accessToken}`,
            },
          });
        } catch (error) {
          console.error('Failed to call logout endpoint:', error);
        }
      }
    }
  }

  /**
   * Get current access token
   */
  async getAccessToken(): Promise<string | null> {
    const token = await SecureStore.getItemAsync('access_token');
    if (!token) {
      return null;
    }

    // Check if token is expired
    const expiry = await SecureStore.getItemAsync('token_expiry');
    if (expiry && Date.now() >= parseInt(expiry)) {
      // Try to refresh
      try {
        const tokenResponse = await this.refreshAccessToken();
        return tokenResponse.accessToken;
      } catch (error) {
        return null;
      }
    }

    return token;
  }

  /**
   * Check if user is authenticated
   */
  async isAuthenticated(): Promise<boolean> {
    const token = await this.getAccessToken();
    return token !== null;
  }

  /**
   * Save tokens to secure storage
   */
  private async saveTokens(tokenResponse: TokenResponse): Promise<void> {
    const expiry = Date.now() + tokenResponse.expiresIn * 1000;

    await Promise.all([
      SecureStore.setItemAsync('access_token', tokenResponse.accessToken),
      SecureStore.setItemAsync('refresh_token', tokenResponse.refreshToken),
      SecureStore.setItemAsync('token_expiry', expiry.toString()),
    ]);
  }

  /**
   * Clear all tokens
   */
  private async clearTokens(): Promise<void> {
    await Promise.all([
      SecureStore.deleteItemAsync('access_token'),
      SecureStore.deleteItemAsync('refresh_token'),
      SecureStore.deleteItemAsync('token_expiry'),
    ]);
  }

  /**
   * Check if tokens are saved
   */
  private async hasSavedTokens(): Promise<boolean> {
    const token = await SecureStore.getItemAsync('access_token');
    return token !== null;
  }

  /**
   * Check if biometric authentication is available
   */
  async isBiometricAvailable(): Promise<boolean> {
    const hasHardware = await LocalAuthentication.hasHardwareAsync();
    const isEnrolled = await LocalAuthentication.isEnrolledAsync();
    const hasSavedTokens = await this.hasSavedTokens();
    return hasHardware && isEnrolled && hasSavedTokens;
  }
}

// Create singleton instance
const keycloakConfig: KeycloakConfig = {
  authority: process.env.EXPO_PUBLIC_KEYCLOAK_URL || 'http://localhost:8080',
  realm: process.env.EXPO_PUBLIC_KEYCLOAK_REALM || 'alpha-bank-realm',
  clientId: process.env.EXPO_PUBLIC_KEYCLOAK_CLIENT_ID || 'workspace-manager-mobile',
  redirectUri: AuthSession.makeRedirectUri({ scheme: 'workspacemanager' }),
};

export const mobileAuthService = new MobileAuthService(keycloakConfig);
