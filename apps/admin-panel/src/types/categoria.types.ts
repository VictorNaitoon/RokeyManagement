/**
 * Categoria Types - TypeScript interfaces for categories
 * RoKey MANAGEMENT - Multi-tenant SaaS ERP/POS for locksmiths
 * 
 * These interfaces map to the API responses from /api/v1/categorias endpoints
 */

// ============================================
// Main Entity Types
// ============================================

/**
 * Categoria entity - represents a product category
 * GET /api/v1/categorias, GET /api/v1/categorias/{id}
 */
export interface Categoria {
  id: number;
  nombre: string;
  descripcion: string | null;
  activo: boolean;
}

/**
 * Response wrapper for GET /api/v1/categorias
 * GET /api/v1/categorias
 */
export interface CategoriaListResponse {
  categorias: Categoria[];
  total: number;
}

// ============================================
// Request Types (for creating/updating)
// ============================================

/**
 * Request for creating a new category
 * POST /api/v1/categorias
 */
export interface CrearCategoriaRequest {
  nombre: string;
  descripcion?: string;
}

/**
 * Request for updating an existing category
 * PUT /api/v1/categorias/{id}
 */
export interface ActualizarCategoriaRequest {
  nombre?: string;
  descripcion?: string;
  activo?: boolean;
}

// ============================================
// Utility Types
// ============================================

/**
 * Filter options for categories listing
 */
export interface CategoriaFilters {
  busqueda?: string;
  activo?: boolean;
}

/**
 * Options for pagination
 */
export interface CategoriaPaginationOptions {
  pagina?: number;
  tamanoPagina?: number;
  ordenarPor?: string;
  orden?: 'asc' | 'desc';
}