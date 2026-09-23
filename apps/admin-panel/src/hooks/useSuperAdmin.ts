import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { api } from '@/lib/api/axios';
import { toast } from 'sonner';

export interface TenantSuscripcion {
  id: number;
  plan: string;
  estado: string;
  tipoFacturacion: string;
  monto: number;
  fechaProximoPago: string;
  fechaInicio: string;
}

export interface Tenant {
  id: number;
  nombre: string;
  cuit: string;
  direccion: string;
  telefono: string | null;
  estado: string;
  tipo: string;
  fechaInicio: string;
  totalUsuarios: number;
  totalProductos: number;
  suscripcion: TenantSuscripcion | null;
}

export interface Plan {
  id: number;
  nombre: string;
  descripcion: string;
  precioMensual: number;
  precioAnual: number;
  maxUsuarios: number;
  maxProductos: number;
  maxTransaccionesMes: number;
  soportePrioritario: boolean;
  multiSucursal: boolean;
  apiAccess: boolean;
  activo: boolean;
  orden: number;
}

export interface DashboardMetrics {
  totalTenants: number;
  tenantsActivos: number;
  tenantsInactivos: number;
  tenantsTrial: number;
  totalUsuarios: number;
  totalProductos: number;
}

export interface CreateTenantRequest {
  emailAdmin: string;
  passwordAdmin: string;
  nombreAdmin: string;
  apellidoAdmin: string;
  nombre: string;
  cuit: string;
  direccion: string;
  logoURL?: string | null;
  telefono?: string | null;
  email?: string | null;
  puntoDeVenta?: string | null;
  condicionVentas?: string | null;
  tipo: number; // 0 Cerrajeria 1 Ferreteria 2 etc - backend parses via Enum.TryParse
  activo: boolean;
  idPlan: number;
  tipoFacturacion: string; // Mensual | Anual
  activarSuscripcion: boolean;
}

function mapTenant(raw: any): Tenant {
  return {
    id: raw.id ?? raw.Id,
    nombre: raw.nombre ?? raw.Nombre ?? '',
    cuit: raw.cuit ?? raw.CUIT ?? '',
    direccion: raw.direccion ?? raw.Direccion ?? '',
    telefono: raw.telefono ?? raw.Telefono ?? null,
    estado: raw.estado ?? raw.Estado ?? '',
    tipo: raw.tipo ?? raw.Tipo ?? '',
    fechaInicio: raw.fechaInicio ?? raw.FechaInicio ?? '',
    totalUsuarios: raw.totalUsuarios ?? raw.TotalUsuarios ?? 0,
    totalProductos: raw.totalProductos ?? raw.TotalProductos ?? 0,
    suscripcion: raw.suscripcion ?? raw.Suscripcion ? {
      id: (raw.suscripcion ?? raw.Suscripcion).id ?? (raw.suscripcion ?? raw.Suscripcion).Id,
      plan: (raw.suscripcion ?? raw.Suscripcion).plan ?? (raw.suscripcion ?? raw.Suscripcion).Plan ?? '',
      estado: (raw.suscripcion ?? raw.Suscripcion).estado ?? (raw.suscripcion ?? raw.Suscripcion).Estado ?? '',
      tipoFacturacion: (raw.suscripcion ?? raw.Suscripcion).tipoFacturacion ?? (raw.suscripcion ?? raw.Suscripcion).TipoFacturacion ?? '',
      monto: (raw.suscripcion ?? raw.Suscripcion).monto ?? (raw.suscripcion ?? raw.Suscripcion).Monto ?? 0,
      fechaProximoPago: (raw.suscripcion ?? raw.Suscripcion).fechaProximoPago ?? (raw.suscripcion ?? raw.Suscripcion).FechaProximoPago ?? '',
      fechaInicio: (raw.suscripcion ?? raw.Suscripcion).fechaInicio ?? (raw.suscripcion ?? raw.Suscripcion).FechaInicio ?? '',
    } : null,
  };
}

const CONFIG = { staleTime: 30000, retry: 1, refetchOnWindowFocus: false };

export function useSuperAdminTenants() {
  return useQuery({
    queryKey: ['super-admin', 'tenants'],
    queryFn: async () => {
      const res = await api.get<any[]>('/api/v1/super-admin/tenants');
      const arr = Array.isArray(res.data) ? res.data : [];
      return arr.map(mapTenant);
    },
    ...CONFIG,
  });
}

