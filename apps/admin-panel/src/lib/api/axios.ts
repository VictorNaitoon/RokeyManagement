import axios from 'axios';
import { authStore } from '@/stores/authStore';

export const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL || 'https://localhost:7096',
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json',
  },
  withCredentials: true,
});

// Request interceptor: attach Bearer token
api.interceptors.request.use(
  (config) => {
    const token = authStore.getState().token;
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Response interceptor: handle 401 → refresh → retry
let isRefreshing = false;
let failedQueue: Array<{
  resolve: (value: unknown) => void;
  reject: (reason?: unknown) => void;
}> = [];

const processQueue = (error: unknown) => {
  failedQueue.forEach((prom) => {
    if (error) {
      prom.reject(error);
    } else {
      prom.resolve(undefined);
    }
  });
  failedQueue = [];
};

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    // Skip refresh logic for auth endpoints — a 401 on login means wrong credentials,
    // not an expired token. Let the caller handle the error.
    const isAuthEndpoint =
      originalRequest.url?.includes('/auth/login') ||
      originalRequest.url?.includes('/auth/refresh') ||
      originalRequest.url?.includes('/auth/revoke') ||
      originalRequest.url?.includes('/auth/super-admin/login') ||
      originalRequest.url?.includes('/auth/register');

    if (error.response?.status === 401 && !originalRequest._retry && !isAuthEndpoint) {
      if (isRefreshing) {
        return new Promise((resolve, reject) => {
          failedQueue.push({ resolve, reject });
        })
          .then(() => {
            originalRequest.headers.Authorization = `Bearer ${authStore.getState().token}`;
            return api(originalRequest);
          })
          .catch((err) => Promise.reject(err));
      }

      originalRequest._retry = true;
      isRefreshing = true;

      try {
        // Call refresh endpoint - cookies are sent automatically
        const response = await api.post<{ accessToken: string }>('/api/v1/auth/refresh');
        
        // Extract new token from response and update authStore
        const newToken = response.data.accessToken;
        const currentUser = authStore.getState().user;
        
        if (newToken && currentUser) {
          authStore.getState().setAuth(newToken, currentUser);
        } else if (newToken) {
          // If no user in store, at least update the token
          authStore.getState().setAuth(newToken, authStore.getState().user!);
        }
        
        // Update Authorization header with new token
        originalRequest.headers.Authorization = `Bearer ${newToken}`;
        
        // Process any queued requests that were waiting for refresh
        processQueue(null);
        
        // Retry original request with new token
        return api(originalRequest);
      } catch (refreshError) {
        // Refresh failed - process queue with error and logout
        processQueue(refreshError);
        authStore.getState().logout();
        return Promise.reject(refreshError);
      } finally {
        isRefreshing = false;
      }
    }

    return Promise.reject(error);
  }
);