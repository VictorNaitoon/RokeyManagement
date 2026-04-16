/**
 * presupuestos Hooks - React Query hooks for budget/quote management
 * RoKey MANAGEMENT - Multi-tenant SaaS ERP/POS for locksmiths
 * 
 * Phase 6: presupuestos CRUD + Convertir a Venta
 */

import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { api } from '@/lib/api/axios';
import { authStore } from '@/stores/authStore';
import type {
  Presupuesto,
  PresupuestoListResponse,
  PresupuestoFilters,
  CrearPresupuestoRequest,
  UpdatePresupuestoRequest,
} from '@/types';
import { toast } from 'sonner';

// ============================================
// Configuration Constants
// ============================================

const PRESUPUESTOS_QUERY_CONFIG = {
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
 * Check if user can manage presupuestos (Dueño or Gerente or Empleado)
 */
export function canManagePresupuestos(): boolean {
  const role = getUserRole();
  return role === 'Dueño' || role === 'Gerente' || role === 'Empleado';
}

/**
 * Check if user can convert presupuestos to ventas (Dueño or Gerente or Empleado)
 */
export function canConvertirPresupuesto(): boolean {
  const role = getUserRole();
  return role === 'Dueño' || role === 'Gerente' || role === 'Empleado';
}

// ============================================
// Query Hooks
// ============================================

/**
 * Hook for fetching list of presupuestos
 * GET /api/v1/presupuestos
 */
export interface UsePresupuestosOptions {
  filters?: PresupuestoFilters;
}

export function usePresupuestos(options: UsePresupuestosOptions = {}) {
  const { filters = {} } = options;
  
  return useQuery({
    queryKey: ['presupuestos', filters],
    queryFn: async () => {
      const response = await api.get<PresupuestoListResponse>('/api/v1/presupuestos', {
        params: {
          estado: filters.estado || undefined,
          idCliente: filters.idCliente || undefined,
        },
      });
      return response.data;
    },
    ...PRESUPUESTOS_QUERY_CONFIG,
  });
}

/**
 * Hook for fetching a single presupuesto by ID
 * GET /api/v1/presupuestos/{id}
 */
export function usePresupuesto(id: number | null) {
  return useQuery({
    queryKey: ['presupuesto', id],
    queryFn: async () => {
      if (!id) return null;
      const response = await api.get<Presupuesto>(`/api/v1/presupuestos/${id}`);
      return response.data;
    },
    enabled: !!id,
    ...PRESUPUESTOS_QUERY_CONFIG,
  });
}

// ============================================
// Mutation Hooks
// ============================================

/**
 * Hook for creating a new presupuesto
 * POST /api/v1/presupuestos
 */
export function useCreatePresupuesto() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (data: CrearPresupuestoRequest) => {
      // Transform frontend camelCase to backend PascalCase
      const transformedData = {
        IdCliente: data.idCliente,
        FechaVencimiento: data.fechaVencimiento,
        Detalles: data.detalles.map(d => ({
          IdProducto: d.idProducto,
          Cantidad: d.cantidad,
          PrecioPactado: d.precioPactado,
        })),
      };
      const response = await api.post<Presupuesto>('/api/v1/presupuestos', transformedData);
      return response.data;
    },
    onSuccess: () => {
      toast.success('Presupuesto creado exitosamente');
      queryClient.invalidateQueries({ queryKey: ['presupuestos'] });
    },
    onError: (error: unknown) => {
      const err = error as { response?: { data?: { message?: string } } };
      toast.error(err.response?.data?.message || 'Error al crear el presupuesto');
    },
  });
}

/**
 * Hook for updating presupuesto estado
 * PUT /api/v1/presupuestos/{id}
 */
export function useUpdatePresupuestoEstado() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ id, data }: { id: number; data: UpdatePresupuestoRequest }) => {
      const response = await api.put<Presupuesto>(`/api/v1/presupuestos/${id}`, data);
      return response.data;
    },
    onSuccess: () => {
      toast.success('Presupuesto actualizado exitosamente');
      queryClient.invalidateQueries({ queryKey: ['presupuestos'] });
      queryClient.invalidateQueries({ queryKey: ['presupuesto'] });
    },
    onError: (error: unknown) => {
      const err = error as { response?: { data?: { message?: string } } };
      toast.error(err.response?.data?.message || 'Error al actualizar el presupuesto');
    },
  });
}

/**
 * Hook for deleting (anulating) a presupuesto
 * DELETE /api/v1/presupuestos/{id}
 */
export function useAnularPresupuesto() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (id: number) => {
      await api.delete(`/api/v1/presupuestos/${id}`);
    },
    onSuccess: () => {
      toast.success('Presupuesto anulado exitosamente');
      queryClient.invalidateQueries({ queryKey: ['presupuestos'] });
    },
    onError: (error: unknown) => {
      const err = error as { response?: { data?: { message?: string } } };
      toast.error(err.response?.data?.message || 'Error al anular el presupuesto');
    },
  });
}

/**
 * Hook for converting presupuesto to venta
 * POST /api/v1/presupuestos/{id}/convertir
 */
export function useConvertirPresupuesto() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (id: number) => {
      const response = await api.post<{ id: number }>(`/api/v1/presupuestos/${id}/convertir`);
      return response.data;
    },
    onSuccess: () => {
      toast.success('Presupuesto convertido a venta exitosamente');
      queryClient.invalidateQueries({ queryKey: ['presupuestos'] });
      queryClient.invalidateQueries({ queryKey: ['ventas'] });
    },
    onError: (error: unknown) => {
      const err = error as { response?: { data?: { message?: string } } };
      toast.error(err.response?.data?.message || 'Error al convertir el presupuesto a venta');
    },
  });
}