/**
 * Proveedores Hooks - React Query hooks for supplier management
 * RoKey MANAGEMENT - Multi-tenant SaaS ERP/POS for locksmiths
 * 
 * Phase 6: Proveedores CRUD
 */

import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { api } from '@/lib/api/axios';
import { authStore } from '@/stores/authStore';
import type {
  Proveedor,
  ProveedorFilters,
  CrearProveedorRequest,
  ActualizarProveedorRequest,
} from '@/types';
import { toast } from 'sonner';

// ============================================
// Configuration Constants
// ============================================

const PROVEEDORES_QUERY_CONFIG = {
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
 * Check if user can manage proveedores (Dueño only - full access)
 */
export function canManageProveedores(): boolean {
  const role = getUserRole();
  return role === 'Dueño';
}

// ============================================
// Query Hooks
// ============================================

/**
 * Hook for fetching list of proveedores
 * GET /api/v1/proveedores
 */
export interface UseProveedoresOptions {
  filters?: ProveedorFilters;
}

export function useProveedores(options: UseProveedoresOptions = {}) {
  const { filters = {} } = options;
  
  return useQuery({
    queryKey: ['proveedores', filters],
    queryFn: async () => {
      const response = await api.get<{ proveedores: Proveedor[]; total: number }>('/api/v1/proveedores');
      return response.data;
    },
    ...PROVEEDORES_QUERY_CONFIG,
  });
}

/**
 * Hook for fetching a single proveedor by ID
 * GET /api/v1/proveedores/{id}
 */
export function useProveedor(id: number | null) {
  return useQuery({
    queryKey: ['proveedor', id],
    queryFn: async () => {
      if (!id) return null;
      const response = await api.get<Proveedor>(`/api/v1/proveedores/${id}`);
      return response.data;
    },
    enabled: !!id,
    ...PROVEEDORES_QUERY_CONFIG,
  });
}

// ============================================
// Mutation Hooks
// ============================================

/**
 * Hook for creating a new proveedor
 * POST /api/v1/proveedores
 */
export function useCreateProveedor() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (data: CrearProveedorRequest) => {
      const response = await api.post<Proveedor>('/api/v1/proveedores', data);
      return response.data;
    },
    onSuccess: () => {
      toast.success('Proveedor creado exitosamente');
      queryClient.invalidateQueries({ queryKey: ['proveedores'] });
    },
    onError: (error: unknown) => {
      const err = error as { response?: { data?: { message?: string } } };
      toast.error(err.response?.data?.message || 'Error al crear el proveedor');
    },
  });
}

/**
 * Hook for updating an existing proveedor
 * PUT /api/v1/proveedores/{id}
 */
export function useUpdateProveedor() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ id, data }: { id: number; data: ActualizarProveedorRequest }) => {
      const response = await api.put<Proveedor>(`/api/v1/proveedores/${id}`, data);
      return response.data;
    },
    onSuccess: () => {
      toast.success('Proveedor actualizado exitosamente');
      queryClient.invalidateQueries({ queryKey: ['proveedores'] });
      queryClient.invalidateQueries({ queryKey: ['proveedor'] });
    },
    onError: (error: unknown) => {
      const err = error as { response?: { data?: { message?: string } } };
      toast.error(err.response?.data?.message || 'Error al actualizar el proveedor');
    },
  });
}

/**
 * Hook for deleting (deactivating) a proveedor
 * DELETE /api/v1/proveedores/{id}
 */
export function useDeleteProveedor() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (id: number) => {
      await api.delete(`/api/v1/proveedores/${id}`);
    },
    onSuccess: () => {
      toast.success('Proveedor eliminado exitosamente');
      queryClient.invalidateQueries({ queryKey: ['proveedores'] });
    },
    onError: (error: unknown) => {
      const err = error as { response?: { data?: { message?: string } } };
      toast.error(err.response?.data?.message || 'Error al eliminar el proveedor');
    },
  });
}