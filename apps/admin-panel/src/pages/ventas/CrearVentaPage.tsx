/**
 * CrearVentaPage - Create new sale (POS) page
 * RoKey MANAGEMENT - Multi-tenant SaaS ERP/POS for locksmiths
 * 
 * Phase 4: UI - Crear Venta (POS)
 */

import * as React from 'react';
import { useNavigate } from 'react-router-dom';
import { useForm, useFieldArray } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Plus, Trash2, ShoppingCart, DollarSign, Package } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { ScrollArea } from '@/components/ui/scroll-area';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { crearVentaSchema, type CrearVentaFormData } from '@/lib/schemas/venta.schema';
import { useProductos, useCreateVenta, useCategorias } from '@/hooks';
import { Loader2 } from 'lucide-react';
import { toast } from 'sonner';

const METODOS_PAGO = [
  { value: 'Efectivo', label: 'Efectivo' },
  { value: 'TarjetaCredito', label: 'Tarjeta de Crédito' },
  { value: 'TarjetaDebito', label: 'Tarjeta de Débito' },
  { value: 'Transferencia', label: 'Transferencia' },
] as const;

export function CrearVentaPage() {
  const navigate = useNavigate();
  
  // Queries
  const { data: productosData, isLoading: loadingProductos } = useProductos();
  const { data: categoriasData, isLoading: loadingCategorias } = useCategorias();
  
  // Mutations
  const createMutation = useCreateVenta();
  
  // Data
  const productos = productosData?.productos ?? [];
  const categorias = categoriasData?.categorias ?? [];
  
  // Form
  const {
    handleSubmit,
    setValue,
    watch,
    control,
    formState: { errors },
  } = useForm<CrearVentaFormData>({
    resolver: zodResolver(crearVentaSchema),
    defaultValues: {
      idCliente: undefined,
      detalles: [],
      pagos: [],
    },
  });
  
  // Field arrays for details and payments
  const { fields: detalleFields, append: appendDetalle, remove: removeDetalle } = useFieldArray({
    control,
    name: 'detalles',
  });
  
  const { fields: pagoFields, append: appendPago, remove: removePago } = useFieldArray({
    control,
    name: 'pagos',
  });
  
  // Filter states
  const [searchProducto, setSearchProducto] = React.useState('');
  const [categoriaFilter, setCategoriaFilter] = React.useState<number | null>(null);
  
  // Computed values
  const detalles = watch('detalles');
  const pagos = watch('pagos');
  
  const totalDetalle = React.useMemo(() => {
    return detalles.reduce((sum, d) => sum + (d.cantidad * d.precioUnitario), 0);
  }, [detalles]);
  
  const totalPagos = React.useMemo(() => {
    return pagos.reduce((sum, p) => sum + p.monto, 0);
  }, [pagos]);
  
  const diferencia = totalDetalle - totalPagos;
  
  // Filter productos
  const filteredProductos = React.useMemo(() => {
    let result = productos.filter(p => p.activo && p.stockActual > 0);
    
    if (categoriaFilter) {
      result = result.filter(p => p.idCategoria === categoriaFilter);
    }
    
    if (searchProducto) {
      const searchLower = searchProducto.toLowerCase();
      result = result.filter(p => 
        p.nombre.toLowerCase().includes(searchLower) ||
        (p.codigoBusqueda?.toLowerCase().includes(searchLower) ?? false)
      );
    }
    
    return result;
  }, [productos, categoriaFilter, searchProducto]);
  
  // Handlers
  const handleAddProducto = (productoId: number) => {
    const producto = productos.find(p => p.id === productoId);
    if (!producto) return;
    
    // Check if already added
    const existingIndex = detalles.findIndex(d => d.idProducto === productoId);
    if (existingIndex >= 0) {
      toast.warning('El producto ya está en la lista');
      return;
    }
    
    appendDetalle({
      idProducto: productoId,
      cantidad: 1,
      precioUnitario: producto.precioVenta,
    });
  };
  
  const handleAddPago = () => {
    appendPago({
      metodoPago: 'Efectivo',
      monto: 0,
    });
  };
  
  const handleUpdateCantidad = (index: number, cantidad: number) => {
    const detalle = detalles[index];
    if (detalle && cantidad > 0) {
      setValue(`detalles.${index}.cantidad`, cantidad);
    }
  };
  
  const handleUpdatePrecio = (index: number, precio: number) => {
    const detalle = detalles[index];
    if (detalle && precio > 0) {
      setValue(`detalles.${index}.precioUnitario`, precio);
    }
  };
  
  const handleCompletarPago = () => {
    // Clear existing pagos and add one with remaining amount
    if (pagos.length > 0) {
      // Update existing first payment
      setValue('pagos.0.monto', diferencia);
    } else {
      appendPago({
        metodoPago: 'Efectivo',
        monto: diferencia,
      });
    }
  };
  
  const handleFormSubmit = async (data: CrearVentaFormData) => {
    try {
      await createMutation.mutateAsync(data);
      navigate('/ventas');
    } catch (error) {
      // Error handled by mutation
    }
  };
  
  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold text-foreground">Nueva Venta</h1>
          <p className="text-muted-foreground">
            Registrar una nueva venta en el sistema
          </p>
        </div>
      </div>
      
      <form onSubmit={handleSubmit(handleFormSubmit)} className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Left Column - Products Selection */}
        <div className="lg:col-span-2 space-y-4">
          {/* Product Search and Filter */}
          <div className="bg-card border rounded-lg p-4 space-y-4">
            <h3 className="font-medium flex items-center gap-2">
              <Package className="h-4 w-4" />
              Seleccionar Productos
            </h3>
            
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div className="space-y-2">
                <Input
                  placeholder="Buscar producto..."
                  value={searchProducto}
                  onChange={(e) => setSearchProducto(e.target.value)}
                />
              </div>
              <div className="space-y-2">
                <Select
                  value={categoriaFilter ? String(categoriaFilter) : ''}
                  onValueChange={(value) => setCategoriaFilter(value ? Number(value) : null)}
                >
                  <SelectTrigger>
                    <SelectValue placeholder={loadingCategorias ? 'Cargando...' : 'Todas las categorías'} />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="">Todas las categorías</SelectItem>
                    {categorias.filter(c => c.activo).map((cat) => (
                      <SelectItem key={cat.id} value={String(cat.id)}>
                        {cat.nombre}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
            </div>
            
            {/* Product List */}
            <ScrollArea className="h-[300px]">
              {loadingProductos ? (
                <div className="flex items-center justify-center h-32">
                  <Loader2 className="h-6 w-6 animate-spin text-muted-foreground" />
                </div>
              ) : filteredProductos.length > 0 ? (
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-2">
                  {filteredProductos.map((producto) => (
                    <Button
                      key={producto.id}
                      type="button"
                      variant="outline"
                      className="h-auto justify-start text-left py-2 px-3"
                      onClick={() => handleAddProducto(producto.id)}
                    >
                      <div className="flex-1 min-w-0">
                        <p className="font-medium truncate">{producto.nombre}</p>
                        <p className="text-xs text-muted-foreground">
                          Stock: {producto.stockActual} • ${producto.precioVenta.toFixed(2)}
                        </p>
                      </div>
                      <Plus className="h-4 w-4 flex-shrink-0" />
                    </Button>
                  ))}
                </div>
              ) : (
                <p className="text-muted-foreground text-center py-8">
                  No se encontraron productos
                </p>
              )}
            </ScrollArea>
          </div>
          
          {/* Selected Products List */}
          <div className="bg-card border rounded-lg p-4 space-y-4">
            <h3 className="font-medium flex items-center gap-2">
              <ShoppingCart className="h-4 w-4" />
              Productos Seleccionados
            </h3>
            
            {detalleFields.length === 0 ? (
              <p className="text-muted-foreground text-center py-8">
                Agregue productos a la venta
              </p>
            ) : (
              <div className="space-y-2">
                {detalleFields.map((field, index) => {
                  const producto = productos.find(p => p.id === detalles[index]?.idProducto);
                  return (
                    <div
                      key={field.id}
                      className="flex items-center gap-2 p-2 bg-muted/50 rounded-lg"
                    >
                      <div className="flex-1 min-w-0">
                        <p className="font-medium">{producto?.nombre}</p>
                      </div>
                      <div className="flex items-center gap-2">
                        <Input
                          type="number"
                          min="1"
                          max={producto?.stockActual || 1}
                          className="w-16 h-8"
                          value={detalles[index]?.cantidad || 0}
                          onChange={(e) => handleUpdateCantidad(index, parseInt(e.target.value) || 1)}
                        />
                        <span className="text-sm text-muted-foreground">x</span>
                        <Input
                          type="number"
                          step="0.01"
                          min="0"
                          className="w-24 h-8"
                          value={detalles[index]?.precioUnitario || 0}
                          onChange={(e) => handleUpdatePrecio(index, parseFloat(e.target.value) || 0)}
                        />
                        <p className="font-medium w-20 text-right">
                          ${((detalles[index]?.cantidad || 0) * (detalles[index]?.precioUnitario || 0)).toFixed(2)}
                        </p>
                        <Button
                          type="button"
                          variant="ghost"
                          size="icon"
                          onClick={() => removeDetalle(index)}
                        >
                          <Trash2 className="h-4 w-4 text-destructive" />
                        </Button>
                      </div>
                    </div>
                  );
                })}
              </div>
            )}
            
            {errors.detalles && (
              <p className="text-xs text-destructive">{errors.detalles.message || errors.detalles.root?.message}</p>
            )}
          </div>
        </div>
        
        {/* Right Column - Payment Summary */}
        <div className="space-y-4">
          {/* Totals */}
          <div className="bg-card border rounded-lg p-4 space-y-4">
            <h3 className="font-medium flex items-center gap-2">
              <DollarSign className="h-4 w-4" />
              Resumen
            </h3>
            
            <div className="space-y-2">
              <div className="flex justify-between text-sm">
                <span className="text-muted-foreground">Subtotal productos:</span>
                <span className="font-medium">${totalDetalle.toFixed(2)}</span>
              </div>
              <div className="flex justify-between text-lg font-bold border-t pt-2">
                <span>Total:</span>
                <span className="text-green-600">${totalDetalle.toFixed(2)}</span>
              </div>
            </div>
          </div>
          
          {/* Payments */}
          <div className="bg-card border rounded-lg p-4 space-y-4">
            <h3 className="font-medium">Métodos de Pago</h3>
            
            {pagoFields.map((field, index) => (
              <div key={field.id} className="flex items-center gap-2">
                <Select
                  value={pagos[index]?.metodoPago || 'Efectivo'}
                  onValueChange={(value) => setValue(`pagos.${index}.metodoPago`, value as 'Efectivo' | 'TarjetaCredito' | 'TarjetaDebito' | 'Transferencia')}
                >
                  <SelectTrigger className="w-[140px]">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    {METODOS_PAGO.map((metodo) => (
                      <SelectItem key={metodo.value} value={metodo.value}>
                        {metodo.label}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                <Input
                  type="number"
                  step="0.01"
                  min="0"
                  className="flex-1"
                  placeholder="Monto"
                  value={pagos[index]?.monto || ''}
                  onChange={(e) => setValue(`pagos.${index}.monto`, parseFloat(e.target.value) || 0)}
                />
                <Button
                  type="button"
                  variant="ghost"
                  size="icon"
                  onClick={() => removePago(index)}
                >
                  <Trash2 className="h-4 w-4 text-destructive" />
                </Button>
              </div>
            ))}
            
            <div className="flex gap-2">
              <Button type="button" variant="outline" onClick={handleAddPago}>
                <Plus className="h-4 w-4 mr-1" />
                Agregar Pago
              </Button>
              {totalDetalle > 0 && diferencia > 0 && (
                <Button type="button" variant="outline" onClick={handleCompletarPago}>
                  Completar
                </Button>
              )}
            </div>
            
            {errors.pagos && (
              <p className="text-xs text-destructive">{errors.pagos.message || errors.pagos.root?.message}</p>
            )}
            
            <div className="border-t pt-2 space-y-2">
              <div className="flex justify-between text-sm">
                <span className="text-muted-foreground">Total pagos:</span>
                <span className="font-medium">${totalPagos.toFixed(2)}</span>
              </div>
              <div className="flex justify-between text-sm">
                <span className="text-muted-foreground">Diferencia:</span>
                <span className={diferencia === 0 ? 'text-green-600 font-medium' : 'text-destructive font-medium'}>
                  ${diferencia.toFixed(2)}
                </span>
              </div>
            </div>
          </div>
          
          {/* Submit */}
          <div className="flex gap-2">
            <Button
              type="button"
              variant="outline"
              className="flex-1"
              onClick={() => navigate('/ventas')}
            >
              Cancelar
            </Button>
            <Button
              type="submit"
              className="flex-1"
              disabled={createMutation.isPending || totalDetalle === 0 || diferencia !== 0}
            >
              {createMutation.isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
              Registrar Venta
            </Button>
          </div>
        </div>
      </form>
    </div>
  );
}

export default CrearVentaPage;
