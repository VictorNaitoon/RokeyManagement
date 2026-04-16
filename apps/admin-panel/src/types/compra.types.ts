/**
 * Compra Types - TypeScript interfaces for purchases/supplies
 * RoKey MANAGEMENT - Multi-tenant SaaS ERP/POS for locksmiths
 * 
 * These interfaces map to the API responses from /api/v1/compras endpoints
 */

// ============================================
// Main Entity Types
// ============================================

/**
 * Detalle de compra - línea de producto en la compra
 */
export interface DetalleCompra {
  id: number;
  idProducto: number;
  nombreProducto?: string;
  cantidad: number;
  precioUnitario: number;
}

/**
 * Compra entity - represents a purchase/supply order
 * GET /api/v1/compras, GET /api/v1/compras/{id}
 */
export interface Compra {
  id: number;
  numeroComprobante: string;
  idProveedor: number;
  nombreProveedor?: string;
  fechaCompra: string;
  totalGasto: number;
  anulada: boolean;
  motivoAnulacion?: string | null;
  detalles: DetalleCompra[];
}

/**
 * Response wrapper for GET /api/v1/compras
 */
export interface CompraListResponse {
  compras: Compra[];
  total: number;
}

// ============================================
// Request Types (for creating/updating)
// ============================================

/**
 * Request for creating a new purchase
 * POST /api/v1/compras
 */
export interface CrearCompraRequest {
  idProveedor: number;
  numeroComprobante: string;
  detalles: {
    idProducto: number;
    cantidad: number;
    precioUnitario: number;
  }[];
}

export interface ActualizarCompraRequest {
  numeroComprobante?: string;
  idProveedor?: number;
}

export interface CompraAnularRequest {
  motivo?: string;
}

/**
 * Request for cancelling a purchase
 * POST /api/v1/compras/{id}/anular
 */
export interface AnularCompraRequest {
  motivo: string;
}

// ============================================
// Filter Types
// ============================================

/**
 * Filter options for compras listing
 */
export interface CompraFilters {
  idProveedor?: number;
  fechaDesde?: string;
  fechaHasta?: string;
  anulada?: boolean;
}