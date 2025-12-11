/**
 * Authentication service for React web client
 * Handles Keycloak authentication flow and token management
 */

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

class AuthService {
  private config: KeycloakConfig;
  private accessToken: string | null = null;
  private refreshToken: string | null = null;
  private tokenExpiry: number | null = null;
  private refreshTimer: NodeJS.Timeout | null = null;

  constructor(config: KeycloakConfig) {
    this.config = config;
    this.loadTokensFromStorage();
  }

  /**
   * Get the login URL for redirecting to Keycloak
   */
  getLoginUrl(state?: string): string {
    const params = new URLSearchParams({
      client_id: this.config.clientId,
      redirect_uri: this.config.redirectUri,
      response_type: 'code',
      scope: 'openid profile email',
      state: state || this.generateState(),
    });

    return `${this.config.authority}/realms/${this.config.realm}/protocol/openid-connect/auth?${params}`;
  }

  /**
   * Get the logout URL for redirecting to Keycloak
   */
  getLogoutUrl(postLogoutRedirectUri?: string): string {
    const params = new URLSearchParams({
      client_id: this.config.clientId,
      post_logout_redirect_uri: postLogoutRedirectUri || this.config.redirectUri,
    });

    if (this.accessToken) {
      params.append('id_token_hint', this.accessToken);
    }

    return `${this.config.authority}/realms/${this.config.realm}/protocol/openid-connect/logout?${params}`;
  }

  /**
   * Handle OAuth callback and exchange code for tokens
   */
  async handleCallback(code: string): Promise<TokenResponse> {
    const tokenEndpoint = `${this.config.authority}/realms/${this.config.realm}/protocol/openid-connect/token`;

    const body = new URLSearchParams({
      grant_type: 'authorization_code',
      client_id: this.config.clientId,
      redirect_uri: this.config.redirectUri,
      code: code,
    });

    const response = await fetch(tokenEndpoint, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/x-www-form-urlencoded',
      },
      body: body.toString(),
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(`Failed to exchange code for token: ${error.error_description || error.error}`);
    }

    const data = await response.json();
    
    const tokenResponse: TokenResponse = {
      accessToken: data.access_token,
      refreshToken: data.refresh_token,
      expiresIn: data.expires_in,
      tokenType: data.token_type,
    };

