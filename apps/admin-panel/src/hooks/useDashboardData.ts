/**
 * Dashboard Data Hooks - React Query hooks for fetching dashboard data
 * RoKey MANAGEMENT - Multi-tenant SaaS ERP/POS for locksmiths
 * 
 * Phase 2: Data Layer - Parallel fetching with role-based access control
 */

import { useQuery } from '@tanstack/react-query';
import { api } from '@/lib/api/axios';
import { authStore } from '@/stores/authStore';
import type {
  VentasResumen,
  IngresosGastos,
  AlertaStock,
  AlertasStockResponse,
  ProductoTop,
  ProductosTopResponse,
  VentasPorPago,
  VentasPorPagoResponse,
  FlujoCaja,
  VentasPorVendedor,
  VentasPorVendedorResponse,
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
  // Map from auth store role (includes SuperAdmin) to dashboard role
  if (user.rol === 'SuperAdmin') return 'Admin';
  return user.rol as UserRole;
}

/**
 * Check if user has access to admin-only data
 */
function canAccessAdminData(): boolean {
  const role = getUserRole();
  return role === 'Admin' || role === 'Gerente';
}

// ============================================
// Individual Query Hooks
// ============================================

/**
 * Hook for daily sales summary (all roles)
 */
export function useVentasResumen(fecha: string = 'hoy') {
  return useQuery({
    queryKey: ['dashboard', 'ventas-resumen', fecha],
    queryFn: async () => {
      const response = await api.get<VentasResumen>('/api/v1/informes/ventas-resumen', {
        params: { fecha },
      });
      return response.data;
    },
    ...DASHBOARD_QUERY_CONFIG,
  });
}

/**
 * Hook for income vs expenses (Admin/Gerente only)
 */
export function useIngresosGastos(fecha: string = 'mes') {
  const isAdmin = canAccessAdminData();
  
  return useQuery({
    queryKey: ['dashboard', 'ingresos-gastos', fecha],
    queryFn: async () => {
      const response = await api.get<IngresosGastos>('/api/v1/informes/ingresos-gastos', {
        params: { fecha },
      });
      return response.data;
    },
    enabled: isAdmin,
    ...DASHBOARD_QUERY_CONFIG,
  });
}

/**
 * Hook for low stock alerts (all roles)
 */
export function useAlertasStock() {
  return useQuery({
    queryKey: ['dashboard', 'alertas-stock'],
    queryFn: async () => {
      const response = await api.get<AlertasStockResponse>('/api/v1/informes/alertas-stock');
      return response.data.productos;
    },
    ...DASHBOARD_QUERY_CONFIG,
  });
}

/**
 * Hook for top selling products (all roles)
 */
export function useProductosTop(limite: number = 10) {
  return useQuery({
    queryKey: ['dashboard', 'productos-top', limite],
    queryFn: async () => {
      const response = await api.get<ProductosTopResponse>('/api/v1/informes/productos-top', {
        params: { limite },
      });
      return response.data.productos;
    },
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
      return response.data.ventas;
    },
    enabled: isAdmin,
    ...DASHBOARD_QUERY_CONFIG,
  });
}

/**
 * Hook for cash flow summary (all roles)
 */
export function useFlujoCaja(fecha: string = 'mes') {
  return useQuery({
    queryKey: ['dashboard', 'flujo-caja', fecha],
    queryFn: async () => {
      const response = await api.get<FlujoCaja>('/api/v1/informes/flujo-caja', {
        params: { fecha },
      });
      return response.data;
    },
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
      return response.data.ventas;
    },
    enabled: isAdmin,
    ...DASHBOARD_QUERY_CONFIG,
  });
}

// ============================================
// Combined Hook for Convenience
// ============================================

export interface UseDashboardDataOptions {
  fecha?: string;
  limiteProductos?: number;
}

export interface DashboardDataReturn {
  // Data states
  ventasResumen: VentasResumen | undefined;
  ingresosGastos: IngresosGastos | undefined;
  alertasStock: AlertaStock[];
  productosTop: ProductoTop[];
  ventasPorPago: VentasPorPago[];
  flujoCaja: FlujoCaja | undefined;
  ventasPorVendedor: VentasPorVendedor[];
  
  // Loading states
  isLoading: boolean;
  isLoadingVentasResumen: boolean;
  isLoadingIngresosGastos: boolean;
  isLoadingAlertasStock: boolean;
  isLoadingProductosTop: boolean;
  isLoadingVentasPorPago: boolean;
  isLoadingFlujoCaja: boolean;
  isLoadingVentasPorVendedor: boolean;
  
