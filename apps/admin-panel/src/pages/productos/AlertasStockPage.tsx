/**
 * AlertasStockPage - Low stock alerts page
 * RoKey MANAGEMENT - Multi-tenant SaaS ERP/POS for locksmiths
 * 
 * Phase 6: Pages - Stock alerts listing
 */

import * as React from 'react';
import type { ColumnDef } from '@tanstack/react-table';
import { AlertTriangle, Package, ArrowRight } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { DataTable } from '@/components/ui/DataTable';
import { useAlertasStockProductos } from '@/hooks';
import type { ProductoAlerta } from '@/types';

export function AlertasStockPage() {
  // State
  const [search, setSearch] = React.useState('');
  const [pageIndex, setPageIndex] = React.useState(0);
  const [pageSize, setPageSize] = React.useState(20);

  // Queries
  const { data: alertas, isLoading, refetch } = useAlertasStockProductos();

  // Filter by search
  const filteredAlertas = React.useMemo(() => {
    if (!search) return alertas ?? [];
    const lower = search.toLowerCase();
    return alertas?.filter(
      (a: ProductoAlerta) =>
        a.nombre.toLowerCase().includes(lower) ||
        a.categoriaNombre?.toLowerCase().includes(lower)
    ) ?? [];
  }, [alertas, search]);

  // Pagination
  const paginatedAlertas = React.useMemo(() => {
    const start = pageIndex * pageSize;
    return filteredAlertas.slice(start, start + pageSize);
  }, [filteredAlertas, pageIndex, pageSize]);

  const pageCount = Math.ceil(filteredAlertas.length / pageSize);

  // Column definitions
  const columns: ColumnDef<ProductoAlerta>[] = React.useMemo(() => [
    {
      accessorKey: 'nombre',
      header: 'Producto',
      cell: ({ row }) => (
        <div className="flex items-center gap-2">
          <div className="w-8 h-8 rounded bg-muted flex items-center justify-center">
            <Package className="h-4 w-4 text-muted-foreground" />
          </div>
          <div>
            <div className="font-medium">{row.original.nombre}</div>
          </div>
        </div>
      ),
    },
    {
      accessorKey: 'categoriaNombre',
      header: 'Categoría',
      cell: ({ getValue }) => {
        const value = getValue() as string | undefined;
        return value ? <Badge variant="outline">{value}</Badge> : '-';
      },
    },
    {
      accessorKey: 'stockActual',
      header: 'Stock Actual',
      cell: ({ row }) => {
        const stock = row.original.stockActual;
        const min = row.original.stockMinimo;
        const porcentaje = Math.round((stock / min) * 100);
        
        return (
          <div className="flex items-center gap-2">
            <span className="font-medium text-destructive">{stock}</span>
            <Badge 
              variant={porcentaje <= 50 ? 'destructive' : 'outline'}
              className="text-xs"
            >
              {porcentaje}% del mínimo
            </Badge>
          </div>
        );
      },
    },
    {
      accessorKey: 'stockMinimo',
      header: 'Stock Mínimo',
      cell: ({ getValue }) => {
        const value = getValue() as number;
        return value;
      },
    },
    {
      accessorKey: 'diferencia',
      header: 'Faltante',
      cell: ({ row }) => {
        const stock = row.original.stockActual;
        const min = row.original.stockMinimo;
        const faltante = Math.max(0, min - stock);
        return faltante > 0 ? (
          <span className="text-destructive font-medium">-{faltante}</span>
        ) : (
          <span className="text-muted-foreground">-</span>
        );
      },
    },
  ], []);

  // Handlers
  const handleSearch = (value: string) => {
    setSearch(value);
    setPageIndex(0);
  };

  const handlePageChange = (newPageIndex: number, newPageSize: number) => {
    setPageIndex(newPageIndex);
    setPageSize(newPageSize);
  };

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold text-foreground">Alertas de Stock</h1>
          <p className="text-muted-foreground">
            Productos con stock bajo el nivel mínimo
          </p>
        </div>
        
        <Button variant="outline" onClick={() => refetch()} className="gap-2">
          <ArrowRight className="h-4 w-4" />
          Actualizar
        </Button>
      </div>

      {/* Alert Summary */}
      {(alertas?.length ?? 0) > 0 && (
        <div className="flex items-center gap-2 p-4 rounded-lg bg-destructive/10 border border-destructive/20">
          <AlertTriangle className="h-5 w-5 text-destructive" />
          <span className="text-sm text-destructive">
            <strong>{alertas?.length}</strong> {alertas?.length === 1 ? 'producto requiere' : 'productos requieren'} atención
          </span>
        </div>
      )}

      {/* Alerts Table */}
      <DataTable
        columns={columns}
        data={paginatedAlertas}
        pageCount={pageCount}
        pageIndex={pageIndex}
        pageSize={pageSize}
        onPageChange={handlePageChange}
        onSearch={handleSearch}
        searchPlaceholder="Buscar alertas..."
        searchValue={search}
        showSearch
        showPagination
        loading={isLoading}
        emptyMessage="No hay alertas de stock"
      />
    </div>
  );
}

export default AlertasStockPage;