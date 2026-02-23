import React, { createContext, useCallback, useContext, useEffect, useState } from 'react';
import { api, setAccessToken, clearAccessToken, getAccessToken, type UserInfo } from '../api/client';

type AuthState = { user: UserInfo | null; loading: boolean };

const AuthContext = createContext<{
  user: UserInfo | null;
  loading: boolean;
  login: (emailOrPhone: string, password: string) => Promise<void>;
  register: (data: { email: string; phone: string; password: string; fullName: string; aadhaarNumber?: string }) => Promise<void>;
  logout: () => void;
  refreshUser: () => Promise<void>;
} | null>(null);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [state, setState] = useState<AuthState>({ user: null, loading: true });

  const refreshUser = useCallback(async () => {
    if (!getAccessToken()) {
      setState((s) => ({ ...s, loading: false }));
      return;
    }
    try {
      const user = await api.auth.me();
      setState({ user, loading: false });
    } catch {
      clearAccessToken();
      setState({ user: null, loading: false });
    }
  }, []);

  useEffect(() => {
    const token = localStorage.getItem('accessToken');
    if (token) {
      setAccessToken(token);
      api.auth.me()
        .then((user) => setState({ user, loading: false }))
        .catch(() => {
          clearAccessToken();
          localStorage.removeItem('accessToken');
          localStorage.removeItem('refreshToken');
          setState({ user: null, loading: false });
        });
    } else {
      setState((s) => ({ ...s, loading: false }));
    }
  }, []);

  const login = useCallback(async (emailOrPhone: string, password: string) => {
    const res = await api.auth.login({ emailOrPhone, password });
    setAccessToken(res.accessToken);
    localStorage.setItem('accessToken', res.accessToken);
    localStorage.setItem('refreshToken', res.refreshToken);
    setState({ user: res.user, loading: false });
  }, []);

  const register = useCallback(async (data: { email: string; phone: string; password: string; fullName: string; aadhaarNumber?: string }) => {
    const res = await api.auth.register({
      ...data,
      preferredLanguage: localStorage.getItem('lang') || 'en',
    });
    setAccessToken(res.accessToken);
    localStorage.setItem('accessToken', res.accessToken);
    localStorage.setItem('refreshToken', res.refreshToken);
    setState({ user: res.user, loading: false });
  }, []);

  const logout = useCallback(() => {
    clearAccessToken();
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    setState({ user: null, loading: false });
  }, []);

  return (
    <AuthContext.Provider value={{ user: state.user, loading: state.loading, login, register, logout, refreshUser }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used within AuthProvider');
  return ctx;
}
