/**
 * ResumenHoy - Today's KPI strip for the Inicio page
 * RoKey MANAGEMENT - Multi-tenant SaaS ERP/POS for locksmiths
 *
 * One card, four metrics, ALL of them scoped to today so the period is never ambiguous.
 * Only rendered for Dueño/Gerente: /api/v1/informes/* is restricted to those roles.
 */

import { ArrowUpRight, CalendarDays } from 'lucide-react';
import { Link } from 'react-router-dom';
import { format } from 'date-fns';
import { es } from 'date-fns/locale';
import { Card, CardAction, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Skeleton } from '@/components/ui/skeleton';
import { useIngresosGastos, useVentasResumen } from '@/hooks/useDashboardData';
import { formatCurrency } from '@/lib/format';

interface MetricCellProps {
  label: string;
  value: string;
  sub?: string;
  tone?: 'default' | 'positive' | 'negative';
  loading?: boolean;
}

function MetricCell({ label, value, sub, tone = 'default', loading = false }: MetricCellProps) {
  const valueTone = {
    default: 'text-foreground',
    positive: 'text-emerald-600 dark:text-emerald-400',
    negative: 'text-destructive',
  }[tone];

  if (loading) {
    return (
      <div className="bg-card p-4 space-y-2">
        <Skeleton className="h-3.5 w-24" />
        <Skeleton className="h-7 w-28" />
        <Skeleton className="h-3 w-20" />
      </div>
    );
  }

  return (
    <div className="bg-card p-4">
      <p className="text-xs font-medium text-muted-foreground">{label}</p>
      <p className={`mt-1 text-2xl font-semibold tracking-tight ${valueTone}`}>{value}</p>
      <p className="mt-1 text-xs text-muted-foreground">{sub ?? '\u00a0'}</p>
    </div>
  );
}

export function ResumenHoy() {
  const ventas = useVentasResumen('hoy');
  const ingresos = useIngresosGastos('hoy');

  const loading = ventas.isLoading || ingresos.isLoading;
  const error = ventas.error || ingresos.error;

  const ventasData = ventas.data;
  const ingresosData = ingresos.data;

  const ganancia = ingresosData?.gananciaBruta ?? 0;
  const gananciaTone: 'positive' | 'negative' | 'default' =
    ganancia > 0 ? 'positive' : ganancia < 0 ? 'negative' : 'default';

  const hoy = format(new Date(), "EEEE d 'de' MMMM", { locale: es });

  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <CalendarDays className="h-4 w-4 text-primary" aria-hidden />
          <span>Resumen de hoy</span>
          <span className="font-normal text-muted-foreground first-letter:uppercase">{hoy}</span>
        </CardTitle>
        <CardAction>
          <Link
            to="/informes"
            className="inline-flex items-center gap-1 text-xs font-medium text-primary hover:underline"
          >
            Ver informes
            <ArrowUpRight className="h-3.5 w-3.5" aria-hidden />
          </Link>
        </CardAction>
      </CardHeader>

      <CardContent>
        {error ? (
          <p className="py-2 text-sm text-muted-foreground">
            No pudimos cargar el resumen del día. Usá el botón «Actualizar» para reintentar.
          </p>
        ) : (
          <div className="grid grid-cols-1 gap-px overflow-hidden rounded-lg bg-border sm:grid-cols-2 lg:grid-cols-4">
            <MetricCell
              label="Ventas del día"
              value={ventasData ? formatCurrency(ventasData.totalVentas) : '-'}
              sub={
                ventasData
                  ? `${ventasData.cantidadVentas} ${ventasData.cantidadVentas === 1 ? 'venta' : 'ventas'}`
                  : undefined
              }
              loading={loading}
            />
            <MetricCell
              label="Ticket promedio"
              value={ventasData ? formatCurrency(ventasData.ticketPromedio) : '-'}
              sub="por venta"
              loading={loading}
            />
            <MetricCell
              label="Ganancia del día"
              value={ingresosData ? formatCurrency(ganancia) : '-'}
              sub={ingresosData ? `${Math.round(ingresosData.margen)}% de margen` : undefined}
              tone={gananciaTone}
              loading={loading}
            />
            <MetricCell
              label="Compras del día"
              value={ingresosData ? formatCurrency(ingresosData.totalCompras) : '-'}
              sub="cargadas a proveedores"
              loading={loading}
            />
          </div>
        )}
      </CardContent>
    </Card>
  );
}

export default ResumenHoy;
