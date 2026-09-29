/**
 * Dashboard Data Hooks - React Query hooks for fetching dashboard data
 * RoKey MANAGEMENT - Multi-tenant SaaS ERP/POS for locksmiths
 *
 * Phase 2: Data Layer - Parallel fetching with role-based access control
 * FIXED (R07): Now sends preset/cantidad/fechaDesde/fechaHasta matching InformesController;
 * legacy fecha/limite params removed. Prefer useInformes() for /informes; this remains for /dashboard.
 */

import { useQuery } from '@tanstack/react-query';
import { api } from '@/lib/api/axios';
import { authStore } from '@/stores/authStore';
import type {
  VentasResumen,
  IngresosGastos,
  AlertaStock,
  AlertasStockResponse,
  ProductosTopResponse,
  VentasPorPagoResponse,
  FlujoCaja,
  VentasPorVendedorResponse,
  UltimaVenta,
  UserRole,
} from '@/types';

// ============================================
// Configuration Constants
// ============================================

const DASHBOARD_QUERY_CONFIG = {
  staleTime: 30000,    // 30 seconds
  refetchInterval: 60000, // 60 seconds (auto-refresh)
  retry: 2,
  refetchOnWindowFocus: false,
};

// ============================================
// Helper Functions
// ============================================

/**
 * Get current user role from auth store
 */
function getUserRole(): UserRole | null {
  const user = authStore.getState().user;
  if (!user) return null;
  // SuperAdmin no accede a informes de negocio
  if (user.rol === 'SuperAdmin') return 'SuperAdmin';
  return user.rol as UserRole;
}

/**
  * Check if user has access to admin-only data (Dueño or Gerente)
 */
function canAccessAdminData(): boolean {
  const role = getUserRole();
  return role === 'Dueño' || role === 'Gerente';
}

// ============================================
// Individual Query Hooks
// ============================================

/**
 * Hook for daily sales summary (Admin/Gerente only - backend restriction)
 * FIXED: sends preset instead of legacy fecha (R07)
 */
export function useVentasResumen(preset: string = 'hoy') {
  const isAdmin = canAccessAdminData();

  return useQuery({
    queryKey: ['dashboard', 'ventas-resumen', preset],
    queryFn: async () => {
      const response = await api.get<VentasResumen>('/api/v1/informes/ventas-resumen', {
        params: { preset },
      });
      return response.data ?? { totalVentas: 0, cantidadVentas: 0, ticketPromedio: 0 };
    },
    enabled: isAdmin,
    ...DASHBOARD_QUERY_CONFIG,
  });
}

/**
 * Hook for income vs expenses (Admin/Gerente only)
 * FIXED: sends preset instead of legacy fecha
 */
export function useIngresosGastos(preset: string = 'mes') {
  const isAdmin = canAccessAdminData();

  return useQuery({
    queryKey: ['dashboard', 'ingresos-gastos', preset],
    queryFn: async () => {
      const response = await api.get<IngresosGastos>('/api/v1/informes/ingresos-gastos', {
        params: { preset },
      });
      return response.data ?? { totalVentas: 0, totalCompras: 0, gananciaBruta: 0, margen: 0 };
    },
    enabled: isAdmin,
    ...DASHBOARD_QUERY_CONFIG,
  });
}

/**
 * Hook for low stock alerts (Admin/Gerente only - backend restriction)
 */
export function useAlertasStock() {
  const isAdmin = canAccessAdminData();
  
  return useQuery({
    queryKey: ['dashboard', 'alertas-stock'],
    queryFn: async () => {
      const response = await api.get<AlertasStockResponse>('/api/v1/informes/alertas-stock');
      return response.data?.productos ?? [];
    },
    enabled: isAdmin,
    ...DASHBOARD_QUERY_CONFIG,
  });
}

/**
 * Hook for top selling products (Admin/Gerente only - backend restriction)
 * FIXED: sends cantidad instead of legacy limite
 */
export function useProductosTop(cantidad: number = 10) {
  const isAdmin = canAccessAdminData();

  return useQuery({
    queryKey: ['dashboard', 'productos-top', cantidad],
    queryFn: async () => {
      const response = await api.get<ProductosTopResponse>('/api/v1/informes/productos-top', {
        params: { cantidad },
      });
      return response.data?.productos ?? [];
    },
    enabled: isAdmin,
    ...DASHBOARD_QUERY_CONFIG,
  });
}

/**
 * Hook for sales by payment method (Admin/Gerente only)
 */
export function useVentasPorPago() {
  const isAdmin = canAccessAdminData();
  
  return useQuery({
    queryKey: ['dashboard', 'ventas-por-pago'],
    queryFn: async () => {
      const response = await api.get<VentasPorPagoResponse>('/api/v1/informes/ventas-por-pago');
      return response.data?.ventas ?? [];
    },
    enabled: isAdmin,
    ...DASHBOARD_QUERY_CONFIG,
  });
}

/**
 * Hook for cash flow summary (Admin/Gerente only - backend restriction)
 * FIXED: sends preset instead of legacy fecha
 */
