import * as React from 'react';
import { Link } from 'react-router-dom';
import { authStore } from '@/stores/authStore';
import { toast } from 'sonner';
import {
  useCajaActual,
  useCajaMovimientos,
  useAperturaMutation,
  useCierreMutation,
  useMovimientoMutation,
} from '@/hooks/useCaja';
import { KPICards } from '@/components/caja/KPICards';
import { MovimientoForm } from '@/components/caja/MovimientoForm';
import { MovimientosTable } from '@/components/caja/MovimientosTable';
import { WalletIcon } from 'lucide-react';

function formatAuditDate(iso: string): string {
  const d = new Date(iso);
  if (Number.isNaN(d.getTime())) return iso;
  const date = d.toLocaleDateString('es-AR', { day: '2-digit', month: '2-digit', year: 'numeric' });
  const time = d.toLocaleTimeString('es-AR', { hour: '2-digit', minute: '2-digit' });
  return `${date} ${time}`;
}

export function CajaPage() {
  const { user, isAuthenticated } = authStore();

  const { data: estadoCaja, isLoading, isError, error, refetch: refetchActual } = useCajaActual();
  const apertura = useAperturaMutation();
  const cierre = useCierreMutation();
  const movimiento = useMovimientoMutation();

  // Server-side audit: fechaApertura comes from backend (UtcNow), never from client
  // Backend serializes camelCase: { tieneCajaAbierta, caja: { id, estado, fechaApertura, ... } }
  const caja = (estadoCaja as any)?.caja ?? (estadoCaja as any)?.Caja ?? null;
  const cajaId = caja?.id ?? caja?.Id ?? 0;
  const tieneCajaAbierta = (estadoCaja as any)?.tieneCajaAbierta ?? (estadoCaja as any)?.TieneCajaAbierta ?? false;
  const estadoCajaStr = caja?.estado ?? caja?.Estado ?? (tieneCajaAbierta ? 'Abierta' : 'Cerrada');

  const { data: movimientos, isLoading: movLoading, refetch } = useCajaMovimientos(cajaId);

  // Local state for apertura/cierre montos (no fechas - audit is server-side)
  const [montoInicial, setMontoInicial] = React.useState<string>('0');
  const [montoFinal, setMontoFinal] = React.useState<string>('0');
  const [observacionesCierre, setObservacionesCierre] = React.useState<string>('');
  const [observacionesApertura, setObservacionesApertura] = React.useState<string>('');

  React.useEffect(() => {
    if (isError) {
      toast.error((error as Error)?.message || 'Error al cargar el estado de la caja');
    }
  }, [isError, error]);

  const esDueñoOGerente = user?.rol === 'Dueño' || user?.rol === 'Gerente';
  const esDueñoGerenteOEmpleado = user?.rol === 'Dueño' || user?.rol === 'Gerente' || user?.rol === 'Empleado';
  const esSuperAdmin = user?.rol === 'SuperAdmin';

  if (!isAuthenticated) {
    return (
      <div className="p-8">
        <h1 className="text-2xl font-bold text-foreground">Caja</h1>
        <p className="text-muted-foreground">Inicie sesión para acceder</p>
      </div>
    );
  }

  if (isLoading) {
    return (
      <div className="p-8">
        <div className="grid gap-6">
          <KPICards skeleton />
          <div className="space-y-4">
            <MovimientoForm skeleton />
            <MovimientosTable skeleton={true} />
          </div>
        </div>
      </div>
    );
  }

  if (isError) {
    return (
      <div className="p-8">
        <h1 className="text-2xl font-bold text-foreground">Caja</h1>
        <div className="mt-4 text-sm text-destructive">
          <p>{(error as Error)?.message || 'Error inesperado al cargar el estado de la caja'}</p>
        </div>
      </div>
    );
  }

  // Adapt backend EstadoCajaResponse (camelCase) -> legacy KPICards shape (keeps component compatible)
  const kpiCajaActual = caja
    ? {
        estado: ((caja.estado ?? caja.Estado ?? '') as string).toLowerCase() as 'abierta' | 'cerrada',
        fecha_apertura: caja.fechaApertura ?? caja.FechaApertura,
        usuario_nombre: String(caja.id_usuario_apertura ?? caja.Id_usuario_apertura ?? ''),
        nombre: `Caja #${caja.id ?? caja.Id}`,
        id: caja.id ?? caja.Id,
        fecha_cierre: caja.fechaCierre ?? caja.FechaCierre,
        usuario_id: caja.id_usuario_apertura ?? caja.Id_usuario_apertura,
      }
    : null;

  // Map backend MovimientoCajaResponse (camelCase) -> MovimientosTable row shape
  const movimientosTableData = (movimientos ?? []).map((m: any) => ({
    id: m.id ?? m.Id,
    fecha: m.fecha ?? m.Fecha,
    tipo: (String(m.tipo ?? m.Tipo ?? '').toLowerCase() as 'ingreso' | 'egreso'),
    descripcion: m.descripcion ?? m.Descripcion ?? '',
    monto: m.monto ?? m.Monto,
    usuario_nombre: m.usuarioNombre ?? m.usuario_nombre ?? m.UsuarioNombre ?? String(m.id_usuario ?? m.Id_usuario ?? ''),
  }));

  const handleApertura = async () => {
    const monto = Number(montoInicial);
    if (Number.isNaN(monto) || monto < 0) {
      toast.error('Monto inicial debe ser mayor o igual a 0');
      return;
    }
    try {
      await apertura.mutateAsync({
        MontoInicial: monto,
        Observaciones: observacionesApertura.trim() ? observacionesApertura.trim() : undefined,
      });
      // Explicit refetch guarantees fresh estado even if invalidation is coalesced
      await refetchActual();
    } catch {
      // error handled by mutation onError
    }
  };

  const handleCierre = async () => {
    const monto = Number(montoFinal);
    if (Number.isNaN(monto) || monto < 0) {
      toast.error('Monto final debe ser mayor o igual a 0');
      return;
    }
    try {
      await cierre.mutateAsync({
        MontoFinal: monto,
        Observaciones: observacionesCierre.trim() ? observacionesCierre.trim() : undefined,
      });
      await refetchActual();
    } catch {
      // error handled by mutation onError
    }
  };

  return (
    <div className="min-h-screen p-8 bg-background">
      <header className="mb-8">
        <Link to="/caja" className="flex items-center gap-2">
          <WalletIcon className="h-6 w-6 text-primary" />
          <h1 className="text-3xl font-bold text-foreground">Caja</h1>
        </Link>
        <p className="text-muted-foreground mt-1">
          {caja ? `Caja #${caja.id ?? caja.Id} — ${caja.estado ?? caja.Estado}` : 'Registro de Caja'}
        </p>
        {(caja?.fechaApertura ?? caja?.FechaApertura) && (
          <p className="text-sm text-muted-foreground mt-1" data-testid="auditoria-fecha-apertura">
            Apertura: {formatAuditDate(caja.fechaApertura ?? caja.FechaApertura)} (servidor)
          </p>
        )}
      </header>

      <div className="mb-8">
        <KPICards cajaActual={kpiCajaActual as never} />
      </div>

      <main className="space-y-8">
        {esDueñoOGerente && (
          <div className="grid gap-4 md:grid-cols-2">
            <div className="p-6 bg-white rounded-lg shadow-sm border">
              <h3 className="font-semibold mb-2">Abrir Caja</h3>
              <p className="text-muted-foreground text-sm">
                Estado actual: <span className={estadoCajaStr === 'Abierta' ? 'text-green-600' : 'text-red-600'}>{estadoCajaStr || 'Cerrada'}</span>
              </p>
              <div className="mt-4 space-y-3">
                <div>
                  <label className="block text-sm font-medium mb-1">Monto inicial</label>
                  <input
                    type="number"
                    step="0.01"
                    min="0"
                    value={montoInicial}
                    onChange={(e) => setMontoInicial(e.target.value)}
                    className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm shadow-sm"
                    placeholder="0.00"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium mb-1">Observaciones (opcional)</label>
                  <input
                    value={observacionesApertura}
                    onChange={(e) => setObservacionesApertura(e.target.value)}
                    maxLength={500}
                    className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm shadow-sm"
                    placeholder="Observaciones"
                  />
                </div>
              </div>
              <button
                onClick={handleApertura}
                disabled={apertura.isPending || tieneCajaAbierta}
                className="mt-4 w-full py-3 px-4 bg-green-600 text-white rounded-md font-medium hover:bg-green-700 disabled:opacity-50 disabled:pointer-events-none"
              >
                {apertura.isPending ? 'Abriendo...' : 'Abrir Caja'}
              </button>
            </div>

            <div className="p-6 bg-white rounded-lg shadow-sm border">
              <h3 className="font-semibold mb-2">Cerrar Caja</h3>
              <p className="text-muted-foreground text-sm">
                Estado actual: <span className={estadoCajaStr === 'Cerrada' ? 'text-green-600' : 'text-red-600'}>{estadoCajaStr || 'Abierta'}</span>
              </p>
              <div className="mt-4 space-y-3">
                <div>
                  <label className="block text-sm font-medium mb-1">Monto final</label>
                  <input
                    type="number"
                    step="0.01"
                    min="0"
                    value={montoFinal}
                    onChange={(e) => setMontoFinal(e.target.value)}
                    className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm shadow-sm"
                    placeholder="0.00"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium mb-1">Observaciones (opcional)</label>
                  <input
                    value={observacionesCierre}
                    onChange={(e) => setObservacionesCierre(e.target.value)}
                    maxLength={500}
                    className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm shadow-sm"
                    placeholder="Observaciones"
                  />
                </div>
              </div>
              <button
                onClick={handleCierre}
                disabled={cierre.isPending || !tieneCajaAbierta}
                className="mt-4 w-full py-3 px-4 bg-red-600 text-white rounded-md font-medium hover:bg-red-700 disabled:opacity-50 disabled:pointer-events-none"
              >
                {cierre.isPending ? 'Cerrando...' : 'Cerrar Caja'}
              </button>
            </div>
          </div>
        )}

        {esDueñoGerenteOEmpleado && (
          <div className="p-6 bg-white rounded-lg shadow-sm border">
            <h3 className="font-semibold mb-4">Agregar Movimiento</h3>
            {/* MovimientoForm uses its own mutation internally; parent just reflects pending state */}
            <MovimientoForm disabled={movimiento.isPending} />
          </div>
        )}

        {esSuperAdmin && (
          <div className="p-6 bg-white rounded-lg shadow-sm border text-muted-foreground">
            <p className="text-sm">
              No tiene permiso para operar la caja. Solo Dueño y Gerente pueden abrir/cerrar.
            </p>
          </div>
        )}

        {movimientosTableData && movimientosTableData.length > 0 && (
          <MovimientosTable
            movimientos={movimientosTableData}
            refetch={refetch}
            loading={movLoading}
          />
        )}

        {!esSuperAdmin && (!movimientosTableData || movimientosTableData.length === 0) && (
          <p className="text-muted-foreground text-center py-8">
            No hay movimientos registrados
          </p>
        )}
      </main>
    </div>
  );
}

export default CajaPage;
