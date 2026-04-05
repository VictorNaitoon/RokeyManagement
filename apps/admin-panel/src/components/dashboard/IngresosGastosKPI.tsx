/**
 * IngresosGastos KPI - Income vs Expenses card
 * RoKey MANAGEMENT - Multi-tenant SaaS ERP/POS for locksmiths
 * 
 * Phase 3: KPI Components - Shows income vs expenses (Admin/Manager only)
 */

import { TrendingUp, TrendingDown } from 'lucide-react';
import { KPICard } from './KPICard';
import { useIngresosGastos } from '@/hooks/useDashboardData';

/**
 * Income vs Expenses KPI card
 * Displays: total ventas, total compras, ganancia bruta, margen %
 */
export function IngresosGastosKPI() {
  const { data, isLoading, error, isEnabled } = useIngresosGastos('mes');

  // Don't render if user doesn't have access
  if (!isEnabled) {
    return null;
  }

  // Format currency
  const formatCurrency = (value: number) => {
    return new Intl.NumberFormat('es-AR', {
      style: 'currency',
      currency: 'ARS',
      minimumFractionDigits: 0,
      maximumFractionDigits: 0,
    }).format(value);
  };

  // Calculate gain/loss variant
  const getVariant = () => {
    if (!data) return 'default';
    if (data.gananciaBruta > 0) return 'success';
    if (data.gananciaBruta < 0) return 'danger';
    return 'default';
  };

  return (
    <KPICard
      title="Ingresos vs Gastos"
      value={data ? formatCurrency(data.gananciaBruta) : '-'}
      subtitle={
        data
          ? `Ventas: ${formatCurrency(data.totalVentas)} • Compras: ${formatCurrency(data.totalCompras)}`
          : undefined
      }
      icon={data && data.gananciaBruta >= 0 ? <TrendingUp className="h-4 w-4" /> : <TrendingDown className="h-4 w-4" />}
      loading={isLoading}
      variant={error ? 'danger' : getVariant()}
      trend={
        data
          ? {
              value: Math.round(data.margen),
              label: 'margen',
            }
          : undefined
      }
    />
  );
}

export default IngresosGastosKPI;