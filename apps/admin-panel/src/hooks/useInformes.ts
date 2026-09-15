import { useQuery, useMutation } from '@tanstack/react-query';
import { api } from '@/lib/api/axios';
import { authStore } from '@/stores/authStore';
import type {
  InformesFilter,
  VentasResumenResponse,
  ProductosTopResponse,
  FlujoCajaResponse,
  IngresosGastosResponse,
  AlertasStockResponse,
  VentasPorPagoResponse,
  VentasPorVendedorResponse,
} from '@/types/informes.types';
import { INFORME_TIPO } from '@/types/informes.types';
import type { TipoInforme, Formato } from '@/types/informes.types';
import { extractFilename, triggerDownload } from '@/lib/export/download';

const INFORMES_QUERY_CONFIG = {
  staleTime: 30000,
  retry: 2,
  refetchOnWindowFocus: false,
} as const;

function canAccessInformes(): boolean {
  const user = authStore.getState().user;
  if (!user) return false;
  if (user.rol === 'SuperAdmin') return false;
  return user.rol === 'Dueño' || user.rol === 'Gerente';
}

function buildParams(filter: InformesFilter): Record<string, string | number> {
  const p: Record<string, string | number> = {};
  if (filter.fechaDesde && filter.fechaHasta) {
    p.fechaDesde = filter.fechaDesde;
    p.fechaHasta = filter.fechaHasta;
  } else if (filter.preset) {
    p.preset = filter.preset;
  }
  if (filter.cantidad !== undefined) {
    p.cantidad = filter.cantidad;
  }
  return p;
}

function buildProductosTopParams(filter: InformesFilter): Record<string, string | number> {
  const p = buildParams(filter);
  if (p.cantidad === undefined) {
    p.cantidad = 10;
  }
  return p;
}

export function useVentasResumen(filter: InformesFilter) {
  const enabled = canAccessInformes();
  return useQuery({
    queryKey: ['informes', INFORME_TIPO.VENTAS_RESUMEN, filter],
    queryFn: async () => {
      const params = buildParams({ preset: filter.preset, fechaDesde: filter.fechaDesde, fechaHasta: filter.fechaHasta });
      const res = await api.get<VentasResumenResponse>('/api/v1/informes/ventas-resumen', { params });
      return res.data;
    },
    enabled,
    ...INFORMES_QUERY_CONFIG,
  });
}

export function useProductosTop(filter: InformesFilter) {
  const enabled = canAccessInformes();
  return useQuery({
    queryKey: ['informes', INFORME_TIPO.PRODUCTOS_TOP, filter],
    queryFn: async () => {
      const params = buildProductosTopParams(filter);
      const res = await api.get<ProductosTopResponse>('/api/v1/informes/productos-top', { params });
      return res.data;
    },
    enabled,
    ...INFORMES_QUERY_CONFIG,
  });
}

export function useFlujoCaja(filter: InformesFilter) {
  const enabled = canAccessInformes();
  return useQuery({
    queryKey: ['informes', INFORME_TIPO.FLUJO_CAJA, filter],
    queryFn: async () => {
      const params = buildParams({ preset: filter.preset, fechaDesde: filter.fechaDesde, fechaHasta: filter.fechaHasta });
      const res = await api.get<FlujoCajaResponse>('/api/v1/informes/flujo-caja', { params });
      return res.data;
    },
    enabled,
    ...INFORMES_QUERY_CONFIG,
  });
}

export function useIngresosGastos(filter: InformesFilter) {
  const enabled = canAccessInformes();
  return useQuery({
    queryKey: ['informes', INFORME_TIPO.INGRESOS_GASTOS, filter],
    queryFn: async () => {
      const params = buildParams({ preset: filter.preset, fechaDesde: filter.fechaDesde, fechaHasta: filter.fechaHasta });
      const res = await api.get<IngresosGastosResponse>('/api/v1/informes/ingresos-gastos', { params });
      return res.data;
    },
    enabled,
    ...INFORMES_QUERY_CONFIG,
  });
}

