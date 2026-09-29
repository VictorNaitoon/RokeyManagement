/**
 * VentasDetallesModal - Modal to view sale details with print functionality
 * RoKey MANAGEMENT - Multi-tenant SaaS ERP/POS for locksmiths
 * 
 * Phase 5: UI - Detalles Modal (Redesigned for ticket/receipt style)
 */

import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Separator } from '@/components/ui/separator';
import { useVentaDetalles, useVentaPagos } from '@/hooks';
import { useNegocio } from '@/hooks/useNegocio';
import type { Venta } from '@/types';
import { PrinterIcon } from 'lucide-react';

interface VentasDetallesModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  venta: Venta | null;
}

export function VentasDetallesModal({ open, onOpenChange, venta }: VentasDetallesModalProps) {
  const { data: detalles, isLoading: loadingDetalles } = useVentaDetalles(venta?.id ?? null);
  const { data: pagos, isLoading: loadingPagos } = useVentaPagos(venta?.id ?? null);
  const { data: negocio } = useNegocio();

  if (!venta) return null;

  const formatDate = (dateStr: string) => {
    const date = new Date(dateStr);
    return date.toLocaleDateString('es-AR', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
      hour12: false,
    });
  };

  // Normaliza detalle: el backend devuelve PascalCase (CantidadVendida, ProductoNombre, PrecioUnitario)
  const getCantidad = (d: any) => d.cantidadVendida ?? d.CantidadVendida ?? d.cantidad ?? d.Cantidad ?? '-';
  const getProductoNombre = (d: any) => d.productoNombre ?? d.ProductoNombre ?? d.nombreProducto ?? d.NombreProducto ?? d.nombre ?? d.Nombre ?? '-';
  const getPrecioUnitario = (d: any) => d.precioUnitario ?? d.PrecioUnitario ?? d.precio ?? d.Precio ?? 0;

  // La API devuelve números: 1=Efectivo, 2=Tarjeta Crédito, 3=Tarjeta Débito, 4=Transferencia
  const getMetodoPagoLabel = (metodo: string | number) => {
    const num = typeof metodo === 'string' ? parseInt(metodo, 10) : metodo;
    const labels: Record<number, string> = {
      0: 'Efectivo',
      1: 'Efectivo',
      2: 'Tarjeta de Crédito',
      3: 'Tarjeta de Débito',
      4: 'Transferencia',
    };
    // Si ya viene como string (Efectivo, TarjetaCredito, etc.), mapear también
    if (typeof metodo === 'string' && isNaN(num)) {
      const strLabels: Record<string, string> = {
        'Efectivo': 'Efectivo',
        'TarjetaCredito': 'Tarjeta de Crédito',
        'TarjetaDebito': 'Tarjeta de Débito',
        'Transferencia': 'Transferencia',
      };
      return strLabels[metodo] || metodo;
    }
    return labels[num] || 'Desconocido';
  };

  const getEstadoLabel = (estado: string) => {
    return estado === 'Activa' ? 'Pagado' : 'Anulada';
  };

  const handlePrint = () => {
    const printContent = document.getElementById('venta-detalle-print');
    if (!printContent) return;
    
    const htmlContent = printContent.innerHTML;
    const printWindow = window.open('', '_blank', 'width=400,height=600');
    
    if (printWindow) {
      printWindow.document.write(`
        <!DOCTYPE html>
        <html>
          <head>
            <title>Detalle de Venta ${venta.id}</title>
            <style>
              body { font-family: 'Courier New', Courier, monospace; padding: 20px; color: #000; margin: 0; }
              .text-center { text-align: center; }
              .text-left { text-align: left; }
              .font-bold { font-weight: bold; }
              .line { border-bottom: 1px dashed #000; margin: 10px 0; }
              table { width: 100%; border-collapse: collapse; margin: 10px 0; }
              th, td { padding: 5px; text-align: center; border-bottom: 1px solid #000; }
              th { font-weight: bold; }
              .total-row-print { display: table; width: 100%; font-weight: bold; font-size: 1.1em; margin: 10px 0; }
              .total-row-print > span { display: table-cell; }
              .total-left { text-align: left; }
              .total-center { text-align: center; }
              .total-right { text-align: right; }
              @media print {
                body { padding: 0; }
                .no-print { display: none; }
                .only-print { display: inline; }
                @page { margin: 0; size: auto; }
              }
              .only-print { display: none; }
            </style>
          </head>
          <body onload="window.print();">
            ${htmlContent}
          </body>
        </html>
      `);
      printWindow.document.close();
      printWindow.focus();
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-md max-h-[90vh] overflow-y-auto p-6" style={{ color: '#000' }}>
        <DialogHeader className="pb-4">
          <div className="flex items-center justify-between">
            <DialogTitle className="text-lg font-bold" style={{ color: '#000' }}>
              Detalle de Venta
            </DialogTitle>
            <Button
              variant="outline"
              size="icon"
              onClick={handlePrint}
              className="no-print"
              aria-label="Imprimir detalle de venta"
            >
              <PrinterIcon className="h-4 w-4" />
            </Button>
          </div>
        </DialogHeader>

        <div id="venta-detalle-print" style={{ color: '#000' }}>
          {/* 1. Nombre de local como título - CENTRADO */}
          <div className="text-center mb-4">
            <h2 className="text-xl font-bold mb-1" style={{ color: '#000' }}>
              {negocio?.nombre || 'Mi Local'}
            </h2>
            
            {/* 2. Ubicación del local - CENTRADA */}
            {negocio?.direccion && (
              <p className="text-sm mb-1" style={{ color: '#000' }}>
                {negocio.direccion}
              </p>
            )}

            {/* 3. Celular para comunicarse - CENTRADO */}
            {negocio?.telefono && (
              <p className="text-sm mb-1" style={{ color: '#000' }}>
                Tel: {negocio.telefono}
              </p>
            )}
          </div>

          <div className="line mb-4" style={{ borderColor: '#000' }} />

          {/* 4. Número de venta - CENTRADO, SIN # */}
          <div className="text-center mb-2" style={{ color: '#000' }}>
            <span className="font-bold">Venta N°: </span>
            <span>{venta.id}</span>
          </div>

          {/* 5. Fecha y hora - CENTRADA */}
          <div className="text-center mb-2" style={{ color: '#000' }}>
            <span className="font-bold">Fecha: </span>
            <span>{formatDate(venta.fecha)}</span>
          </div>

          {/* 6. Usuario que realizó la venta y estado - IZQUIERDA (no centrados) */}
          <div className="mb-2" style={{ color: '#000' }}>
            <span className="font-bold">Vendedor: </span>
            <span>{venta.usuarioNombre || '-'}</span>
          </div>
          <div className="mb-2" style={{ color: '#000' }}>
            <span className="font-bold">Estado: </span>
            <span style={{ color: '#000' }}>
              {getEstadoLabel(venta.estado)}
            </span>
          </div>

          <div className="line my-4" style={{ borderColor: '#000' }} />

          {/* 7. Tabla con Cant. | Producto | Valor (precio unitario) - TODO CENTRADO */}
          <div className="mb-4">
            <table style={{ color: '#000', width: '100%' }}>
              <thead>
                <tr>
                  <th className="pb-2" style={{ color: '#000', borderBottom: '1px solid #000', textAlign: 'center' }}>Cant.</th>
                  <th className="pb-2" style={{ color: '#000', borderBottom: '1px solid #000', textAlign: 'center' }}>Producto</th>
                  <th className="pb-2" style={{ color: '#000', borderBottom: '1px solid #000', textAlign: 'center' }}>Valor</th>
                </tr>
              </thead>
              <tbody>
                {loadingDetalles ? (
                  <tr>
                    <td colSpan={3} className="py-4" style={{ color: '#000', textAlign: 'center' }}>Cargando...</td>
                  </tr>
                ) : detalles && detalles.length > 0 ? (
                  detalles.map((detalle) => (
                    <tr key={detalle.id} style={{ color: '#000' }}>
                      <td className="py-1" style={{ color: '#000', textAlign: 'center' }}>
                        {getCantidad(detalle)}
                      </td>
                      <td className="py-1" style={{ color: '#000', textAlign: 'center' }}>
                        {getProductoNombre(detalle)}
                      </td>
                      <td className="py-1" style={{ color: '#000', textAlign: 'center' }}>
                        ${Number(getPrecioUnitario(detalle)).toFixed(2)}
                      </td>
                    </tr>
                  ))
                ) : (
                  <tr>
                    <td colSpan={3} className="py-4" style={{ color: '#000', textAlign: 'center' }}>No hay productos registrados</td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>

          <div className="line mb-4" style={{ borderColor: '#000' }} />

          {/* 8. Total: En modal simple "Total: $precio" a la izquierda. En impresión: 3 columnas */}
          <div style={{ color: '#000', display: 'flex', justifyContent: 'space-between', fontWeight: 'bold', fontSize: '1.1em', margin: '10px 0' }}>
            <span>Total:</span>
            <span className="only-print"></span>
            <span>${venta.totalVenta.toFixed(2)}</span>
          </div>

          <div className="line my-4" style={{ borderColor: '#000' }} />

          {/* 9. Método de pago: "Método de pago: Efectivo" (con label, sin números) */}
          <div className="mb-4" style={{ color: '#000' }}>
            {loadingPagos ? (
              <p className="text-center" style={{ color: '#000' }}>Cargando...</p>
            ) : pagos && pagos.length > 0 ? (
              <div className="space-y-1" style={{ color: '#000' }}>
                {pagos.map((pago) => (
                  <div key={pago.id} className="text-center" style={{ color: '#000' }}>
                    <span className="font-bold">Método de pago: </span>
                    <span>{getMetodoPagoLabel(pago.metodoPago)}</span>
                  </div>
                ))}
              </div>
            ) : (
              <p className="text-center" style={{ color: '#000' }}>No hay pagos registrados</p>
            )}
          </div>

          <div className="line my-4" style={{ borderColor: '#000' }} />

          {/* 10. Muchas gracias por su compra - CENTRADA */}
          <div className="text-center mt-4 mb-2">
            <p className="text-lg font-bold" style={{ color: '#000' }}>
              ¡Muchas gracias por su compra!
            </p>
          </div>
        </div>

        {/* Print button at bottom for easy access */}
        <div className="mt-6 pt-4 border-t no-print" style={{ borderColor: '#000' }}>
          <Button 
            className="w-full" 
            onClick={handlePrint}
            style={{ backgroundColor: '#000', color: '#fff', borderColor: '#000' }}
          >
            <PrinterIcon className="h-4 w-4 mr-2" />
            Imprimir Detalle
          </Button>
        </div>
      </DialogContent>
    </Dialog>
  );
}