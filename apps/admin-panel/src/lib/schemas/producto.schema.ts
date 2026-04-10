import { z } from 'zod';

/**
 * Schema for creating a new product
 * POST /api/v1/productos
 */
export const crearProductoSchema = z.object({
  nombre: z.string().min(1, 'El nombre es requerido').max(200, 'El nombre no puede exceder 200 caracteres'),
  codigoBusqueda: z.string().max(50, 'El código de búsqueda no puede exceder 50 caracteres').optional(),
  descripcion: z.string().max(500, 'La descripción no puede exceder 500 caracteres').optional(),
  precioCompra: z.number().min(0, 'El precio de compra debe ser mayor o igual a 0').optional().nullable(),
  precioVenta: z.number().min(0, 'El precio de venta debe ser mayor o igual a 0'),
  stockActual: z.number().int('El stock debe ser un número entero').min(0, 'El stock no puede ser negativo'),
  stockMinimo: z.number().int('El stock mínimo debe ser un número entero').min(0, 'El stock mínimo no puede ser negativo'),
  idCategoria: z.number().int('El ID de categoría debe ser un número entero').positive('Debe seleccionar una categoría'),
  esServicio: z.boolean().optional(),
  foto: z.string().url('La URL de la foto debe ser válida').optional().nullable(),
}).strict();

export type CrearProductoFormData = z.infer<typeof crearProductoSchema>;

/**
 * Schema for updating an existing product
 * PUT /api/v1/productos/{id}
 */
export const actualizarProductoSchema = z.object({
  nombre: z.string().min(1, 'El nombre es requerido').max(200, 'El nombre no puede exceder 200 caracteres').optional(),
  codigoBusqueda: z.string().max(50, 'El código de búsqueda no puede exceder 50 caracteres').optional(),
  descripcion: z.string().max(500, 'La descripción no puede exceder 500 caracteres').optional(),
  precioCompra: z.number().min(0, 'El precio de compra debe ser mayor o igual a 0').optional().nullable(),
  precioVenta: z.number().min(0, 'El precio de venta debe ser mayor o igual a 0').optional(),
  stockActual: z.number().int('El stock debe ser un número entero').min(0, 'El stock no puede ser negativo').optional(),
  stockMinimo: z.number().int('El stock mínimo debe ser un número entero').min(0, 'El stock mínimo no puede ser negativo').optional(),
  idCategoria: z.number().int('El ID de categoría debe ser un número entero').positive('Debe seleccionar una categoría').optional(),
  esServicio: z.boolean().optional(),
  foto: z.string().url('La URL de la foto debe ser válida').optional().nullable(),
}).strict();

export type ActualizarProductoFormData = z.infer<typeof actualizarProductoSchema>;

/**
 * Combined schema for product form (create or edit)
 */
export const productoFormSchema = crearProductoSchema;

export type ProductoFormData = z.infer<typeof productoFormSchema>;

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