    this.setTokens(tokenResponse);
    return tokenResponse;
  }

  /**
   * Login with username and password (for development/testing)
   */
  async loginWithPassword(username: string, password: string): Promise<TokenResponse> {
    const tokenEndpoint = `${this.config.authority}/realms/${this.config.realm}/protocol/openid-connect/token`;

    const body = new URLSearchParams({
      grant_type: 'password',
      client_id: this.config.clientId,
      username: username,
      password: password,
    });

    const response = await fetch(tokenEndpoint, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/x-www-form-urlencoded',
      },
      body: body.toString(),
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(`Login failed: ${error.error_description || error.error}`);
    }

    const data = await response.json();
    
    const tokenResponse: TokenResponse = {
      accessToken: data.access_token,
      refreshToken: data.refresh_token,
      expiresIn: data.expires_in,
      tokenType: data.token_type,
    };

    this.setTokens(tokenResponse);
    return tokenResponse;
  }

  /**
   * Refresh the access token using refresh token
   */
  async refreshAccessToken(): Promise<TokenResponse> {
    if (!this.refreshToken) {
      throw new Error('No refresh token available');
    }

    const tokenEndpoint = `${this.config.authority}/realms/${this.config.realm}/protocol/openid-connect/token`;

    const body = new URLSearchParams({
      grant_type: 'refresh_token',
      client_id: this.config.clientId,
      refresh_token: this.refreshToken,
    });

    const response = await fetch(tokenEndpoint, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/x-www-form-urlencoded',
      },
      body: body.toString(),
    });

    if (!response.ok) {
      // Refresh token expired or invalid, clear tokens
      this.clearTokens();
      throw new Error('Failed to refresh token');
    }

    const data = await response.json();
    
    const tokenResponse: TokenResponse = {
      accessToken: data.access_token,
      refreshToken: data.refresh_token || this.refreshToken,
      expiresIn: data.expires_in,
      tokenType: data.token_type,
    };

    this.setTokens(tokenResponse);
    return tokenResponse;
  }

  /**
   * Get user info from access token
   */
  async getUserInfo(): Promise<UserInfo> {
    if (!this.accessToken) {
      throw new Error('No access token available');
    }

    const userInfoEndpoint = `${this.config.authority}/realms/${this.config.realm}/protocol/openid-connect/userinfo`;

    const response = await fetch(userInfoEndpoint, {
      headers: {
        Authorization: `Bearer ${this.accessToken}`,
      },
    });

    if (!response.ok) {
      throw new Error('Failed to get user info');
    }

    return await response.json();
  }

  /**
   * Logout and clear tokens
   */
  logout(): void {
    this.clearTokens();
  }

  /**
   * Get current access token
   */
  getAccessToken(): string | null {
    return this.accessToken;
  }

  /**
   * Check if user is authenticated
   */
  isAuthenticated(): boolean {
    return this.accessToken !== null && !this.isTokenExpired();
  }

  /**
   * Check if token is expired
   */
  private isTokenExpired(): boolean {
    if (!this.tokenExpiry) {
      return true;
    }
    return Date.now() >= this.tokenExpiry;
  }

  /**
   * Set tokens and schedule refresh
   */
  private setTokens(tokenResponse: TokenResponse): void {
    this.accessToken = tokenResponse.accessToken;
    this.refreshToken = tokenResponse.refreshToken;
    this.tokenExpiry = Date.now() + tokenResponse.expiresIn * 1000;

    // Save to localStorage
    localStorage.setItem('access_token', this.accessToken);
    localStorage.setItem('refresh_token', this.refreshToken);
    localStorage.setItem('token_expiry', this.tokenExpiry.toString());

    // Schedule token refresh (refresh 1 minute before expiry)
    this.scheduleTokenRefresh(tokenResponse.expiresIn - 60);
  }

  /**
   * Clear tokens and cancel refresh timer
   */
  private clearTokens(): void {
    this.accessToken = null;
    this.refreshToken = null;
    this.tokenExpiry = null;

    localStorage.removeItem('access_token');
    localStorage.removeItem('refresh_token');
    localStorage.removeItem('token_expiry');

    if (this.refreshTimer) {
      clearTimeout(this.refreshTimer);
      this.refreshTimer = null;
    }
  }

  /**
   * Load tokens from localStorage
   */
  private loadTokensFromStorage(): void {
    this.accessToken = localStorage.getItem('access_token');
    this.refreshToken = localStorage.getItem('refresh_token');
    const expiryStr = localStorage.getItem('token_expiry');
    this.tokenExpiry = expiryStr ? parseInt(expiryStr) : null;

    // Check if token is expired
    if (this.isTokenExpired()) {
      // Try to refresh
      if (this.refreshToken) {
        this.refreshAccessToken().catch(() => {
          this.clearTokens();
        });
      } else {
        this.clearTokens();
      }
    } else if (this.tokenExpiry) {
      // Schedule refresh
      const expiresIn = Math.floor((this.tokenExpiry - Date.now()) / 1000);
      this.scheduleTokenRefresh(expiresIn - 60);
    }
  }

  /**
   * Schedule automatic token refresh
   */
  private scheduleTokenRefresh(delaySeconds: number): void {
    if (this.refreshTimer) {
      clearTimeout(this.refreshTimer);
    }

    if (delaySeconds > 0) {
      this.refreshTimer = setTimeout(() => {
        this.refreshAccessToken().catch(() => {
          console.error('Failed to refresh token automatically');
          this.clearTokens();
        });
      }, delaySeconds * 1000);
    }
  }

  /**
   * Generate random state for OAuth flow
   */
  private generateState(): string {
    const array = new Uint8Array(32);
    crypto.getRandomValues(array);
    return Array.from(array, byte => byte.toString(16).padStart(2, '0')).join('');
  }
}

// Create singleton instance
const keycloakConfig: KeycloakConfig = {
  authority: process.env.REACT_APP_KEYCLOAK_URL || 'http://localhost:8080',
  realm: process.env.REACT_APP_KEYCLOAK_REALM || 'alpha-bank-realm',
  clientId: process.env.REACT_APP_KEYCLOAK_CLIENT_ID || 'workspace-manager-web',
  redirectUri: process.env.REACT_APP_REDIRECT_URI || `${window.location.origin}/callback`,
};

export const authService = new AuthService(keycloakConfig);
