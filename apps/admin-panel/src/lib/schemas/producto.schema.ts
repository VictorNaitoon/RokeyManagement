import { z } from 'zod';

/**
 * Schema for product form (create or edit)
 * POST /api/v1/productos, PUT /api/v1/productos/{id}
 */
export const productoFormSchema = z.object({
  nombre: z.string().min(1, 'El nombre es requerido').max(200, 'El nombre no puede exceder 200 caracteres'),
  codigoBusqueda: z.string().max(50, 'El código de búsqueda no puede exceder 50 caracteres').optional().nullable(),
  descripcion: z.string().max(500, 'La descripción no puede exceder 500 caracteres').optional().nullable(),
  precioCompra: z.number().min(0, 'El precio de compra debe ser mayor o igual a 0').optional().nullable(),
  precioVenta: z.number().min(0.01, 'El precio de venta debe ser mayor a 0'),
  stockActual: z.number().int('El stock debe ser un número entero').min(0, 'El stock no puede ser negativo').optional().nullable(),
  stockMinimo: z.number().int('El stock mínimo debe ser un número entero').min(0, 'El stock mínimo no puede ser negativo').optional().nullable(),
  idCategoria: z.number().int().positive().optional().nullable(),
  esServicio: z.boolean().optional().nullable(),
  imagenURL: z.string().url('La URL de la imagen debe ser válida').optional().nullable(),
  activo: z.boolean().optional().nullable(),
}).strict();

export type ProductoFormData = z.infer<typeof productoFormSchema>;

// Aliases for backwards compatibility
export type CrearProductoFormData = ProductoFormData;
export type ActualizarProductoFormData = ProductoFormData;

/**
 * Schema for product filters
 */
export const productoFiltersSchema = z.object({
  busqueda: z.string().optional(),
  idCategoria: z.number().int().positive().optional(),
  activo: z.boolean().optional(),
  esServicio: z.boolean().optional(),
});

export type ProductoFiltersFormData = z.infer<typeof productoFiltersSchema>;