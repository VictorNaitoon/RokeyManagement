import { z } from 'zod';

/**
 * Schema for creating a new category
 * POST /api/v1/categorias
 */
export const crearCategoriaSchema = z.object({
  nombre: z.string().min(1, 'El nombre es requerido').max(100, 'El nombre no puede exceder 100 caracteres'),
  descripcion: z.string().max(500, 'La descripción no puede exceder 500 caracteres').optional().nullable(),
});

export type CrearCategoriaFormData = z.infer<typeof crearCategoriaSchema>;

/**
 * Schema for updating an existing category
 * PUT /api/v1/categorias/{id}
 */
export const actualizarCategoriaSchema = z.object({
  nombre: z.string().min(1, 'El nombre es requerido').max(100, 'El nombre no puede exceder 100 caracteres').optional(),
  descripcion: z.string().max(500, 'La descripción no puede exceder 500 caracteres').optional().nullable(),
  activo: z.boolean().optional(),
});

export type ActualizarCategoriaFormData = z.infer<typeof actualizarCategoriaSchema>;

/**
 * Combined schema for category form (create or edit)
 */
export const categoriaFormSchema = crearCategoriaSchema;

export type CategoriaFormData = z.infer<typeof categoriaFormSchema>;

/**
 * Schema for category filters
 */
export const categoriaFiltersSchema = z.object({
  busqueda: z.string().optional(),
  activo: z.boolean().optional(),
});

export type CategoriaFiltersFormData = z.infer<typeof categoriaFiltersSchema>;