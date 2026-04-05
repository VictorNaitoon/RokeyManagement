/**
 * VentasPorPagoChart - Donut/Pie chart showing breakdown by payment method
 * RoKey MANAGEMENT - Multi-tenant SaaS ERP/POS for locksmiths
 * 
 * Phase 4: Chart Components
 * Uses useVentasPorPago hook (Admin/Gerente only)
 */

import {
  PieChart,
  Pie,
  Cell,
  Tooltip,
  Legend,
  ResponsiveContainer,
} from 'recharts';
import { Card, CardHeader, CardTitle, CardContent } from '@/components/ui/card';
import { Skeleton } from '@/components/ui/skeleton';
import { useVentasPorPago } from '@/hooks/useDashboardData';

/**
 * Format currency for display
 */
function formatCurrency(value: number): string {
  return new Intl.NumberFormat('es-AR', {
    style: 'currency',
    currency: 'ARS',
    minimumFractionDigits: 0,
    maximumFractionDigits: 0,
  }).format(value);
}

/**
 * Color mapping for payment methods
 */
const PAYMENT_COLORS: Record<string, string> = {
  'Efectivo': '#10b981',        // Emerald - green
  'Tarjeta': '#3b82f6',         // Blue
  'TarjetaCrédito': '#8b5cf6', // Violet
  'TarjetaDébito': '#06b6d4',   // Cyan
  'Transferencia': '#f59e0b',   // Amber
  'Otro': '#6b7280',            // Gray
};

/**
 * Get color for payment method
 */
function getPaymentColor(metodo: string): string {
  return PAYMENT_COLORS[metodo] || PAYMENT_COLORS['Otro'];
}

/**
 * Format payment method name for display
 */
function formatPaymentMethod(metodo: string): string {
  const methodMap: Record<string, string> = {
    'Efectivo': 'Efectivo',
    'Tarjeta': 'Tarjeta',
    'TarjetaCrédito': 'Tarjeta Crédito',
    'TarjetaDébito': 'Tarjeta Débito',
    'Transferencia': 'Transferencia',
    'Otro': 'Otro',
  };
  return methodMap[metodo] || metodo;
}

/**
 * Donut/Pie chart showing sales by payment method
 * Only visible to Admin/Gerente roles
 */
export function VentasPorPagoChart() {
  const { data, isLoading, error, isEnabled } = useVentasPorPago();

  // If user doesn't have access, return null
  if (!isEnabled) {
    return null;
  }

  // Loading state
  if (isLoading) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>Ventas por Método de Pago</CardTitle>
        </CardHeader>
        <CardContent>
          <Skeleton className="h-[300px] w-full" />
        </CardContent>
      </Card>
    );
  }

  // Error state
  if (error) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>Ventas por Método de Pago</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex flex-col items-center justify-center h-[300px] text-center">
            <p className="text-destructive text-sm mb-2">
              Error al cargar datos. Intente nuevamente.
            </p>
          </div>
        </CardContent>
      </Card>
    );
  }

  // Empty data state
  if (!data || data.length === 0) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>Ventas por Método de Pago</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex flex-col items-center justify-center h-[300px] text-center text-muted-foreground">
            <p>Sin datos disponibles</p>
          </div>
        </CardContent>
      </Card>
    );
  }

  // Prepare chart data
  const chartData = data.map((item) => ({
    name: formatPaymentMethod(item.metodo),
    value: item.total,
    cantidad: item.cantidad,
    color: getPaymentColor(item.metodo),
  }));

  // Calculate total for percentage
  const total = chartData.reduce((sum, item) => sum + item.value, 0);

  return (
    <Card>
      <CardHeader>
        <CardTitle>Ventas por Método de Pago</CardTitle>
      </CardHeader>
      <CardContent>
        <div className="h-[300px] w-full">
          <ResponsiveContainer width="100%" height="100%">
            <PieChart>
              <Pie
                data={chartData}
                cx="50%"
                cy="50%"
                innerRadius={60}
                outerRadius={100}
                paddingAngle={2}
                dataKey="value"
                nameKey="name"
              >
                {chartData.map((entry, index) => (
                  <Cell key={`cell-${index}`} fill={entry.color} />
                ))}
              </Pie>
              <Tooltip
                formatter={(value: any, name: any) => [
                  formatCurrency(value as number),
                  name as string,
                ]}
                contentStyle={{
                  backgroundColor: 'var(--card)',
                  border: '1px solid var(--border)',
                  borderRadius: '8px',
                }}
              />
              <Legend
                formatter={(value, entry) => {
                  const data = entry.payload as { value: number };
                  const percentage = total > 0 ? ((data?.value || 0) / total * 100).toFixed(1) : '0';
                  return `${value} (${percentage}%)`;
                }}
              />
            </PieChart>
          </ResponsiveContainer>
        </div>
      </CardContent>
    </Card>
  );
}

export default VentasPorPagoChart;
