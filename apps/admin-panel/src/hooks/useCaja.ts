/**
 * Caja Hooks - React Query hooks for cash register management
 * RoKey MANAGEMENT - Multi-tenant SaaS ERP/POS for locksmiths
 *
 * Fixed DTO alignment: backend serializes with camelCase (System.Text.Json
 * default PropertyNamingPolicy = JsonNamingPolicy.CamelCase). Frontend types
 * MUST use camelCase (tieneCajaAbierta/caja/estado/fechaApertura) to match
 * actual JSON. Requests remain case-insensitive but we keep PascalCase for
 * C# compatibility; responses are always camelCase.
 * Single axios instance: '@/lib/api/axios' (baseURL '' -> /api/v1/Caja/...)
 */

import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { api } from '@/lib/api/axios';
import { toast } from 'sonner';

// ============================================
// Configuration Constants
// ============================================

const CajaQueryConfig = {
  staleTime: 30000,
  gcTime: 30000,
  refetchOnWindowFocus: true,
  retry: 1,
};

// ============================================
// Query Keys
// ============================================

export const CajaQueryKey = {
  Actual: ['caja', 'actual'],
  Movimientos: ['caja', 'movimientos'],
  Apertura: ['caja', 'apertura'],
  Cierre: ['caja', 'cierre'],
  Movimiento: ['caja', 'movimiento'],
} as const;

export type CajaQueryKeyType = keyof typeof CajaQueryKey;

// ============================================
// DTOs matching backend JSON (camelCase)
// ============================================

// Request DTOs (what we send) - backend deserialization is case-insensitive,
// so PascalCase or camelCase both work. Keep PascalCase for C# parity.
export interface AgregarMovimientoCajaRequest {
  Tipo: 'Ingreso' | 'Egreso';
  Monto: number;
  Descripcion?: string;
}

export interface AperturaCajaRequest {
  MontoInicial: number;
  Observaciones?: string;
}

export interface CierreCajaRequest {
  MontoFinal: number;
  Observaciones?: string;
}

// Response DTOs (what backend returns) - MUST be camelCase because
// AddControllers().AddJsonOptions defaults to PropertyNamingPolicy.CamelCase
// (no explicit config in backend/API/Program.cs). Example payload:
// { tieneCajaAbierta: true, caja: { id: 1, estado: "Abierta", fechaApertura: "...", id_negocio: 1 } }
export interface CajaResponse {
  id: number;
  id_negocio: number;
  id_usuario_apertura: number;
  id_usuario_cierre?: number | null;
  fechaApertura: string; // ISO from server (UtcNow)
  fechaCierre?: string | null;
  montoInicial: number;
  montoFinal?: number | null;
  estado: string; // 'Abierta' | 'Cerrada'
}

export interface EstadoCajaResponse {
  tieneCajaAbierta: boolean;
  caja: CajaResponse | null;
}

export interface MovimientoCajaResponse {
  id: number;
  id_caja: number;
  id_negocio: number;
  tipo: string; // 'Ingreso' | 'Egreso'
  monto: number;
  descripcion?: string | null;
  fecha: string;
  id_usuario: number;
  usuarioNombre?: string | null;
  usuario_nombre?: string | null;
  UsuarioNombre?: string | null;
}

// Legacy aliases kept for backwards compat (KPICards etc.)
// Keep old snake_case shapes deprecated - new code should use EstadoCajaResponse/MovimientoCajaResponse
export interface LegacyCajaActualDTO {
  id: number;
  nombre: string;
  estado: 'abierta' | 'cerrada';
  fecha_apertura?: string | null;
  fecha_cierre?: string | null;
  usuario_id?: number | null;
  usuario_nombre?: string | null;
}
export interface LegacyMovimientoListDTO {
  id: number;
  fecha: string;
  tipo: 'ingreso' | 'egreso';
  descripcion: string;
  monto: number;
  usuario_nombre: string;
}
export type CajaActualDTO = LegacyCajaActualDTO | EstadoCajaResponse;
export type MovimientoDTO = AgregarMovimientoCajaRequest;
export type CajaAperturaDTO = AperturaCajaRequest;
export type CajaCierreDTO = CierreCajaRequest;
export type MovimientoListDTO = LegacyMovimientoListDTO | MovimientoCajaResponse;

