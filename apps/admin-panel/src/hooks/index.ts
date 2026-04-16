/**
 * Hooks Barrel Export
 * RoKey MANAGEMENT - Multi-tenant SaaS ERP/POS for locksmiths
 */

// Dashboard hooks
export { useDashboardData, useVentasResumen, useIngresosGastos, useAlertasStock as useAlertasStockDashboard, useProductosTop, useVentasPorPago, useFlujoCaja, useVentasPorVendedor } from './useDashboardData';

// Productos hooks (renamed to avoid conflict)
export { useAlertasStock as useAlertasStockProductos, useProductos, useProducto, useCreateProducto, useUpdateProducto, useDeleteProducto, useReactivarProducto, canViewPrecioCompra, canManageProductos, canManageEstadoProductos } from './useProductos';

// Categorías hooks
export { useCategorias, useCategoria, useCreateCategoria, useUpdateCategoria, useDeleteCategoria, canManageCategorias } from './useCategorias';

// Ventas hooks
export { useVentas, useVenta, useVentaDetalles, useVentaPagos, useCreateVenta, useAnularVenta, canAnularVenta } from './useVentas';
