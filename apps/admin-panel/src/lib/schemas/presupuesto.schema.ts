import { z } from 'zod';

/**
 * Schema for presupuesto form (create)
 * POST /api/v1/presupuestos
 */
export const presupuestoFormSchema = z.object({
  idCliente: z.number().int().positive().nullable(),
  fechaVencimiento: z.string().min(1, 'La fecha de vencimiento es requerida'),
  detalles: z.array(z.object({
    idProducto: z.number().int().positive('Debe seleccionar un producto'),
    cantidad: z.number().int().min(1, 'La cantidad debe ser mayor a 0'),
    precioPactado: z.number().min(0, 'El precio no puede ser negativo'),
  })).min(1, 'Debe agregar al menos un producto al presupuesto'),
});

export type PresupuestoFormData = z.infer<typeof presupuestoFormSchema>;

/**
 * Schema for updating presupuesto estado
 * PUT /api/v1/presupuestos/{id}
 */
export const presupuestoEstadoSchema = z.object({
  estado: z.enum(['Pendiente', 'Aceptado', 'Vencido']),
});

export type PresupuestoEstadoData = z.infer<typeof presupuestoEstadoSchema>;