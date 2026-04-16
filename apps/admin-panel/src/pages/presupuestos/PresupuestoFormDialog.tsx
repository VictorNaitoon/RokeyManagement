/**
 * PresupuestoFormDialog - Create new Presupuesto dialog
 * RoKey MANAGEMENT - Multi-tenant SaaS ERP/POS for locksmiths
 * 
 * Phase 6: Presupuestos CRUD
 */

import { useState } from 'react';
import { useForm } from 'react-hook-form';
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
import { type PresupuestoFormData } from '@/lib/schemas';
import { useClientes, useProductos } from '@/hooks';
import { Plus, Trash2 } from 'lucide-react';
import type { Cliente, Producto } from '@/types';

interface PresupuestoFormDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onSubmit: (data: PresupuestoFormData) => Promise<void>;
  isLoading?: boolean;
}

export function PresupuestoFormDialog({
  open,
  onOpenChange,
  onSubmit,
  isLoading = false,
}: PresupuestoFormDialogProps) {
  const { data: clientes } = useClientes();
  const { data: productosData } = useProductos();
  
  const productos = productosData?.productos ?? [];
  const productosActivos = productos.filter((p: Producto) => p.activo);
  const clientesActivos = clientes?.filter((c: Cliente) => c.activo) ?? [];

  const [idCliente, setIdCliente] = useState<number | null>(null);
  const [fechaVencimiento, setFechaVencimiento] = useState(
    new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString().split('T')[0]
  );
  const [detalles, setDetalles] = useState<Array<{ idProducto: number; cantidad: number; precioPactado: number }>>([
    { idProducto: 0, cantidad: 1, precioPactado: 0 }
  ]);

  const { handleSubmit, reset } = useForm();

  const total = detalles.reduce((sum, d) => {
    const cantidad = Number(d.cantidad) || 0;
    const precio = Number(d.precioPactado) || 0;
    return sum + cantidad * precio;
  }, 0);

  const updateDetalle = (index: number, field: string, value: number) => {
    const newDetalles = [...detalles];
    (newDetalles[index] as Record<string, number>)[field] = value;
    
    // Auto-fill precio when product changes
    if (field === 'idProducto' && value > 0) {
      const producto = productosActivos.find((p: Producto) => p.id === value);
      if (producto) {
        newDetalles[index].precioPactado = producto.precioVenta;
      }
    }
    
    setDetalles(newDetalles);
  };

  const addDetalle = () => {
    setDetalles([...detalles, { idProducto: 0, cantidad: 1, precioPactado: 0 }]);
  };

  const removeDetalle = (index: number) => {
    if (detalles.length > 1) {
      setDetalles(detalles.filter((_, i) => i !== index));
    }
  };

  const onFormSubmit = async () => {
    const data: PresupuestoFormData = {
      idCliente,
      fechaVencimiento,
      detalles: detalles.filter(d => d.idProducto > 0),
    };
    await onSubmit(data);
    reset();
    setDetalles([{ idProducto: 0, cantidad: 1, precioPactado: 0 }]);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[600px] max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>Nuevo Presupuesto</DialogTitle>
          <DialogDescription>
            Crea un nuevo presupuesto para un cliente.
          </DialogDescription>
        </DialogHeader>

        <form onSubmit={handleSubmit(onFormSubmit)} className="space-y-4">
          {/* Cliente */}
          <div className="space-y-2">
            <Label htmlFor="idCliente">Cliente</Label>
            <select
              value={idCliente ?? ''}
              onChange={(e) => setIdCliente(e.target.value ? parseInt(e.target.value) : null)}
              className="w-full h-10 px-3 rounded-md border border-input bg-background text-sm"
            >
              <option value="">Consumidor Final</option>
              {clientesActivos.map((cliente: Cliente) => (
                <option key={cliente.id} value={cliente.id}>
                  {cliente.nombre} {cliente.apellido}
                </option>
              ))}
            </select>
          </div>

          {/* Fecha de Vencimiento */}
          <div className="space-y-2">
            <Label htmlFor="fechaVencimiento">Fecha de Vencimiento</Label>
            <Input
              id="fechaVencimiento"
              type="date"
              value={fechaVencimiento}
              onChange={(e) => setFechaVencimiento(e.target.value)}
            />
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
                        {producto.nombre} - ${producto.precioVenta.toLocaleString()}
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
                        placeholder="Precio pactado"
                        step="0.01"
                        value={detalle.precioPactado}
                        onChange={(e) => updateDetalle(index, 'precioPactado', parseFloat(e.target.value) || 0)}
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
              {isLoading ? 'Guardando...' : 'Crear Presupuesto'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
