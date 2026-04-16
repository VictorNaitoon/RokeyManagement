/**
 * Clientes Hooks - React Query hooks for customer management
 * RoKey MANAGEMENT - Multi-tenant SaaS ERP/POS for locksmiths
 * 
 * Phase 6: Clientes CRUD + Cuenta Corriente
 */

import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { api } from '@/lib/api/axios';
import { authStore } from '@/stores/authStore';
import type {
  Cliente,
  ClienteFilters,
  CrearClienteRequest,
  ActualizarClienteRequest,
  VentaCliente,
  SaldoCliente,
} from '@/types';
import { toast } from 'sonner';

// ============================================
// Configuration Constants
// ============================================

const CLIENTES_QUERY_CONFIG = {
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
 * Check if user can manage clientes (Dueño or Gerente - full access)
 */
export function canManageClientes(): boolean {
  const role = getUserRole();
  return role === 'Dueño' || role === 'Gerente';
}

/**
 * Check if user can view clientes (Dueño, Gerente, or Empleado - view only)
 */
export function canViewClientes(): boolean {
  const role = getUserRole();
  return role === 'Dueño' || role === 'Gerente' || role === 'Empleado';
}

// ============================================
// Query Hooks
// ============================================

/**
 * Hook for fetching list of clientes
 * GET /api/v1/clientes
 */
export interface UseClientesOptions {
  filters?: ClienteFilters;
}

export function useClientes(options: UseClientesOptions = {}) {
  const { filters = {} } = options;
  
  return useQuery({
    queryKey: ['clientes', filters],
    queryFn: async () => {
      const response = await api.get<Cliente[]>('/api/v1/clientes', {
        params: {
          incluirConsumidorFinal: true,
        },
      });
      return response.data;
    },
    ...CLIENTES_QUERY_CONFIG,
  });
}

/**
 * Hook for fetching a single cliente by ID
 * GET /api/v1/clientes/{id}
 */
export function useCliente(id: number | null) {
  return useQuery({
    queryKey: ['cliente', id],
    queryFn: async () => {
      if (!id) return null;
      const response = await api.get<Cliente>(`/api/v1/clientes/${id}`);
      return response.data;
    },
    enabled: !!id,
    ...CLIENTES_QUERY_CONFIG,
  });
}

/**
 * Hook for fetching cliente saldo (cuenta corriente)
 * GET /api/v1/clientes/{id}/saldo
 */
export function useClienteSaldo(id: number | null) {
  return useQuery({
    queryKey: ['cliente', id, 'saldo'],
    queryFn: async () => {
      if (!id) return null;
      const response = await api.get<SaldoCliente>(`/api/v1/clientes/${id}/saldo`);
      return response.data;
    },
    enabled: !!id,
    ...CLIENTES_QUERY_CONFIG,
  });
}

/**
 * Hook for fetching cliente ventas (cuenta corriente)
 * GET /api/v1/clientes/{id}/ventas
 */
export function useClienteVentas(id: number | null, page = 1, pageSize = 10) {
  return useQuery({
    queryKey: ['cliente', id, 'ventas', page, pageSize],
    queryFn: async () => {
      if (!id) return null;
      const response = await api.get<{ ventas: VentaCliente[]; total: number }>(`/api/v1/clientes/${id}/ventas`, {
        params: { page, pageSize },
      });
      return response.data;
    },
    enabled: !!id,
    ...CLIENTES_QUERY_CONFIG,
  });
}

// ============================================
// Mutation Hooks
// ============================================

/**
 * Hook for creating a new cliente
 * POST /api/v1/clientes
 */
export function useCreateCliente() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (data: CrearClienteRequest) => {
      const response = await api.post<Cliente>('/api/v1/clientes', data);
      return response.data;
    },
    onSuccess: () => {
      toast.success('Cliente creado exitosamente');
      queryClient.invalidateQueries({ queryKey: ['clientes'] });
    },
    onError: (error: unknown) => {
      const err = error as { response?: { data?: { error?: string } } };
      toast.error(err.response?.data?.error || 'Error al crear el cliente');
    },
  });
}

/**
 * Hook for updating an existing cliente
 * PUT /api/v1/clientes/{id}
 */
export function useUpdateCliente() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ id, data }: { id: number; data: ActualizarClienteRequest }) => {
      const response = await api.put<Cliente>(`/api/v1/clientes/${id}`, data);
      return response.data;
    },
    onSuccess: () => {
      toast.success('Cliente actualizado exitosamente');
      queryClient.invalidateQueries({ queryKey: ['clientes'] });
      queryClient.invalidateQueries({ queryKey: ['cliente'] });
    },
    onError: (error: unknown) => {
      const err = error as { response?: { data?: { error?: string } } };
      toast.error(err.response?.data?.error || 'Error al actualizar el cliente');
    },
  });
}

/**
 * Hook for deleting a cliente
 * DELETE /api/v1/clientes/{id}
 */
export function useDeleteCliente() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (id: number) => {
      await api.delete(`/api/v1/clientes/${id}`);
    },
    onSuccess: () => {
      toast.success('Cliente eliminado exitosamente');
      queryClient.invalidateQueries({ queryKey: ['clientes'] });
    },
    onError: (error: unknown) => {
      const err = error as { response?: { data?: { error?: string } } };
      toast.error(err.response?.data?.error || 'Error al eliminar el cliente');
    },
  });
}