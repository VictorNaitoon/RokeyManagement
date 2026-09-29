/**
 * UltimasVentas - Latest sales list for the Inicio page
 * RoKey MANAGEMENT - Multi-tenant SaaS ERP/POS for locksmiths
 *
 * Replaces the three recharts cards: an actionable list instead of decorative charts.
 * Uses GET /api/v1/ventas, which every role can read.
 */

import { ArrowUpRight, ReceiptText } from 'lucide-react';
import { Link } from 'react-router-dom';
import { Card, CardAction, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
import { useUltimasVentas } from '@/hooks/useDashboardData';
import { formatCurrency, formatHora } from '@/lib/format';

export function UltimasVentas() {
  const { data, isLoading, error } = useUltimasVentas(6);
  const ventas = data ?? [];

  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <ReceiptText className="h-4 w-4 text-primary" aria-hidden />
          <span>Últimas ventas</span>
        </CardTitle>
        <CardAction>
          <Link
            to="/ventas"
            className="inline-flex items-center gap-1 text-xs font-medium text-primary hover:underline"
          >
            Ver todas
            <ArrowUpRight className="h-3.5 w-3.5" aria-hidden />
          </Link>
        </CardAction>
      </CardHeader>

      <CardContent>
        {isLoading ? (
          <ul className="divide-y">
            {Array.from({ length: 4 }).map((_, i) => (
              <li key={i} className="flex items-center gap-3 py-3">
                <Skeleton className="h-4 w-12" />
                <Skeleton className="h-4 flex-1" />
                <Skeleton className="h-4 w-20" />
              </li>
            ))}
          </ul>
        ) : error ? (
          <p className="text-sm text-muted-foreground">
            No pudimos cargar las últimas ventas.
          </p>
        ) : ventas.length === 0 ? (
          <p className="py-4 text-sm text-muted-foreground">
            Todavía no se registraron ventas.
          </p>
        ) : (
          <ul className="divide-y">
            {ventas.map((venta) => {
              const anulada = venta.estado === 'Anulada';
              return (
                <li key={venta.id}>
                  <Link
                    to="/ventas"
                    className="flex items-center gap-3 py-3 transition-colors hover:bg-muted/50"
                  >
                    <span className="w-12 shrink-0 text-sm tabular-nums text-muted-foreground">
                      {formatHora(venta.fecha)}
                    </span>

                    <span className="min-w-0 flex-1">
                      <span className="block truncate text-sm font-medium text-foreground">
                        {venta.clienteNombre || 'Sin cliente identificado'}
                      </span>
                      {venta.usuarioNombre && (
                        <span className="block truncate text-xs text-muted-foreground">
                          Vendido por {venta.usuarioNombre}
                        </span>
                      )}
                    </span>

                    {anulada && <Badge variant="destructive">Anulada</Badge>}

                    <span
                      className={`shrink-0 text-sm font-semibold tabular-nums ${
                        anulada ? 'text-muted-foreground line-through' : 'text-foreground'
                      }`}
                    >
                      {formatCurrency(venta.totalVenta)}
                    </span>
                  </Link>
                </li>
              );
            })}
          </ul>
        )}
      </CardContent>
    </Card>
  );
}

export default UltimasVentas;
