import { create } from 'zustand';
import { api } from '@/lib/api/axios';

export type UserRole = 'SuperAdmin' | 'Admin' | 'Gerente' | 'Vendedor';

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
  isLoggingOut: boolean;
  setAuth: (token: string, user: User) => void;
  setUser: (user: User) => void;
  clearState: () => void;
  logout: () => Promise<void>;
}

export const authStore = create<AuthState>((set, get) => ({
  token: null,
  user: null,
  isAuthenticated: false,
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

  clearState: () =>
    set({
      token: null,
      user: null,
      isAuthenticated: false,
    }),

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