export function useFlujoCaja(preset: string = 'mes') {
  const isAdmin = canAccessAdminData();

  return useQuery({
    queryKey: ['dashboard', 'flujo-caja', preset],
    queryFn: async () => {
      const response = await api.get<FlujoCaja>('/api/v1/informes/flujo-caja', {
        params: { preset },
      });
      return response.data ?? { ingresos: 0, egresos: 0, balance: 0 };
    },
    enabled: isAdmin,
    ...DASHBOARD_QUERY_CONFIG,
  });
}

/**
 * Hook for sales by seller (Admin/Gerente only)
 */
export function useVentasPorVendedor() {
  const isAdmin = canAccessAdminData();
  
  return useQuery({
    queryKey: ['dashboard', 'ventas-por-vendedor'],
    queryFn: async () => {
      const response = await api.get<VentasPorVendedorResponse>('/api/v1/informes/ventas-por-vendedor');
      return response.data?.ventas ?? [];
    },
    enabled: isAdmin,
    ...DASHBOARD_QUERY_CONFIG,
  });
}

/**
 * Hook for the latest sales shown on Inicio
 * GET /api/v1/ventas - available to every role (no informes restriction)
 * The backend already returns newest-first ordering.
 */
export function useUltimasVentas(limite: number = 6) {
  return useQuery({
    queryKey: ['dashboard', 'ultimas-ventas', limite],
    queryFn: async () => {
      const response = await api.get<{ items?: Record<string, unknown>[] }>('/api/v1/ventas', {
        params: { page: 1, pageSize: limite },
      });

      return (response.data.items ?? []).map((item): UltimaVenta => {
        const fecha = typeof item.fecha === 'string' ? item.fecha : new Date().toISOString();
        const estado = item.estado === 'Anulada' ? 'Anulada' : 'Activa';
        return {
          id: typeof item.id === 'number' ? item.id : 0,
          fecha,
          totalVenta: typeof item.totalVenta === 'number' ? item.totalVenta : 0,
          estado,
          clienteNombre: typeof item.nombreCliente === 'string' ? item.nombreCliente : '',
          usuarioNombre: typeof item.nombreUsuario === 'string' ? item.nombreUsuario : '',
        };
      });
    },
    ...DASHBOARD_QUERY_CONFIG,
  });
}

// ============================================
// Combined Hook for Convenience
// ============================================


export interface DashboardDataReturn {
  // Data
  ventasResumen: VentasResumen | undefined;
  ingresosGastos: IngresosGastos | undefined;
  alertasStock: AlertaStock[] | undefined;
  ultimasVentas: UltimaVenta[] | undefined;

  // Data states
  isLoading: boolean;
  isFetching: boolean;
  isError: boolean;
  error: Error | null;
  /** Epoch ms of the newest successful fetch (0 when nothing has loaded yet) */
  dataUpdatedAt: number;

  // Refetch
  refetch: () => void;

  // Access control
  canViewAdminData: boolean;
  userRole: UserRole | null;
}

/**
 * Combined hook for the Inicio page.
 *
 * Only aggregates the queries the Inicio actually renders. The chart/product-top
 * series live in useInformes and are not requested here (they used to be fetched
 * on every Inicio load even when nothing consumed them).
 *
 * React Query de-duplicates by queryKey, so the components that render the same
 * data (ResumenHoy, AlertasStockPanel, UltimasVentas) share these requests.
 */
export function useDashboardData(): DashboardDataReturn {
  const ventasResumen = useVentasResumen('hoy');
  const ingresosGastos = useIngresosGastos('hoy');
  const alertasStock = useAlertasStock();
  const ultimasVentas = useUltimasVentas(6);

  const queries = [ventasResumen, alertasStock, ultimasVentas];
  const adminQueries = [ingresosGastos];

  const isLoading =
    queries.some((q) => q.isLoading) ||
    adminQueries.some((q) => q.isEnabled && q.isLoading);

  const isFetching =
    queries.some((q) => q.isFetching) ||
    adminQueries.some((q) => q.isEnabled && q.isFetching);

  const errors = [
    ...queries.map((q) => q.error),
    ...adminQueries.map((q) => (q.isEnabled ? q.error : null)),
  ].filter(Boolean) as Error[];

  const dataUpdatedAt = Math.max(...queries.map((q) => q.dataUpdatedAt), ...adminQueries.map((q) => q.dataUpdatedAt));

  const refetch = () => {
    queries.forEach((q) => q.refetch());
    adminQueries.forEach((q) => {
      if (q.isEnabled) q.refetch();
    });
  };

  return {
    ventasResumen: ventasResumen.data,
    ingresosGastos: ingresosGastos.data,
    alertasStock: alertasStock.data,
    ultimasVentas: ultimasVentas.data,
    isLoading,
    isFetching,
    isError: errors.length > 0,
    error: errors.length > 0 ? errors[0] : null,
    dataUpdatedAt,
    refetch,
    canViewAdminData: canAccessAdminData(),
    userRole: getUserRole(),
  };
}
