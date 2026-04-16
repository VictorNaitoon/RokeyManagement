/**
 * Presupuesto Types - TypeScript interfaces for budgets/quotes
 * RoKey MANAGEMENT - Multi-tenant SaaS ERP/POS for locksmiths
 * 
 * These interfaces map to the API responses from /api/v1/presupuestos endpoints
 */

// ============================================
// Main Entity Types
// ============================================

/**
 * Estado del presupuesto
 */
export const ESTADO_PRESUPUESTO = {
  PENDIENTE: 'Pendiente',
  ACEPTADO: 'Aceptado',
  VENCIDO: 'Vencido',
} as const;

export type EstadoPresupuesto = (typeof ESTADO_PRESUPUESTO)[keyof typeof ESTADO_PRESUPUESTO];

/**
 * Detalle de presupuesto - línea de producto en el presupuesto
 * GET /api/v1/presupuestos/{id}, POST /api/v1/presupuestos
 */
export interface DetallePresupuesto {
  id: number;
  idProducto: number;
  nombreProducto?: string;
  cantidad: number;
  precioPactado: number;
}

/**
 * Presupuesto entity - represents a budget/quote
 * GET /api/v1/presupuestos, GET /api/v1/presupuestos/{id}
 */
export interface Presupuesto {
  id: number;
  idUsuario: number;
  nombreUsuario?: string;
  idCliente: number | null;
  nombreCliente?: string;
  fechaEmision: string;
  fechaVencimiento: string;
  estado: EstadoPresupuesto;
  total: number;
  detalles: DetallePresupuesto[];
}

/**
 * Item en la lista de presupuestos (respuesta resumida)
 */
export interface PresupuestoListItem {
  id: number;
  nombreCliente?: string;
  fechaEmision: string;
  fechaVencimiento: string;
  estado: EstadoPresupuesto;
  total: number;
}

/**
 * Response wrapper for GET /api/v1/presupuestos
 */
export interface PresupuestoListResponse {
  items: PresupuestoListItem[];
  totalCount: number;
}

// ============================================
// Request Types (for creating/updating)
// ============================================

/**
 * Request for creating a new presupuesto
 * POST /api/v1/presupuestos
 */
export interface CrearPresupuestoRequest {
  idCliente: number | null;
  fechaVencimiento: string;
  detalles: {
    idProducto: number;
    cantidad: number;
    precioPactado: number;
  }[];
}

/**
 * Request for updating presupuesto estado
 * PUT /api/v1/presupuestos/{id}
 */
export interface UpdatePresupuestoRequest {
  estado: EstadoPresupuesto;
}

// ============================================
// Filter Types
// ============================================

/**
 * Filter options for presupuestos listing
 */
export interface PresupuestoFilters {
  estado?: EstadoPresupuesto;
  idCliente?: number;
}