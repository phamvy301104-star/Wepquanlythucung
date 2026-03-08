import { createContext, useContext, useState, useCallback, useEffect, type ReactNode } from 'react';
import { authApi } from '../services/api';
import type { User } from '../models/models';

interface AuthContextType {
  user: User | null;
  isLoggedIn: boolean;
  isAdmin: boolean;
  isStaff: boolean;
  isAdminOrStaff: boolean;
  login: (credentials: { email: string; password: string }) => Promise<void>;
  register: (data: { email: string; password: string; fullName: string; phoneNumber?: string }) => Promise<void>;
  googleLogin: (token: string) => Promise<void>;
  updateUser: (user: User) => void;
  logout: () => void;
  getToken: () => string | null;
}

const AuthContext = createContext<AuthContextType | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(() => {
    const saved = localStorage.getItem('user');
    return saved ? JSON.parse(saved) : null;
  });

  const isLoggedIn = !!localStorage.getItem('accessToken');
  const isAdmin = user?.role === 'Admin';
  const isStaff = user?.role === 'Staff';
  const isAdminOrStaff = isAdmin || isStaff;

  const handleAuth = useCallback((res: any) => {
    if (res.data?.success && res.data?.data) {
      const { user: u, accessToken, refreshToken } = res.data.data;
      localStorage.setItem('accessToken', accessToken);
      localStorage.setItem('refreshToken', refreshToken);
      localStorage.setItem('user', JSON.stringify(u));
      setUser(u);
    }
  }, []);

  const login = useCallback(async (credentials: { email: string; password: string }) => {
    const res = await authApi.login(credentials);
    handleAuth(res);
  }, [handleAuth]);

  const register = useCallback(async (data: { email: string; password: string; fullName: string; phoneNumber?: string }) => {
    const res = await authApi.register(data);
    handleAuth(res);
  }, [handleAuth]);

  const googleLogin = useCallback(async (token: string) => {
    const res = await authApi.googleLogin({ idToken: token });
    handleAuth(res);
  }, [handleAuth]);

  const updateUser = useCallback((u: User) => {
    setUser(u);
    localStorage.setItem('user', JSON.stringify(u));
  }, []);

  const logout = useCallback(() => {
    const refreshToken = localStorage.getItem('refreshToken');
    if (refreshToken) authApi.logout(refreshToken).catch(() => {});
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
    setUser(null);
  }, []);

  const getToken = useCallback(() => localStorage.getItem('accessToken'), []);

  // Refresh user data on mount if token exists
  useEffect(() => {
    if (isLoggedIn && user) {
      authApi.getProfile().then((res) => {
        if (res.data?.success) {
          updateUser(res.data.data);
        }
      }).catch(() => {});
    }
  }, []); // eslint-disable-line react-hooks/exhaustive-deps

  return (
    <AuthContext.Provider value={{ user, isLoggedIn, isAdmin, isStaff, isAdminOrStaff, login, register, googleLogin, updateUser, logout, getToken }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be inside AuthProvider');
  return ctx;
}
