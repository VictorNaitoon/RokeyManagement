/**
 * Ventas Hooks - React Query hooks for sales management
 * RoKey MANAGEMENT - Multi-tenant SaaS ERP/POS for locksmiths
 * 
 * Phase 2: Data Layer - CRUD operations for ventas
 */

import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { api } from '@/lib/api/axios';
import { authStore } from '@/stores/authStore';
import type {
  Venta,
  VentaListResponse,
  DetalleVenta,
  Pago,
  CrearVentaRequest,
  AnularVentaRequest,
  VentaFilters,
} from '@/types';
import { toast } from 'sonner';

// ============================================
// Configuration Constants
// ============================================

const VENTAS_QUERY_CONFIG = {
  staleTime: 30000,    // 30 seconds
  retry: 2,
  refetchOnWindowFocus: false,
};

// ============================================
// Helper Functions
// ============================================

/**
 * Get current user role from auth store
 */
function getUserRole() {
  const user = authStore.getState().user;
  if (!user) return null;
  return user.rol;
}

/**
 * Check if user can cancel (anular) ventas
 * Only Dueño (Admin) can cancel sales
 */
export function canAnularVenta(): boolean {
  const role = getUserRole();
  return role === 'Dueño';
}

// ============================================
// Query Hooks
// ============================================

/**
 * Hook for fetching list of ventas
 * GET /api/v1/ventas with optional date filters
 */
export interface UseVentasOptions {
  filters?: VentaFilters;
}

export function useVentas(options: UseVentasOptions = {}) {
  const { filters = {} } = options;
  
  return useQuery({
    queryKey: ['ventas', filters],
    queryFn: async () => {
      const response = await api.get<VentaListResponse>('/api/v1/ventas', {
        params: {
          fechaDesde: filters.fechaDesde || undefined,
          fechaHasta: filters.fechaHasta || undefined,
        },
      });
      return response.data;
    },
    ...VENTAS_QUERY_CONFIG,
  });
}

/**
 * Hook for fetching a single venta by ID
 * GET /api/v1/ventas/{id}
 */
export function useVenta(id: number | null) {
  return useQuery({
    queryKey: ['venta', id],
    queryFn: async () => {
      if (!id) return null;
      const response = await api.get<Venta>(`/api/v1/ventas/${id}`);
      return response.data;
    },
    enabled: !!id,
    ...VENTAS_QUERY_CONFIG,
  });
}

/**
 * Hook for fetching venta detalles
 * GET /api/v1/ventas/{id}/detalles
 */
export function useVentaDetalles(idVenta: number | null) {
  return useQuery({
    queryKey: ['venta', idVenta, 'detalles'],
    queryFn: async () => {
      if (!idVenta) return [];
      const response = await api.get<DetalleVenta[]>(`/api/v1/ventas/${idVenta}/detalles`);
      return response.data ?? [];
    },
    enabled: !!idVenta,
    ...VENTAS_QUERY_CONFIG,
  });
}

/**
 * Hook for fetching venta pagos
 * GET /api/v1/ventas/{id}/pagos
 */
export function useVentaPagos(idVenta: number | null) {
  return useQuery({
    queryKey: ['venta', idVenta, 'pagos'],
    queryFn: async () => {
      if (!idVenta) return [];
      const response = await api.get<Pago[]>(`/api/v1/ventas/${idVenta}/pagos`);
      return response.data ?? [];
    },
    enabled: !!idVenta,
    ...VENTAS_QUERY_CONFIG,
  });
}

// ============================================
// Mutation Hooks
// ============================================

/**
 * Hook for creating a new venta
 * POST /api/v1/ventas
 */
export function useCreateVenta() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (data: CrearVentaRequest) => {
      const response = await api.post<Venta>('/api/v1/ventas', data);
      return response.data;
    },
    onSuccess: () => {
      toast.success('Venta registrada exitosamente');
      queryClient.invalidateQueries({ queryKey: ['ventas'] });
    },
    onError: (error: unknown) => {
      const err = error as { response?: { data?: { detail?: string } } };
      toast.error(err.response?.data?.detail || 'Error al registrar la venta');
    },
  });
}

/**
 * Hook for canceling (anulating) a venta
 * POST /api/v1/ventas/{id}/anular
 * Only Admin (Dueño) can perform this action
 */
export function useAnularVenta() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ id, motivo }: { id: number; motivo: string }) => {
      const request: AnularVentaRequest = { motivo };
      const response = await api.post<Venta>(`/api/v1/ventas/${id}/anular`, request);
      return response.data;
    },
    onSuccess: () => {
      toast.success('Venta anulada exitosamente');
      queryClient.invalidateQueries({ queryKey: ['ventas'] });
    },
    onError: (error: unknown) => {
      const err = error as { response?: { data?: { detail?: string } } };
      toast.error(err.response?.data?.detail || 'Error al anular la venta');
    },
  });
}

// ============================================
// Utility Exports
// ============================================

export { canAnularVenta };
