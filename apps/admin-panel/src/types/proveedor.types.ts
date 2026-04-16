/**
 * Proveedor Types - TypeScript interfaces for suppliers
 * RoKey MANAGEMENT - Multi-tenant SaaS ERP/POS for locksmiths
 * 
 * These interfaces map to the API responses from /api/v1/proveedores endpoints
 */

// ============================================
// Main Entity Types
// ============================================

/**
 * Proveedor entity - represents a supplier/vendor
 * GET /api/v1/proveedores, GET /api/v1/proveedores/{id}
 */
export interface Proveedor {
  id: number;
  nombre: string;
  telefono?: string | null;
  email?: string | null;
  fechaAlta: string;
  activo: boolean;
}

// ============================================
// Request Types (for creating/updating)
// ============================================

/**
 * Request for creating a new supplier
 * POST /api/v1/proveedores
 */
export interface CrearProveedorRequest {
  nombre: string;
  telefono?: string;
  email?: string;
}

/**
 * Request for updating an existing supplier
 * PUT /api/v1/proveedores/{id}
 */
export interface ActualizarProveedorRequest {
  nombre?: string;
  telefono?: string;
  email?: string;
}

// ============================================
// Filter Types
// ============================================

/**
 * Filter options for proveedores listing
 */
export interface ProveedorFilters {
  busqueda?: string;
  activo?: boolean;
}