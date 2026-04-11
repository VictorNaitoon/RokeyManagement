/**
 * Productos Hooks - React Query hooks for product management
 * RoKey MANAGEMENT - Multi-tenant SaaS ERP/POS for locksmiths
 * 
 * Phase 2: Data Layer - CRUD operations for products
 */

import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { api } from '@/lib/api/axios';
import { authStore } from '@/stores/authStore';
import type {
  Producto,
  ProductoListResponse,
  ProductoAlertasResponse,
  CrearProductoRequest,
  ActualizarProductoRequest,
  ProductoFilters,
} from '@/types';
import { toast } from 'sonner';

// ============================================
// Configuration Constants
// ============================================

const PRODUCTOS_QUERY_CONFIG = {
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
 * Check if user has access to purchase price (Dueño or Gerente)
 */
function canViewPrecioCompra(): boolean {
  const role = getUserRole();
  return role === 'Dueño' || role === 'Gerente';
}

/**
 * Check if user can create/edit/delete products (Dueño or Gerente)
 */
function canManageProductos(): boolean {
  const role = getUserRole();
  return role === 'Dueño' || role === 'Gerente';
}

/**
 * Check if user can deactivate/reactivate products (Dueño or Gerente)
 */
function canManageEstadoProductos(): boolean {
  const role = getUserRole();
  return role === 'Dueño' || role === 'Gerente';
}

// ============================================
// Query Hooks
// ============================================

/**
 * Hook for fetching list of products
 * GET /api/v1/productos
 */
export interface UseProductosOptions {
  filters?: ProductoFilters;
}

export function useProductos(options: UseProductosOptions = {}) {
  const { filters = {} } = options;
  
  return useQuery({
    queryKey: ['productos', filters],
    queryFn: async () => {
      const response = await api.get<ProductoListResponse>('/api/v1/productos', {
        params: {
          busqueda: filters.busqueda || undefined,
        },
      });
      return response.data;
    },
    ...PRODUCTOS_QUERY_CONFIG,
  });
}

/**
 * Hook for fetching a single product by ID
 * GET /api/v1/productos/{id}
 */
export function useProducto(id: number | null) {
  return useQuery({
    queryKey: ['producto', id],
    queryFn: async () => {
      if (!id) return null;
      const response = await api.get<Producto>(`/api/v1/productos/${id}`);
      return response.data;
    },
    enabled: !!id,
    ...PRODUCTOS_QUERY_CONFIG,
  });
}

/**
 * Hook for fetching low stock alerts
 * GET /api/v1/productos/alertas
 */
export function useAlertasStock() {
  return useQuery({
    queryKey: ['productos', 'alertas'],
    queryFn: async () => {
      const response = await api.get<ProductoAlertasResponse>('/api/v1/productos/alertas');
      return response.data.productos ?? [];
    },
    ...PRODUCTOS_QUERY_CONFIG,
  });
}

// ============================================
// Mutation Hooks
// ============================================

/**
 * Hook for creating a new product
 * POST /api/v1/productos
 */
export function useCreateProducto() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (data: CrearProductoRequest) => {
      const response = await api.post<Producto>('/api/v1/productos', data);
      return response.data;
    },
    onSuccess: () => {
      toast.success('Producto creado exitosamente');
      queryClient.invalidateQueries({ queryKey: ['productos'] });
    },
    onError: (error: unknown) => {
      const err = error as { response?: { data?: { detail?: string } } };
      toast.error(err.response?.data?.detail || 'Error al crear el producto');
    },
  });
}

/**
 * Hook for updating an existing product
 * PUT /api/v1/productos/{id}
 */
export function useUpdateProducto() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ id, data }: { id: number; data: ActualizarProductoRequest }) => {
      const response = await api.put<Producto>(`/api/v1/productos/${id}`, data);
      return response.data;
    },
    onSuccess: () => {
      toast.success('Producto actualizado exitosamente');
      queryClient.invalidateQueries({ queryKey: ['productos'] });
      queryClient.invalidateQueries({ queryKey: ['producto'] });
    },
    onError: (error: unknown) => {
      const err = error as { response?: { data?: { detail?: string } } };
      toast.error(err.response?.data?.detail || 'Error al actualizar el producto');
    },
  });
}

/**
 * Hook for deleting (deactivating) a product
 * DELETE /api/v1/productos/{id}
 */
export function useDeleteProducto() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (id: number) => {
      await api.delete(`/api/v1/productos/${id}`);
    },
    onSuccess: () => {
      toast.success('Producto eliminado exitosamente');
      queryClient.invalidateQueries({ queryKey: ['productos'] });
    },
    onError: (error: unknown) => {
      const err = error as { response?: { data?: { detail?: string } } };
      toast.error(err.response?.data?.detail || 'Error al eliminar el producto');
    },
  });
}

/**
 * Hook for reactivating a deactivated product
 * POST /api/v1/productos/{id}/activar
 */
export function useReactivarProducto() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (id: number) => {
      const response = await api.post<Producto>(`/api/v1/productos/${id}/activar`);
      return response.data;
    },
    onSuccess: () => {
      toast.success('Producto reactivado exitosamente');
      queryClient.invalidateQueries({ queryKey: ['productos'] });
    },
    onError: (error: unknown) => {
      const err = error as { response?: { data?: { detail?: string } } };
      toast.error(err.response?.data?.detail || 'Error al reactivar el producto');
    },
  });
}

// ============================================
// Utility Exports
// ============================================

export { canViewPrecioCompra, canManageProductos, canManageEstadoProductos };