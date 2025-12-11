/**
 * React hooks for authentication
 */

import { useState, useEffect, useContext, createContext, ReactNode } from 'react';
import { authService, UserInfo } from './authService';

interface AuthContextType {
  user: UserInfo | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: () => void;
  loginWithPassword: (username: string, password: string) => Promise<void>;
  logout: () => void;
  getAccessToken: () => string | null;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

interface AuthProviderProps {
  children: ReactNode;
}

/**
 * Authentication provider component
 */
export function AuthProvider({ children }: AuthProviderProps) {
  const [user, setUser] = useState<UserInfo | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    // Load user on mount
    loadUser();
  }, []);

  const loadUser = async () => {
    try {
      if (authService.isAuthenticated()) {
        const userInfo = await authService.getUserInfo();
        setUser(userInfo);
      }
    } catch (error) {
      console.error('Failed to load user:', error);
      setUser(null);
    } finally {
      setIsLoading(false);
    }
  };

  const login = () => {
    const loginUrl = authService.getLoginUrl();
    window.location.href = loginUrl;
  };

  const loginWithPassword = async (username: string, password: string) => {
    try {
      await authService.loginWithPassword(username, password);
      await loadUser();
    } catch (error) {
      console.error('Login failed:', error);
      throw error;
    }
  };

  const logout = () => {
    authService.logout();
    setUser(null);
    const logoutUrl = authService.getLogoutUrl();
    window.location.href = logoutUrl;
  };

  const getAccessToken = () => {
    return authService.getAccessToken();
  };

  const value: AuthContextType = {
    user,
    isAuthenticated: authService.isAuthenticated(),
    isLoading,
    login,
    loginWithPassword,
    logout,
    getAccessToken,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

/**
 * Hook to access authentication context
 */
export function useAuth(): AuthContextType {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}

/**
 * Component for handling OAuth callback
 */
export function AuthCallback() {
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const handleCallback = async () => {
      const params = new URLSearchParams(window.location.search);
      const code = params.get('code');
      const error = params.get('error');

      if (error) {
        setError(error);
        return;
      }

      if (code) {
        try {
          await authService.handleCallback(code);
          // Redirect to home or intended page
          const returnUrl = sessionStorage.getItem('returnUrl') || '/';
          sessionStorage.removeItem('returnUrl');
          window.location.href = returnUrl;
        } catch (err) {
          console.error('Failed to handle callback:', err);
          setError('Authentication failed');
        }
      }
    };

    handleCallback();
  }, []);

  if (error) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-center">
          <h1 className="text-2xl font-bold text-red-600 mb-4">Authentication Error</h1>
          <p className="text-gray-600">{error}</p>
          <button
            onClick={() => window.location.href = '/'}
            className="mt-4 px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
          >
            Return to Home
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="flex items-center justify-center min-h-screen">
      <div className="text-center">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto mb-4"></div>
        <p className="text-gray-600">Completing authentication...</p>
      </div>
    </div>
  );
}

/**
 * Higher-order component for protecting routes
 */
export function withAuth<P extends object>(
  Component: React.ComponentType<P>
): React.ComponentType<P> {
  return function AuthenticatedComponent(props: P) {
    const { isAuthenticated, isLoading, login } = useAuth();

    useEffect(() => {
      if (!isLoading && !isAuthenticated) {
        // Save current URL for redirect after login
        sessionStorage.setItem('returnUrl', window.location.pathname);
        login();
      }
    }, [isAuthenticated, isLoading, login]);

    if (isLoading) {
      return (
        <div className="flex items-center justify-center min-h-screen">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
        </div>
      );
    }

    if (!isAuthenticated) {
      return null; // Will redirect to login
    }

    return <Component {...props} />;
  };
}
