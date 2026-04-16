/**
 * ClienteFormDialog - Form dialog for creating/editing customers
 * RoKey MANAGEMENT - Multi-tenant SaaS ERP/POS for locksmiths
 * 
 * Phase 6: Cliente CRUD
 */

import * as React from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Switch } from '@/components/ui/switch';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { clienteFormSchema, type ClienteFormData } from '@/lib/schemas';
import { Loader2 } from 'lucide-react';
import type { Cliente } from '@/types';

export interface ClienteFormDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  cliente?: Cliente | null;
  onSubmit: (data: unknown) => void;
  isLoading?: boolean;
}

export function ClienteFormDialog({
  open,
  onOpenChange,
  cliente,
  onSubmit,
  isLoading = false,
}: ClienteFormDialogProps) {
  const isEditing = !!cliente?.id;

  const {
    register,
    handleSubmit,
    setValue,
    watch,
    reset,
    formState: { errors },
  } = useForm<ClienteFormData>({
    resolver: zodResolver(clienteFormSchema),
    defaultValues: {
      nombre: '',
      apellido: '',
      email: '',
      documento: '',
      condicionIVA: '',
      telefono: '',
      direccion: '',
      permiteFiado: false,
      limiteCredito: undefined,
    },
  });

  // Watch values for controlled inputs
  const watchPermiteFiado = watch('permiteFiado');

  // Reset form when cliente changes or dialog opens
  React.useEffect(() => {
    if (open) {
      if (cliente?.id) {
        reset({
          nombre: cliente.nombre ?? '',
          apellido: cliente.apellido ?? '',
          email: cliente.email ?? '',
          documento: cliente.documento ?? '',
          condicionIVA: cliente.condicionIVA ?? '',
          telefono: cliente.telefono ?? '',
          direccion: cliente.direccion ?? '',
          permiteFiado: cliente.permiteFiado ?? false,
          limiteCredito: cliente.limiteCredito ?? undefined,
        });
      } else {
        reset({
          nombre: '',
          apellido: '',
          email: '',
          documento: '',
          condicionIVA: '',
          telefono: '',
          direccion: '',
          permiteFiado: false,
          limiteCredito: undefined,
        });
      }
    }
  }, [open, cliente, reset]);

  const handleFormSubmit = (data: ClienteFormData) => {
    onSubmit(data);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-lg max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>{isEditing ? 'Editar Cliente' : 'Nuevo Cliente'}</DialogTitle>
          <DialogDescription>
            {isEditing
              ? 'Actualiza los datos del cliente'
              : 'Completa los datos para crear un nuevo cliente'}
          </DialogDescription>
        </DialogHeader>

        <form onSubmit={handleSubmit(handleFormSubmit)} className="space-y-4">
          {/* Nombre */}
          <div className="space-y-2">
            <Label htmlFor="nombre">Nombre *</Label>
            <Input
              id="nombre"
              {...register('nombre')}
              placeholder="Nombre del cliente"
            />
            {errors.nombre && (
              <p className="text-xs text-destructive">{errors.nombre.message}</p>
            )}
          </div>

          {/* Apellido */}
          <div className="space-y-2">
            <Label htmlFor="apellido">Apellido</Label>
            <Input
              id="apellido"
              {...register('apellido')}
              placeholder="Apellido del cliente"
            />
          </div>

          {/* Email */}
          <div className="space-y-2">
            <Label htmlFor="email">Email</Label>
            <Input
              id="email"
              type="email"
              {...register('email')}
              placeholder="email@ejemplo.com"
            />
            {errors.email && (
              <p className="text-xs text-destructive">{errors.email.message}</p>
            )}
          </div>

          {/* Documento */}
          <div className="space-y-2">
            <Label htmlFor="documento">Documento (DNI/CUIT)</Label>
            <Input
              id="documento"
              {...register('documento')}
              placeholder="Número de documento"
            />
          </div>

          {/* Condición IVA */}
          <div className="space-y-2">
            <Label htmlFor="condicionIVA">Condición IVA</Label>
            <Input
              id="condicionIVA"
              {...register('condicionIVA')}
              placeholder="Responsable Inscripto, Monotributo, etc."
            />
          </div>

          {/* Teléfono */}
          <div className="space-y-2">
            <Label htmlFor="telefono">Teléfono</Label>
            <Input
              id="telefono"
              {...register('telefono')}
              placeholder="Teléfono de contacto"
            />
          </div>

          {/* Dirección */}
          <div className="space-y-2">
            <Label htmlFor="direccion">Dirección</Label>
            <Input
              id="direccion"
              {...register('direccion')}
              placeholder="Dirección del cliente"
            />
          </div>

          {/* Permite Fiado */}
          <div className="flex items-center gap-2">
            <Switch
              id="permiteFiado"
              checked={watchPermiteFiado ?? false}
              onCheckedChange={(checked) => setValue('permiteFiado', checked)}
            />
            <Label htmlFor="permiteFiado" className="cursor-pointer">
              Permite fiado
            </Label>
          </div>

          {/* Límite de crédito - only if permite fiado */}
          {watchPermiteFiado && (
            <div className="space-y-2">
              <Label htmlFor="limiteCredito">Límite de crédito</Label>
              <Input
                id="limiteCredito"
                type="number"
                step="0.01"
                min="0"
                {...register('limiteCredito', { valueAsNumber: true })}
                placeholder="0.00"
              />
            </div>
          )}

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

export default ClienteFormDialog;