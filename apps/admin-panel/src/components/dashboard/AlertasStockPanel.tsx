/**
 * AlertasStockPanel - Actionable low-stock alert list for the Inicio page
 * RoKey MANAGEMENT - Multi-tenant SaaS ERP/POS for locksmiths
 *
 * Replaces the old count-only KPI: every row shows severity, stock vs minimum
 * and links to /productos/alertas.
 */

import { ArrowUpRight, CheckCircle2, PackageX, TriangleAlert } from 'lucide-react';
import { Link } from 'react-router-dom';
import { Card, CardAction, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
import { useAlertasStock } from '@/hooks/useDashboardData';
import type { AlertaStock } from '@/types';

const MAX_VISIBLE = 5;

type Severity = 'agotado' | 'critico' | 'bajo';

function getSeverity(item: AlertaStock): Severity {
  if (item.stockActual <= 0) return 'agotado';
  if (item.stockActual <= item.stockMinimo / 2) return 'critico';
  return 'bajo';
}

const SEVERITY = {
  agotado: {
    label: 'Agotado',
    badge: 'destructive' as const,
    bar: 'bg-destructive',
    row: 'border-destructive/30 bg-destructive/5 hover:bg-destructive/10',
    icon: 'text-destructive',
  },
  critico: {
    label: 'Crítico',
    badge: 'destructive' as const,
    bar: 'bg-orange-500',
    row: 'border-orange-500/30 bg-orange-500/5 hover:bg-orange-500/10',
    icon: 'text-orange-600 dark:text-orange-400',
  },
  bajo: {
    label: 'Bajo',
    badge: 'secondary' as const,
    bar: 'bg-amber-500',
    row: 'border-amber-500/30 bg-amber-500/5 hover:bg-amber-500/10',
    icon: 'text-amber-600 dark:text-amber-400',
  },
};

function stockPercentage(item: AlertaStock): number {
  if (item.stockMinimo <= 0) return 100;
  return Math.max(4, Math.min(100, (item.stockActual / item.stockMinimo) * 100));
}

function AlertRow({ item }: { item: AlertaStock }) {
  const severity = getSeverity(item);
  const style = SEVERITY[severity];

  return (
    <li>
      <Link
        to="/productos/alertas"
        className={`flex items-center gap-3 rounded-lg border px-3 py-2.5 transition-colors ${style.row}`}
      >
        <span className={style.icon} aria-hidden>
          {severity === 'agotado' ? (
            <PackageX className="h-4 w-4" />
          ) : (
            <TriangleAlert className="h-4 w-4" />
          )}
        </span>

        <span className="min-w-0 flex-1">
          <span className="flex items-center gap-2">
            <span className="truncate text-sm font-medium text-foreground">{item.nombre}</span>
            <Badge variant={style.badge}>{style.label}</Badge>
          </span>
          <span className="mt-1.5 flex items-center gap-2">
            <span className="h-1.5 flex-1 overflow-hidden rounded-full bg-background/70">
              <span
                className={`block h-full rounded-full ${style.bar}`}
                style={{ width: `${stockPercentage(item)}%` }}
              />
            </span>
            <span className="shrink-0 text-xs tabular-nums text-muted-foreground">
              {item.stockActual} / mín. {item.stockMinimo}
            </span>
          </span>
        </span>

        <ArrowUpRight className="h-4 w-4 shrink-0 text-muted-foreground" aria-hidden />
      </Link>
    </li>
  );
}

export function AlertasStockPanel() {
  const { data, isLoading, error } = useAlertasStock();
  const items = data ?? [];
  const visible = items.slice(0, MAX_VISIBLE);
  const restantes = items.length - visible.length;

  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <span>Alertas de stock</span>
          {items.length > 0 && (
            <Badge variant="destructive">{items.length}</Badge>
          )}
        </CardTitle>
        <CardAction>
          <Link
            to="/productos/alertas"
            className="inline-flex items-center gap-1 text-xs font-medium text-primary hover:underline"
          >
            Ver todas
            <ArrowUpRight className="h-3.5 w-3.5" aria-hidden />
          </Link>
        </CardAction>
      </CardHeader>

      <CardContent>
        {isLoading ? (
          <ul className="space-y-2">
            {Array.from({ length: 3 }).map((_, i) => (
              <li key={i} className="rounded-lg border p-3">
                <Skeleton className="h-4 w-1/2" />
                <Skeleton className="mt-2 h-1.5 w-full" />
              </li>
            ))}
          </ul>
        ) : error ? (
          <p className="text-sm text-muted-foreground">
            No pudimos cargar las alertas de stock.
          </p>
        ) : items.length === 0 ? (
          <div className="flex items-center gap-2 rounded-lg border border-emerald-500/30 bg-emerald-500/10 px-3 py-3 text-sm text-emerald-700 dark:text-emerald-400">
            <CheckCircle2 className="h-4 w-4 shrink-0" aria-hidden />
            Sin faltantes: todos los productos están por encima de su stock mínimo.
          </div>
        ) : (
          <>
            <ul className="space-y-2">
              {visible.map((item) => (
                <AlertRow key={item.nombre} item={item} />
              ))}
            </ul>
            {restantes > 0 && (
              <Link
                to="/productos/alertas"
                className="mt-3 block text-center text-xs font-medium text-primary hover:underline"
              >
                +{restantes} {restantes === 1 ? 'producto más' : 'productos más'}
              </Link>
            )}
          </>
        )}
      </CardContent>
    </Card>
  );
}

export default AlertasStockPanel;
