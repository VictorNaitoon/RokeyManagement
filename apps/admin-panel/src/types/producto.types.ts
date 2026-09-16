/**
 * Producto Types - TypeScript interfaces for products
 * RoKey MANAGEMENT - Multi-tenant SaaS ERP/POS for locksmiths
 * 
 * These interfaces map to the API responses from /api/v1/productos endpoints
 */

// ============================================
// Main Entity Types
// ============================================

/**
 * Producto entity - represents a product in inventory
 * GET /api/v1/productos, GET /api/v1/productos/{id}
 */
export interface Producto {
  id: number;
  nombre: string;
  codigoBusqueda?: string;
  descripcion?: string;
  precioVenta: number;
  precioCompra?: number | null;  // Only visible to Admin (Dueño, Gerente)
  stockActual: number;
  stockMinimo: number;
  stockBajo?: boolean;           // Computed: StockActual <= StockMinimo
  imagenURL?: string;
  esServicio: boolean;
  activo: boolean;
  idCategoria?: number;
  nombreCategoria?: string;
}

/**
 * Response wrapper for GET /api/v1/productos
 * GET /api/v1/productos
 */
export interface ProductoListResponse {
  productos: Producto[];
  total: number;
}

/**
 * Alerta de stock - low stock warning
 * GET /api/v1/productos/alertas
 */
export interface ProductoAlerta {
  id: number;
  nombre: string;
  stockActual: number;
  stockMinimo: number;
  categoriaNombre?: string;
}

export interface ProductoAlertasResponse {
  productos: ProductoAlerta[];
}

/**
 * Movimiento de stock - inventory movement audit
 * GET /api/v1/Producto/{id}/movimientos?page&pageSize — IA-01/IA-02
 * Paginated wrapper MovimientoStockListResponse { movimientos, total, page, pageSize }
 */
export interface MovimientoStock {
  id: number;
  fechaMovimiento: string;
  tipoMovimiento: 'VentaSalida' | 'VentaAnulacion' | 'CompraEntrada' | 'CompraAnulacion' | 'AjusteManual';
  cantidad: number;
  stockAnterior: number;
  stockNuevo: number;
  motivo?: string | null;
  idUsuario: number;
}

/**
 * Paginated wrapper for movimientos — IA-01
 * Backend: MovimientoStockListResponse { Movimientos, Total, Page, PageSize }
 * Frontend consumes as { movimientos, total, page, pageSize } (axios lowercases) or PascalCase.
 */
export interface MovimientoStockListResponse {
  movimientos: MovimientoStock[];
  total: number;
  page: number;
  pageSize: number;
  // PascalCase aliases for direct backend shape
  Movimientos?: MovimientoStock[];
  Total?: number;
  Page?: number;
  PageSize?: number;
}

// ============================================
// Request Types (for creating/updating)
// ============================================

/**
 * Request for creating a new product
 * POST /api/v1/productos
 */
export interface CrearProductoRequest {
  nombre: string;
  codigoBusqueda?: string;
  descripcion?: string;
  precioCompra?: number;
  precioVenta: number;
  stockActual: number;
  stockMinimo: number;
  idCategoria: number;
  esServicio: boolean;
  imagenURL?: string | null;
  activo?: boolean;
  // @deprecated alias kept for backwards compat — use imagenURL
  foto?: string;
}

/**
 * Request for updating an existing product
 * PUT /api/v1/productos/{id}
 */
export interface ActualizarProductoRequest {
  nombre?: string;
  codigoBusqueda?: string;
  descripcion?: string;
  precioCompra?: number;
  precioVenta?: number;
  stockActual?: number;
  stockMinimo?: number;
  idCategoria?: number;
  esServicio?: boolean;
  imagenURL?: string | null;
  activo?: boolean;
  // @deprecated alias kept for backwards compat — use imagenURL
  foto?: string;
}

// ============================================
// CSV Import types — CI-01/02/03
// ============================================

export interface ImportError {
  row: number;
  column: string;
  message: string;
  Row?: number;
  Column?: string;
  Message?: string;
}

export interface ImportCsvResponse {
  totalRows: number;
  created: number;
  skipped: number;
  errors: ImportError[];
  TotalRows?: number;
  Created?: number;
  Skipped?: number;
  Errors?: ImportError[];
}

// ============================================
// Utility Types
// ============================================

/**
 * Filter options for products listing
 */
export interface ProductoFilters {
 busqueda?: string;
  idCategoria?: number;
  activo?: boolean;
  esServicio?: boolean;
}

/**
 * Options for pagination
 */
export interface PaginationOptions {
  pagina?: number;
  tamanoPagina?: number;
  ordenarPor?: string;
  orden?: 'asc' | 'desc';
}