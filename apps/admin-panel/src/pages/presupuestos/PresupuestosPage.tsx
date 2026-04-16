/**
 * PresupuestosPage - Quote/Budget management page with DataTable
 * RoKey MANAGEMENT - Multi-tenant SaaS ERP/POS for locksmiths
 * 
 * Phase 6: Presupuestos CRUD + Convertir a Venta
 */

import * as React from 'react';
import type { ColumnDef } from '@tanstack/react-table';
import { Plus, Eye, X, ArrowRight } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { DataTable } from '@/components/ui/DataTable';
import { PresupuestoFormDialog } from './PresupuestoFormDialog';
import { PresupuestoDetalleDialog } from './PresupuestoDetalleDialog';
import {
  usePresupuestos,
  useCreatePresupuesto,
  useAnularPresupuesto,
  useConvertirPresupuesto,
  canManagePresupuestos,
} from '@/hooks';
import type { PresupuestoListItem, CrearPresupuestoRequest } from '@/types';
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

const ESTADO_COLORS: Record<string, string> = {
  Pendiente: 'bg-yellow-100 text-yellow-800',
  Aceptado: 'bg-green-100 text-green-800',
  Vencido: 'bg-red-100 text-red-800',
  Anulado: 'bg-gray-100 text-gray-800',
  Rechazado: 'bg-gray-100 text-gray-800',
};

