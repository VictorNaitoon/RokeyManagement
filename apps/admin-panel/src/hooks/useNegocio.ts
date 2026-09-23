import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { api } from '@/lib/api/axios';
import { toast } from 'sonner';

export interface Negocio {
  id: number;
  nombre: string;
  cuit: string;
  direccion: string;
  logoURL?: string | null;
  estado: string;
  tipo: string;
  ingresosBrutos: number;
  fechaInicioActividades: string;
  puntoDeVenta?: string | null;
  telefono?: string | null;
  email?: string | null;
  condicionVentas?: string | null;
  totalUsuarios: number;
  totalProductos: number;
}

export interface ActualizarNegocioRequest {
  nombre: string;
  cuit: string;
  direccion: string;
  logoURL?: string | null;
  telefono?: string | null;
  email?: string | null;
  puntoDeVenta?: string | null;
  condicionVentas?: string | null;
  tipo: number;
}

const NEGOCIO_QUERY_CONFIG = {
  staleTime: 30000,
  retry: 1,
  refetchOnWindowFocus: false,
};

export function useNegocio() {
  return useQuery({
    queryKey: ['negocio'],
    queryFn: async () => {
      const res = await api.get<Negocio>('/api/v1/negocio');
      const d = res.data as any;
      // Map PascalCase -> camelCase if needed
      return {
        id: d.id ?? d.Id,
        nombre: d.nombre ?? d.Nombre ?? '',
        cuit: d.cuit ?? d.CUIT ?? '',
        direccion: d.direccion ?? d.Direccion ?? '',
        logoURL: d.logoURL ?? d.LogoURL ?? null,
        estado: d.estado ?? d.Estado ?? '',
        tipo: d.tipo ?? d.Tipo ?? '',
        ingresosBrutos: d.ingresosBrutos ?? d.IngresosBrutos ?? 0,
        fechaInicioActividades: d.fechaInicioActividades ?? d.FechaInicioActividades ?? '',
        puntoDeVenta: d.puntoDeVenta ?? d.PuntoDeVenta ?? null,
        telefono: d.telefono ?? d.Telefono ?? null,
        email: d.email ?? d.Email ?? null,
        condicionVentas: d.condicionVentas ?? d.CondicionVentas ?? null,
        totalUsuarios: d.totalUsuarios ?? d.TotalUsuarios ?? 0,
        totalProductos: d.totalProductos ?? d.TotalProductos ?? 0,
      } as Negocio;
    },
    ...NEGOCIO_QUERY_CONFIG,
  });
}

export function useUpdateNegocio() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (data: ActualizarNegocioRequest) => {
      const res = await api.put<Negocio>('/api/v1/negocio', data);
      return res.data;
    },
    onSuccess: () => {
      toast.success('Datos del negocio actualizados');
      queryClient.invalidateQueries({ queryKey: ['negocio'] });
    },
    onError: (error: unknown) => {
      const err = error as { response?: { data?: { message?: string } } };
      toast.error(err.response?.data?.message || 'Error al actualizar el negocio');
    },
  });
}
