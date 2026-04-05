/**
 * ProductosTopChart - Horizontal bar chart showing top products by revenue
 * RoKey MANAGEMENT - Multi-tenant SaaS ERP/POS for locksmiths
 * 
 * Phase 4: Chart Components
 * Uses useProductosTop hook (all roles)
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
import { useProductosTop } from '@/hooks/useDashboardData';

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
 * Format number with thousands separator
 */
function formatNumber(value: number): string {
  return new Intl.NumberFormat('es-AR').format(value);
}

interface ProductosTopChartProps {
  limite?: number;
}

/**
 * Horizontal bar chart showing top selling products
 * Visible to all roles
 */
export function ProductosTopChart({ limite = 10 }: ProductosTopChartProps) {
  const { data, isLoading, error } = useProductosTop(limite);

  // Loading state
  if (isLoading) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>Productos Más Vendidos</CardTitle>
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
          <CardTitle>Productos Más Vendidos</CardTitle>
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
          <CardTitle>Productos Más Vendidos</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex flex-col items-center justify-center h-[300px] text-center text-muted-foreground">
            <p>Sin datos disponibles</p>
          </div>
        </CardContent>
      </Card>
    );
  }

  // Prepare chart data (reverse for top to show at top of chart)
  const chartData = [...data]
    .sort((a, b) => b.cantidadVendida - a.cantidadVendida)
    .slice(0, limite)
    .map((producto) => ({
      name: producto.nombre.length > 20 
        ? producto.nombre.substring(0, 20) + '...' 
        : producto.nombre,
      cantidad: producto.cantidadVendida,
      ingreso: producto.ingresoTotal,
    }));

  return (
    <Card>
      <CardHeader>
        <CardTitle>Productos Más Vendidos</CardTitle>
      </CardHeader>
      <CardContent>
        <div className="h-[300px] w-full">
          <ResponsiveContainer width="100%" height="100%">
            <BarChart
              layout="vertical"
              data={chartData}
              margin={{
                top: 20,
                right: 30,
                left: 100,
                bottom: 5,
              }}
            >
              <CartesianGrid strokeDasharray="3 3" className="stroke-muted" />
              <XAxis 
                type="number" 
                className="text-xs fill-muted-foreground"
                tickFormatter={formatNumber}
              />
              <YAxis 
                type="category" 
                dataKey="name" 
                className="text-xs fill-muted-foreground"
                width={90}
              />
              <Tooltip
                formatter={(value: any, name: any) => {
                  if (name === 'cantidad') {
                    return [formatNumber(value as number), 'Cantidad Vendida'];
                  }
                  return [formatCurrency(value as number), 'Ingreso Total'];
                }}
                contentStyle={{
                  backgroundColor: 'var(--card)',
                  border: '1px solid var(--border)',
                  borderRadius: '8px',
                }}
              />
              <Legend />
              <Bar
                dataKey="cantidad"
                fill="#3b82f6"
                name="Cantidad Vendida"
                radius={[0, 4, 4, 0]}
              />
            </BarChart>
          </ResponsiveContainer>
        </div>
      </CardContent>
    </Card>
  );
}

export default ProductosTopChart;
