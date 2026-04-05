/**
 * Dashboard Components - Barrel export for all dashboard components
 * RoKey MANAGEMENT - Multi-tenant SaaS ERP/POS for locksmiths
 * 
 * Phase 4: Chart Components
 */

// KPI Components (Phase 3)
export { KPICard, type KPICardProps, type KPITrend } from './KPICard';
export { VentasKPI } from './VentasKPI';
export { IngresosGastosKPI } from './IngresosGastosKPI';
export { AlertasStockKPI } from './AlertasStockKPI';

// Chart Components (Phase 4)
export { IngresosGastosChart } from './IngresosGastosChart';
export { VentasPorPagoChart } from './VentasPorPagoChart';
export { ProductosTopChart } from './ProductosTopChart';

// State Components (Phase 5)
export { DashboardSkeleton } from './DashboardSkeleton';
export { DashboardError, type DashboardErrorProps } from './DashboardError';