export function useAlertasStock() {
  const enabled = canAccessInformes();
  return useQuery({
    queryKey: ['informes', INFORME_TIPO.ALERTAS_STOCK],
    queryFn: async () => {
      const res = await api.get<AlertasStockResponse>('/api/v1/informes/alertas-stock');
      return res.data;
    },
    enabled,
    ...INFORMES_QUERY_CONFIG,
  });
}

export function useVentasPorPago(filter: InformesFilter) {
  const enabled = canAccessInformes();
  return useQuery({
    queryKey: ['informes', INFORME_TIPO.VENTAS_POR_PAGO, filter],
    queryFn: async () => {
      const params = buildParams({ preset: filter.preset, fechaDesde: filter.fechaDesde, fechaHasta: filter.fechaHasta });
      const res = await api.get<VentasPorPagoResponse>('/api/v1/informes/ventas-por-pago', { params });
      return res.data;
    },
    enabled,
    ...INFORMES_QUERY_CONFIG,
  });
}

export function useVentasPorVendedor(filter: InformesFilter) {
  const enabled = canAccessInformes();
  return useQuery({
    queryKey: ['informes', INFORME_TIPO.VENTAS_POR_VENDEDOR, filter],
    queryFn: async () => {
      const params = buildParams({ preset: filter.preset, fechaDesde: filter.fechaDesde, fechaHasta: filter.fechaHasta });
      const res = await api.get<VentasPorVendedorResponse>('/api/v1/informes/ventas-por-vendedor', { params });
      return res.data;
    },
    enabled,
    ...INFORMES_QUERY_CONFIG,
  });
}

export interface UseInformesReturn {
  ventasResumen: ReturnType<typeof useVentasResumen>;
  productosTop: ReturnType<typeof useProductosTop>;
  flujoCaja: ReturnType<typeof useFlujoCaja>;
  ingresosGastos: ReturnType<typeof useIngresosGastos>;
  alertasStock: ReturnType<typeof useAlertasStock>;
  ventasPorPago: ReturnType<typeof useVentasPorPago>;
  ventasPorVendedor: ReturnType<typeof useVentasPorVendedor>;
}

export function useInformes(filter: InformesFilter): UseInformesReturn {
  const ventasResumen = useVentasResumen(filter);
  const productosTop = useProductosTop(filter);
  const flujoCaja = useFlujoCaja(filter);
  const ingresosGastos = useIngresosGastos(filter);
  const alertasStock = useAlertasStock();
  const ventasPorPago = useVentasPorPago(filter);
  const ventasPorVendedor = useVentasPorVendedor(filter);

  return {
    ventasResumen,
    productosTop,
    flujoCaja,
    ingresosGastos,
    alertasStock,
    ventasPorPago,
    ventasPorVendedor,
  };
}

export function useInformesExport() {
  return useMutation({
    mutationFn: async ({
      tipo,
      formato,
      filter,
    }: {
      tipo: TipoInforme;
      formato: Formato;
      filter: InformesFilter;
    }) => {
      const baseParams: Record<string, string | number> = { formato };
      if (tipo !== INFORME_TIPO.ALERTAS_STOCK) {
        if (filter.fechaDesde && filter.fechaHasta) {
          baseParams.fechaDesde = filter.fechaDesde;
          baseParams.fechaHasta = filter.fechaHasta;
        } else if (filter.preset) {
          baseParams.preset = filter.preset;
        }
        if (tipo === INFORME_TIPO.PRODUCTOS_TOP) {
          baseParams.cantidad = filter.cantidad ?? 10;
        }
      }

      const response = await api.get(`/api/v1/informes/${tipo}/export`, {
        params: baseParams,
        responseType: 'blob',
      });

      const contentDisposition: string | undefined = response.headers['content-disposition'] as string | undefined;
      const filename = extractFilename(contentDisposition) ?? `informe-${tipo}-${filter.preset ?? 'custom'}.${formato}`;
      const contentType: string = (response.headers['content-type'] as string) ?? 'application/octet-stream';
      const blob = response.data instanceof Blob ? (response.data as Blob) : new Blob([response.data as BlobPart], { type: contentType });
      triggerDownload(blob, filename);
      return filename;
    },
  });
}

export { buildParams };