export function PresupuestosPage() {
  // State
  const [search, setSearch] = React.useState('');
  const [pageIndex, setPageIndex] = React.useState(0);
  const [pageSize, setPageSize] = React.useState(20);
  const [showForm, setShowForm] = React.useState(false);
  const [selectedPresupuesto, setSelectedPresupuesto] = React.useState<{ id: number; estado: string } | null>(null);
  const [deletePresupuesto, setDeletePresupuesto] = React.useState<PresupuestoListItem | null>(null);
  const [filtroEstado, setFiltroEstado] = React.useState<string>('');

  // Permissions
  const canManage = canManagePresupuestos();

  // Queries
  const { data, isLoading } = usePresupuestos({
    filters: filtroEstado ? { estado: filtroEstado as 'Pendiente' | 'Aceptado' | 'Vencido' } : {},
  });

  // Mutations
  const createMutation = useCreatePresupuesto();
  const anularMutation = useAnularPresupuesto();
  const convertirMutation = useConvertirPresupuesto();

  // Data - Filter client-side
  const allPresupuestos = data?.items ?? [];
  const filteredPresupuestos = React.useMemo(() => {
    let result = allPresupuestos;
    
    if (search) {
      const searchLower = search.toLowerCase();
      result = result.filter(p => 
        (p.nombreCliente?.toLowerCase().includes(searchLower) ?? false) ||
        p.id.toString().includes(searchLower)
      );
    }
    
    return result;
  }, [allPresupuestos, search]);
  
  const total = filteredPresupuestos.length;
  const pageCount = Math.ceil(total / pageSize);
  const paginatedPresupuestos = filteredPresupuestos.slice(
    pageIndex * pageSize,
    (pageIndex + 1) * pageSize
  );

  // Column definitions
  const columns: ColumnDef<PresupuestoListItem>[] = React.useMemo(() => [
    {
      accessorKey: 'id',
      header: 'ID',
      cell: ({ row }) => <span className="text-muted-foreground">#{row.original.id}</span>,
    },
    {
      accessorKey: 'nombreCliente',
      header: 'Cliente',
      cell: ({ getValue }) => (getValue() as string) || 'Consumidor Final',
    },
    {
      accessorKey: 'fechaEmision',
      header: 'Fecha',
      cell: ({ getValue }) => {
        const date = getValue() as string;
        return new Date(date).toLocaleDateString('es-AR');
      },
    },
    {
      accessorKey: 'fechaVencimiento',
      header: 'Vencimiento',
      cell: ({ getValue }) => {
        const date = getValue() as string;
        return new Date(date).toLocaleDateString('es-AR');
      },
    },
    {
      accessorKey: 'total',
      header: 'Total',
      cell: ({ getValue }) => {
        const total = getValue() as number;
        return `$${total.toLocaleString('es-AR', { minimumFractionDigits: 2 })}`;
      },
    },
    {
      accessorKey: 'estado',
      header: 'Estado',
      cell: ({ getValue }) => {
        const estado = getValue() as string;
        return (
          <Badge className={ESTADO_COLORS[estado] || 'bg-gray-100 text-gray-800'}>
            {estado}
          </Badge>
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
          
          {row.original.estado === 'Pendiente' && (
            <>
              <Button
                variant="ghost"
                size="sm"
                onClick={() => handleConvertir(row.original)}
                title="Convertir a venta"
                className="text-green-600 hover:text-green-700"
              >
                <ArrowRight className="h-4 w-4" />
              </Button>
              
              {canManage && (
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => setDeletePresupuesto(row.original)}
                  title="Anular"
                  className="text-destructive hover:text-destructive"
                >
                  <X className="h-4 w-4" />
                </Button>
              )}
            </>
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
    setSelectedPresupuesto(null);
    setShowForm(true);
  };

  const handleVerDetalle = (presupuesto: PresupuestoListItem) => {
    setSelectedPresupuesto({ id: presupuesto.id, estado: presupuesto.estado });
  };

  const handleDetalleOpenChange = (open: boolean) => {
    if (!open) {
      setSelectedPresupuesto(null);
    }
  };

  const handleFormSubmit = async (data: unknown) => {
    try {
      await createMutation.mutateAsync(data as CrearPresupuestoRequest);
      setShowForm(false);
    } catch {
      // Error is handled by the mutation
    }
  };

  const handleConvertir = async (presupuesto: { id: number }) => {
    try {
      await convertirMutation.mutateAsync(presupuesto.id);
    } catch {
      // Error is handled by the mutation
    }
  };

  const handleAnular = async () => {
    if (!deletePresupuesto) return;
    try {
      await anularMutation.mutateAsync(deletePresupuesto.id);
      setDeletePresupuesto(null);
    } catch {
      // Error is handled by the mutation
    }
  };

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold text-foreground">Presupuestos</h1>
          <p className="text-muted-foreground">
            Gestiona los presupuestos de tu negocio
          </p>
        </div>
        
        {canManage && (
          <Button onClick={handleCreate} className="gap-2">
            <Plus className="h-4 w-4" />
            Nuevo Presupuesto
          </Button>
        )}
      </div>

      {/* Filters */}
      <div className="flex items-center gap-4">
        <select
          value={filtroEstado}
          onChange={(e) => setFiltroEstado(e.target.value)}
          className="w-[180px] h-10 px-3 rounded-md border border-input bg-background text-sm"
        >
          <option value="">Todos los estados</option>
          <option value="Pendiente">Pendiente</option>
          <option value="Aceptado">Aceptado</option>
          <option value="Vencido">Vencido</option>
        </select>
      </div>

      {/* Presupuestos Table */}
      <DataTable
        columns={columns}
        data={paginatedPresupuestos}
        pageCount={pageCount}
        pageIndex={pageIndex}
        pageSize={pageSize}
        onPageChange={handlePageChange}
        onSearch={handleSearch}
        searchPlaceholder="Buscar por cliente..."
        searchValue={search}
        showSearch
        showPagination
        loading={isLoading}
        emptyMessage="No se encontraron presupuestos"
      />

      {/* Presupuesto Form Dialog */}
      <PresupuestoFormDialog
        open={showForm}
        onOpenChange={setShowForm}
        onSubmit={handleFormSubmit}
        isLoading={createMutation.isPending}
      />

      {/* Presupuesto Detalle Dialog */}
      <PresupuestoDetalleDialog
        presupuesto={selectedPresupuesto}
        onOpenChange={handleDetalleOpenChange}
        onConvertir={handleConvertir}
        onAnular={handleConvertir}
        isConverting={convertirMutation.isPending}
        canManage={canManage}
      />

      {/* Anular Confirmation Dialog */}
      <AlertDialog open={!!deletePresupuesto} onOpenChange={() => setDeletePresupuesto(null)}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>¿Anular presupuesto?</AlertDialogTitle>
            <AlertDialogDescription>
              Esta acción marcará el presupuesto como anulado. Esta acción no se puede deshacer.
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

export default PresupuestosPage;
