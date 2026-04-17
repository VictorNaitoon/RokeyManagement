/**
 * Compras Hooks - React Query hooks for purchase management
 * RoKey MANAGEMENT - Multi-tenant SaaS ERP/POS for locksmiths
 * 
 * Phase 6: Compras CRUD + Anular
 */

import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { api } from '@/lib/api/axios';
import { authStore } from '@/stores/authStore';
import type {
  Compra,
  CompraListResponse,
  CompraFilters,
  CrearCompraRequest,
} from '@/types';
import { toast } from 'sonner';

// ============================================
// Configuration Constants
// ============================================

const COMPRAS_QUERY_CONFIG = {
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
 * Check if user can manage compras (Dueño or Gerente - full access)
 */
export function canManageCompras(): boolean {
  const role = getUserRole();
  return role === 'Dueño' || role === 'Gerente';
}

// ============================================
// Query Hooks
// ============================================

/**
 * Hook for fetching list of compras
 * GET /api/v1/compras
 */
export interface UseComprasOptions {
  filters?: CompraFilters;
}

export function useCompras(options: UseComprasOptions = {}) {
  const { filters = {} } = options;
  
  return useQuery({
    queryKey: ['compras', filters],
    queryFn: async () => {
      const response = await api.get<CompraListResponse>('/api/v1/compras', {
        params: {
          idProveedor: filters.idProveedor || undefined,
        },
      });
      return response.data;
    },
    ...COMPRAS_QUERY_CONFIG,
  });
}

/**
 * Hook for fetching a single compra by ID
 * GET /api/v1/compras/{id}
 */
export function useCompra(id: number | null) {
  return useQuery({
    queryKey: ['compra', id],
    queryFn: async () => {
      if (!id) return null;
      const response = await api.get<Compra>(`/api/v1/compras/${id}`);
      return response.data;
    },
    enabled: !!id,
    ...COMPRAS_QUERY_CONFIG,
  });
}

// ============================================
// Mutation Hooks
// ============================================

/**
 * Hook for creating a new compra
 * POST /api/v1/compras
 */
export function useCreateCompra() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (data: CrearCompraRequest) => {
      // Transform camelCase to PascalCase for .NET backend
      const transformedData = {
        NumeroComprobante: data.numeroComprobante,
        IdProveedor: data.idProveedor,
        Detalles: data.detalles.map(d => ({
          IdProducto: d.idProducto,
          Cantidad: d.cantidad,
          PrecioUnitario: d.precioUnitario,
        })),
      };
      const response = await api.post<Compra>('/api/v1/compras', transformedData);
      return response.data;
    },
    onSuccess: () => {
      toast.success('Compra registrada exitosamente');
      queryClient.invalidateQueries({ queryKey: ['compras'] });
    },
    onError: (error: unknown) => {
      const err = error as { response?: { data?: { message?: string } } };
      toast.error(err.response?.data?.message || 'Error al registrar la compra');
    },
  });
}

/**
 * Hook for cancelling (anulating) a compra
 * POST /api/v1/compras/{id}/anular
 */
export function useAnularCompra() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (id: number) => {
      const response = await api.post<Compra>(`/api/v1/compras/${id}/anular`, {});
      return response.data;
    },
    onSuccess: () => {
      toast.success('Compra anulada exitosamente');
      queryClient.invalidateQueries({ queryKey: ['compras'] });
      queryClient.invalidateQueries({ queryKey: ['compra'] });
    },
    onError: (error: unknown) => {
      const err = error as { response?: { data?: { message?: string } } };
      toast.error(err.response?.data?.message || 'Error al anular la compra');
    },
  });
}