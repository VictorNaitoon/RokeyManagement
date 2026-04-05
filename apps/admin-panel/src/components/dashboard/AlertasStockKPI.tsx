/**
 * AlertasStock KPI - Stock alerts card
 * RoKey MANAGEMENT - Multi-tenant SaaS ERP/POS for locksmiths
 * 
 * Phase 3: KPI Components - Shows stock alerts count
 */

import { AlertTriangle } from 'lucide-react';
import { KPICard } from './KPICard';
import { useAlertasStock } from '@/hooks/useDashboardData';

/**
 * Stock alerts KPI card
 * Displays: count of products below minimum stock
 * Shows red badge if there are alerts
 */
export function AlertasStockKPI() {
  const { data, isLoading, error } = useAlertasStock();

  const alertCount = data?.length ?? 0;
  const hasAlerts = alertCount > 0;

  // Determine variant based on alert count
  const getVariant = () => {
    if (error) return 'danger';
    if (hasAlerts) return 'warning';
    return 'default';
  };

  // Format subtitle with first few products
  const formatSubtitle = () => {
    if (!data || data.length === 0) return undefined;
    
    const productNames = data.slice(0, 3).map((p) => p.nombre).join(', ');
    const remaining = data.length > 3 ? ` +${data.length - 3} más` : '';
    
    return `${productNames}${remaining}`;
  };

  return (
    <KPICard
      title="Alertas de Stock"
      value={alertCount}
      subtitle={formatSubtitle()}
      icon={<AlertTriangle className="h-4 w-4" />}
      loading={isLoading}
      variant={getVariant()}
    />
  );
}

export default AlertasStockKPI;