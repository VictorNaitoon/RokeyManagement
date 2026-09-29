import type { CajaActualDTO, EstadoCajaResponse } from '@/hooks/useCaja';
import { Skeleton } from '@/components/ui/skeleton';
import { WalletIcon, ClockIcon, ShoppingCartIcon, BanknoteIcon, CircleHelpIcon } from 'lucide-react';
import { cn } from '@/lib/utils';

interface KpiMovimiento {
  tipo: string;
  descripcion: string;
  monto: number;
}

export interface KPICardsProps {
  cajaActual?: CajaActualDTO | EstadoCajaResponse | null;
  movimientos?: KpiMovimiento[] | null;
  /** Ganancia real del día (margen por producto). undefined = no disponible para este rol. */
  ganancia?: number;
  gananciaLoading?: boolean;
  skeleton?: boolean;
}

function isEstadoCajaResponse(v: unknown): v is EstadoCajaResponse {
  if (!v || typeof v !== 'object') return false;
  const r = v as Record<string, unknown>;
  return 'tieneCajaAbierta' in r || 'TieneCajaAbierta' in r;
}

export function KPICards({ cajaActual, movimientos, ganancia, gananciaLoading = false, skeleton = false }: KPICardsProps) {
  if (skeleton) {
    return (
      <div className="flex flex-wrap gap-4">
        <Skeleton className="w-64 h-48 rounded-lg" />
        <Skeleton className="w-64 h-48 rounded-lg" />
        <Skeleton className="w-64 h-48 rounded-lg" />
        <Skeleton className="w-64 h-48 rounded-lg" />
        <Skeleton className="w-64 h-48 rounded-lg" />
      </div>
    );
  }

  let estado: string | undefined;
  let fechaApertura: Date | null = null;
  let usuarioApertura: string | undefined;

  if (isEstadoCajaResponse(cajaActual)) {
    const raw = cajaActual as any;
    const caja = raw.caja ?? raw.Caja ?? null;
    const estadoRaw = caja?.estado ?? caja?.Estado ?? null;
    estado = estadoRaw ? String(estadoRaw).toLowerCase() : undefined;
    const fechaRaw = caja?.fechaApertura ?? caja?.FechaApertura ?? null;
    fechaApertura = fechaRaw ? new Date(fechaRaw) : null;
    const userRaw = caja?.id_usuario_apertura ?? caja?.Id_usuario_apertura ?? null;
    usuarioApertura = userRaw != null ? String(userRaw) : undefined;
  } else {
    estado = (cajaActual as { estado?: string })?.estado;
    const rawFecha = (cajaActual as { fecha_apertura?: string })?.fecha_apertura;
    fechaApertura = rawFecha ? new Date(rawFecha) : null;
    usuarioApertura = (cajaActual as { usuario_nombre?: string })?.usuario_nombre;
  }

  const estadoCard = (
    <div key="1" className="bg-white rounded-lg p-4 shadow-sm hover:shadow-md flex flex-col min-w-0">
      <div className="flex items-center gap-2 mb-2">
        <WalletIcon className="h-5 w-5 text-primary" />
        <span className="text-sm font-medium text-foreground">Estado</span>
      </div>
      <div className={cn('text-2xl font-bold', estado === 'abierta' ? 'text-green-600' : 'text-red-600')}>
        {estado ?? '—'}
      </div>
      <p className="text-sm mt-1">
        {estado === 'abierta' ? 'Abierta' : 'Cerrada'}
      </p>
    </div>
  );

  const ultimaAperturaCard = (
    <div key="2" className="bg-white rounded-lg p-4 shadow-sm hover:shadow-md flex flex-col min-w-0">
      <div className="flex items-center gap-2 mb-2">
        <ClockIcon className="h-5 w-5 text-primary" />
        <span className="text-sm font-medium text-foreground">Última Apertura</span>
      </div>
      <div className="text-lg font-medium">
        {fechaApertura ? (
          <span>
            {fechaApertura.toLocaleDateString('es-AR', { day: '2-digit', month: '2-digit', year: 'numeric' })}{' '}
            {fechaApertura.toLocaleTimeString('es-AR', { hour: '2-digit', minute: '2-digit' })}
          </span>
        ) : (
          'Nunca abierta'
        )}
      </div>
      <p className="text-xs text-muted-foreground mt-1">
        {usuarioApertura ?? '—'}
      </p>
    </div>
  );

  const lista = movimientos ?? [];
  // Las ventas generan movimientos con descripcion "Venta #<id>" (VentaService);
  // las anulaciones ("Anulación de Venta #...") son Egreso y no cuentan como venta.
  const ventasHoy = lista.filter((m) => {
    const desc = String(m.descripcion ?? '');
    return desc.startsWith('Venta #');
  }).length;
  const totalDiaFmt =
    ganancia == null
      ? null
      : ganancia.toLocaleString('es-AR', { style: 'currency', currency: 'ARS' });
  const totalAyuda =
    'Ganancia real del día: por cada producto vendido se calcula (precio de venta − costo de compra) × cantidad, y se suman todas las ventas de esta caja. No incluye movimientos manuales ni el monto inicial.';

  const ventasCard = (
    <div key="4" className="bg-white rounded-lg p-4 shadow-sm hover:shadow-md flex flex-col min-w-0">
      <div className="flex items-center gap-2 mb-2">
        <ShoppingCartIcon className="h-5 w-5 text-primary" />
        <span className="text-sm font-medium text-foreground">Ventas hoy</span>
      </div>
      <div className="text-2xl font-bold">{ventasHoy.toLocaleString()}</div>
      <p className="text-sm mt-1 text-muted-foreground">ventas realizadas hoy</p>
    </div>
  );

  const totalCard = (
    <div key="5" className="bg-white rounded-lg p-4 shadow-sm hover:shadow-md flex flex-col min-w-0">
      <div className="flex items-center gap-2 mb-2">
        <BanknoteIcon className="h-5 w-5 text-primary" />
        <span className="text-sm font-medium text-foreground">Ganancia del día</span>
        <span title={totalAyuda} aria-label={totalAyuda} className="cursor-help text-muted-foreground">
          <CircleHelpIcon className="h-4 w-4" />
        </span>
      </div>
      <div className={cn('text-2xl font-bold', ganancia != null && ganancia < 0 ? 'text-red-600' : 'text-green-600')}>
        {gananciaLoading ? '…' : (totalDiaFmt ?? '—')}
      </div>
      <p className="text-sm mt-1 text-muted-foreground">
        precio de venta menos costo, por producto
      </p>
    </div>
  );

  return (
    <div className="flex flex-wrap gap-4">
      {estadoCard}
      {ultimaAperturaCard}
      {ventasCard}
      {totalCard}
    </div>
  );
}
