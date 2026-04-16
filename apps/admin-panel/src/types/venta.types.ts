/**
 * Venta Types - TypeScript interfaces for sales (ventas)
 * RoKey MANAGEMENT - Multi-tenant SaaS ERP/POS for locksmiths
 * 
 * These interfaces map to the API responses from /api/v1/ventas endpoints
 */

// ============================================
// Main Entity Types
// ============================================

/**
 * Venta entity - represents a sale
 * GET /api/v1/ventas, GET /api/v1/ventas/{id}
 */
export interface Venta {
  id: number;
  fecha: string;
  totalVenta: number;
  estado: 'Activa' | 'Anulada';
  idUsuario: number;
  usuarioNombre?: string;
  idCliente?: number | null;
  clienteNombre?: string;
  anulacionFecha?: string | null;
  anulacionUsuarioId?: number | null;
  anulacionUsuarioNombre?: string | null;
  motivoAnulacion?: string | null;
}

/**
 * Response for GET /api/v1/ventas
 */
export interface VentaListResponse {
  ventas: Venta[];
  total: number;
}

/**
 * Detalle de venta - sale detail line item
 * GET /api/v1/ventas/{id}/detalles
 */
export interface DetalleVenta {
  id: number;
  idVenta: number;
  idProducto: number;
  productoNombre: string;
  productoCodigo?: string;
  cantidadVendida: number;
  precioUnitario: number;
  subtotal: number;
}

/**
 * Pago - payment record
 * GET /api/v1/ventas/{id}/pagos
 */
export interface Pago {
  id: number;
  idVenta: number;
  metodoPago: 'Efectivo' | 'TarjetaCredito' | 'TarjetaDebito' | 'Transferencia';
  monto: number;
}

// ============================================
// Request Types
// ============================================

/**
 * Request for creating a new sale
 * POST /api/v1/ventas
 */
export interface CrearVentaRequest {
  idCliente?: number | null;
  detalles: CrearVentaDetalleRequest[];
  pagos: CrearVentaPagoRequest[];
}

export interface CrearVentaDetalleRequest {
  idProducto: number;
  cantidad: number;
  precioUnitario: number;
}

export interface CrearVentaPagoRequest {
  metodoPago: 'Efectivo' | 'TarjetaCredito' | 'TarjetaDebito' | 'Transferencia';
  monto: number;
}

/**
 * Request for canceling (anulating) a sale
 * POST /api/v1/ventas/{id}/anular
 */
export interface AnularVentaRequest {
  motivo: string;
}

// ============================================
// Filter Types
// ============================================

/**
 * Filter options for ventas listing
 */
export interface VentaFilters {
  fechaDesde?: string;
  fechaHasta?: string;
}

// ============================================
// Modal Types
// ============================================

/**
 * Full venta with details and payments
 */
export interface VentaCompleta {
  venta: Venta;
  detalles: DetalleVenta[];
  pagos: Pago[];
}
