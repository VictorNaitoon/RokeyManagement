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

// Clientes hooks (Phase 6)
export { useClientes, useCliente, useClienteSaldo, useClienteVentas, useCreateCliente, useUpdateCliente, useDeleteCliente, canManageClientes, canViewClientes } from './useClientes';

// Proveedores hooks (Phase 6)
export { useProveedores, useProveedor, useCreateProveedor, useUpdateProveedor, useDeleteProveedor, canManageProveedores } from './useProveedores';

// Presupuestos hooks (Phase 6)
export { usePresupuestos, usePresupuesto, useCreatePresupuesto, useUpdatePresupuestoEstado, useAnularPresupuesto, useConvertirPresupuesto, canManagePresupuestos, canConvertirPresupuesto } from './usePresupuestos';

// Compras hooks (Phase 6)
export { useCompras, useCompra, useCreateCompra, useAnularCompra, canManageCompras } from './useCompras';
