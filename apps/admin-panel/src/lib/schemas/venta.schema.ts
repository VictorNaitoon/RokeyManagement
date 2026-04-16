import { z } from 'zod';

/**
 * Schema for crear venta (create sale)
 * POST /api/v1/ventas
 */

const detalleVentaSchema = z.object({
  idProducto: z.number().int().positive('Debe seleccionar un producto'),
  cantidad: z.number().int().positive('La cantidad debe ser mayor a cero'),
  precioUnitario: z.number().positive('El precio debe ser mayor a cero'),
});

const metodoPagoEnum = z.enum(['Efectivo', 'TarjetaCredito', 'TarjetaDebito', 'Transferencia']);

const pagoVentaSchema = z.object({
  metodoPago: metodoPagoEnum,
  monto: z.number().positive('El monto debe ser mayor a cero'),
});

export const crearVentaSchema = z.object({
  idCliente: z.number().int().positive().optional().nullable(),
  detalles: z.array(detalleVentaSchema)
    .min(1, 'Debe agregar al menos un producto a la venta'),
  pagos: z.array(pagoVentaSchema)
    .min(1, 'Debe registrar al menos un método de pago'),
}).refine(
  (data) => {
    // Calculate total from details
    const totalDetalle = data.detalles.reduce(
      (sum, d) => sum + d.cantidad * d.precioUnitario,
      0
    );
    // Calculate total from payments
    const totalPagos = data.pagos.reduce((sum, p) => sum + p.monto, 0);
    // They must match
    return Math.abs(totalDetalle - totalPagos) < 0.01;
  },
  {
    message: 'La suma de pagos debe coincidir con el total de la venta',
    path: ['pagos'],
  }
);

export type CrearVentaFormData = z.infer<typeof crearVentaSchema>;

/**
 * Schema for anular venta (cancel sale)
 * POST /api/v1/ventas/{id}/anular
 */
export const anularVentaSchema = z.object({
  motivo: z.string()
    .min(1, 'El motivo es requerido')
    .max(500, 'El motivo no puede exceder 500 caracteres'),
});

export type AnularVentaFormData = z.infer<typeof anularVentaSchema>;

/**
 * Schema for venta filters
 */
export const ventaFiltersSchema = z.object({
  fechaDesde: z.string().optional(),
  fechaHasta: z.string().optional(),
});

export type VentaFiltersFormData = z.infer<typeof ventaFiltersSchema>;
