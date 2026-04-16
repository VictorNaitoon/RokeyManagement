import { z } from 'zod';

/**
 * Schema for client form (create)
 * POST /api/v1/clientes
 */
export const clienteFormSchema = z.object({
  nombre: z.string().min(1, 'El nombre es requerido').max(200, 'El nombre no puede exceder 200 caracteres'),
  apellido: z.string().max(100).optional().nullable(),
  email: z.string().email('El email debe ser válido').max(100).optional().nullable(),
  documento: z.string().max(20).optional().nullable(),
  condicionIVA: z.string().max(50).optional().nullable(),
  telefono: z.string().max(50).optional().nullable(),
  direccion: z.string().max(200).optional().nullable(),
  permiteFiado: z.boolean().optional().nullable(),
  limiteCredito: z.number().min(0).optional().nullable(),
});

export type ClienteFormData = z.infer<typeof clienteFormSchema>;

/**
 * Schema for client filters
 */
export const clienteFiltersSchema = z.object({
  busqueda: z.string().optional(),
  activo: z.boolean().optional(),
});

export type ClienteFiltersData = z.infer<typeof clienteFiltersSchema>;