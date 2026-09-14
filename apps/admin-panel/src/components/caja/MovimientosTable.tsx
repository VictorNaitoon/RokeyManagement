import * as React from 'react';
import {
  useReactTable,
  getCoreRowModel,
  getSortedRowModel,
  getFilteredRowModel,
} from '@tanstack/react-table';
import type {
  ColumnDef,
  SortingState,
  ColumnFiltersState,
} from '@tanstack/react-table';
import {
  Table,
  TableHeader,
  TableBody,
  TableRow,
  TableHead,
  TableCell,
} from '@/components/ui/table';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter,
  DialogClose,
} from '@/components/ui/dialog';
import { format } from 'date-fns';

export interface MovimientoListDTO {
  id: number;
  fecha: string;
  tipo: 'ingreso' | 'egreso';
  descripcion: string;
  monto: number;
  usuario_nombre: string;
}

export interface MovimientosTableProps {
  movimientos?: MovimientoListDTO[];
  loading?: boolean;
  refetch?: () => void;
  error?: string | null;
  skeleton?: boolean;
}

export function MovimientosTable({ movimientos = [], loading, refetch: _refetch, error, skeleton = false }: MovimientosTableProps) {
  const [sorting, setSorting] = React.useState<SortingState>([]);
  const [columnFilters, setColumnFilters] = React.useState<ColumnFiltersState>([]);
  const [selectedMovimiento, setSelectedMovimiento] = React.useState<MovimientoListDTO | null>(null);

  const columns: ColumnDef<MovimientoListDTO>[] = React.useMemo(() => [
    {
      accessorKey: 'fecha',
      header: 'Fecha',
      cell: ({ row }) => {
        const raw = row.getValue('fecha') as string;
        const fecha = new Date(raw);
        return Number.isNaN(fecha.getTime()) ? raw : format(fecha, 'dd/MM/yyyy HH:mm');
      },
    },
    {
      accessorKey: 'tipo',
      header: 'Tipo',
      cell: ({ row }) => {
        const tipo = row.getValue('tipo') as string;
        const badgeClass = tipo === 'ingreso'
          ? 'bg-green-100 text-green-800'
          : 'bg-red-100 text-red-800';
        return (
          <Badge className={badgeClass} variant="default">
            {tipo === 'ingreso' ? 'Ingreso' : 'Egreso'}
          </Badge>
        );
      },
    },
    {
      accessorKey: 'descripcion',
      header: 'Descripción',
      cell: ({ row }) => {
        const descripcion = row.getValue('descripcion') as string;
        const truncated = descripcion.length > 50
          ? `${descripcion.substring(0, 50)}...`
          : descripcion;
        return <span title={descripcion}>{truncated}</span>;
      },
    },
    {
      accessorKey: 'monto',
      header: 'Monto',
      cell: ({ row }) => {
        const monto = row.getValue('monto') as number;
        return new Intl.NumberFormat('es-AR', {
          style: 'currency',
          currency: 'ARS',
        }).format(monto);
      },
    },
    {
      accessorKey: 'usuario_nombre',
      header: 'Usuario',
      cell: ({ row }) => (row.getValue('usuario_nombre') as string) || '—',
    },
    {
      id: 'acciones',
      header: 'Acciones',
      cell: ({ row }) => {
        return (
          <div className="flex gap-1">
            <button
              type="button"
              className="text-sm text-primary hover:underline"
              onClick={() => setSelectedMovimiento(row.original)}
            >
              Ver detalles
            </button>
          </div>
        );
      },
    },
  ], []);

  const table = useReactTable({
    data: movimientos,
    columns,
    state: {
      sorting,
      columnFilters,
    },
    onSortingChange: setSorting,
    onColumnFiltersChange: setColumnFilters,
    getCoreRowModel: getCoreRowModel(),
    getSortedRowModel: getSortedRowModel(),
    getFilteredRowModel: getFilteredRowModel(),
  });

  if (skeleton) {
    return (
      <div className="rounded-md border p-4 space-y-2">
        <Skeleton className="h-6 w-full" />
        <Skeleton className="h-6 w-full" />
        <Skeleton className="h-6 w-full" />
      </div>
    );
  }

  if (error) {
    return (
      <div className="p-8">
        <p className="text-destructive text-center">
          {error}
        </p>
      </div>
    );
  }

  if (!movimientos || movimientos.length === 0) {
    return (
      <div className="p-8">
        <p className="text-muted-foreground text-center">
          No hay movimientos registrados
        </p>
      </div>
    );
  }

  return (
    <>
      <div className="rounded-md border">
        <Table>
          <TableHeader>
            <TableRow>
              {columns.map((col) => {
                const key = (col as { accessorKey?: string; id?: string }).accessorKey ?? (col as { id?: string }).id ?? '';
                const header = typeof col.header === 'string' ? col.header : key;
                return (
                  <TableHead key={key}>
                    {header as string}
                  </TableHead>
                );
              })}
            </TableRow>
          </TableHeader>
          <TableBody>
            {loading ? (
              <TableRow>
                <TableCell colSpan={columns.length} className="h-24 text-center text-muted-foreground">
                  Cargando...
                </TableCell>
              </TableRow>
            ) : table.getRowModel().rows?.length ? table.getRowModel().rows.map((row) => (
              <TableRow key={row.original.id} className="hover:bg-muted/50">
                {row.getVisibleCells().map((cell) => (
                  <TableCell key={cell.id} className="p-2 align-middle whitespace-nowrap">
                    {cell.column.columnDef.cell
                      ? (cell.column.columnDef.cell as unknown as (props: { row: typeof row; getValue: () => unknown }) => React.ReactNode)({ row, getValue: () => cell.getValue() } as never)
                      : String(cell.getValue() ?? '')}
                  </TableCell>
                ))}
              </TableRow>
            )) : (
              <TableRow>
                <TableCell colSpan={columns.length} className="h-24 text-center text-muted-foreground">
                  No hay movimientos registrados
                </TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      </div>

      <Dialog open={!!selectedMovimiento} onOpenChange={(open) => !open && setSelectedMovimiento(null)}>
        <DialogContent className="sm:max-w-md">
          <DialogHeader>
            <DialogTitle>Detalle del movimiento</DialogTitle>
            <DialogDescription>Movimiento #{selectedMovimiento?.id}</DialogDescription>
          </DialogHeader>
          {selectedMovimiento && (
            <div className="space-y-3 py-2 text-sm">
              <div className="flex justify-between">
                <span className="text-muted-foreground">Fecha</span>
                <span className="font-medium">
                  {(() => {
                    const d = new Date(selectedMovimiento.fecha);
                    return Number.isNaN(d.getTime()) ? selectedMovimiento.fecha : format(d, 'dd/MM/yyyy HH:mm');
                  })()}
                </span>
              </div>
              <div className="flex justify-between items-center">
                <span className="text-muted-foreground">Tipo</span>
                <Badge className={selectedMovimiento.tipo === 'ingreso' ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'} variant="default">
                  {selectedMovimiento.tipo === 'ingreso' ? 'Ingreso' : 'Egreso'}
                </Badge>
              </div>
              <div className="flex justify-between">
                <span className="text-muted-foreground">Monto</span>
                <span className="font-medium">
                  {new Intl.NumberFormat('es-AR', { style: 'currency', currency: 'ARS' }).format(selectedMovimiento.monto)}
                </span>
              </div>
              <div className="flex flex-col gap-1">
                <span className="text-muted-foreground">Descripción</span>
                <span className="font-medium break-words">{selectedMovimiento.descripcion || '—'}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-muted-foreground">Usuario</span>
                <span className="font-medium">{selectedMovimiento.usuario_nombre || '—'}</span>
              </div>
            </div>
          )}
          <DialogFooter>
            <DialogClose render={<Button variant="outline" />}>Cerrar</DialogClose>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </>
  );
}
