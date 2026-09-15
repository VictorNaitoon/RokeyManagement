export const INFORME_TIPO = {
  VENTAS_RESUMEN: 'ventas-resumen',
  PRODUCTOS_TOP: 'productos-top',
  FLUJO_CAJA: 'flujo-caja',
  INGRESOS_GASTOS: 'ingresos-gastos',
  ALERTAS_STOCK: 'alertas-stock',
  VENTAS_POR_PAGO: 'ventas-por-pago',
  VENTAS_POR_VENDEDOR: 'ventas-por-vendedor',
} as const;

export type TipoInforme = (typeof INFORME_TIPO)[keyof typeof INFORME_TIPO];

export const FORMATO = {
  CSV: 'csv',
  PDF: 'pdf',
  DOCX: 'docx',
} as const;

export type Formato = (typeof FORMATO)[keyof typeof FORMATO];

export const PRESET = {
  HOY: 'hoy',
  SEMANA: 'semana',
  MES: 'mes',
  CUSTOM: 'custom',
} as const;

export type Preset = (typeof PRESET)[keyof typeof PRESET];

export interface InformesFilter {
  preset?: 'hoy' | 'semana' | 'mes';
  fechaDesde?: string;
  fechaHasta?: string;
  cantidad?: number;
}

export interface VentasResumenResponse {
  totalVentas: number;
  cantidadVentas: number;
  ticketPromedio: number;
  ventasAnuladas: number;
  periodo: string;
}

export interface TopProductoResponse {
  idProducto: number;
  nombre: string;
  cantidadVendida: number;
  montoTotal: number;
}

export interface ProductosTopResponse {
  productos: TopProductoResponse[];
}

export interface FlujoCajaResponse {
  ingresos: number;
  egresos: number;
  balance: number;
  movimientosIngreso: number;
  movimientosEgreso: number;
}

export interface IngresosGastosResponse {
  ventasTotales: number;
  comprasTotales: number;
  gananciaBruta: number;
  margenPorcentaje: number;
}

export interface StockAlertResponse {
  idProducto: number;
  nombre: string;
  stockActual: number;
  stockMinimo: number;
  diferencia: number;
}

export interface AlertasStockResponse {
  productos: StockAlertResponse[];
}

export interface MetodoPagoResponse {
  metodo: string;
  cantidad: number;
  monto: number;
  porcentaje: number;
}

export interface VentasPorPagoResponse {
  metodos: MetodoPagoResponse[];
}

export interface VendedorResponse {
  idUsuario: number;
  nombre: string;
  cantidadVentas: number;
  montoTotal: number;
}

export interface VentasPorVendedorResponse {
  vendedores: VendedorResponse[];
}
