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
  ProductoAlerta,
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
 * Check if user can create/edit/delete products — Solo Dueño (Gerente solo ajuste-stock via AjusteStockDialog)
 */
function canManageProductos(): boolean {
  const role = getUserRole();
  return role === 'Dueño';
}

/**
 * Check if user can deactivate/reactivate products — Solo Dueño (Gerente solo ajuste-stock via AjusteStockDialog)
 */
function canManageEstadoProductos(): boolean {
  const role = getUserRole();
  return role === 'Dueño';
}

/**
 * Check if user can view audit history (Dueño or Gerente) — IA-02
 */
function canViewHistorial(): boolean {
  const role = getUserRole();
  return role === 'Dueño' || role === 'Gerente';
}

// ============================================
// Query Hooks
// ============================================

/**
 * Hook for fetching list of products
 * GET /v1/productos
 */
export interface UseProductosOptions {
  filters?: ProductoFilters;
}

export function useProductos(options: UseProductosOptions = {}) {
  const { filters = {} } = options;
  
  return useQuery({
    queryKey: ['productos', filters],
    queryFn: async () => {
      const response = await api.get<ProductoListResponse>('/api/v1/Producto', {
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
 * GET /api/v1/Producto/{id}
 */
export function useProducto(id: number | null) {
  return useQuery({
    queryKey: ['producto', id],
    queryFn: async () => {
      if (!id) return null;
      const response = await api.get<Producto>(`/api/v1/Producto/${id}`);
      return response.data;
    },
    enabled: !!id,
    ...PRODUCTOS_QUERY_CONFIG,
  });
}

/**
 * Hook for fetching low stock alerts
 * GET /api/v1/Producto/alertas
 */
export function useAlertasStock() {
  return useQuery({
    queryKey: ['productos', 'alertas'],
    queryFn: async () => {
      // Backend returns List<ProductoAlertaResponse> directly (not wrapped)
      const response = await api.get<ProductoAlerta[]>('/api/v1/Producto/alertas');
      return response.data ?? [];
    },
    ...PRODUCTOS_QUERY_CONFIG,
  });
}

/**
 * Hook for fetching paginated stock movements — IA-01/IA-02
 * GET /api/v1/Producto/{id}/movimientos?page&pageSize
 * Enabled only for Dueño || Gerente; disabled for Empleado (hook never fires).
 */
export function useMovimientosStock(
  productoId: number | null,
  page: number,
  pageSize: number
) {
  const enabled = !!productoId && canViewHistorial();
  return useQuery({
    queryKey: ['movimientos', productoId, page, pageSize],
    queryFn: async () => {
      const response = await api.get(`/api/v1/Producto/${productoId}/movimientos`, {
        params: { page, pageSize },
      });
      // Normalize PascalCase vs camelCase from backend
      const data = response.data as Record<string, unknown>;
      const movimientos = (data.movimientos ?? data.Movimientos ?? []) as import('@/types').MovimientoStock[];
      const total = (data.total ?? data.Total ?? 0) as number;
      const p = (data.page ?? data.Page ?? page) as number;
      const ps = (data.pageSize ?? data.PageSize ?? pageSize) as number;
      return { movimientos, total, page: p, pageSize: ps };
    },
    enabled,
    ...PRODUCTOS_QUERY_CONFIG,
  });
}

/**
 * Hook for adjusting stock — SA-01 (placeholder for wiring, POST ajuste-stock)
 * Kept here for PR3 task 3.1 completeness; actual dialog in AjusteStockDialog.
 */
export function useAjusteStock() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async ({ id, cantidadDelta, motivo }: { id: number; cantidadDelta: number; motivo: string }) => {
      const response = await api.post(`/api/v1/Producto/${id}/ajuste-stock`, { cantidadDelta, motivo });
      return response.data;
    },
    onSuccess: () => {
      toast.success('Stock ajustado correctamente');
      queryClient.invalidateQueries({ queryKey: ['productos'] });
      queryClient.invalidateQueries({ queryKey: ['movimientos'] });
    },
    onError: (error: unknown) => {
      const err = error as { response?: { data?: { detail?: string; message?: string } } };
      toast.error(err.response?.data?.detail || err.response?.data?.message || 'Error al ajustar stock');
    },
  });
}

// ============================================
// Mutation Hooks
// ============================================

/**
 * Hook for creating a new product
 * POST /v1/productos
 */
export function useCreateProducto() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (data: CrearProductoRequest) => {
      const response = await api.post<Producto>('/api/v1/Producto', data);
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
 * PUT /api/v1/Producto/{id}
 */
export function useUpdateProducto() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ id, data }: { id: number; data: ActualizarProductoRequest }) => {
      const response = await api.put<Producto>(`/api/v1/Producto/${id}`, data);
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
 * DELETE /api/v1/Producto/{id}
 */
export function useDeleteProducto() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (id: number) => {
      await api.delete(`/api/v1/Producto/${id}`);
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
 * POST /api/v1/Producto/{id}/activar
 */
export function useReactivarProducto() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (id: number) => {
      const response = await api.post<Producto>(`/api/v1/Producto/${id}/activar`);
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
// CSV Import — CI-01/02/03
// ============================================

export function useImportCsv() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (file: File) => {
      const form = new FormData();
      form.append('file', file);
      const res = await api.post('/api/v1/Producto/import/csv', form, {
        headers: { 'Content-Type': 'multipart/form-data' },
      });
      return res.data as import('@/types').ImportCsvResponse;
    },
    onSuccess: (data) => {
      const d = data as unknown as Record<string, unknown>;
      const created = (d.created ?? d.Created ?? 0) as number;
      const skipped = (d.skipped ?? d.Skipped ?? 0) as number;
      if (skipped === 0) toast.success(`${created} productos importados`);
      else toast.warning(`${created} creados, ${skipped} omitidos — revisar errores`);
      queryClient.invalidateQueries({ queryKey: ['productos'] });
    },
    onError: (error: unknown) => {
      const err = error as { response?: { status?: number; data?: { message?: string; detail?: string } } };
      if (err.response?.status === 413) toast.error('Archivo excede 5MB');
      else toast.error(err.response?.data?.message || err.response?.data?.detail || 'Error al importar CSV');
    },
  });
}

// ============================================
// Utility Exports
// ============================================

export { canViewPrecioCompra, canManageProductos, canManageEstadoProductos, canViewHistorial, getUserRole };