/**
 * ProductoForm - Form component for creating/editing products
 * RoKey MANAGEMENT - Multi-tenant SaaS ERP/POS for locksmiths
 * 
 * Phase 4: Feature Components - Product form with validation
 */

import * as React from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Switch } from '@/components/ui/switch';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { ImageUpload } from '@/components/ui/ImageUpload';
import { productoFormSchema, type ProductoFormData } from '@/lib/schemas/producto.schema';
import { useCategorias, canViewPrecioCompra } from '@/hooks';
import { Loader2 } from 'lucide-react';

export interface ProductoFormProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  producto?: ProductoFormData & { id: number } | null;
  onSubmit: (data: unknown) => void;
  isLoading?: boolean;
}

export function ProductoForm({
  open,
  onOpenChange,
  producto,
  onSubmit,
  isLoading = false,
}: ProductoFormProps) {
  const isEditing = !!producto?.id;
  const canSeePrecioCompra = canViewPrecioCompra();
  
  // Get categorias for dropdown
  const { data: categoriasData, isLoading: loadingCategorias } = useCategorias();
  
  const categorias = categoriasData?.categorias ?? [];

  const {
    register,
    handleSubmit,
    setValue,
    watch,
    reset,
    formState: { errors },
  } = useForm<ProductoFormData>({
    resolver: zodResolver(productoFormSchema),
    defaultValues: {
      nombre: '',
      codigoBusqueda: '',
      descripcion: '',
      precioCompra: undefined,
      precioVenta: 0,
      stockActual: 0,
      stockMinimo: 0,
      idCategoria: undefined,
      esServicio: false,
      imagenURL: undefined,
    },
  });

  // Watch values for controlled inputs
  const watchIdCategoria = watch('idCategoria');
  const watchImagenURL = watch('imagenURL');

  // Reset form when product changes or dialog opens
  React.useEffect(() => {
    if (open) {
      if (producto?.id) {
        reset({
          nombre: producto.nombre ?? '',
          codigoBusqueda: producto.codigoBusqueda ?? '',
          descripcion: producto.descripcion ?? '',
          precioCompra: producto.precioCompra ?? undefined,
          precioVenta: producto.precioVenta ?? 0,
          stockActual: producto.stockActual ?? 0,
          stockMinimo: producto.stockMinimo ?? 0,
          idCategoria: producto.idCategoria ?? undefined,
          esServicio: producto.esServicio ?? false,
          imagenURL: producto.imagenURL ?? undefined,
        });
      } else {
        reset({
          nombre: '',
          codigoBusqueda: '',
          descripcion: '',
          precioCompra: undefined,
          precioVenta: 0,
          stockActual: 0,
          stockMinimo: 0,
          idCategoria: undefined,
          esServicio: false,
          imagenURL: undefined,
        });
      }
    }
  }, [open, producto, reset]);

  const handleImagenURLChange = (url: string) => {
    setValue('imagenURL', url || undefined, { shouldValidate: true });
  };

  const handleFormSubmit = (data: ProductoFormData) => {
    onSubmit(data);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-lg max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>{isEditing ? 'Editar Producto' : 'Nuevo Producto'}</DialogTitle>
          <DialogDescription>
            {isEditing
              ? 'Actualiza los datos del producto'
              : 'Completa los datos para crear un nuevo producto'}
          </DialogDescription>
        </DialogHeader>

        <form onSubmit={handleSubmit(handleFormSubmit)} className="space-y-4">
          {/* Nombre */}
          <div className="space-y-2">
            <Label htmlFor="nombre">Nombre *</Label>
            <Input
              id="nombre"
              {...register('nombre')}
              placeholder="Nombre del producto"
            />
            {errors.nombre && (
              <p className="text-xs text-destructive">{errors.nombre.message}</p>
            )}
          </div>

          {/* Código de búsqueda */}
          <div className="space-y-2">
            <Label htmlFor="codigoBusqueda">Código de búsqueda</Label>
            <Input
              id="codigoBusqueda"
              {...register('codigoBusqueda')}
              placeholder="Código interno"
            />
          </div>

          {/* Descripción */}
          <div className="space-y-2">
            <Label htmlFor="descripcion">Descripción</Label>
            <Input
              id="descripcion"
              {...register('descripcion')}
              placeholder="Descripción del producto"
            />
          </div>

          {/* Precio de venta */}
          <div className="space-y-2">
            <Label htmlFor="precioVenta">Precio de venta *</Label>
            <Input
              id="precioVenta"
              type="number"
              step="0.01"
              min="0"
              {...register('precioVenta', { valueAsNumber: true })}
              placeholder="0.00"
            />
            {errors.precioVenta && (
              <p className="text-xs text-destructive">{errors.precioVenta.message}</p>
            )}
          </div>

          {/* Precio de compra - Only for Admin */}
          {canSeePrecioCompra && (
            <div className="space-y-2">
              <Label htmlFor="precioCompra">Precio de compra</Label>
              <Input
                id="precioCompra"
                type="number"
                step="0.01"
                min="0"
                {...register('precioCompra', { valueAsNumber: true })}
                placeholder="0.00"
              />
              {errors.precioCompra && (
                <p className="text-xs text-destructive">{errors.precioCompra.message}</p>
              )}
            </div>
          )}

          {/* Stock actual */}
          <div className="space-y-2">
            <Label htmlFor="stockActual">Stock actual</Label>
            <Input
              id="stockActual"
              type="number"
              min="0"
              {...register('stockActual', { valueAsNumber: true })}
              placeholder="0"
            />
            {errors.stockActual && (
              <p className="text-xs text-destructive">{errors.stockActual.message}</p>
            )}
          </div>

          {/* Stock mínimo */}
          <div className="space-y-2">
            <Label htmlFor="stockMinimo">Stock mínimo (alerta)</Label>
            <Input
              id="stockMinimo"
              type="number"
              min="0"
              {...register('stockMinimo', { valueAsNumber: true })}
              placeholder="0"
            />
            {errors.stockMinimo && (
              <p className="text-xs text-destructive">{errors.stockMinimo.message}</p>
            )}
          </div>

          {/* Categoría */}
          <div className="space-y-2">
            <Label htmlFor="idCategoria">Categoría *</Label>
            <Select
              value={watchIdCategoria ? String(watchIdCategoria) : ''}
              onValueChange={(value) => setValue('idCategoria', Number(value), { shouldValidate: true })}
              disabled={loadingCategorias}
            >
              <SelectTrigger>
                <SelectValue placeholder={loadingCategorias ? 'Cargando...' : 'Seleccionar categoría'} />
              </SelectTrigger>
              <SelectContent>
                {categorias.map((cat) => (
                  <SelectItem key={cat.id} value={String(cat.id)}>
                    {cat.nombre}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            {errors.idCategoria && (
              <p className="text-xs text-destructive">{errors.idCategoria.message}</p>
            )}
          </div>

          {/* Es servicio */}
          <div className="flex items-center gap-2">
            <Switch
              id="esServicio"
              checked={watch('esServicio') ?? false}
              onCheckedChange={(checked) => setValue('esServicio', checked)}
            />
            <Label htmlFor="esServicio" className="cursor-pointer">
              Es servicio (sin stock)
            </Label>
          </div>

          {/* Imagen */}
          <div className="space-y-2">
            <Label>Imagen del producto</Label>
            <ImageUpload
              value={watchImagenURL ?? null}
              onChange={handleImagenURLChange}
              placeholder="Subir imagen"
            />
          </div>

          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={() => onOpenChange(false)}
              disabled={isLoading}
            >
              Cancelar
            </Button>
            <Button type="submit" disabled={isLoading}>
              {isLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
              {isEditing ? 'Actualizar' : 'Crear'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

export default ProductoForm;