export function useSuperAdminTenant(id: number | null) {
  return useQuery({
    queryKey: ['super-admin', 'tenant', id],
    queryFn: async () => {
      const res = await api.get(`/api/v1/super-admin/tenants/${id}`);
      return mapTenant(res.data);
    },
    enabled: !!id,
    ...CONFIG,
  });
}

export function useSuperAdminPlanes() {
  return useQuery({
    queryKey: ['super-admin', 'planes'],
    queryFn: async () => {
      const res = await api.get<any[]>('/api/v1/super-admin/planes');
      return (res.data as any[]).map((p: any) => ({
        id: p.id ?? p.Id,
        nombre: p.nombre ?? p.Nombre ?? '',
        descripcion: p.descripcion ?? p.Descripcion ?? '',
        precioMensual: p.precioMensual ?? p.PrecioMensual ?? 0,
        precioAnual: p.precioAnual ?? p.PrecioAnual ?? 0,
        maxUsuarios: p.maxUsuarios ?? p.MaxUsuarios ?? 0,
        maxProductos: p.maxProductos ?? p.MaxProductos ?? 0,
        maxTransaccionesMes: p.maxTransaccionesMes ?? p.MaxTransaccionesMes ?? 0,
        soportePrioritario: p.soportePrioritario ?? p.SoportePrioritario ?? false,
        multiSucursal: p.multiSucursal ?? p.MultiSucursal ?? false,
        apiAccess: p.apiAccess ?? p.APIAccess ?? false,
        activo: p.activo ?? p.Activo ?? true,
        orden: p.orden ?? p.Orden ?? 0,
      } as Plan));
    },
    ...CONFIG,
  });
}

export function useSuperAdminMetrics() {
  return useQuery({
    queryKey: ['super-admin', 'metrics'],
    queryFn: async () => {
      const res = await api.get<any>('/api/v1/super-admin/dashboard');
      const d = res.data;
      return {
        totalTenants: d.totalTenants ?? d.TotalTenants ?? 0,
        tenantsActivos: d.tenantsActivos ?? d.TenantsActivos ?? 0,
        tenantsInactivos: d.tenantsInactivos ?? d.TenantsInactivos ?? 0,
        tenantsTrial: d.tenantsTrial ?? d.TenantsTrial ?? 0,
        totalUsuarios: d.totalUsuarios ?? d.TotalUsuarios ?? 0,
        totalProductos: d.totalProductos ?? d.TotalProductos ?? 0,
      } as DashboardMetrics;
    },
    ...CONFIG,
  });
}

export function useCreateTenant() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (data: CreateTenantRequest) => {
      // Backend expects Tipo as string name? Controller does Enum.TryParse on request.Tipo.ToString() -> try int->enum name
      // We send Tipo as number and backend will parse correctly (e.g., 0 -> Cerrajeria)
      const payload: any = {
        emailAdmin: data.emailAdmin,
        passwordAdmin: data.passwordAdmin,
        nombreAdmin: data.nombreAdmin,
        apellidoAdmin: data.apellidoAdmin,
        nombre: data.nombre,
        cuit: data.cuit,
        direccion: data.direccion,
        logoURL: data.logoURL ?? null,
        telefono: data.telefono ?? null,
        email: data.email ?? null,
        puntoDeVenta: data.puntoDeVenta ?? null,
        condicionVentas: data.condicionVentas ?? null,
        tipo: data.tipo,
        activo: data.activo,
        idPlan: data.idPlan,
        tipoFacturacion: data.tipoFacturacion,
        activarSuscripcion: data.activarSuscripcion,
      };
      const res = await api.post('/api/v1/super-admin/tenants', payload);
      return mapTenant(res.data);
    },
    onSuccess: () => {
      toast.success('Negocio creado correctamente');
      qc.invalidateQueries({ queryKey: ['super-admin', 'tenants'] });
      qc.invalidateQueries({ queryKey: ['super-admin', 'metrics'] });
    },
    onError: (error: unknown) => {
      const err = error as { response?: { data?: { error?: string; message?: string } } };
      toast.error(err.response?.data?.error || err.response?.data?.message || 'Error al crear negocio');
    },
  });
}

export function useUpdateTenantEstado() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async ({ id, estado }: { id: number; estado: string }) => {
      const res = await api.put(`/api/v1/super-admin/tenants/${id}/estado`, { estado });
      return res.data;
    },
    onSuccess: () => {
      toast.success('Estado actualizado');
      qc.invalidateQueries({ queryKey: ['super-admin', 'tenants'] });
    },
    onError: (error: unknown) => {
      const err = error as { response?: { data?: { error?: string } } };
      toast.error(err.response?.data?.error || 'Error al actualizar estado');
    },
  });
}
