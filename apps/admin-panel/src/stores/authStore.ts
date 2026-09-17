import { create } from 'zustand';
import { api } from '@/lib/api/axios';
import { decodeUserFromToken } from '@/lib/auth/decodeJwt';

export type UserRole = 'SuperAdmin' | 'Dueño' | 'Gerente' | 'Empleado';

export interface User {
  id: number;
  email: string;
  nombre: string;
  apellido: string;
  rol: UserRole;
  id_negocio?: number;
  negocio_nombre?: string;
}

interface AuthState {
  token: string | null;
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  isLoggingOut: boolean;
  setAuth: (token: string, user: User) => void;
  setUser: (user: User) => void;
  setLoading: (loading: boolean) => void;
  clearState: () => void;
  logout: () => Promise<void>;
  initializeAuth: () => Promise<void>;
}

export const authStore = create<AuthState>((set, get) => ({
  token: null,
  user: null,
  isAuthenticated: false,
  isLoading: true,
  isLoggingOut: false,

  setAuth: (token, user) =>
    set({
      token,
      user,
      isAuthenticated: true,
    }),

  setUser: (user) =>
    set({
      user,
    }),

  setLoading: (loading) => set({ isLoading: loading }),

  clearState: () =>
    set({
      token: null,
      user: null,
      isAuthenticated: false,
    }),

  initializeAuth: async () => {
    set({ isLoading: true });
    try {
      // withCredentials:true envía cookie HttpOnly refreshToken
      const response = await api.post<{ accessToken: string }>('/api/v1/auth/refresh');
      const newToken = response.data.accessToken;
      if (newToken) {
        const user = decodeUserFromToken(newToken);
        if (user) {
          set({ token: newToken, user, isAuthenticated: true });
        } else {
          // Token válido pero sin claims esperados -> limpia
          get().clearState();
        }
      } else {
        get().clearState();
      }
    } catch {
      // 401 o sin cookie -> sesión no restaurable
      // Nota: SuperAdmin/login no setea cookie (AuthController.cs:183), F5 lo desloguea esperado.
      get().clearState();
    } finally {
      set({ isLoading: false });
    }
  },

  logout: async () => {
    set({ isLoggingOut: true });
    
    try {
      // Call logout API endpoint
      await api.post('/api/v1/auth/logout');
    } catch (error) {
      // Log error but continue with local logout (graceful degradation)
      console.warn('Logout API call failed, proceeding with local logout:', error);
    } finally {
      // Always clear state regardless of API result
      get().clearState();
      set({ isLoggingOut: false });
      
      // Navigate to login page
      window.location.href = '/login';
    }
  },
}));