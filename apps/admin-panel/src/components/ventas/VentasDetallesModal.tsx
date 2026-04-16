/**
 * VentasDetallesModal - Modal to view sale details
 * RoKey MANAGEMENT - Multi-tenant SaaS ERP/POS for locksmiths
 * 
 * Phase 5: UI - Detalles Modal
 */

import * as React from 'react';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Badge } from '@/components/ui/badge';
import { useVentaDetalles, useVentaPagos } from '@/hooks';
import type { Venta } from '@/types';
import { Separator } from '@/components/ui/separator';

interface VentasDetallesModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  venta: Venta | null;
}

export function VentasDetallesModal({ open, onOpenChange, venta }: VentasDetallesModalProps) {
  const { data: detalles, isLoading: loadingDetalles } = useVentaDetalles(venta?.id ?? null);
  const { data: pagos, isLoading: loadingPagos } = useVentaPagos(venta?.id ?? null);

  if (!venta) return null;

  const formatDate = (dateStr: string) => {
    const date = new Date(dateStr);
    return date.toLocaleDateString('es-AR', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  const getMetodoPagoLabel = (metodo: string) => {
    const labels: Record<string, string> = {
      'Efectivo': 'Efectivo',
      'TarjetaCredito': 'Tarjeta de Crédito',
      'TarjetaDebito': 'Tarjeta de Débito',
      'Transferencia': 'Transferencia',
    };
    return labels[metodo] || metodo;
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-lg max-h-[80vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            Detalles de Venta #{venta.id}
            {venta.estado === 'Anulada' && (
              <Badge variant="destructive">Anulada</Badge>
            )}
          </DialogTitle>
        </DialogHeader>

        <div className="space-y-4">
          {/* Sale Info */}
          <div className="grid grid-cols-2 gap-4 text-sm">
            <div>
              <span className="text-muted-foreground">Fecha:</span>
              <p className="font-medium">{formatDate(venta.fecha)}</p>
            </div>
            <div>
              <span className="text-muted-foreground">Vendedor:</span>
              <p className="font-medium">{venta.usuarioNombre || '-'}</p>
            </div>
            <div>
              <span className="text-muted-foreground">Cliente:</span>
              <p className="font-medium">{venta.clienteNombre || 'Consumidor Final'}</p>
            </div>
            <div>
              <span className="text-muted-foreground">Total:</span>
              <p className="font-medium text-green-600 text-lg">
                ${venta.totalVenta.toFixed(2)}
              </p>
            </div>
          </div>

          {/* Anulation Info */}
          {venta.estado === 'Anulada' && (
            <>
              <Separator />
              <div className="bg-destructive/10 p-3 rounded-lg">
                <p className="text-sm text-destructive font-medium">Venta Anulada</p>
                <p className="text-xs text-muted-foreground">
                  {venta.anulacionUsuarioNombre && `Por: ${venta.anulacionUsuarioNombre}`}
                  {venta.anulacionFecha && ` el ${formatDate(venta.anulacionFecha)}`}
                </p>
                {venta.motivoAnulacion && (
                  <p className="text-sm mt-1">Motivo: {venta.motivoAnulacion}</p>
                )}
              </div>
            </>
          )}

          <Separator />

          {/* Products (Detalles) */}
          <div>
            <h4 className="font-medium mb-2">Productos</h4>
            {loadingDetalles ? (
              <p className="text-muted-foreground text-sm">Cargando...</p>
            ) : detalles && detalles.length > 0 ? (
              <div className="space-y-2">
                {detalles.map((detalle) => (
                  <div
                    key={detalle.id}
                    className="flex justify-between items-center text-sm border-b pb-2 last:border-0"
                  >
                    <div>
                      <p className="font-medium">{detalle.productoNombre}</p>
                      <p className="text-muted-foreground text-xs">
                        {detalle.cantidadVendida} x ${detalle.precioUnitario.toFixed(2)}
                      </p>
                    </div>
                    <p className="font-medium">${detalle.subtotal.toFixed(2)}</p>
                  </div>
                ))}
              </div>
            ) : (
              <p className="text-muted-foreground text-sm">No hay productos registrados</p>
            )}
          </div>

          <Separator />

          {/* Payments (Pagos) */}
          <div>
            <h4 className="font-medium mb-2">Métodos de Pago</h4>
            {loadingPagos ? (
              <p className="text-muted-foreground text-sm">Cargando...</p>
            ) : pagos && pagos.length > 0 ? (
              <div className="space-y-2">
                {pagos.map((pago) => (
                  <div
                    key={pago.id}
                    className="flex justify-between items-center text-sm border-b pb-2 last:border-0"
                  >
                    <span className="font-medium">{getMetodoPagoLabel(pago.metodoPago)}</span>
                    <span className="font-medium">${pago.monto.toFixed(2)}</span>
                  </div>
                ))}
                <div className="flex justify-between items-center text-sm font-bold pt-2">
                  <span>Total</span>
                  <span className="text-green-600">
                    ${pagos.reduce((sum, p) => sum + p.monto, 0).toFixed(2)}
                  </span>
                </div>
              </div>
            ) : (
              <p className="text-muted-foreground text-sm">No hay pagos registrados</p>
            )}
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
}
