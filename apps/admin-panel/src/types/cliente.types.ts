/**
 * Cliente Types - TypeScript interfaces for customers
 * RoKey MANAGEMENT - Multi-tenant SaaS ERP/POS for locksmiths
 * 
 * These interfaces map to the API responses from /api/v1/clientes endpoints
 */

// ============================================
// Main Entity Types
// ============================================

/**
 * Condición IVA del cliente
 */
export const CONDICION_IVA = {
  RESPONSABLE_INSCRIPTO: 'Responsable Inscripto',
  MONOTRIBUTO: 'Monotributista',
  CONSUMIDOR_FINAL: 'Consumidor Final',
  EXENTO: 'Exento',
} as const;

export type CondicionIVA = (typeof CONDICION_IVA)[keyof typeof CONDICION_IVA];

/**
 * Cliente entity - represents a customer/client
 * GET /api/v1/clientes, GET /api/v1/clientes/{id}
 */
export interface Cliente {
  id: number;
  nombre: string;
  apellido?: string | null;
  email: string;
  documento?: string | null;
  condicionIVA?: string | null;
  telefono?: string | null;
  direccion?: string | null;
  permiteFiado: boolean;
  limiteCredito?: number | null;
  activo: boolean;
  fechaAlta?: string;
}

/**
 * Cliente con cuenta corriente
 */
export interface ClienteCtaCte extends Cliente {
  saldoPendiente: number;
  totalVentas: number;
  cantidadVentas: number;
}

/**
 * Venta asociada a un cliente (para cuenta corriente)
 */
export interface VentaCliente {
  id: number;
  fecha: string;
  total: number;
  estado: string;
  cantidadItems: number;
}

/**
 * Pago asociado a una venta de cliente
 */
export interface PagoCliente {
  id: number;
  idVenta: number;
  metodoPago: string;
  monto: number;
  fecha: string;
}

/**
 * Saldo del cliente
 */
export interface SaldoCliente {
  clienteId: number;
  saldoPendiente: number;
  totalPurchases: number;
}

// ============================================
// Request Types (for creating/updating)
// ============================================

/**
 * Request for creating a new client
 * POST /api/v1/clientes
 */
export interface CrearClienteRequest {
  nombre: string;
  apellido?: string;
  email: string;
  documento?: string;
  condicionIVA?: string;
  telefono?: string;
  direccion?: string;
  permiteFiado?: boolean;
  limiteCredito?: number;
}

/**
 * Request for updating an existing client
 * PUT /api/v1/clientes/{id}
 */
export interface ActualizarClienteRequest {
  nombre?: string;
  apellido?: string;
  email?: string;
  documento?: string;
  condicionIVA?: string;
  telefono?: string;
  direccion?: string;
  permiteFiado?: boolean;
  limiteCredito?: number;
}

// ============================================
// Filter Types
// ============================================

/**
 * Filter options for clientes listing
 */
export interface ClienteFilters {
  busqueda?: string;
  activo?: boolean;
}