/**
 * CategoriaDialog - Dialog for CRUD operations on categories
 * RoKey MANAGEMENT - Multi-tenant SaaS ERP/POS for locksmiths
 * 
 * Phase 5: Category Components - Create/Edit category dialog
 */

import * as React from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import {
  categoriaFormSchema,
  type CategoriaFormData,
  type CrearCategoriaFormData,
  type ActualizarCategoriaFormData,
} from '@/lib/schemas/categoria.schema';
import { canManageCategorias } from '@/hooks';
import { Loader2 } from 'lucide-react';

export interface CategoriaDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  categoria?: (Omit<ActualizarCategoriaFormData, 'descripcion'> & { id: number; descripcion?: string | null }) | null;
  onSubmit: (data: unknown) => void;
  isLoading?: boolean;
}

export function CategoriaDialog({
  open,
  onOpenChange,
  categoria,
  onSubmit,
  isLoading = false,
}: CategoriaDialogProps) {
  const isEditing = !!categoria?.id;
  const canManage = canManageCategorias();

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<CategoriaFormData>({
    resolver: zodResolver(categoriaFormSchema),
    defaultValues: {
      nombre: '',
      descripcion: '',
    },
  });

  // Reset form when dialog opens
  React.useEffect(() => {
    if (open) {
      if (categoria?.id) {
        reset({
          nombre: categoria.nombre ?? '',
          descripcion: categoria.descripcion ?? '',
        });
      } else {
        reset({
          nombre: '',
          descripcion: '',
        });
      }
    }
  }, [open, categoria, reset]);

  const handleFormSubmit = (data: CategoriaFormData) => {
    const submitData = isEditing
      ? { ...data, descripcion: data.descripcion ?? null } as ActualizarCategoriaFormData
      : data as CrearCategoriaFormData;
    onSubmit(submitData);
  };

  if (!canManage) {
    return null;
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-md">
        <DialogHeader>
          <DialogTitle>{isEditing ? 'Editar Categoría' : 'Nueva Categoría'}</DialogTitle>
          <DialogDescription>
            {isEditing
              ? 'Actualiza los datos de la categoría'
              : 'Completa los datos para crear una nueva categoría'}
          </DialogDescription>
        </DialogHeader>

        <form onSubmit={handleSubmit(handleFormSubmit)} className="space-y-4">
          {/* Nombre */}
          <div className="space-y-2">
            <Label htmlFor="nombre">Nombre *</Label>
            <Input
              id="nombre"
              {...register('nombre')}
              placeholder="Nombre de la categoría"
            />
            {errors.nombre && (
              <p className="text-xs text-destructive">{errors.nombre.message}</p>
            )}
          </div>

          {/* Descripción */}
          <div className="space-y-2">
            <Label htmlFor="descripcion">Descripción</Label>
            <Input
              id="descripcion"
              {...register('descripcion')}
              placeholder="Descripción de la categoría"
            />
          </div>

          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={() => onOpenChange(false)}
              disabled={isLoading}
            >
              Cancelar
            </Button>
            <Button type="submit" disabled={isLoading}>
              {isLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
              {isEditing ? 'Actualizar' : 'Crear'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

export default CategoriaDialog;