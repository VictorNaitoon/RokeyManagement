import { useState } from 'react';
import { format } from 'date-fns';
import { es } from 'date-fns/locale';
import { useInformes } from '@/hooks/useInformes';
import { FilterBar } from '@/components/informes/FilterBar';
import { ReportSection } from '@/components/informes/ReportSection';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow, TableCaption } from '@/components/ui/table';
import type { InformesFilter } from '@/types/informes.types';
import { INFORME_TIPO } from '@/types/informes.types';

const currency = new Intl.NumberFormat('es-AR', { style: 'currency', currency: 'ARS' });

function EmptyTableCaption() {
  return <TableCaption>No hay datos para el periodo seleccionado</TableCaption>;
}

export function InformesPage() {
  const [filter, setFilter] = useState<InformesFilter>({ preset: 'mes', cantidad: 10 });
  const { ventasResumen, productosTop, flujoCaja, ingresosGastos, alertasStock, ventasPorPago, ventasPorVendedor } =
    useInformes(filter);

  const periodoLabel = filter.fechaDesde && filter.fechaHasta
    ? `${format(new Date(filter.fechaDesde), 'dd/MM/yyyy', { locale: es })} - ${format(new Date(filter.fechaHasta), 'dd/MM/yyyy', { locale: es })}`
    : filter.preset ?? 'mes';

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold">Informes</h1>
        <p className="text-sm text-muted-foreground">Periodo: {periodoLabel}</p>
      </div>

      <FilterBar filter={filter} onApply={setFilter} />

      <ReportSection
        title="Resumen de ventas"
        description={ventasResumen.data?.periodo}
        isLoading={ventasResumen.isLoading}
        isError={ventasResumen.isError}
        error={ventasResumen.error}
        onRetry={() => ventasResumen.refetch()}
        isEmpty={!!ventasResumen.data && ventasResumen.data.cantidadVentas === 0}
        tipo={INFORME_TIPO.VENTAS_RESUMEN}
        filter={filter}
      >
        {ventasResumen.data && (
          <div className="grid gap-4 grid-cols-1 md:grid-cols-3">
            <Card>
              <CardHeader>
                <CardTitle className="text-sm text-muted-foreground">Total ventas</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">{currency.format(ventasResumen.data.totalVentas)}</div>
                <p className="text-xs text-muted-foreground">{ventasResumen.data.cantidadVentas} ventas</p>
              </CardContent>
            </Card>
            <Card>
              <CardHeader>
                <CardTitle className="text-sm text-muted-foreground">Ticket promedio</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">{currency.format(ventasResumen.data.ticketPromedio)}</div>
              </CardContent>
            </Card>
            <Card>
              <CardHeader>
                <CardTitle className="text-sm text-muted-foreground">Ventas anuladas</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">{ventasResumen.data.ventasAnuladas}</div>
              </CardContent>
            </Card>
          </div>
        )}
      </ReportSection>

      <div className="grid gap-6 md:grid-cols-2">
        <ReportSection
          title="Ingresos y gastos"
          isLoading={ingresosGastos.isLoading}
          isError={ingresosGastos.isError}
          error={ingresosGastos.error}
          onRetry={() => ingresosGastos.refetch()}
          isEmpty={!!ingresosGastos.data && ingresosGastos.data.ventasTotales === 0 && ingresosGastos.data.comprasTotales === 0}
          tipo={INFORME_TIPO.INGRESOS_GASTOS}
          filter={filter}
        >
          {ingresosGastos.data && (
            <div className="space-y-2">
              <div className="flex justify-between">
                <span>Ventas totales</span>
                <span className="font-medium">{currency.format(ingresosGastos.data.ventasTotales)}</span>
              </div>
              <div className="flex justify-between">
                <span>Compras totales</span>
                <span className="font-medium">{currency.format(ingresosGastos.data.comprasTotales)}</span>
              </div>
              <div className="flex justify-between border-t pt-2">
                <span>Ganancia bruta</span>
                <span className="font-bold">{currency.format(ingresosGastos.data.gananciaBruta)}</span>
              </div>
              <p className="text-xs text-muted-foreground">Margen: {ingresosGastos.data.margenPorcentaje}%</p>
            </div>
          )}
        </ReportSection>

        <ReportSection
          title="Flujo de caja"
          description="Solo incluye movimientos de cajas cerradas"
          isLoading={flujoCaja.isLoading}
          isError={flujoCaja.isError}
          error={flujoCaja.error}
          onRetry={() => flujoCaja.refetch()}
          isEmpty={!!flujoCaja.data && flujoCaja.data.movimientosIngreso === 0 && flujoCaja.data.movimientosEgreso === 0}
          tipo={INFORME_TIPO.FLUJO_CAJA}
          filter={filter}
        >
          {flujoCaja.data && (
            <div className="space-y-2">
              <div className="flex justify-between">
                <span>Ingresos</span>
                <span className="font-medium text-emerald-600">{currency.format(flujoCaja.data.ingresos)}</span>
              </div>
              <div className="flex justify-between">
                <span>Egresos</span>
                <span className="font-medium text-red-600">{currency.format(flujoCaja.data.egresos)}</span>
              </div>
              <div className="flex justify-between border-t pt-2">
                <span>Balance</span>
                <span className="font-bold">{currency.format(flujoCaja.data.balance)}</span>
              </div>
              <p className="text-xs text-muted-foreground">
                {flujoCaja.data.movimientosIngreso} ingresos / {flujoCaja.data.movimientosEgreso} egresos
              </p>
            </div>
          )}
        </ReportSection>
      </div>

      <ReportSection
        title="Productos más vendidos"
        isLoading={productosTop.isLoading}
        isError={productosTop.isError}
        error={productosTop.error}
        onRetry={() => productosTop.refetch()}
        isEmpty={!!productosTop.data && productosTop.data.productos.length === 0}
        tipo={INFORME_TIPO.PRODUCTOS_TOP}
        filter={filter}
      >
        {productosTop.data && productosTop.data.productos.length > 0 && (
          <Table>
            <TableCaption>Productos ordenados por ingresos</TableCaption>
            <TableHeader>
              <TableRow>
                <TableHead>Producto</TableHead>
                <TableHead className="text-right">Cantidad</TableHead>
                <TableHead className="text-right">Monto total</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {productosTop.data.productos.map((p) => (
                <TableRow key={p.idProducto}>
                  <TableCell>{p.nombre}</TableCell>
                  <TableCell className="text-right">{p.cantidadVendida}</TableCell>
                  <TableCell className="text-right">{currency.format(p.montoTotal)}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        )}
        {productosTop.data && productosTop.data.productos.length === 0 && <EmptyTableCaption />}
      </ReportSection>

      <ReportSection
        title="Ventas por método de pago"
        isLoading={ventasPorPago.isLoading}
        isError={ventasPorPago.isError}
        error={ventasPorPago.error}
        onRetry={() => ventasPorPago.refetch()}
        isEmpty={!!ventasPorPago.data && ventasPorPago.data.metodos.length === 0}
        tipo={INFORME_TIPO.VENTAS_POR_PAGO}
        filter={filter}
      >
        {ventasPorPago.data && ventasPorPago.data.metodos.length > 0 && (
          <Table>
            <TableCaption aria-label="Ventas por método de pago">Distribución por método de pago</TableCaption>
            <TableHeader>
              <TableRow>
                <TableHead>Método</TableHead>
                <TableHead className="text-right">Cantidad</TableHead>
                <TableHead className="text-right">Monto</TableHead>
                <TableHead className="text-right">Porcentaje</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {ventasPorPago.data.metodos.map((m) => (
                <TableRow key={m.metodo}>
                  <TableCell>{m.metodo}</TableCell>
                  <TableCell className="text-right">{m.cantidad}</TableCell>
                  <TableCell className="text-right">{currency.format(m.monto)}</TableCell>
                  <TableCell className="text-right">{m.porcentaje}%</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        )}
      </ReportSection>

      <ReportSection
        title="Ventas por vendedor"
        isLoading={ventasPorVendedor.isLoading}
        isError={ventasPorVendedor.isError}
        error={ventasPorVendedor.error}
        onRetry={() => ventasPorVendedor.refetch()}
        isEmpty={!!ventasPorVendedor.data && ventasPorVendedor.data.vendedores.length === 0}
        tipo={INFORME_TIPO.VENTAS_POR_VENDEDOR}
        filter={filter}
      >
        {ventasPorVendedor.data && ventasPorVendedor.data.vendedores.length > 0 && (
          <Table>
            <TableCaption>Ventas agrupadas por vendedor</TableCaption>
            <TableHeader>
              <TableRow>
                <TableHead>Vendedor</TableHead>
                <TableHead className="text-right">Cantidad</TableHead>
                <TableHead className="text-right">Monto total</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {ventasPorVendedor.data.vendedores.map((v) => (
                <TableRow key={v.idUsuario}>
                  <TableCell>{v.nombre}</TableCell>
                  <TableCell className="text-right">{v.cantidadVentas}</TableCell>
                  <TableCell className="text-right">{currency.format(v.montoTotal)}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        )}
      </ReportSection>

      <ReportSection
        title="Alertas de stock"
        description="Productos por debajo del stock mínimo"
        isLoading={alertasStock.isLoading}
        isError={alertasStock.isError}
        error={alertasStock.error}
        onRetry={() => alertasStock.refetch()}
        isEmpty={!!alertasStock.data && alertasStock.data.productos.length === 0}
        tipo={INFORME_TIPO.ALERTAS_STOCK}
        filter={filter}
      >
        {alertasStock.data && alertasStock.data.productos.length > 0 && (
          <Table>
            <TableCaption>Productos con alerta de stock</TableCaption>
            <TableHeader>
              <TableRow>
                <TableHead>Producto</TableHead>
                <TableHead className="text-right">Actual</TableHead>
                <TableHead className="text-right">Mínimo</TableHead>
                <TableHead className="text-right">Diferencia</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {alertasStock.data.productos.map((p) => (
                <TableRow key={p.idProducto}>
                  <TableCell>{p.nombre}</TableCell>
                  <TableCell className="text-right">{p.stockActual}</TableCell>
                  <TableCell className="text-right">{p.stockMinimo}</TableCell>
                  <TableCell className="text-right text-red-600">{p.diferencia}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        )}
        {alertasStock.data && alertasStock.data.productos.length === 0 && (
          <p className="py-4 text-center text-sm text-muted-foreground">Sin alertas de stock</p>
        )}
      </ReportSection>

      <style>{`@media print { .print\\:hidden { display: none !important; } }`}</style>
    </div>
  );
}
