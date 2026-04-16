/**
 * ComprasPage - Purchase management page with DataTable
 * RoKey MANAGEMENT - Multi-tenant SaaS ERP/POS for locksmiths
 * 
 * Phase 6: Compras CRUD + Anular
 */

import * as React from 'react';
import type { ColumnDef } from '@tanstack/react-table';
import { Plus, Truck, Eye, XCircle } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { DataTable } from '@/components/ui/DataTable';
import { CompraFormDialog } from './CompraFormDialog';
import { CompraDetalleDialog } from './CompraDetalleDialog';
import {
  useCompras,
  useCreateCompra,
  useAnularCompra,
  canManageCompras,
} from '@/hooks';
import type { Compra, CrearCompraRequest } from '@/types';
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog';

export function ComprasPage() {
  // State
  const [search, setSearch] = React.useState('');
  const [pageIndex, setPageIndex] = React.useState(0);
  const [pageSize, setPageSize] = React.useState(20);
  const [showForm, setShowForm] = React.useState(false);
  const [showDetalle, setShowDetalle] = React.useState(false);
  const [selectedCompra, setSelectedCompra] = React.useState<Compra | null>(null);
  const [anularCompra, setAnularCompra] = React.useState<Compra | null>(null);

  // Permissions
  const canManage = canManageCompras();

  // Queries
  const { data, isLoading } = useCompras();

  // Mutations
  const createMutation = useCreateCompra();
  const anularMutation = useAnularCompra();

  // Data - Filter client-side
  const allCompras = data?.compras ?? [];
  const filteredCompras = React.useMemo(() => {
    if (!search) return allCompras;
    const searchLower = search.toLowerCase();
    return allCompras.filter(c => 
      (c.numeroComprobante?.toLowerCase().includes(searchLower) ?? false) ||
      (c.nombreProveedor?.toLowerCase().includes(searchLower) ?? false) ||
      c.id.toString().includes(searchLower)
    );
  }, [allCompras, search]);
  
  const total = filteredCompras.length;
  const pageCount = Math.ceil(total / pageSize);
  const paginatedCompras = filteredCompras.slice(
    pageIndex * pageSize,
    (pageIndex + 1) * pageSize
  );

  // Column definitions
  const columns: ColumnDef<Compra>[] = React.useMemo(() => [
    {
      accessorKey: 'id',
      header: 'ID',
      cell: ({ row }) => <span className="text-muted-foreground">#{row.original.id}</span>,
    },
    {
      accessorKey: 'numeroComprobante',
      header: 'Nº Comprobante',
      cell: ({ getValue }) => (getValue() as string) || '-',
    },
    {
      accessorKey: 'nombreProveedor',
      header: 'Proveedor',
      cell: ({ getValue }) => (getValue() as string) || '-',
    },
    {
      accessorKey: 'fechaCompra',
      header: 'Fecha',
      cell: ({ getValue }) => {
        const date = getValue() as string;
        return new Date(date).toLocaleDateString('es-AR');
      },
    },
    {
      accessorKey: 'totalGasto',
      header: 'Total',
      cell: ({ getValue }) => {
        const total = getValue() as number;
        return `$${total.toLocaleString('es-AR', { minimumFractionDigits: 2 })}`;
      },
    },
    {
      accessorKey: 'anulada',
      header: 'Estado',
      cell: ({ getValue }) => {
        const anulada = getValue() as boolean;
        return anulada ? (
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
        <div className="flex items-center gap-1">
          <Button
            variant="ghost"
            size="sm"
            onClick={() => handleVerDetalle(row.original)}
            title="Ver detalles"
          >
            <Eye className="h-4 w-4" />
          </Button>
          
          {!row.original.anulada && canManage && (
            <Button
              variant="ghost"
              size="sm"
              onClick={() => setAnularCompra(row.original)}
              title="Anular"
              className="text-destructive hover:text-destructive"
            >
              <XCircle className="h-4 w-4" />
            </Button>
          )}
        </div>
      ),
    },
  ], [canManage]);

  // Handlers
  const handleSearch = (value: string) => {
    setSearch(value);
    setPageIndex(0);
  };

  const handlePageChange = (newPageIndex: number, newPageSize: number) => {
    setPageIndex(newPageIndex);
    setPageSize(newPageSize);
  };

  const handleCreate = () => {
    setSelectedCompra(null);
    setShowForm(true);
  };

  const handleVerDetalle = (compra: Compra) => {
    setSelectedCompra(compra);
    setShowDetalle(true);
  };

  const handleFormSubmit = async (data: unknown) => {
    try {
      await createMutation.mutateAsync(data as CrearCompraRequest);
      setShowForm(false);
    } catch {
      // Error is handled by the mutation
    }
  };

  const handleAnular = async () => {
    if (!anularCompra) return;
    try {
      await anularMutation.mutateAsync(anularCompra.id);
      setAnularCompra(null);
    } catch {
      // Error is handled by the mutation
    }
  };

  if (!canManage) {
    return (
      <div className="flex flex-col items-center justify-center h-64 text-center">
        <Truck className="h-12 w-12 text-muted-foreground mb-4" />
        <h3 className="text-lg font-medium">Sin permisos</h3>
        <p className="text-muted-foreground">No tienes permiso para gestionar compras.</p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold text-foreground">Compras</h1>
          <p className="text-muted-foreground">
            Gestiona las compras de tu negocio
          </p>
        </div>
        
        <Button onClick={handleCreate} className="gap-2">
          <Plus className="h-4 w-4" />
          Nueva Compra
        </Button>
      </div>

      {/* Compras Table */}
      <DataTable
        columns={columns}
        data={paginatedCompras}
        pageCount={pageCount}
        pageIndex={pageIndex}
        pageSize={pageSize}
        onPageChange={handlePageChange}
        onSearch={handleSearch}
        searchPlaceholder="Buscar por comprobante o proveedor..."
        searchValue={search}
        showSearch
        showPagination
        loading={isLoading}
        emptyMessage="No se encontraron compras"
      />

      {/* Compra Form Dialog */}
      <CompraFormDialog
        open={showForm}
        onOpenChange={setShowForm}
        onSubmit={handleFormSubmit}
        isLoading={createMutation.isPending}
      />

      {/* Compra Detalle Dialog */}
      <CompraDetalleDialog
        open={showDetalle}
        onOpenChange={setShowDetalle}
        compra={selectedCompra}
      />

      {/* Anular Confirmation Dialog */}
      <AlertDialog open={!!anularCompra} onOpenChange={() => setAnularCompra(null)}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>¿Anular compra?</AlertDialogTitle>
            <AlertDialogDescription>
              Esta acción revertirá el stock de los productos. Esta acción no se puede deshacer.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancelar</AlertDialogCancel>
            <AlertDialogAction
              onClick={handleAnular}
              className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
            >
              Anular
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}

export default ComprasPage;
