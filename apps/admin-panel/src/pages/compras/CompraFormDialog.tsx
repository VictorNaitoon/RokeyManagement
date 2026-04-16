/**
 * CompraFormDialog - Create new Compra dialog
 * RoKey MANAGEMENT - Multi-tenant SaaS ERP/POS for locksmiths
 * 
 * Phase 6: Compras CRUD
 */

import { useState } from 'react';
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
import { compraFormSchema, type CompraFormData } from '@/lib/schemas';
import { useProveedores, useProductos } from '@/hooks';
import { Plus, Trash2 } from 'lucide-react';
import type { Proveedor, Producto } from '@/types';

interface CompraFormDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onSubmit: (data: CompraFormData) => Promise<void>;
  isLoading?: boolean;
}

export function CompraFormDialog({
  open,
  onOpenChange,
  onSubmit,
  isLoading = false,
}: CompraFormDialogProps) {
  const { data: proveedoresData } = useProveedores();
  const { data: productosData } = useProductos();
  
  const proveedores = proveedoresData?.proveedores ?? [];
  const productos = productosData?.productos ?? [];
  const productosActivos = productos.filter((p: Producto) => p.activo);

  const [detalles, setDetalles] = useState<Array<{ idProducto: number; cantidad: number; precioUnitario: number }>>([
    { idProducto: 0, cantidad: 1, precioUnitario: 0 }
  ]);

  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
  } = useForm<{ numeroComprobante: string; idProveedor: number }>({
    resolver: zodResolver(compraFormSchema.pick({ numeroComprobante: true, idProveedor: true })),
    defaultValues: {
      numeroComprobante: '',
      idProveedor: 0,
    },
  });

  const total = detalles.reduce((sum, d) => {
    const cantidad = Number(d.cantidad) || 0;
    const precio = Number(d.precioUnitario) || 0;
    return sum + cantidad * precio;
  }, 0);

  const updateDetalle = (index: number, field: string, value: number) => {
    const newDetalles = [...detalles];
    (newDetalles[index] as Record<string, number>)[field] = value;
    
    // Auto-fill precio when product changes
    if (field === 'idProducto' && value > 0) {
      const producto = productosActivos.find((p: Producto) => p.id === value);
      if (producto && producto.precioCompra) {
        newDetalles[index].precioUnitario = producto.precioCompra;
      }
    }
    
    setDetalles(newDetalles);
  };

  const addDetalle = () => {
    setDetalles([...detalles, { idProducto: 0, cantidad: 1, precioUnitario: 0 }]);
  };

  const removeDetalle = (index: number) => {
    if (detalles.length > 1) {
      setDetalles(detalles.filter((_, i) => i !== index));
    }
  };

  const onFormSubmit = async (formData: { numeroComprobante: string; idProveedor: number }) => {
    const data: CompraFormData = {
      numeroComprobante: formData.numeroComprobante,
      idProveedor: formData.idProveedor,
      detalles: detalles.filter(d => d.idProducto > 0),
    };
    await onSubmit(data);
    reset();
    setDetalles([{ idProducto: 0, cantidad: 1, precioUnitario: 0 }]);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[600px] max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>Nueva Compra</DialogTitle>
          <DialogDescription>
            Registra una nueva compra a un proveedor.
          </DialogDescription>
        </DialogHeader>

        <form onSubmit={handleSubmit(onFormSubmit)} className="space-y-4">
          {/* Número de Comprobante */}
          <div className="space-y-2">
            <Label htmlFor="numeroComprobante">
              Número de Comprobante <span className="text-destructive">*</span>
            </Label>
            <Input
              id="numeroComprobante"
              {...register('numeroComprobante')}
              placeholder="Ej: FACT-001234"
            />
            {errors.numeroComprobante && (
              <p className="text-sm text-destructive">{errors.numeroComprobante.message}</p>
            )}
          </div>

          {/* Proveedor */}
          <div className="space-y-2">
            <Label htmlFor="idProveedor">
              Proveedor <span className="text-destructive">*</span>
            </Label>
            <select
              {...register('idProveedor', { valueAsNumber: true })}
              className="w-full h-10 px-3 rounded-md border border-input bg-background text-sm"
            >
              <option value={0}>Seleccionar proveedor</option>
              {proveedores.map((proveedor: Proveedor) => (
                <option key={proveedor.id} value={proveedor.id}>
                  {proveedor.nombre}
                </option>
              ))}
            </select>
            {errors.idProveedor && (
              <p className="text-sm text-destructive">{errors.idProveedor.message}</p>
            )}
          </div>

          {/* Detalles */}
          <div className="space-y-2">
            <Label>Productos</Label>
            {detalles.map((detalle, index) => (
              <div key={index} className="flex items-start gap-2 p-2 border rounded-lg">
                <div className="flex-1 space-y-2">
                  <select
                    value={detalle.idProducto}
                    onChange={(e) => updateDetalle(index, 'idProducto', parseInt(e.target.value) || 0)}
                    className="w-full h-10 px-3 rounded-md border border-input bg-background text-sm"
                  >
                    <option value={0}>Seleccionar producto</option>
                    {productosActivos.map((producto: Producto) => (
                        <option key={producto.id} value={producto.id}>
                          {producto.nombre} - Compra: ${(producto.precioCompra || 0).toLocaleString()}
                      </option>
                    ))}
                  </select>
                  
                  <div className="flex gap-2">
                    <div className="flex-1">
                      <Input
                        type="number"
                        placeholder="Cantidad"
                        value={detalle.cantidad}
                        onChange={(e) => updateDetalle(index, 'cantidad', parseInt(e.target.value) || 0)}
                        min={1}
                      />
                    </div>
                    <div className="flex-1">
                      <Input
                        type="number"
                        placeholder="Costo unitario"
                        step="0.01"
                        value={detalle.precioUnitario}
                        onChange={(e) => updateDetalle(index, 'precioUnitario', parseFloat(e.target.value) || 0)}
                      />
                    </div>
                  </div>
                </div>
                
                <Button
                  type="button"
                  variant="ghost"
                  size="icon"
                  onClick={() => removeDetalle(index)}
                  className="text-destructive"
                >
                  <Trash2 className="h-4 w-4" />
                </Button>
              </div>
            ))}
            
            <Button
              type="button"
              variant="outline"
              size="sm"
              onClick={addDetalle}
              className="mt-2"
            >
              <Plus className="h-4 w-4 mr-2" />
              Agregar Producto
            </Button>
          </div>

          {/* Total */}
          <div className="flex justify-between items-center p-4 bg-muted rounded-lg">
            <span className="font-medium">Total:</span>
            <span className="text-xl font-bold">
              ${total.toLocaleString('es-AR', { minimumFractionDigits: 2 })}
            </span>
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
              {isLoading ? 'Guardando...' : 'Registrar Compra'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
