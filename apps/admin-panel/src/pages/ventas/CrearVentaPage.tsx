/**
 * CrearVentaPage - Create new sale (POS) page
 * RoKey MANAGEMENT - Multi-tenant SaaS ERP/POS for locksmiths
 *
 * Phase 4: UI - Crear Venta (POS)
 */

import * as React from 'react';
import { useNavigate } from 'react-router-dom';
import { useForm, useFieldArray, useWatch } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Plus, Trash2, ShoppingCart, DollarSign, Package, Wallet, CheckCircle2 } from 'lucide-react';
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
import { Badge } from '@/components/ui/badge';
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

function StepTitle({ n, icon, title, extra }: { n: string; icon: React.ReactNode; title: string; extra?: React.ReactNode }) {
  return (
    <div className="flex items-center justify-between">
      <h3 className="font-semibold flex items-center gap-2 text-base">
        <span className="flex h-6 w-6 items-center justify-center rounded-full bg-primary text-xs font-bold text-primary-foreground">{n}</span>
        <span className="flex items-center gap-2 text-foreground">{icon}{title}</span>
      </h3>
      {extra}
    </div>
  );
}

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

  // FIX bug #2: useWatch subscribes to field-array changes reliably,
  // plain watch('detalles') could keep a stale reference and the total
  // would not update when adding a product from "Productos Seleccionados".
  const detalles = useWatch({ control, name: 'detalles' }) ?? [];
  const pagos = useWatch({ control, name: 'pagos' }) ?? [];

  const totalDetalle = React.useMemo(() => {
    return detalles.reduce((sum, d) => sum + (Number(d?.cantidad) || 0) * (Number(d?.precioUnitario) || 0), 0);
  }, [detalles]);

  const totalPagos = React.useMemo(() => {
    return pagos.reduce((sum, p) => sum + (Number(p?.monto) || 0), 0);
  }, [pagos]);

  const diferencia = totalDetalle - totalPagos;
  const diferenciaOk = Math.abs(diferencia) < 0.01;
  const cantidadItems = detalles.reduce((sum, d) => sum + (Number(d?.cantidad) || 0), 0);

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
      // Instead of blocking, increment quantity (better UX)
      const current = Number(detalles[existingIndex]?.cantidad) || 1;
      const max = producto.stockActual || 99;
      if (current + 1 > max) {
        toast.warning(`Stock máximo alcanzado (${max})`);
        return;
      }
      setValue(`detalles.${existingIndex}.cantidad`, current + 1, { shouldValidate: true, shouldDirty: true });
      return;
    }

    appendDetalle({
      idProducto: productoId,
      cantidad: 1,
      precioUnitario: producto.precioVenta,
    });
  };

  // FIX bug #3: "Agregar Pago" now prefills the remaining amount so the
  // sale can be registered without forcing the "Completar" button.
  const handleAddPago = () => {
    const restante = Math.max(totalDetalle - totalPagos, 0);
    appendPago({
      metodoPago: 'Efectivo',
      monto: restante > 0 ? Number(restante.toFixed(2)) : 0,
    });
  };

  const handleUpdateCantidad = (index: number, cantidad: number) => {
    const detalle = detalles[index];
    if (detalle && cantidad > 0) {
      setValue(`detalles.${index}.cantidad`, cantidad, { shouldValidate: true, shouldDirty: true });
    }
  };

  const handleUpdatePrecio = (index: number, precio: number) => {
    const detalle = detalles[index];
    if (detalle && precio > 0) {
      setValue(`detalles.${index}.precioUnitario`, precio, { shouldValidate: true, shouldDirty: true });
    }
  };

  // FIX bug #3 (math): previously it overwrote pagos[0] with `diferencia`,
  // which shrinks the total. Correct is current + diferencia.
  const handleCompletarPago = () => {
    if (pagos.length > 0) {
      const actual = Number(pagos[0]?.monto) || 0;
      setValue('pagos.0.monto', Number((actual + diferencia).toFixed(2)), { shouldValidate: true, shouldDirty: true });
    } else {
      appendPago({
        metodoPago: 'Efectivo',
        monto: Number(diferencia.toFixed(2)),
      });
    }
  };

  const handleFormSubmit = async (data: CrearVentaFormData) => {
    try {
      await createMutation.mutateAsync(data);
      navigate('/ventas');
    } catch {
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
            Agregá productos, revisá el detalle y cobrá
          </p>
        </div>
        <div className="flex items-center gap-2">
          <Badge variant="secondary" className="text-sm px-3 py-1">
            <ShoppingCart className="h-3.5 w-3.5 mr-1" />
            {cantidadItems} ítems
          </Badge>
          <Badge className="text-base px-4 py-1.5 bg-green-600 hover:bg-green-600">
            Total ${totalDetalle.toFixed(2)}
          </Badge>
        </div>
      </div>

      <form onSubmit={handleSubmit(handleFormSubmit)} className="space-y-6">
        {/* STEP 1 — lo primero que se ve: productos seleccionados */}
        <section className="bg-card border-2 border-primary/20 rounded-xl p-4 sm:p-5 space-y-4 shadow-sm">
          <StepTitle
            n="1"
            icon={<ShoppingCart className="h-4 w-4 text-primary" />}
            title="Productos Seleccionados"
            extra={
              detalleFields.length > 0 ? (
                <Badge variant="outline">{detalleFields.length} producto{detalleFields.length !== 1 ? 's' : ''}</Badge>
              ) : undefined
            }
          />

          {detalleFields.length === 0 ? (
            <div className="text-center py-8 border border-dashed rounded-lg bg-muted/30">
              <ShoppingCart className="h-8 w-8 mx-auto text-muted-foreground/60 mb-2" />
              <p className="font-medium">Todavía no hay productos en la venta</p>
              <p className="text-sm text-muted-foreground">
                Buscá abajo en el paso 2 y tocá <Plus className="inline h-3.5 w-3.5" /> para agregarlos acá
              </p>
            </div>
          ) : (
            <>
              {/* Header row (desktop) */}
              <div className="hidden md:grid grid-cols-[1fr_90px_120px_100px_40px] gap-2 px-2 text-xs font-medium text-muted-foreground uppercase tracking-wide">
                <span>Producto</span>
                <span className="text-center">Cant.</span>
                <span className="text-center">Precio unit.</span>
                <span className="text-right">Subtotal</span>
                <span />
              </div>
              <div className="space-y-2">
                {detalleFields.map((field, index) => {
                  const producto = productos.find(p => p.id === detalles[index]?.idProducto);
                  const subtotal = (Number(detalles[index]?.cantidad) || 0) * (Number(detalles[index]?.precioUnitario) || 0);
                  return (
                    <div
                      key={field.id}
                      className="grid grid-cols-1 md:grid-cols-[1fr_90px_120px_100px_40px] gap-2 items-center p-3 bg-muted/50 border rounded-lg"
                    >
                      <div className="min-w-0">
                        <p className="font-medium truncate">{producto?.nombre ?? `Producto #${detalles[index]?.idProducto}`}</p>
                        <p className="text-xs text-muted-foreground">
                          Stock: {producto?.stockActual ?? '—'}
                        </p>
                      </div>
                      <div className="flex items-center gap-1">
                        <Input
                          type="number"
                          min="1"
                          max={producto?.stockActual || 99}
                          className="h-9 text-center"
                          aria-label="Cantidad"
                          value={detalles[index]?.cantidad ?? 0}
                          onChange={(e) => handleUpdateCantidad(index, parseInt(e.target.value) || 1)}
                        />
                      </div>
                      <Input
                        type="number"
                        step="0.01"
                        min="0"
                        className="h-9 text-right"
                        aria-label="Precio unitario"
                        value={detalles[index]?.precioUnitario ?? 0}
                        onChange={(e) => handleUpdatePrecio(index, parseFloat(e.target.value) || 0)}
                      />
                      <p className="font-semibold text-right tabular-nums">
                        ${subtotal.toFixed(2)}
                      </p>
                      <Button
                        type="button"
                        variant="ghost"
                        size="icon"
                        aria-label="Quitar producto"
                        onClick={() => removeDetalle(index)}
                      >
                        <Trash2 className="h-4 w-4 text-destructive" />
                      </Button>
                    </div>
                  );
                })}
              </div>
              <div className="flex justify-end border-t pt-3">
                <p className="text-lg">
                  Total productos: <span className="font-bold text-green-600 tabular-nums">${totalDetalle.toFixed(2)}</span>
                </p>
              </div>
            </>
          )}

          {errors.detalles && (
            <p className="text-xs text-destructive">{errors.detalles.message || errors.detalles.root?.message}</p>
          )}
        </section>

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          {/* STEP 2 - Products Selection */}
          <div className="lg:col-span-2 bg-card border rounded-xl p-4 sm:p-5 space-y-4 h-fit">
            <StepTitle
              n="2"
              icon={<Package className="h-4 w-4 text-primary" />}
              title="Buscar y agregar productos"
            />

            <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
              <Input
                placeholder="Buscar por nombre o código..."
                value={searchProducto}
                onChange={(e) => setSearchProducto(e.target.value)}
              />
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

            {/* Product List */}
            <ScrollArea className="h-[320px] pr-2">
              {loadingProductos ? (
                <div className="flex items-center justify-center h-32">
                  <Loader2 className="h-6 w-6 animate-spin text-muted-foreground" />
                </div>
              ) : filteredProductos.length > 0 ? (
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-2">
                  {filteredProductos.map((producto) => {
                    const yaAgregado = detalles.some(d => d.idProducto === producto.id);
                    return (
                      <Button
                        key={producto.id}
                        type="button"
                        variant={yaAgregado ? 'secondary' : 'outline'}
                        className="h-auto justify-start text-left py-2.5 px-3"
                        onClick={() => handleAddProducto(producto.id)}
                      >
                        <div className="flex-1 min-w-0">
                          <p className="font-medium truncate">{producto.nombre}</p>
                          <p className="text-xs text-muted-foreground">
                            Stock: {producto.stockActual} • <span className="font-semibold text-foreground">${producto.precioVenta.toFixed(2)}</span>
                            {yaAgregado ? ' • en la venta ✓' : ''}
                          </p>
                        </div>
                        <Plus className="h-4 w-4 flex-shrink-0" />
                      </Button>
                    );
                  })}
                </div>
              ) : (
                <p className="text-muted-foreground text-center py-8">
                  No se encontraron productos
                </p>
              )}
            </ScrollArea>
          </div>

          {/* STEP 3 - Payment Summary (sticky) */}
          <div className="space-y-4 lg:sticky lg:top-4 h-fit">
            <div className="bg-card border rounded-xl p-4 sm:p-5 space-y-3">
              <StepTitle
                n="3"
                icon={<DollarSign className="h-4 w-4 text-primary" />}
                title="Resumen"
              />

              <div className="space-y-2 text-sm">
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Productos:</span>
                  <span className="font-medium tabular-nums">{cantidadItems} ítems</span>
                </div>
                <div className="flex justify-between text-lg font-bold border-t pt-2">
                  <span>Total:</span>
                  <span className="text-green-600 tabular-nums">${totalDetalle.toFixed(2)}</span>
                </div>
              </div>
            </div>

            {/* Payments */}
            <div className="bg-card border rounded-xl p-4 sm:p-5 space-y-4">
              <StepTitle
                n="4"
                icon={<Wallet className="h-4 w-4 text-primary" />}
                title="Cobro"
                extra={
                  diferenciaOk && totalDetalle > 0 ? (
                    <Badge className="bg-green-600 hover:bg-green-600"><CheckCircle2 className="h-3 w-3 mr-1" />Listo</Badge>
                  ) : undefined
                }
              />

              {pagoFields.map((field, index) => (
                <div key={field.id} className="flex items-center gap-2 p-2 bg-muted/40 border rounded-lg">
                  <Select
                    value={pagos[index]?.metodoPago || 'Efectivo'}
                    onValueChange={(value) => setValue(`pagos.${index}.metodoPago`, value as 'Efectivo' | 'TarjetaCredito' | 'TarjetaDebito' | 'Transferencia', { shouldValidate: true, shouldDirty: true })}
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
                    className="flex-1 tabular-nums"
                    placeholder="Monto"
                    aria-label="Monto del pago"
                    value={pagos[index]?.monto ?? ''}
                    onChange={(e) => setValue(`pagos.${index}.monto`, parseFloat(e.target.value) || 0, { shouldValidate: true, shouldDirty: true })}
                  />
                  <Button
                    type="button"
                    variant="ghost"
                    size="icon"
                    aria-label="Quitar pago"
                    onClick={() => removePago(index)}
                  >
                    <Trash2 className="h-4 w-4 text-destructive" />
                  </Button>
                </div>
              ))}

              {pagoFields.length === 0 && (
                <p className="text-sm text-muted-foreground text-center py-3 border border-dashed rounded-lg">
                  Tocá «Agregar pago» y se completa solo con el total
                </p>
              )}

              <div className="flex gap-2">
                <Button type="button" variant="outline" onClick={handleAddPago} disabled={totalDetalle === 0}>
                  <Plus className="h-4 w-4 mr-1" />
                  Agregar Pago
                </Button>
                {totalDetalle > 0 && !diferenciaOk && (
                  <Button type="button" variant="secondary" onClick={handleCompletarPago}>
                    Completar ${diferencia.toFixed(2)}
                  </Button>
                )}
              </div>

              {errors.pagos && (
                <p className="text-xs text-destructive">{errors.pagos.message || errors.pagos.root?.message}</p>
              )}

              <div className="border-t pt-2 space-y-1.5 text-sm">
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Total pagos:</span>
                  <span className="font-medium tabular-nums">${totalPagos.toFixed(2)}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Resta cobrar:</span>
                  <span className={diferenciaOk ? 'text-green-600 font-semibold tabular-nums' : 'text-destructive font-semibold tabular-nums'}>
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
                className="flex-1 bg-green-600 hover:bg-green-700"
                disabled={createMutation.isPending || totalDetalle === 0 || !diferenciaOk}
              >
                {createMutation.isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                Registrar Venta
              </Button>
            </div>
            {!diferenciaOk && totalDetalle > 0 && (
              <p className="text-xs text-muted-foreground text-center">
                El cobro debe igualar al total para registrar la venta
              </p>
            )}
          </div>
        </div>
      </form>
    </div>
  );
}

export default CrearVentaPage;
