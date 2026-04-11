/**
 * ProductosPage - Product management page with DataTable
 * RoKey MANAGEMENT - Multi-tenant SaaS ERP/POS for locksmiths
 * 
 * Phase 6: Pages - Product listing and management
 */

import * as React from 'react';
import type { ColumnDef } from '@tanstack/react-table';
import { Plus, Package, AlertTriangle } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { DataTable } from '@/components/ui/DataTable';
import { ProductoForm } from '@/components/productos/ProductoForm';
import { ProductoActions } from '@/components/productos/ProductoActions';
import {
  useProductos,
  useCreateProducto,
  useUpdateProducto,
  useDeleteProducto,
  useReactivarProducto,
  canViewPrecioCompra,
  canManageProductos,
} from '@/hooks';
import type { Producto, CrearProductoRequest, ActualizarProductoRequest } from '@/types';

export function ProductosPage() {
  // State
  const [search, setSearch] = React.useState('');
  const [pageIndex, setPageIndex] = React.useState(0);
  const [pageSize, setPageSize] = React.useState(20);
  const [showForm, setShowForm] = React.useState(false);
  const [editingProducto, setEditingProducto] = React.useState<Producto | null>(null);

  // Permissions
  const canManage = canManageProductos();
  const canSeePrice = canViewPrecioCompra();

  // Queries
  const { data, isLoading } = useProductos();

  // Mutations
  const createMutation = useCreateProducto();
  const updateMutation = useUpdateProducto();
  const deleteMutation = useDeleteProducto();
  const activarMutation = useReactivarProducto();

  // Data - Filter client-side since backend doesn't support search
  const allProductos = data?.productos ?? [];
  const filteredProductos = React.useMemo(() => {
    if (!search) return allProductos;
    const searchLower = search.toLowerCase();
    return allProductos.filter(p => 
      p.nombre.toLowerCase().includes(searchLower) ||
      (p.codigoBusqueda?.toLowerCase().includes(searchLower) ?? false)
    );
  }, [allProductos, search]);
  
  const total = filteredProductos.length;
  const pageCount = Math.ceil(total / pageSize);
  const paginatedProductos = filteredProductos.slice(
    pageIndex * pageSize,
    (pageIndex + 1) * pageSize
  );

  // Column definitions
  const columns: ColumnDef<Producto>[] = React.useMemo(() => [
    {
      accessorKey: 'nombre',
      header: 'Nombre',
      cell: ({ row }) => (
        <div className="flex items-center gap-2">
          {row.original.imagenURL ? (
            <img
              src={row.original.imagenURL}
              alt={row.original.nombre}
              className="w-8 h-8 rounded object-cover"
            />
          ) : (
            <div className="w-8 h-8 rounded bg-muted flex items-center justify-center">
              <Package className="h-4 w-4 text-muted-foreground" />
            </div>
          )}
          <div>
            <div className="font-medium">{row.original.nombre}</div>
            {row.original.codigoBusqueda && (
              <div className="text-xs text-muted-foreground">{row.original.codigoBusqueda}</div>
            )}
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
      accessorKey: 'precioVenta',
      header: 'Precio',
      cell: ({ getValue }) => {
        const value = getValue() as number;
        return <span className="font-medium">${value.toFixed(2)}</span>;
      },
    },
    {
      accessorKey: 'precioCompra',
      header: 'Costo',
      cell: ({ getValue }) => {
        if (!canSeePrice) return <span className="text-muted-foreground text-xs">-</span>;
        const value = getValue() as number | null;
        return value !== null ? <span className="text-muted-foreground">${value.toFixed(2)}</span> : '-';
      },
    },
    {
      accessorKey: 'stockActual',
      header: 'Stock',
      cell: ({ row }) => {
        const stock = row.original.stockActual;
        const min = row.original.stockMinimo;
        const isLow = stock <= min;
        const isService = row.original.esServicio;
        
        if (isService) {
          return <Badge variant="secondary">Servicio</Badge>;
        }
        
        return (
          <div className="flex items-center gap-1">
            <span className={isLow ? 'text-destructive font-medium' : ''}>{stock}</span>
            {isLow && <AlertTriangle className="h-3 w-3 text-destructive" />}
          </div>
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
        <ProductoActions
          producto={row.original}
          onEdit={handleEdit}
          onDelete={handleDelete}
          onActivar={handleActivar}
          isDeleting={deleteMutation.isPending}
          isActivating={activarMutation.isPending}
        />
      ),
    },
  ], [canSeePrice]);

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
    setEditingProducto(null);
    setShowForm(true);
  };

  const handleEdit = (producto: Producto) => {
    setEditingProducto(producto);
    setShowForm(true);
  };

  const handleFormSubmit = async (data: unknown) => {
    if (editingProducto?.id) {
      const updateData = data as ActualizarProductoRequest;
      // Create a clean object without precioCompra if it's null, since Zod schema doesn't accept null
      const sanitizedData: Partial<ActualizarProductoRequest> = { ...updateData };
      if (sanitizedData.precioCompra === null) {
        delete sanitizedData.precioCompra;
      }
      await updateMutation.mutateAsync({ id: editingProducto.id, data: sanitizedData });
    } else {
      await createMutation.mutateAsync(data as CrearProductoRequest);
    }
    setShowForm(false);
    setEditingProducto(null);
  };

  const handleDelete = async (producto: Producto) => {
    await deleteMutation.mutateAsync(producto.id);
  };

  const handleActivar = async (producto: Producto) => {
    await activarMutation.mutateAsync(producto.id);
  };

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold text-foreground">Productos</h1>
          <p className="text-muted-foreground">
            Gestiona el inventario de tu negocio
          </p>
        </div>
        
        {canManage && (
          <Button onClick={handleCreate} className="gap-2">
            <Plus className="h-4 w-4" />
            Nuevo Producto
          </Button>
        )}
      </div>

      {/* Products Table */}
      <DataTable
        columns={columns}
        data={paginatedProductos}
        pageCount={pageCount}
        pageIndex={pageIndex}
        pageSize={pageSize}
        onPageChange={handlePageChange}
        onSearch={handleSearch}
        searchPlaceholder="Buscar productos..."
        searchValue={search}
        showSearch
        showPagination
        loading={isLoading}
        emptyMessage="No se encontraron productos"
      />

      {/* Product Form Dialog */}
      <ProductoForm
        open={showForm}
        onOpenChange={setShowForm}
        producto={editingProducto}
        onSubmit={handleFormSubmit}
        isLoading={createMutation.isPending || updateMutation.isPending}
      />
    </div>
  );
}

export default ProductosPage;