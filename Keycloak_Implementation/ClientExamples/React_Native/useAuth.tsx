/**
 * React Native authentication context and hooks
 */

import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import { mobileAuthService, UserInfo } from './AuthService';

interface AuthContextType {
  user: UserInfo | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  isBiometricAvailable: boolean;
  login: () => Promise<void>;
  loginWithBiometric: () => Promise<void>;
  logout: () => Promise<void>;
  getAccessToken: () => Promise<string | null>;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

interface AuthProviderProps {
  children: ReactNode;
}

/**
 * Authentication provider for React Native
 */
export function AuthProvider({ children }: AuthProviderProps) {
  const [user, setUser] = useState<UserInfo | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isBiometricAvailable, setIsBiometricAvailable] = useState(false);

  useEffect(() => {
    initialize();
  }, []);

  const initialize = async () => {
    try {
      // Initialize auth service
      await mobileAuthService.initialize();

      // Check if authenticated
      const isAuth = await mobileAuthService.isAuthenticated();
      if (isAuth) {
        const userInfo = await mobileAuthService.getUserInfo();
        setUser(userInfo);
      }

      // Check biometric availability
      const biometricAvailable = await mobileAuthService.isBiometricAvailable();
      setIsBiometricAvailable(biometricAvailable);
    } catch (error) {
      console.error('Failed to initialize auth:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const login = async () => {
    try {
      setIsLoading(true);
      await mobileAuthService.login();
      const userInfo = await mobileAuthService.getUserInfo();
      setUser(userInfo);

      // Check biometric availability after login
      const biometricAvailable = await mobileAuthService.isBiometricAvailable();
      setIsBiometricAvailable(biometricAvailable);
    } catch (error) {
      console.error('Login failed:', error);
      throw error;
    } finally {
      setIsLoading(false);
    }
  };

  const loginWithBiometric = async () => {
    try {
      setIsLoading(true);
      await mobileAuthService.loginWithBiometric();
      const userInfo = await mobileAuthService.getUserInfo();
      setUser(userInfo);
    } catch (error) {
      console.error('Biometric login failed:', error);
      throw error;
    } finally {
      setIsLoading(false);
    }
  };

  const logout = async () => {
    try {
      await mobileAuthService.logout();
      setUser(null);
      setIsBiometricAvailable(false);
    } catch (error) {
      console.error('Logout failed:', error);
      throw error;
    }
  };

  const getAccessToken = async () => {
    return await mobileAuthService.getAccessToken();
  };

  const value: AuthContextType = {
    user,
    isAuthenticated: user !== null,
    isLoading,
    isBiometricAvailable,
    login,
    loginWithBiometric,
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
 * Example Login Screen Component
 */
import { View, Text, TouchableOpacity, StyleSheet, Alert } from 'react-native';
import { MaterialIcons } from '@expo/vector-icons';

export function LoginScreen() {
  const { login, loginWithBiometric, isBiometricAvailable, isLoading } = useAuth();

  const handleLogin = async () => {
    try {
      await login();
    } catch (error: any) {
      Alert.alert('Login Failed', error.message);
    }
  };

  const handleBiometricLogin = async () => {
    try {
      await loginWithBiometric();
    } catch (error: any) {
      Alert.alert('Biometric Login Failed', error.message);
    }
  };

  return (
    <View style={styles.container}>
      <View style={styles.logoContainer}>
        <MaterialIcons name="business" size={80} color="#3B82F6" />
        <Text style={styles.title}>WorkSpace Manager</Text>
        <Text style={styles.subtitle}>Enterprise Workspace Booking</Text>
      </View>

      <View style={styles.buttonContainer}>
        <TouchableOpacity
          style={styles.loginButton}
          onPress={handleLogin}
          disabled={isLoading}
        >
          <MaterialIcons name="login" size={24} color="white" />
          <Text style={styles.loginButtonText}>
            {isLoading ? 'Signing in...' : 'Sign In with SSO'}
          </Text>
        </TouchableOpacity>

        {isBiometricAvailable && (
          <TouchableOpacity
            style={styles.biometricButton}
            onPress={handleBiometricLogin}
            disabled={isLoading}
          >
            <MaterialIcons name="fingerprint" size={24} color="#3B82F6" />
            <Text style={styles.biometricButtonText}>
              Use Biometric Authentication
            </Text>
          </TouchableOpacity>
        )}
      </View>

      <Text style={styles.footer}>
        Secure authentication powered by Keycloak
      </Text>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#F9FAFB',
    justifyContent: 'center',
    padding: 24,
  },
  logoContainer: {
    alignItems: 'center',
    marginBottom: 48,
  },
  title: {
    fontSize: 28,
    fontWeight: 'bold',
    color: '#1F2937',
    marginTop: 16,
  },
  subtitle: {
    fontSize: 16,
    color: '#6B7280',
    marginTop: 8,
  },
  buttonContainer: {
    gap: 16,
  },
  loginButton: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    backgroundColor: '#3B82F6',
    paddingVertical: 16,
    paddingHorizontal: 24,
    borderRadius: 12,
    gap: 8,
  },
  loginButtonText: {
    color: 'white',
    fontSize: 16,
    fontWeight: '600',
  },
  biometricButton: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    backgroundColor: 'white',
    paddingVertical: 16,
    paddingHorizontal: 24,
    borderRadius: 12,
    borderWidth: 2,
    borderColor: '#3B82F6',
    gap: 8,
  },
  biometricButtonText: {
    color: '#3B82F6',
    fontSize: 16,
    fontWeight: '600',
  },
  footer: {
    textAlign: 'center',
    color: '#9CA3AF',
    fontSize: 14,
    marginTop: 32,
  },
});
