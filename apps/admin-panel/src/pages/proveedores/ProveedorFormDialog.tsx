/**
 * ProveedorFormDialog - Create/Edit Proveedor dialog
 * RoKey MANAGEMENT - Multi-tenant SaaS ERP/POS for locksmiths
 * 
 * Phase 6: Proveedores CRUD
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
import { proveedorFormSchema, type ProveedorFormData } from '@/lib/schemas';
import type { Proveedor } from '@/types';

interface ProveedorFormDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  proveedor?: Proveedor | null;
  onSubmit: (data: ProveedorFormData) => Promise<void>;
  isLoading?: boolean;
}

export function ProveedorFormDialog({
  open,
  onOpenChange,
  proveedor,
  onSubmit,
  isLoading = false,
}: ProveedorFormDialogProps) {
  const isEditing = !!proveedor?.id;

  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
  } = useForm<ProveedorFormData>({
    resolver: zodResolver(proveedorFormSchema),
    defaultValues: {
      nombre: '',
      telefono: '',
      email: '',
    },
  });

  // Reset form when dialog opens/closes or proveedor changes
  React.useEffect(() => {
    if (open) {
      reset({
        nombre: proveedor?.nombre ?? '',
        telefono: proveedor?.telefono ?? '',
        email: proveedor?.email ?? '',
      });
    }
  }, [open, proveedor, reset]);

  const onFormSubmit = async (data: ProveedorFormData) => {
    await onSubmit(data);
    reset();
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[425px]">
        <DialogHeader>
          <DialogTitle>
            {isEditing ? 'Editar Proveedor' : 'Nuevo Proveedor'}
          </DialogTitle>
          <DialogDescription>
            {isEditing
              ? 'Modifica los datos del proveedor.'
              : 'Ingresa los datos del nuevo proveedor.'}
          </DialogDescription>
        </DialogHeader>

        <form onSubmit={handleSubmit(onFormSubmit)} className="space-y-4">
          {/* Nombre */}
          <div className="space-y-2">
            <Label htmlFor="nombre">
              Nombre <span className="text-destructive">*</span>
            </Label>
            <Input
              id="nombre"
              {...register('nombre')}
              placeholder="Nombre del proveedor"
              autoComplete="off"
            />
            {errors.nombre && (
              <p className="text-sm text-destructive">{errors.nombre.message}</p>
            )}
          </div>

          {/* Teléfono */}
          <div className="space-y-2">
            <Label htmlFor="telefono">Teléfono</Label>
            <Input
              id="telefono"
              {...register('telefono')}
              placeholder="Ej: 3511234567"
              autoComplete="tel"
            />
            {errors.telefono && (
              <p className="text-sm text-destructive">{errors.telefono.message}</p>
            )}
          </div>

          {/* Email */}
          <div className="space-y-2">
            <Label htmlFor="email">Email</Label>
            <Input
              id="email"
              type="email"
              {...register('email')}
              placeholder="proveedor@ejemplo.com"
              autoComplete="email"
            />
            {errors.email && (
              <p className="text-sm text-destructive">{errors.email.message}</p>
            )}
          </div>

          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={() => onOpenChange(false)}
            >
              Cancelar
            </Button>
            <Button type="submit" disabled={isLoading}>
              {isLoading ? 'Guardando...' : isEditing ? 'Guardar Cambios' : 'Crear Proveedor'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
