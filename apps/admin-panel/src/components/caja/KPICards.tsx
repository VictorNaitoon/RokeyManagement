import type { CajaActualDTO, EstadoCajaResponse } from '@/hooks/useCaja';
import { Skeleton } from '@/components/ui/skeleton';
import { WalletIcon, ClockIcon, UserIcon } from 'lucide-react';
import { cn } from '@/lib/utils';

export interface KPICardsProps {
  cajaActual?: CajaActualDTO | EstadoCajaResponse | null;
  skeleton?: boolean;
}

function isEstadoCajaResponse(v: unknown): v is EstadoCajaResponse {
  if (!v || typeof v !== 'object') return false;
  const r = v as Record<string, unknown>;
  return 'tieneCajaAbierta' in r || 'TieneCajaAbierta' in r;
}

export function KPICards({ cajaActual, skeleton = false }: KPICardsProps) {
  if (skeleton) {
    return (
      <div className="flex flex-wrap gap-4">
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

  const movimientosHoy = 0;

  const hoyCard = (
    <div key="3" className="bg-white rounded-lg p-4 shadow-sm hover:shadow-md flex flex-col min-w-0">
      <div className="flex items-center gap-2 mb-2">
        <UserIcon className="h-5 w-5 text-primary" />
        <span className="text-sm font-medium text-foreground">Hoy</span>
      </div>
      <div className="text-2xl font-bold">{movimientosHoy.toLocaleString()}</div>
      <p className="text-sm mt-1 text-muted-foreground">movimientos registrados hoy</p>
    </div>
  );

  return (
    <div className="flex flex-wrap gap-4">
      {estadoCard}
      {ultimaAperturaCard}
      {hoyCard}
    </div>
  );
}
