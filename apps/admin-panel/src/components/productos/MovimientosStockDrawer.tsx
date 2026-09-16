import * as React from 'react';
import { useMovimientosStock } from '@/hooks/useProductos';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
} from '@/components/ui/dialog';
import {
  Table,
  TableHeader,
  TableBody,
  TableHead,
  TableRow,
  TableCell,
} from '@/components/ui/table';

export interface MovimientosStockDrawerProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  productoId: number | null;
  productoNombre?: string;
}

const TIPO_COLOR_MAP: Record<string, string> = {
  VentaSalida: 'bg-amber-500 text-white',
  VentaAnulacion: 'bg-green-600 text-white',
  CompraEntrada: 'bg-blue-600 text-white',
  CompraAnulacion: 'bg-red-600 text-white',
  AjusteManual: 'bg-violet-600 text-white',
};

function formatFecha(iso: string): string {
  try {
    return new Date(iso).toLocaleString('es-AR', {
      dateStyle: 'short',
      timeStyle: 'short',
    });
  } catch {
    return iso;
  }
}

export function MovimientosStockDrawer({
  open,
  onOpenChange,
  productoId,
  productoNombre,
}: MovimientosStockDrawerProps) {
  const [page, setPage] = React.useState(1);
  const [pageSize, setPageSize] = React.useState(10);

  React.useEffect(() => {
    if (open) {
      setPage(1);
    }
  }, [open, productoId]);

  const { data, isLoading, isError } = useMovimientosStock(productoId, page, pageSize);

  const movimientos = data?.movimientos ?? [];
  const total = data?.total ?? 0;
  const totalPages = Math.max(1, Math.ceil(total / pageSize));

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-4xl max-h-[85vh] overflow-hidden flex flex-col">
        <DialogHeader>
          <DialogTitle>Historial de movimientos</DialogTitle>
          <DialogDescription>
            {productoNombre ? `Producto: ${productoNombre}` : productoId ? `Producto #${productoId}` : ''}
            {total > 0 ? ` — ${total} movimiento${total !== 1 ? 's' : ''}` : ''}
          </DialogDescription>
        </DialogHeader>

        <div className="flex items-center gap-2 py-2">
          <span className="text-sm text-muted-foreground">Filas por pagina:</span>
          <select
            value={pageSize}
            onChange={(e) => {
              setPageSize(Number(e.target.value));
              setPage(1);
            }}
            className="h-8 rounded border px-2 text-sm"
          >
            <option value={10}>10</option>
            <option value={20}>20</option>
            <option value={50}>50</option>
          </select>
          <span className="ml-auto text-sm text-muted-foreground">
            Pagina {page} de {totalPages}
          </span>
        </div>

        <div className="flex-1 overflow-auto rounded border">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Fecha</TableHead>
                <TableHead>Tipo</TableHead>
                <TableHead className="text-right">Cantidad</TableHead>
                <TableHead>StockAnterior → Nuevo</TableHead>
                <TableHead>Motivo</TableHead>
                <TableHead>Usuario</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {isLoading ? (
                <TableRow>
                  <TableCell colSpan={6} className="text-center py-8 text-muted-foreground">
                    Cargando movimientos...
                  </TableCell>
                </TableRow>
              ) : isError ? (
                <TableRow>
                  <TableCell colSpan={6} className="text-center py-8 text-destructive">
                    Error al cargar el historial
                  </TableCell>
                </TableRow>
              ) : movimientos.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={6} className="text-center py-8 text-muted-foreground">
                    Sin movimientos para este producto
                  </TableCell>
                </TableRow>
              ) : (
                movimientos.map((m) => (
                  <TableRow key={m.id}>
                    <TableCell className="whitespace-nowrap text-xs">{formatFecha(m.fechaMovimiento)}</TableCell>
                    <TableCell>
                      <Badge className={TIPO_COLOR_MAP[m.tipoMovimiento] ?? ''} variant="secondary">
                        {m.tipoMovimiento}
                      </Badge>
                    </TableCell>
                    <TableCell className="text-right font-mono">
                      <span className={m.cantidad > 0 ? 'text-green-600' : m.cantidad < 0 ? 'text-red-600' : ''}>
                        {m.cantidad > 0 ? `+${m.cantidad}` : m.cantidad}
                      </span>
                    </TableCell>
                    <TableCell className="font-mono text-xs">
                      {m.stockAnterior} → {m.stockNuevo}
                    </TableCell>
                    <TableCell className="max-w-[220px] truncate text-xs" title={m.motivo ?? ''}>
                      {m.motivo || '-'}
                    </TableCell>
                    <TableCell className="text-xs">#{m.idUsuario}</TableCell>
                  </TableRow>
                ))
              )}
            </TableBody>
          </Table>
        </div>

        <div className="flex items-center justify-between pt-2">
          <Button variant="outline" size="sm" disabled={page <= 1} onClick={() => setPage((p) => Math.max(1, p - 1))}>
            Anterior
          </Button>
          <span className="text-sm text-muted-foreground">
            {total === 0 ? '0 resultados' : `${(page - 1) * pageSize + 1}-${Math.min(page * pageSize, total)} de ${total}`}
          </span>
          <Button
            variant="outline"
            size="sm"
            disabled={page >= totalPages}
            onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
          >
            Siguiente
          </Button>
        </div>
      </DialogContent>
    </Dialog>
  );
}

export default MovimientosStockDrawer;
