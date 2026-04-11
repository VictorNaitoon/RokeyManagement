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
 * GET /api/v1/productos/{id}/movimientos
 */
export interface MovimientoStock {
  id: number;
  idProducto: number;
  idUsuario: number;
  usuarioNombre?: string;
  idVenta: number | null;
  idCompra: number | null;
  fecha: string;
  cantidad: number;
  tipoMovimiento: 'VentaSalida' | 'VentaAnulacion' | 'CompraEntrada' | 'CompraAnulacion' | 'AjusteManual';
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
  foto?: string;
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