import { z } from 'zod';

/**
 * Schema for purchase form (create)
 * POST /api/v1/compras
 */
export const compraFormSchema = z.object({
  idProveedor: z.number().int().positive('Debe seleccionar un proveedor'),
  numeroComprobante: z.string().min(1, 'El número de comprobante es requerido').max(50),
  detalles: z.array(z.object({
    idProducto: z.number().int().positive('Debe seleccionar un producto'),
    cantidad: z.number().int().min(1, 'La cantidad debe ser mayor a 0'),
    precioUnitario: z.number().min(0, 'El precio no puede ser negativo'),
  })).min(1, 'Debe agregar al menos un producto a la compra'),
});

export type CompraFormData = z.infer<typeof compraFormSchema>;

/**
 * Schema for cancelling a purchase
 * POST /api/v1/compras/{id}/anular
 */
export const compraAnularSchema = z.object({
  motivo: z.string().min(1, 'El motivo es requerido').max(500),
});

export type CompraAnularData = z.infer<typeof compraAnularSchema>;

/**
 * Schema for purchase filters
 */
export const compraFiltersSchema = z.object({
  idProveedor: z.number().int().positive().optional(),
  fechaDesde: z.string().optional(),
  fechaHasta: z.string().optional(),
  anulada: z.boolean().optional(),
});

export type CompraFiltersData = z.infer<typeof compraFiltersSchema>;