// ============================================
// Normalizers (tolerant to PascalCase legacy payloads)
// ============================================

function normalizeCaja(raw: any): CajaResponse | null {
  if (!raw || typeof raw !== 'object') return null;
  // Prefer camelCase; fallback to PascalCase for backwards compat or if backend config changes
  return {
    id: raw.id ?? raw.Id ?? 0,
    id_negocio: raw.id_negocio ?? raw.Id_negocio ?? raw.idNegocio ?? 0,
    id_usuario_apertura: raw.id_usuario_apertura ?? raw.Id_usuario_apertura ?? raw.idUsuarioApertura ?? 0,
    id_usuario_cierre: raw.id_usuario_cierre ?? raw.Id_usuario_cierre ?? null,
    fechaApertura: raw.fechaApertura ?? raw.FechaApertura ?? '',
    fechaCierre: raw.fechaCierre ?? raw.FechaCierre ?? null,
    montoInicial: raw.montoInicial ?? raw.MontoInicial ?? 0,
    montoFinal: raw.montoFinal ?? raw.MontoFinal ?? null,
    estado: raw.estado ?? raw.Estado ?? 'Cerrada',
  };
}

function normalizeEstado(raw: any): EstadoCajaResponse {
  if (!raw || typeof raw !== 'object') return { tieneCajaAbierta: false, caja: null };
  const tiene = raw.tieneCajaAbierta ?? raw.TieneCajaAbierta ?? false;
  const cajaRaw = raw.caja ?? raw.Caja ?? null;
  return {
    tieneCajaAbierta: Boolean(tiene),
    caja: normalizeCaja(cajaRaw),
  };
}

function normalizeMovimiento(raw: any): MovimientoCajaResponse {
  const nombre = raw.usuarioNombre ?? raw.UsuarioNombre ?? raw.usuario_nombre ?? null;
  return {
    id: raw.id ?? raw.Id ?? 0,
    id_caja: raw.id_caja ?? raw.Id_caja ?? 0,
    id_negocio: raw.id_negocio ?? raw.Id_negocio ?? 0,
    tipo: raw.tipo ?? raw.Tipo ?? '',
    monto: raw.monto ?? raw.Monto ?? 0,
    descripcion: raw.descripcion ?? raw.Descripcion ?? null,
    fecha: raw.fecha ?? raw.Fecha ?? '',
    id_usuario: raw.id_usuario ?? raw.Id_usuario ?? 0,
    usuarioNombre: nombre,
    usuario_nombre: nombre,
  };
}

// ============================================
// Query Hooks
// ============================================

/**
 * GET /api/v1/Caja/actual
 * Returns EstadoCajaResponse with server-side fechaApertura (audit).
 */
export function useCajaActual() {
  return useQuery({
    queryKey: CajaQueryKey.Actual,
    queryFn: async () => {
      const response = await api.get<EstadoCajaResponse>('/api/v1/Caja/actual');
      // Normalize to handle both camelCase and PascalCase payloads
      return normalizeEstado(response.data as any);
    },
    ...CajaQueryConfig,
    enabled: true,
    refetchOnWindowFocus: true,
  });
}

/**
 * GET /api/v1/Caja/{id}/movimientos
 * @param cajaId - Id of the Caja (not negocioId)
 */
export function useCajaMovimientos(cajaId: number) {
  return useQuery({
    queryKey: [...CajaQueryKey.Movimientos, cajaId],
    queryFn: async () => {
      const response = await api.get<MovimientoCajaResponse[]>(`/api/v1/Caja/${cajaId}/movimientos`);
      const raw = response.data as any;
      if (!Array.isArray(raw)) return [] as MovimientoCajaResponse[];
      return raw.map(normalizeMovimiento);
    },
    ...CajaQueryConfig,
    staleTime: 0,
    refetchOnMount: 'always' as const,
    enabled: !!cajaId && cajaId > 0,
  });
}

