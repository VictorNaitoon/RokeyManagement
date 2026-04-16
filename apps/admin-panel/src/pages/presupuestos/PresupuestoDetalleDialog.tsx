/**
 * PresupuestoDetalleDialog - View Presupuesto details
 * RoKey MANAGEMENT - Multi-tenant SaaS ERP/POS for locksmiths
 * 
 * Phase 6: Presupuestos CRUD
 */

import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
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
  usePresupuesto,
} from '@/hooks';
import { Loader2, ArrowRight, X } from 'lucide-react';

interface PresupuestoDetalleDialogProps {
  presupuesto: { id: number; estado: string } | null;
  onOpenChange: (open: boolean) => void;
  onConvertir: (presupuesto: { id: number }) => void;
  onAnular: (presupuesto: { id: number }) => void;
  isConverting?: boolean;
  canManage?: boolean;
}

const ESTADO_COLORS: Record<string, string> = {
  Pendiente: 'bg-yellow-100 text-yellow-800',
  Aceptado: 'bg-green-100 text-green-800',
  Vencido: 'bg-red-100 text-red-800',
  Anulado: 'bg-gray-100 text-gray-800',
  Rechazado: 'bg-gray-100 text-gray-800',
};

export function PresupuestoDetalleDialog({
  presupuesto,
  onOpenChange,
  onConvertir,
  onAnular,
  isConverting = false,
  canManage = false,
}: PresupuestoDetalleDialogProps) {
  const { data: detalle, isLoading } = usePresupuesto(presupuesto?.id ?? null);

  if (!presupuesto) return null;

  return (
    <Dialog open={!!presupuesto} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle>Detalle del Presupuesto #{presupuesto.id}</DialogTitle>
          <DialogDescription>
            Información completa del presupuesto
          </DialogDescription>
        </DialogHeader>

        {isLoading ? (
          <div className="flex justify-center py-8">
            <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
          </div>
        ) : detalle ? (
          <div className="space-y-4">
            {/* Header Info */}
            <div className="grid grid-cols-2 gap-4">
              <div>
                <Label className="text-muted-foreground">Cliente</Label>
                <p className="font-medium">{detalle.nombreCliente || 'Consumidor Final'}</p>
              </div>
              <div>
                <Label className="text-muted-foreground">Estado</Label>
                <Badge className={ESTADO_COLORS[detalle.estado] || 'bg-gray-100'}>
                  {detalle.estado}
                </Badge>
              </div>
              <div>
                <Label className="text-muted-foreground">Fecha de Emisión</Label>
                <p>{new Date(detalle.fechaEmision).toLocaleDateString('es-AR')}</p>
              </div>
              <div>
                <Label className="text-muted-foreground">Fecha de Vencimiento</Label>
                <p>{new Date(detalle.fechaVencimiento).toLocaleDateString('es-AR')}</p>
              </div>
              <div>
                <Label className="text-muted-foreground">Vendedor</Label>
                <p>{detalle.nombreUsuario || '-'}</p>
              </div>
            </div>

            {/* Detalles */}
            <div>
              <Label className="text-muted-foreground">Productos</Label>
              <div className="mt-2 border rounded-lg overflow-hidden">
                <table className="w-full text-sm">
                  <thead className="bg-muted">
                    <tr>
                      <th className="px-3 py-2 text-left">Producto</th>
                      <th className="px-3 py-2 text-right">Cantidad</th>
                      <th className="px-3 py-2 text-right">Precio</th>
                      <th className="px-3 py-2 text-right">Subtotal</th>
                    </tr>
                  </thead>
                  <tbody>
                    {detalle.detalles?.map((detalle) => (
                      <tr key={detalle.id} className="border-t">
                        <td className="px-3 py-2">{detalle.nombreProducto}</td>
                        <td className="px-3 py-2 text-right">{detalle.cantidad}</td>
                        <td className="px-3 py-2 text-right">
                          ${detalle.precioPactado.toLocaleString('es-AR', { minimumFractionDigits: 2 })}
                        </td>
                        <td className="px-3 py-2 text-right">
                          ${(detalle.cantidad * detalle.precioPactado).toLocaleString('es-AR', { minimumFractionDigits: 2 })}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                  <tfoot className="bg-muted border-t-2">
                    <tr>
                      <td colSpan={3} className="px-3 py-2 text-right font-medium">Total:</td>
                      <td className="px-3 py-2 text-right font-bold">
                        ${detalle.total.toLocaleString('es-AR', { minimumFractionDigits: 2 })}
                      </td>
                    </tr>
                  </tfoot>
                </table>
              </div>
            </div>

            {/* Actions */}
            {detalle.estado === 'Pendiente' && canManage && (
              <DialogFooter className="gap-2 sm:gap-0">
                <Button
                  variant="outline"
                  onClick={() => onAnular(presupuesto)}
                  className="text-destructive"
                >
                  <X className="h-4 w-4 mr-2" />
                  Anular
                </Button>
                <Button
                  onClick={() => onConvertir(presupuesto)}
                  disabled={isConverting}
                >
                  {isConverting ? (
                    <>
                      <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                      Convirtiendo...
                    </>
                  ) : (
                    <>
                      <ArrowRight className="h-4 w-4 mr-2" />
                      Convertir a Venta
                    </>
                  )}
                </Button>
              </DialogFooter>
            )}
          </div>
        ) : (
          <p className="text-center text-muted-foreground py-4">
            No se pudo cargar el detalle del presupuesto
          </p>
        )}
      </DialogContent>
    </Dialog>
  );
}
