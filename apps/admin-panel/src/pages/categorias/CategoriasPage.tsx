/**
 * CategoriasPage - Category management page with DataTable
 * RoKey MANAGEMENT - Multi-tenant SaaS ERP/POS for locksmiths
 * 
 * Phase 6: Pages - Category listing and management
 * Only Dueño and Gerente can access this page
 */

import * as React from 'react';
import type { ColumnDef } from '@tanstack/react-table';
import { Plus, Tag } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { DataTable } from '@/components/ui/DataTable';
import { CategoriaDialog } from '@/components/categorias/CategoriaDialog';
import { CategoriaActions } from '@/components/categorias/CategoriaActions';
import {
  useCategorias,
  useCreateCategoria,
  useUpdateCategoria,
  useDeleteCategoria,
  canManageCategorias,
} from '@/hooks';
import type { Categoria, CrearCategoriaRequest, ActualizarCategoriaRequest } from '@/types';
import { useNavigate } from 'react-router-dom';

export function CategoriasPage() {
  const navigate = useNavigate();
  const canManage = canManageCategorias();
  
  // Redirect if no permission
  React.useEffect(() => {
    if (!canManage) {
      navigate('/');
    }
  }, [canManage, navigate]);

  // State
  const [search, setSearch] = React.useState('');
  const [pageIndex, setPageIndex] = React.useState(0);
  const [pageSize, setPageSize] = React.useState(20);
  const [showDialog, setShowDialog] = React.useState(false);
  const [editingCategoria, setEditingCategoria] = React.useState<Categoria | null>(null);

  // Queries
  const { data, isLoading } = useCategorias();

  // Mutations
  const createMutation = useCreateCategoria();
  const updateMutation = useUpdateCategoria();
  const deleteMutation = useDeleteCategoria();

  // Data - Filter client-side since backend doesn't support search
  const allCategorias = data?.categorias ?? [];
  const filteredCategorias = React.useMemo(() => {
    if (!search) return allCategorias;
    const searchLower = search.toLowerCase();
    return allCategorias.filter(c => 
      c.nombre.toLowerCase().includes(searchLower) ||
      (c.descripcion?.toLowerCase().includes(searchLower) ?? false)
    );
  }, [allCategorias, search]);
  
  const total = filteredCategorias.length;
  const pageCount = Math.ceil(total / pageSize);
  const paginatedCategorias = filteredCategorias.slice(
    pageIndex * pageSize,
    (pageIndex + 1) * pageSize
  );

  // Column definitions
  const columns: ColumnDef<Categoria>[] = React.useMemo(() => [
    {
      accessorKey: 'nombre',
      header: 'Nombre',
      cell: ({ row }) => (
        <div className="flex items-center gap-2">
          <div className="w-8 h-8 rounded bg-muted flex items-center justify-center">
            <Tag className="h-4 w-4 text-muted-foreground" />
          </div>
          <div className="font-medium">{row.original.nombre}</div>
        </div>
      ),
    },
    {
      accessorKey: 'descripcion',
      header: 'Descripción',
      cell: ({ getValue }) => {
        const value = getValue() as string | null;
        return value || '-';
      },
    },
    {
      accessorKey: 'id',
      header: 'ID',
      cell: ({ getValue }) => {
        const value = getValue() as number;
        return <span className="text-muted-foreground text-xs">#{value}</span>;
      },
    },
    {
      accessorKey: 'activo',
      header: 'Estado',
      cell: ({ getValue }) => {
        const activo = getValue() as boolean;
        return activo ? (
          <Badge variant="default">Activa</Badge>
        ) : (
          <Badge variant="secondary">Inactiva</Badge>
        );
      },
    },
    {
      id: 'acciones',
      header: '',
      cell: ({ row }) => (
        <CategoriaActions
          categoria={row.original}
          onEdit={handleEdit}
          onDelete={handleDelete}
          onToggleActivo={handleToggleActivo}
          isDeleting={deleteMutation.isPending}
        />
      ),
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

  const handleCreate = () => {
    setEditingCategoria(null);
    setShowDialog(true);
  };

  const handleEdit = (categoria: Categoria) => {
    setEditingCategoria(categoria);
    setShowDialog(true);
  };

  const handleFormSubmit = async (data: unknown) => {
    if (editingCategoria?.id) {
      const updateData = data as ActualizarCategoriaRequest;
      // Create a clean object without descripcion if it's null, since Zod schema doesn't accept null
      const sanitizedData: Partial<ActualizarCategoriaRequest> = { ...updateData };
      if (sanitizedData.descripcion === null) {
        delete sanitizedData.descripcion;
      }
      await updateMutation.mutateAsync({ id: editingCategoria.id, data: sanitizedData });
    } else {
      await createMutation.mutateAsync(data as CrearCategoriaRequest);
    }
    setShowDialog(false);
    setEditingCategoria(null);
  };

  const handleDelete = async (categoria: Categoria) => {
    await deleteMutation.mutateAsync(categoria.id);
  };

  const handleToggleActivo = async (categoria: Categoria) => {
    await updateMutation.mutateAsync({
      id: categoria.id,
      data: { activo: !categoria.activo },
    });
  };

  if (!canManage) {
    return null;
  }

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold text-foreground">Categorías</h1>
          <p className="text-muted-foreground">
            Organiza tus productos por categorías
          </p>
        </div>
        
        <Button onClick={handleCreate} className="gap-2">
          <Plus className="h-4 w-4" />
          Nueva Categoría
        </Button>
      </div>

      {/* Categories Table */}
      <DataTable
        columns={columns}
        data={paginatedCategorias}
        pageCount={pageCount}
        pageIndex={pageIndex}
        pageSize={pageSize}
        onPageChange={handlePageChange}
        onSearch={handleSearch}
        searchPlaceholder="Buscar categorías..."
        searchValue={search}
        showSearch
        showPagination
        loading={isLoading}
        emptyMessage="No se encontraron categorías"
      />

      {/* Category Dialog */}
      <CategoriaDialog
        open={showDialog}
        onOpenChange={setShowDialog}
        categoria={editingCategoria}
        onSubmit={handleFormSubmit}
        isLoading={createMutation.isPending || updateMutation.isPending}
      />
    </div>
  );
}

export default CategoriasPage;