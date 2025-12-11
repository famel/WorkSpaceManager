import axios from 'axios';
import { jwtDecode } from 'jwt-decode';

const KEYCLOAK_URL = process.env.REACT_APP_KEYCLOAK_URL || 'http://localhost:8080';
const REALM = process.env.REACT_APP_KEYCLOAK_REALM || 'alpha-bank-realm';
const CLIENT_ID = process.env.REACT_APP_KEYCLOAK_CLIENT_ID || 'workspace-manager-web';

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
  private tokenRefreshTimer: NodeJS.Timeout | null = null;

  // Login with username and password
  async login(username: string, password: string): Promise<boolean> {
    try {
      const tokenUrl = `${KEYCLOAK_URL}/realms/${REALM}/protocol/openid-connect/token`;
      
      const params = new URLSearchParams();
      params.append('grant_type', 'password');
      params.append('client_id', CLIENT_ID);
      params.append('username', username);
      params.append('password', password);

      const response = await axios.post<TokenResponse>(tokenUrl, params, {
        headers: {
          'Content-Type': 'application/x-www-form-urlencoded',
        },
      });

      this.storeTokens(response.data);
      this.scheduleTokenRefresh(response.data.expires_in);
      
      return true;
    } catch (error) {
      console.error('Login failed:', error);
      return false;
    }
  }

  // Logout
  logout(): void {
    localStorage.removeItem('access_token');
    localStorage.removeItem('refresh_token');
    
    if (this.tokenRefreshTimer) {
      clearTimeout(this.tokenRefreshTimer);
      this.tokenRefreshTimer = null;
    }
  }

  // Check if user is authenticated
  isAuthenticated(): boolean {
    const token = localStorage.getItem('access_token');
    if (!token) return false;

    try {
      const decoded = jwtDecode<DecodedToken>(token);
      return decoded.exp * 1000 > Date.now();
    } catch {
      return false;
    }
  }

  // Get current user info
  getCurrentUser(): DecodedToken | null {
    const token = localStorage.getItem('access_token');
    if (!token) return null;

    try {
      return jwtDecode<DecodedToken>(token);
    } catch {
      return null;
    }
  }

  // Get user roles
  getUserRoles(): string[] {
    const user = this.getCurrentUser();
    return user?.roles || [];
  }

  // Check if user has role
  hasRole(role: string): boolean {
    return this.getUserRoles().includes(role);
  }

  // Refresh token
  async refreshToken(): Promise<boolean> {
    try {
      const refreshToken = localStorage.getItem('refresh_token');
      if (!refreshToken) return false;

      const tokenUrl = `${KEYCLOAK_URL}/realms/${REALM}/protocol/openid-connect/token`;
      
      const params = new URLSearchParams();
      params.append('grant_type', 'refresh_token');
      params.append('client_id', CLIENT_ID);
      params.append('refresh_token', refreshToken);

      const response = await axios.post<TokenResponse>(tokenUrl, params, {
        headers: {
          'Content-Type': 'application/x-www-form-urlencoded',
        },
      });

      this.storeTokens(response.data);
      this.scheduleTokenRefresh(response.data.expires_in);
      
      return true;
    } catch (error) {
      console.error('Token refresh failed:', error);
      this.logout();
      return false;
    }
  }

  // Store tokens
  private storeTokens(tokenResponse: TokenResponse): void {
    localStorage.setItem('access_token', tokenResponse.access_token);
    localStorage.setItem('refresh_token', tokenResponse.refresh_token);
  }

  // Schedule automatic token refresh
  private scheduleTokenRefresh(expiresIn: number): void {
    if (this.tokenRefreshTimer) {
      clearTimeout(this.tokenRefreshTimer);
    }

    // Refresh 1 minute before expiration
    const refreshTime = (expiresIn - 60) * 1000;
    
    this.tokenRefreshTimer = setTimeout(() => {
      this.refreshToken();
    }, refreshTime);
  }

  // Initialize auth (check and refresh token if needed)
  async initialize(): Promise<boolean> {
    if (this.isAuthenticated()) {
      const user = this.getCurrentUser();
      if (user) {
        const timeUntilExpiry = user.exp * 1000 - Date.now();
        if (timeUntilExpiry < 5 * 60 * 1000) {
          // Less than 5 minutes until expiry, refresh now
          return await this.refreshToken();
        }
        this.scheduleTokenRefresh(timeUntilExpiry / 1000);
        return true;
      }
    }
    return false;
  }
}

export default new AuthService();
