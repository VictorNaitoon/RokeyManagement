/**
 * ProveedoresPage - Supplier management page with DataTable
 * RoKey MANAGEMENT - Multi-tenant SaaS ERP/POS for locksmiths
 * 
 * Phase 6: Proveedores CRUD
 */

import * as React from 'react';
import type { ColumnDef } from '@tanstack/react-table';
import { Plus, Building2, Pencil, Trash2 } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { DataTable } from '@/components/ui/DataTable';
import { ProveedorFormDialog } from './ProveedorFormDialog';
import {
  useProveedores,
  useCreateProveedor,
  useUpdateProveedor,
  useDeleteProveedor,
  canManageProveedores,
} from '@/hooks';
import type { Proveedor } from '@/types';
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

export function ProveedoresPage() {
  // State
  const [search, setSearch] = React.useState('');
  const [pageIndex, setPageIndex] = React.useState(0);
  const [pageSize, setPageSize] = React.useState(20);
  const [showForm, setShowForm] = React.useState(false);
  const [editingProveedor, setEditingProveedor] = React.useState<Proveedor | null>(null);
  const [deleteProveedor, setDeleteProveedor] = React.useState<Proveedor | null>(null);

  // Permissions
  const canManage = canManageProveedores();

  // Queries
  const { data, isLoading } = useProveedores();

  // Mutations
  const createMutation = useCreateProveedor();
  const updateMutation = useUpdateProveedor();
  const deleteMutation = useDeleteProveedor();

  // Data - Filter client-side
  const allProveedores = data?.proveedores ?? [];
  const filteredProveedores = React.useMemo(() => {
    if (!search) return allProveedores;
    const searchLower = search.toLowerCase();
    return allProveedores.filter(p => 
      p.nombre.toLowerCase().includes(searchLower) ||
      (p.email?.toLowerCase().includes(searchLower) ?? false) ||
      (p.telefono?.toLowerCase().includes(searchLower) ?? false)
    );
  }, [allProveedores, search]);
  
  const total = filteredProveedores.length;
  const pageCount = Math.ceil(total / pageSize);
  const paginatedProveedores = filteredProveedores.slice(
    pageIndex * pageSize,
    (pageIndex + 1) * pageSize
  );

  // Column definitions
  const columns: ColumnDef<Proveedor>[] = React.useMemo(() => [
    {
      accessorKey: 'nombre',
      header: 'Nombre',
      cell: ({ row }) => (
        <div className="font-medium">{row.original.nombre}</div>
      ),
    },
    {
      accessorKey: 'telefono',
      header: 'Teléfono',
      cell: ({ getValue }) => getValue() as string || '-',
    },
    {
      accessorKey: 'email',
      header: 'Email',
      cell: ({ getValue }) => getValue() as string || '-',
    },
    {
      accessorKey: 'fechaAlta',
      header: 'Fecha de Alta',
      cell: ({ getValue }) => {
        const date = getValue() as string;
        if (!date) return '-';
        return new Date(date).toLocaleDateString('es-AR');
      },
    },
    {
      id: 'acciones',
      header: '',
      cell: ({ row }) => canManage ? (
        <div className="flex items-center gap-1">
          <Button
            variant="ghost"
            size="sm"
            onClick={() => handleEdit(row.original)}
            title="Editar"
          >
            <Pencil className="h-4 w-4" />
          </Button>
          <Button
            variant="ghost"
            size="sm"
            onClick={() => setDeleteProveedor(row.original)}
            title="Eliminar"
            className="text-destructive hover:text-destructive"
          >
            <Trash2 className="h-4 w-4" />
          </Button>
        </div>
      ) : null,
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
    setEditingProveedor(null);
    setShowForm(true);
  };

  const handleEdit = (proveedor: Proveedor) => {
    setEditingProveedor(proveedor);
    setShowForm(true);
  };

  const handleFormSubmit = async (data: unknown) => {
    try {
      if (editingProveedor?.id) {
        await updateMutation.mutateAsync({ id: editingProveedor.id, data: data as { nombre: string; telefono?: string; email?: string } });
      } else {
        await createMutation.mutateAsync(data as { nombre: string; telefono?: string; email?: string });
      }
      setShowForm(false);
      setEditingProveedor(null);
    } catch {
      // Error is handled by the mutation
    }
  };

  const handleDelete = async () => {
    if (!deleteProveedor) return;
    try {
      await deleteMutation.mutateAsync(deleteProveedor.id);
      setDeleteProveedor(null);
    } catch {
      // Error is handled by the mutation
    }
  };

  if (!canManage) {
    return (
      <div className="flex flex-col items-center justify-center h-64 text-center">
        <Building2 className="h-12 w-12 text-muted-foreground mb-4" />
        <h3 className="text-lg font-medium">Sin permisos</h3>
        <p className="text-muted-foreground">No tienes permiso para gestionar proveedores.</p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold text-foreground">Proveedores</h1>
          <p className="text-muted-foreground">
            Gestiona los proveedores de tu negocio
          </p>
        </div>
        
        <Button onClick={handleCreate} className="gap-2">
          <Plus className="h-4 w-4" />
          Nuevo Proveedor
        </Button>
      </div>

      {/* Proveedores Table */}
      <DataTable
        columns={columns}
        data={paginatedProveedores}
        pageCount={pageCount}
        pageIndex={pageIndex}
        pageSize={pageSize}
        onPageChange={handlePageChange}
        onSearch={handleSearch}
        searchPlaceholder="Buscar proveedores..."
        searchValue={search}
        showSearch
        showPagination
        loading={isLoading}
        emptyMessage="No se encontraron proveedores"
      />

      {/* Proveedor Form Dialog */}
      <ProveedorFormDialog
        open={showForm}
        onOpenChange={setShowForm}
        proveedor={editingProveedor}
        onSubmit={handleFormSubmit}
        isLoading={createMutation.isPending || updateMutation.isPending}
      />

      {/* Delete Confirmation Dialog */}
      <AlertDialog open={!!deleteProveedor} onOpenChange={() => setDeleteProveedor(null)}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>¿Estás seguro?</AlertDialogTitle>
            <AlertDialogDescription>
              Esta acción eliminará el proveedor "{deleteProveedor?.nombre}". Esta acción no se puede deshacer.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancelar</AlertDialogCancel>
            <AlertDialogAction
              onClick={handleDelete}
              className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
            >
              Eliminar
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}

export default ProveedoresPage;
