/**
 * IngresosGastosChart - Bar chart comparing Ventas vs Compras
 * RoKey MANAGEMENT - Multi-tenant SaaS ERP/POS for locksmiths
 * 
 * Phase 4: Chart Components
 * Uses useIngresosGastos hook (Admin/Gerente only)
 */

import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
} from 'recharts';
import { Card, CardHeader, CardTitle, CardContent } from '@/components/ui/card';
import { Skeleton } from '@/components/ui/skeleton';
import { useIngresosGastos } from '@/hooks/useDashboardData';

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

interface IngresosGastosChartProps {
  fecha?: string;
}

/**
 * Bar chart showing income vs expenses comparison
 * Only visible to Admin/Gerente roles
 */
export function IngresosGastosChart({ fecha = 'mes' }: IngresosGastosChartProps) {
  const { data, isLoading, error, isEnabled } = useIngresosGastos(fecha);

  // If user doesn't have access, return null
  if (!isEnabled) {
    return null;
  }

  // Loading state
  if (isLoading) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>Ingresos vs Gastos</CardTitle>
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
          <CardTitle>Ingresos vs Gastos</CardTitle>
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
  if (!data) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>Ingresos vs Gastos</CardTitle>
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
  const chartData = [
    {
      name: 'Mes Actual',
      Ventas: data.totalVentas,
      Compras: data.totalCompras,
    },
  ];

  return (
    <Card>
      <CardHeader>
        <CardTitle>Ingresos vs Gastos</CardTitle>
      </CardHeader>
      <CardContent>
        <div className="h-[300px] w-full">
          <ResponsiveContainer width="100%" height="100%">
            <BarChart
              data={chartData}
              margin={{
                top: 20,
                right: 30,
                left: 20,
                bottom: 5,
              }}
            >
              <CartesianGrid strokeDasharray="3 3" className="stroke-muted" />
              <XAxis dataKey="name" className="text-xs fill-muted-foreground" />
              <YAxis 
                className="text-xs fill-muted-foreground"
                tickFormatter={(value) => formatCurrency(value)}
              />
              <Tooltip
                formatter={(value: any) => formatCurrency(value as number)}
                contentStyle={{
                  backgroundColor: 'var(--card)',
                  border: '1px solid var(--border)',
                  borderRadius: '8px',
                }}
              />
              <Legend />
              <Bar
                dataKey="Ventas"
                fill="#10b981"
                name="Ventas"
                radius={[4, 4, 0, 0]}
              />
              <Bar
                dataKey="Compras"
                fill="#ef4444"
                name="Compras"
                radius={[4, 4, 0, 0]}
              />
            </BarChart>
          </ResponsiveContainer>
        </div>
      </CardContent>
    </Card>
  );
}

export default IngresosGastosChart;
