/**
 * ProductoActions - Action buttons for product management
 * RoKey MANAGEMENT - Multi-tenant SaaS ERP/POS for locksmiths
 * 
 * Phase 4: Feature Components - Edit, delete, and activate buttons
 */

import * as React from 'react';
import { Pencil, Trash2, RotateCcw, MoreHorizontal } from 'lucide-react';
import { Button } from '@/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
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
import { canManageProductos, canManageEstadoProductos } from '@/hooks';
import type { Producto } from '@/types';

export interface ProductoActionsProps {
  producto: Producto;
  onEdit: (producto: Producto) => void;
  onDelete: (producto: Producto) => void;
  onActivar: (producto: Producto) => void;
  isDeleting?: boolean;
  isActivating?: boolean;
}

export function ProductoActions({
  producto,
  onEdit,
  onDelete,
  onActivar,
  isDeleting = false,
  isActivating = false,
}: ProductoActionsProps) {
  const [showDeleteDialog, setShowDeleteDialog] = React.useState(false);
  
  const canManage = canManageProductos();
  const canManageEstado = canManageEstadoProductos();
  const isActive = producto.activo;

  if (!canManage && !canManageEstado) {
    // No actions for employees
    return null;
  }

  const handleDelete = () => {
    onDelete(producto);
    setShowDeleteDialog(false);
  };

  if (!canManage) {
    // Only activate/deactivate option for some roles
    return (
      <Button
        variant="ghost"
        size="icon"
        onClick={() => onActivar(producto)}
        disabled={isActivating}
        title={isActive ? 'Desactivar' : 'Activar'}
      >
        <RotateCcw className="h-4 w-4" />
      </Button>
    );
  }

  return (
    <>
      <DropdownMenu>
        <DropdownMenuTrigger className="cursor-pointer rounded-md p-1.5 hover:bg-accent">
          <MoreHorizontal className="h-4 w-4" />
        </DropdownMenuTrigger>
        <DropdownMenuContent align="end">
          <DropdownMenuItem onClick={() => onEdit(producto)}>
            <Pencil className="mr-2 h-4 w-4" />
            Editar
          </DropdownMenuItem>
          
          {canManageEstado && (
            <>
              <DropdownMenuSeparator />
              {isActive ? (
                <DropdownMenuItem onClick={() => onActivar(producto)}>
                  <RotateCcw className="mr-2 h-4 w-4" />
                  Desactivar
                </DropdownMenuItem>
              ) : (
                <DropdownMenuItem onClick={() => onActivar(producto)}>
                  <RotateCcw className="mr-2 h-4 w-4" />
                  Activar
                </DropdownMenuItem>
              )}
            </>
          )}
          
          <DropdownMenuSeparator />
          <DropdownMenuItem
            onClick={() => setShowDeleteDialog(true)}
            className="text-destructive focus:text-destructive"
          >
            <Trash2 className="mr-2 h-4 w-4" />
            Eliminar
          </DropdownMenuItem>
        </DropdownMenuContent>
      </DropdownMenu>

      {/* Delete Confirmation Dialog */}
      <AlertDialog open={showDeleteDialog} onOpenChange={setShowDeleteDialog}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>¿Eliminar producto?</AlertDialogTitle>
            <AlertDialogDescription>
              Estás a punto de eliminar el producto &quot;{producto.nombre}&quot;. 
              Esta acción lo desactivará y no podrá ser utilizado en nuevas ventas.
              ¿Estás seguro de continuar?
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancelar</AlertDialogCancel>
            <AlertDialogAction
              onClick={handleDelete}
              disabled={isDeleting}
              className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
            >
              {isDeleting ? 'Eliminando...' : 'Eliminar'}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </>
  );
}

export default ProductoActions;