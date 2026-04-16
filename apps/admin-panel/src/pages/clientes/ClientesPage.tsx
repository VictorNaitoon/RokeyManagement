/**
 * ClientesPage - Customer management page with DataTable
 * RoKey MANAGEMENT - Multi-tenant SaaS ERP/POS for locksmiths
 * 
 * Phase 6: Clientes CRUD + Cuenta Corriente
 */

import * as React from 'react';
import type { ColumnDef } from '@tanstack/react-table';
import { Plus, Users, Eye } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { DataTable } from '@/components/ui/DataTable';
import { ClienteFormDialog } from './ClienteFormDialog';
import { ClienteCtaCteDialog } from './ClienteCtaCteDialog';
import { ClienteActions } from '@/components/clientes/ClienteActions';
import {
  useClientes,
  useCreateCliente,
  useUpdateCliente,
  useDeleteCliente,
  canManageClientes,
  canViewClientes,
} from '@/hooks';
import type { Cliente, CrearClienteRequest, ActualizarClienteRequest } from '@/types';

export function ClientesPage() {
  // State
  const [search, setSearch] = React.useState('');
  const [pageIndex, setPageIndex] = React.useState(0);
  const [pageSize, setPageSize] = React.useState(20);
  const [showForm, setShowForm] = React.useState(false);
  const [editingCliente, setEditingCliente] = React.useState<Cliente | null>(null);
  const [showCtaCte, setShowCtaCte] = React.useState(false);
  const [ctaCteCliente, setCtaCteCliente] = React.useState<Cliente | null>(null);

  // Permissions
  const canManage = canManageClientes();
  const canView = canViewClientes();

  // Queries
  const { data, isLoading } = useClientes();

  // Mutations
  const createMutation = useCreateCliente();
  const updateMutation = useUpdateCliente();
  const deleteMutation = useDeleteCliente();

  // Redirect if no view permission
  React.useEffect(() => {
    if (!canView) {
      // Could redirect to dashboard or show message
    }
  }, [canView]);

  // Data - Filter client-side
  const allClientes = data ?? [];
  const filteredClientes = React.useMemo(() => {
    if (!search) return allClientes;
    const searchLower = search.toLowerCase();
    return allClientes.filter(c => 
      c.nombre.toLowerCase().includes(searchLower) ||
      (c.apellido?.toLowerCase().includes(searchLower) ?? false) ||
      (c.email?.toLowerCase().includes(searchLower) ?? false) ||
      (c.documento?.toLowerCase().includes(searchLower) ?? false)
    );
  }, [allClientes, search]);
  
  const total = filteredClientes.length;
  const pageCount = Math.ceil(total / pageSize);
  const paginatedClientes = filteredClientes.slice(
    pageIndex * pageSize,
    (pageIndex + 1) * pageSize
  );

  // Column definitions
  const columns: ColumnDef<Cliente>[] = React.useMemo(() => [
    {
      accessorKey: 'nombre',
      header: 'Nombre',
      cell: ({ row }) => (
        <div>
          <div className="font-medium">{row.original.nombre} {row.original.apellido}</div>
          {row.original.documento && (
            <div className="text-xs text-muted-foreground">{row.original.documento}</div>
          )}
        </div>
      ),
    },
    {
      accessorKey: 'email',
      header: 'Email',
      cell: ({ getValue }) => getValue() as string || '-',
    },
    {
      accessorKey: 'telefono',
      header: 'Teléfono',
      cell: ({ getValue }) => getValue() as string || '-',
    },
    {
      accessorKey: 'permiteFiado',
      header: 'Fiado',
      cell: ({ getValue }) => {
        const permite = getValue() as boolean;
        return permite ? (
          <Badge variant="default">Sí</Badge>
        ) : (
          <Badge variant="secondary">No</Badge>
        );
      },
    },
    {
      accessorKey: 'activo',
      header: 'Estado',
      cell: ({ getValue }) => {
        const activo = getValue() as boolean;
        return activo ? (
          <Badge variant="default">Activo</Badge>
        ) : (
          <Badge variant="secondary">Inactivo</Badge>
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
            onClick={() => handleVerCtaCte(row.original)}
            title="Ver cuenta corriente"
          >
            <Eye className="h-4 w-4" />
          </Button>
          <ClienteActions
            cliente={row.original}
            onEdit={handleEdit}
            onDelete={handleDelete}
            canManage={canManage}
            isDeleting={deleteMutation.isPending}
          />
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
    setEditingCliente(null);
    setShowForm(true);
  };

  const handleEdit = (cliente: Cliente) => {
    setEditingCliente(cliente);
    setShowForm(true);
  };

  const handleVerCtaCte = (cliente: Cliente) => {
    setCtaCteCliente(cliente);
    setShowCtaCte(true);
  };

  const handleFormSubmit = async (data: unknown) => {
    if (editingCliente?.id) {
      await updateMutation.mutateAsync({ id: editingCliente.id, data: data as ActualizarClienteRequest });
    } else {
      await createMutation.mutateAsync(data as CrearClienteRequest);
    }
    setShowForm(false);
    setEditingCliente(null);
  };

  const handleDelete = async (cliente: Cliente) => {
    await deleteMutation.mutateAsync(cliente.id);
  };

  if (!canView) {
    return (
      <div className="flex flex-col items-center justify-center h-64 text-center">
        <Users className="h-12 w-12 text-muted-foreground mb-4" />
        <h3 className="text-lg font-medium">Sin permisos</h3>
        <p className="text-muted-foreground">No tienes permiso para ver clientes.</p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold text-foreground">Clientes</h1>
          <p className="text-muted-foreground">
            Gestiona los clientes de tu negocio
          </p>
        </div>
        
        {canManage && (
          <Button onClick={handleCreate} className="gap-2">
            <Plus className="h-4 w-4" />
            Nuevo Cliente
          </Button>
        )}
      </div>

      {/* Clientes Table */}
      <DataTable
        columns={columns}
        data={paginatedClientes}
        pageCount={pageCount}
        pageIndex={pageIndex}
        pageSize={pageSize}
        onPageChange={handlePageChange}
        onSearch={handleSearch}
        searchPlaceholder="Buscar clientes..."
        searchValue={search}
        showSearch
        showPagination
        loading={isLoading}
        emptyMessage="No se encontraron clientes"
      />

      {/* Cliente Form Dialog */}
      <ClienteFormDialog
        open={showForm}
        onOpenChange={setShowForm}
        cliente={editingCliente}
        onSubmit={handleFormSubmit}
        isLoading={createMutation.isPending || updateMutation.isPending}
      />

      {/* Cliente Cuenta Corriente Dialog */}
      <ClienteCtaCteDialog
        open={showCtaCte}
        onOpenChange={setShowCtaCte}
        cliente={ctaCteCliente}
      />
    </div>
  );
}

export default ClientesPage;