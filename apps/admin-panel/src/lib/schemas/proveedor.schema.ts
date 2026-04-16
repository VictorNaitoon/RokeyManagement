import { z } from 'zod';

/**
 * Schema for supplier form (create)
 * POST /api/v1/proveedores
 */
export const proveedorFormSchema = z.object({
  nombre: z.string().min(1, 'El nombre es requerido').max(200, 'El nombre no puede exceder 200 caracteres'),
  telefono: z.string().max(50).optional().nullable(),
  email: z.string().email('El email debe ser válido').max(100).optional().nullable(),
});

export type ProveedorFormData = z.infer<typeof proveedorFormSchema>;

/**
 * Schema for supplier filters
 */
export const proveedorFiltersSchema = z.object({
  busqueda: z.string().optional(),
  activo: z.boolean().optional(),
});

export type ProveedorFiltersData = z.infer<typeof proveedorFiltersSchema>;