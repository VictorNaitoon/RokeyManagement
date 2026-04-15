/**
 * CategoriaActions - Action buttons for category management
 * RoKey MANAGEMENT - Multi-tenant SaaS ERP/POS for locksmiths
 * 
 * Phase 5: Category Components - Edit, delete, and toggle status buttons
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
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { canManageCategorias } from '@/hooks';
import type { Categoria } from '@/types';

export interface CategoriaActionsProps {
  categoria: Categoria;
  onEdit: (categoria: Categoria) => void;
  onDelete: (categoria: Categoria) => void;
  onToggleActivo: (categoria: Categoria) => void;
  isDeleting?: boolean;
  isToggling?: boolean;
}

export function CategoriaActions({
  categoria,
  onEdit,
  onDelete,
  onToggleActivo,
  isDeleting = false,
  isToggling = false,
}: CategoriaActionsProps) {
  const [showDeleteDialog, setShowDeleteDialog] = React.useState(false);
  
  const canManage = canManageCategorias();
  const isActive = categoria.activo;

  if (!canManage) {
    return null;
  }

  const handleDelete = () => {
    onDelete(categoria);
    setShowDeleteDialog(false);
  };

  return (
    <>
      <DropdownMenu>
        <DropdownMenuTrigger asChild>
          <button
            type="button"
            className="inline-flex shrink-0 items-center justify-center rounded-md text-sm font-medium ring-offset-background transition-colors hover:bg-accent hover:text-accent-foreground h-8 w-8"
            aria-label="Abrir menú de acciones"
          >
            <MoreHorizontal className="h-4 w-4" />
          </button>
        </DropdownMenuTrigger>
        <DropdownMenuContent align="end">
          <DropdownMenuItem onClick={() => onEdit(categoria)}>
            <Pencil className="mr-2 h-4 w-4" />
            Editar
          </DropdownMenuItem>
          
          <DropdownMenuSeparator />
          <DropdownMenuItem onClick={() => onToggleActivo(categoria)} disabled={isToggling}>
            <RotateCcw className="mr-2 h-4 w-4" />
            {isActive ? 'Desactivar' : 'Activar'}
          </DropdownMenuItem>
          
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
      <Dialog open={showDeleteDialog} onOpenChange={setShowDeleteDialog}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>¿Eliminar categoría?</DialogTitle>
            <DialogDescription>
              Estás a punto de eliminar la categoría &quot;{categoria.nombre}&quot;. 
              Esta acción la desactivará y no podrá ser utilizada en nuevos productos.
              ¿Estás seguro de continuar?
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button variant="outline" onClick={() => setShowDeleteDialog(false)}>
              Cancelar
            </Button>
            <Button 
              onClick={handleDelete} 
              disabled={isDeleting}
              className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
            >
              {isDeleting ? 'Eliminando...' : 'Eliminar'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </>
  );
}

export default CategoriaActions;