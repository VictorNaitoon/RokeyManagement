/**
 * VentasPage - Sales management page with DataTable
 * RoKey MANAGEMENT - Multi-tenant SaaS ERP/POS for locksmiths
 * 
 * Phase 3: UI - Listado de ventas
 */

import * as React from 'react';
import { useNavigate } from 'react-router-dom';
import type { ColumnDef } from '@tanstack/react-table';
import { Eye, Ban, DollarSign, Plus } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { DataTable } from '@/components/ui/DataTable';
import { VentasDetallesModal } from '@/components/ventas/VentasDetallesModal';
import { VentaAnularModal } from '@/components/ventas/VentaAnularModal';
import { useVentas, useAnularVenta, canAnularVenta } from '@/hooks';
import type { Venta, VentaFilters } from '@/types';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';

export function VentasPage() {
  const navigate = useNavigate();
  
  // State
  const [filters, setFilters] = React.useState<VentaFilters>({
    fechaDesde: '',
    fechaHasta: '',
  });
  const [search, setSearch] = React.useState('');
  const [pageIndex, setPageIndex] = React.useState(0);
  const [pageSize, setPageSize] = React.useState(20);
  
  // Modal states
  const [selectedVenta, setSelectedVenta] = React.useState<Venta | null>(null);
  const [showDetallesModal, setShowDetallesModal] = React.useState(false);
  const [anularVenta, setAnularVenta] = React.useState<Venta | null>(null);

  // Permissions
  const canAnular = canAnularVenta();

  // Queries
  const { data, isLoading, refetch } = useVentas({ filters });

  // Mutations
  const anularMutation = useAnularVenta();

  // Handle filter changes
  const handleFilterChange = (field: keyof VentaFilters, value: string) => {
    setFilters(prev => ({ ...prev, [field]: value }));
  };

  const handleApplyFilters = () => {
    refetch();
    setPageIndex(0);
  };

  const handleClearFilters = () => {
    setFilters({ fechaDesde: '', fechaHasta: '' });
    setSearch('');
    setPageIndex(0);
  };

  // Data - Filter client-side since backend doesn't support search
  const allVentas = data?.ventas ?? [];
  const filteredVentas = React.useMemo(() => {
    let result = allVentas;
    
    // Client-side search
    if (search) {
      const searchLower = search.toLowerCase();
      result = result.filter(v => 
        v.id.toString().includes(searchLower) ||
        (v.clienteNombre?.toLowerCase().includes(searchLower) ?? false) ||
        (v.usuarioNombre?.toLowerCase().includes(searchLower) ?? false)
      );
    }
    
    return result;
  }, [allVentas, search]);
  
  const total = filteredVentas.length;
  const pageCount = Math.ceil(total / pageSize);
  const paginatedVentas = filteredVentas.slice(
    pageIndex * pageSize,
    (pageIndex + 1) * pageSize
  );

  // Column definitions
  const columns: ColumnDef<Venta>[] = React.useMemo(() => [
    {
      accessorKey: 'id',
      header: 'N° Venta',
      cell: ({ getValue }) => {
        const value = getValue() as number;
        return <span className="font-medium">#{value}</span>;
      },
    },
    {
      accessorKey: 'fecha',
      header: 'Fecha',
      cell: ({ getValue }) => {
        const value = getValue() as string;
        const date = new Date(value);
        return (
          <div>
            <div className="font-medium">
              {date.toLocaleDateString('es-AR', {
                day: '2-digit',
                month: '2-digit',
                year: 'numeric',
              })}
            </div>
            <div className="text-xs text-muted-foreground">
              {date.toLocaleTimeString('es-AR', {
                hour: '2-digit',
                minute: '2-digit',
              })}
            </div>
          </div>
        );
      },
    },
    {
      accessorKey: 'usuarioNombre',
      header: 'Vendedor',
      cell: ({ getValue }) => {
        const value = getValue() as string | undefined;
        return value || '-';
      },
    },
    {
      accessorKey: 'clienteNombre',
      header: 'Cliente',
      cell: ({ getValue }) => {
        const value = getValue() as string | undefined;
        return value || <span className="text-muted-foreground">Consumidor Final</span>;
      },
    },
    {
      accessorKey: 'totalVenta',
      header: 'Total',
      cell: ({ getValue }) => {
        const value = getValue() as number;
        return (
          <span className="font-medium text-green-600">
            ${value.toFixed(2)}
          </span>
        );
      },
    },
    {
      accessorKey: 'estado',
      header: 'Estado',
      cell: ({ getValue }) => {
        const estado = getValue() as string;
        return estado === 'Anulada' ? (
          <Badge variant="destructive">Anulada</Badge>
        ) : (
          <Badge variant="default">Activa</Badge>
        );
      },
    },
    {
      id: 'acciones',
      header: '',
      cell: ({ row }) => (
        <div className="flex items-center gap-2">
          <Button
            variant="ghost"
            size="icon"
            onClick={() => handleViewDetalles(row.original)}
            title="Ver detalles"
          >
            <Eye className="h-4 w-4" />
          </Button>
          {canAnular && row.original.estado === 'Activa' && (
            <Button
              variant="ghost"
              size="icon"
              onClick={() => setAnularVenta(row.original)}
              title="Anular venta"
              className="text-destructive hover:text-destructive"
            >
              <Ban className="h-4 w-4" />
            </Button>
          )}
        </div>
      );
    },
  ], [canAnular]);

  // Handlers
  const handleSearch = (value: string) => {
    setSearch(value);
    setPageIndex(0);
  };

  const handlePageChange = (newPageIndex: number, newPageSize: number) => {
    setPageIndex(newPageIndex);
    setPageSize(newPageSize);
  };

  const handleViewDetalles = (venta: Venta) => {
    setSelectedVenta(venta);
    setShowDetallesModal(true);
  };

  const handleAnular = async (motivo: string) => {
    if (!anularVenta) return;
    await anularMutation.mutateAsync({
      id: anularVenta.id,
      motivo,
    });
    setAnularVenta(null);
  };

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold text-foreground">Ventas</h1>
          <p className="text-muted-foreground">
            Gestiona las ventas del negocio
          </p>
        </div>
        
        <Button onClick={() => navigate('/ventas/nueva')} className="gap-2">
          <Plus className="h-4 w-4" />
          Nueva Venta
        </Button>
      </div>

      {/* Filters */}
      <div className="flex flex-col sm:flex-row gap-4 items-end">
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div className="space-y-2">
            <Label htmlFor="fechaDesde">Fecha desde</Label>
            <Input
              id="fechaDesde"
              type="date"
              value={filters.fechaDesde}
              onChange={(e) => handleFilterChange('fechaDesde', e.target.value)}
            />
          </div>
          <div className="space-y-2">
            <Label htmlFor="fechaHasta">Fecha hasta</Label>
            <Input
              id="fechaHasta"
              type="date"
              value={filters.fechaHasta}
              onChange={(e) => handleFilterChange('fechaHasta', e.target.value)}
            />
          </div>
        </div>
        <div className="flex gap-2">
          <Button onClick={handleApplyFilters} variant="outline">
            Filtrar
          </Button>
          <Button onClick={handleClearFilters} variant="ghost">
            Limpiar
          </Button>
        </div>
      </div>

      {/* Sales Table */}
      <DataTable
        columns={columns}
        data={paginatedVentas}
        pageCount={pageCount}
        pageIndex={pageIndex}
        pageSize={pageSize}
        onPageChange={handlePageChange}
        onSearch={handleSearch}
        searchPlaceholder="Buscar ventas..."
        searchValue={search}
        showSearch
        showPagination
        loading={isLoading}
        emptyMessage="No se encontraron ventas"
      />

      {/* Detalles Modal */}
      <VentasDetallesModal
        open={showDetallesModal}
        onOpenChange={setShowDetallesModal}
        venta={selectedVenta}
      />

      {/* Anular Modal */}
      <VentaAnularModal
        open={!!anularVenta}
        onOpenChange={(open) => !open && setAnularVenta(null)}
        onConfirm={handleAnular}
        isLoading={anularMutation.isPending}
        ventaId={anularVenta?.id}
      />
    </div>
  );
}

export default VentasPage;
