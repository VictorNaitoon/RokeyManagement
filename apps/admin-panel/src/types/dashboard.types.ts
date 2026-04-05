/**
 * Dashboard Types - TypeScript interfaces matching backend DTOs
 * RoKey MANAGEMENT - Multi-tenant SaaS ERP/POS for locksmiths
 * 
 * These interfaces map to the API responses from /api/v1/informes/* endpoints
 */

// ============================================
// Response Types from Backend API
// ============================================

/**
 * GET /api/v1/informes/ventas-resumen
 * Daily sales summary
 */
export interface VentasResumen {
  totalVentas: number;
  cantidadVentas: number;
  ticketPromedio: number;
}

/**
 * GET /api/v1/informes/productos-top
 * Top selling products
 */
export interface ProductoTop {
  nombre: string;
  cantidadVendida: number;
  ingresoTotal: number;
}

export interface ProductosTopResponse {
  productos: ProductoTop[];
}

/**
 * GET /api/v1/informes/flujo-caja
 * Cash flow summary
 */
export interface FlujoCaja {
  ingresos: number;
  egresos: number;
  balance: number;
}

/**
 * GET /api/v1/informes/ingresos-gastos
 * Income vs Expenses (Admin/Manager only)
 */
export interface IngresosGastos {
  totalVentas: number;
  totalCompras: number;
  gananciaBruta: number;
  margen: number;
}

/**
 * GET /api/v1/informes/alertas-stock
 * Low stock alerts
 */
export interface AlertaStock {
  nombre: string;
  stockActual: number;
  stockMinimo: number;
}

export interface AlertasStockResponse {
  productos: AlertaStock[];
}

/**
 * GET /api/v1/informes/ventas-por-pago
 * Sales by payment method (Admin/Manager only)
 */
export interface VentasPorPago {
  metodo: string;
  total: number;
  cantidad: number;
}

export interface VentasPorPagoResponse {
  ventas: VentasPorPago[];
}

/**
 * GET /api/v1/informes/ventas-por-vendedor
 * Sales by seller (Admin/Manager only)
 */
export interface VentasPorVendedor {
  vendedor: string;
  total: number;
  cantidad: number;
}

export interface VentasPorVendedorResponse {
  ventas: VentasPorVendedor[];
}

// ============================================
// Combined Dashboard Data Type
// ============================================

/**
 * Combined dashboard data structure for convenience
 */
export interface DashboardData {
  ventasResumen: VentasResumen | null;
  productosTop: ProductoTop[];
  flujoCaja: FlujoCaja | null;
  ingresosGastos: IngresosGastos | null;
  alertasStock: AlertaStock[];
  ventasPorPago: VentasPorPago[];
  ventasPorVendedor: VentasPorVendedor[];
}

// ============================================
// Utility Types
// ============================================

/**
 * User roles for role-based access control
 */
export type UserRole = 'SuperAdmin' | 'Dueño' | 'Gerente' | 'Empleado';

/**
 * Payment method types
 */
export type MetodoPago = 'Efectivo' | 'TarjetaCrédito' | 'TarjetaDébito' | 'Transferencia';

/**
 * Date range options for dashboard queries
 */
export type DateRange = 'hoy' | 'semana' | 'mes' | 'año';