  // Error states
  error: Error | null;
  errorVentasResumen: Error | null;
  errorIngresosGastos: Error | null;
  errorAlertasStock: Error | null;
  errorProductosTop: Error | null;
  errorVentasPorPago: Error | null;
  errorFlujoCaja: Error | null;
  errorVentasPorVendedor: Error | null;
  
  // Refetch functions
  refetch: () => void;
  refetchVentasResumen: () => void;
  refetchIngresosGastos: () => void;
  refetchAlertasStock: () => void;
  refetchProductosTop: () => void;
  refetchVentasPorPago: () => void;
  refetchFlujoCaja: () => void;
  refetchVentasPorVendedor: () => void;
  
  // Access control
  canViewAdminData: boolean;
  userRole: UserRole | null;
}

/**
 * Combined hook for fetching all dashboard data
 * Automatically handles role-based access control
 */
export function useDashboardData(options: UseDashboardDataOptions = {}): DashboardDataReturn {
  const { fecha = 'mes', limiteProductos = 10 } = options;
  
  // Individual queries
  const ventasResumen = useVentasResumen('hoy');
  const ingresosGastos = useIngresosGastos(fecha);
  const alertasStock = useAlertasStock();
  const productosTop = useProductosTop(limiteProductos);
  const ventasPorPago = useVentasPorPago();
  const flujoCaja = useFlujoCaja(fecha);
  const ventasPorVendedor = useVentasPorVendedor();
  
  // Determine if any query is loading (excluding disabled admin-only queries)
  const isLoading = 
    ventasResumen.isLoading ||
    alertasStock.isLoading ||
    productosTop.isLoading ||
    flujoCaja.isLoading ||
    (ingresosGastos.isEnabled && ingresosGastos.isLoading) ||
    (ventasPorPago.isEnabled && ventasPorPago.isLoading) ||
    (ventasPorVendedor.isEnabled && ventasPorVendedor.isLoading);
  
  // Collect all errors
  const errors = [
    ventasResumen.error,
    alertasStock.error,
    productosTop.error,
    flujoCaja.error,
    ingresosGastos.isEnabled ? ingresosGastos.error : null,
    ventasPorPago.isEnabled ? ventasPorPago.error : null,
    ventasPorVendedor.isEnabled ? ventasPorVendedor.error : null,
  ].filter(Boolean) as Error[];
  
  const error = errors.length > 0 ? errors[0] : null;
  
  // Refetch all enabled queries
  const refetch = () => {
    ventasResumen.refetch();
    alertasStock.refetch();
    productosTop.refetch();
    flujoCaja.refetch();
    if (ingresosGastos.isEnabled) ingresosGastos.refetch();
    if (ventasPorPago.isEnabled) ventasPorPago.refetch();
    if (ventasPorVendedor.isEnabled) ventasPorVendedor.refetch();
  };
  
  return {
    // Data
    ventasResumen: ventasResumen.data,
    ingresosGastos: ingresosGastos.data,
    alertasStock: alertasStock.data ?? [],
    productosTop: productosTop.data ?? [],
    ventasPorPago: ventasPorPago.data ?? [],
    flujoCaja: flujoCaja.data,
    ventasPorVendedor: ventasPorVendedor.data ?? [],
    
    // Loading
    isLoading,
    isLoadingVentasResumen: ventasResumen.isLoading,
    isLoadingIngresosGastos: ingresosGastos.isLoading,
    isLoadingAlertasStock: alertasStock.isLoading,
    isLoadingProductosTop: productosTop.isLoading,
    isLoadingVentasPorPago: ventasPorPago.isLoading,
    isLoadingFlujoCaja: flujoCaja.isLoading,
    isLoadingVentasPorVendedor: ventasPorVendedor.isLoading,
    
    // Errors
    error,
    errorVentasResumen: ventasResumen.error,
    errorIngresosGastos: ingresosGastos.error,
    errorAlertasStock: alertasStock.error,
    errorProductosTop: productosTop.error,
    errorVentasPorPago: ventasPorPago.error,
    errorFlujoCaja: flujoCaja.error,
    errorVentasPorVendedor: ventasPorVendedor.error,
    
    // Refetch
    refetch,
    refetchVentasResumen: ventasResumen.refetch,
    refetchIngresosGastos: ingresosGastos.refetch,
    refetchAlertasStock: alertasStock.refetch,
    refetchProductosTop: productosTop.refetch,
    refetchVentasPorPago: ventasPorPago.refetch,
    refetchFlujoCaja: flujoCaja.refetch,
    refetchVentasPorVendedor: ventasPorVendedor.refetch,
    
    // Access control
    canViewAdminData: canAccessAdminData(),
    userRole: getUserRole(),
  };
}
