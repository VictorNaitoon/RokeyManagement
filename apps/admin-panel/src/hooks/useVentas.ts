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
      const response = await api.get<{items: any[], totalCount: number}>('/api/v1/ventas', {
        params: {
          fechaDesde: filters.fechaDesde || undefined,
          fechaHasta: filters.fechaHasta || undefined,
        },
      });
      console.log('API response:', response.data);
      // Map backend PascalCase to frontend camelCase
      const ventas = (response.data.items || []).map((item: any) => ({
        id: item.Id ?? 0,
        fecha: item.Fecha ?? new Date().toISOString(),
        totalVenta: item.TotalVenta ?? 0,
        estado: item.Estado ?? 'Activa',
        idUsuario: item.IdUsuario,
        usuarioNombre: item.NombreUsuario ?? '',
        idCliente: item.IdCliente,
        clienteNombre: item.NombreCliente ?? '',
      }));
      console.log('Mapped ventas:', ventas);
      return {
        ventas,
        total: response.data.totalCount || 0,
      };
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

// Map frontend string to backend number for MetodoPago
const metodoPagoMap: Record<string, number> = {
  'Efectivo': 1,
  'TarjetaCredito': 2,
  'TarjetaDebito': 3,
  'Transferencia': 4,
};

/**
 * Hook for creating a new venta
 * POST /api/v1/ventas
 */
export function useCreateVenta() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (data: CrearVentaRequest) => {
      // Transform frontend camelCase to backend PascalCase
      // Also convert metodoPago string to number
      const transformedData = {
        idCliente: data.idCliente,
        detalles: data.detalles.map(d => ({
          IdProducto: d.idProducto,
          Cantidad: d.cantidad,
          PrecioUnitario: d.precioUnitario,
        })),
        pagos: data.pagos.map(p => ({
          MetodoPago: metodoPagoMap[p.metodoPago as string] || 1,
          Monto: p.monto,
        })),
      };
      const response = await api.post<Venta>('/api/v1/ventas', transformedData);
      return response.data;
    },
    onSuccess: () => {
      toast.success('Venta registrada exitosamente');
      queryClient.invalidateQueries({ queryKey: ['ventas'] });
    },
    onError: (error: unknown) => {
      const err = error as { response?: { data?: { detail?: string, message?: string } } };
      toast.error(err.response?.data?.detail || err.response?.data?.message || 'Error al registrar la venta');
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
