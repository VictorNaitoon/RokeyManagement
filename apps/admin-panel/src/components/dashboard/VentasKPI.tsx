/**
 * Ventas KPI - Daily sales summary card
 * RoKey MANAGEMENT - Multi-tenant SaaS ERP/POS for locksmiths
 * 
 * Phase 3: KPI Components - Shows daily sales summary
 */

import { DollarSign } from 'lucide-react';
import { KPICard } from './KPICard';
import { useVentasResumen } from '@/hooks/useDashboardData';

/**
 * Daily sales summary KPI card
 * Displays: total ventas, cantidad ventas, ticket promedio
 */
export function VentasKPI() {
  const { data, isLoading, error } = useVentasResumen('hoy');

  // Format currency
  const formatCurrency = (value: number) => {
    return new Intl.NumberFormat('es-AR', {
      style: 'currency',
      currency: 'ARS',
      minimumFractionDigits: 0,
      maximumFractionDigits: 0,
    }).format(value);
  };

  return (
    <KPICard
      title="Ventas del día"
      value={data ? formatCurrency(data.totalVentas) : '-'}
      subtitle={
        data
          ? `${data.cantidadVentas} ventas • Ticket promedio: ${formatCurrency(data.ticketPromedio)}`
          : undefined
      }
      icon={<DollarSign className="h-4 w-4" />}
      loading={isLoading}
      variant={error ? 'danger' : 'default'}
    />
  );
}

export default VentasKPI;