// ============================================
// Mutation Hooks
// ============================================

/**
 * POST /api/v1/Caja/apertura
 * Payload: { MontoInicial, Observaciones? } - fechaApertura set server-side via UtcNow
 */
export function useAperturaMutation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationKey: CajaQueryKey.Apertura,
    mutationFn: async (data: AperturaCajaRequest) => {
      const response = await api.post<CajaResponse>('/api/v1/Caja/apertura', data);
      return normalizeCaja(response.data as any) as CajaResponse;
    },
    onSuccess: async (cajaCreada) => {
      toast.success('Caja abierta');
      // Optimistic direct cache update - guarantees UI reflects open state even if GET is stale
      if (cajaCreada) {
        queryClient.setQueryData<EstadoCajaResponse>(CajaQueryKey.Actual, {
          tieneCajaAbierta: true,
          caja: cajaCreada,
        });
      }
      await queryClient.invalidateQueries({ queryKey: CajaQueryKey.Actual });
      await queryClient.invalidateQueries({ queryKey: CajaQueryKey.Movimientos });
      await queryClient.refetchQueries({ queryKey: CajaQueryKey.Actual });
      // Movimientos for the new caja should refetch with new cajaId
      await queryClient.refetchQueries({ queryKey: CajaQueryKey.Movimientos });
    },
    onError: (error: unknown) => {
      const e = error as { response?: { data?: { detail?: string; message?: string } }; message?: string };
      const message =
        e.response?.data?.detail ||
        e.response?.data?.message ||
        e.message ||
        'Error al abrir la caja';
      toast.error(message);
    },
  });
}

export function useCierreMutation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationKey: CajaQueryKey.Cierre,
    mutationFn: async (data: CierreCajaRequest) => {
      const response = await api.post<CajaResponse>('/api/v1/Caja/cierre', data);
      return normalizeCaja(response.data as any) as CajaResponse;
    },
    onSuccess: async (cajaCerrada) => {
      toast.success('Caja cerrada');
      // Update cache to closed state
      if (cajaCerrada) {
        queryClient.setQueryData<EstadoCajaResponse>(CajaQueryKey.Actual, {
          tieneCajaAbierta: false,
          caja: cajaCerrada,
        });
      } else {
        queryClient.setQueryData<EstadoCajaResponse>(CajaQueryKey.Actual, {
          tieneCajaAbierta: false,
          caja: null,
        });
      }
      await queryClient.invalidateQueries({ queryKey: CajaQueryKey.Actual });
      await queryClient.invalidateQueries({ queryKey: CajaQueryKey.Movimientos });
      await queryClient.refetchQueries({ queryKey: CajaQueryKey.Actual });
    },
    onError: (error: unknown) => {
      const e = error as { response?: { data?: { detail?: string; message?: string } }; message?: string };
      const message =
        e.response?.data?.detail ||
        e.response?.data?.message ||
        e.message ||
        'Error al cerrar la caja';
      toast.error(message);
    },
  });
}

export function useMovimientoMutation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationKey: CajaQueryKey.Movimiento,
    mutationFn: async (data: AgregarMovimientoCajaRequest) => {
      const response = await api.post<MovimientoCajaResponse>('/api/v1/Caja/movimientos', data);
      return normalizeMovimiento(response.data as any);
    },
    onSuccess: async () => {
      toast.success('Movimiento agregado');
      // Prefix invalidation so ['caja','movimientos', cajaId] refetches (active)
      await queryClient.invalidateQueries({ queryKey: CajaQueryKey.Movimientos, refetchType: 'active' });
      await queryClient.invalidateQueries({ queryKey: CajaQueryKey.Actual, refetchType: 'active' });
      await queryClient.refetchQueries({ queryKey: CajaQueryKey.Movimientos });
    },
    onError: (error: unknown) => {
      const e = error as { response?: { data?: { detail?: string; message?: string } }; message?: string };
      const message =
        e.response?.data?.detail ||
        e.response?.data?.message ||
        e.message ||
        'Error al agregar el movimiento';
      toast.error(message);
    },
  });
}
