import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { api } from '@/lib/api/axios';
import { authStore } from '@/stores/authStore';
import { toast } from 'sonner';

export interface Usuario {
  id: number;
  nombre: string;
  apellido: string;
  email: string;
  rol: string;
  activo?: boolean;
}

export interface CrearUsuarioRequest {
  nombre: string;
  apellido: string;
  email: string;
  password: string;
  rol: number; // 1=Dueño 2=Gerente 3=Empleado
}

export interface ActualizarUsuarioRequest {
  nombre: string;
  apellido: string;
  rol: number;
  activo: boolean;
}

export interface CambiarPasswordRequest {
  passwordActual: string;
  passwordNuevo: string;
}

const USUARIOS_CONFIG = {
  staleTime: 30000,
  retry: 1,
  refetchOnWindowFocus: false,
};

function mapUsuario(raw: any): Usuario {
  return {
    id: raw.id ?? raw.Id,
    nombre: raw.nombre ?? raw.Nombre ?? '',
    apellido: raw.apellido ?? raw.Apellido ?? '',
    email: raw.email ?? raw.Email ?? '',
    rol: raw.rol ?? raw.Rol ?? '',
    activo: raw.activo ?? raw.Activo ?? true,
  };
}

export function useUsuarios(enabled = true) {
  return useQuery({
    queryKey: ['usuarios'],
    queryFn: async () => {
      const res = await api.get('/api/v1/usuario');
      const data = res.data as any;
      const arr = Array.isArray(data) ? data : data.items ?? data.usuarios ?? [];
      return (arr as any[]).map(mapUsuario);
    },
    enabled,
    ...USUARIOS_CONFIG,
  });
}

export function useCreateUsuario() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (data: CrearUsuarioRequest) => {
      const res = await api.post('/api/v1/usuario', data);
      return mapUsuario(res.data);
    },
    onSuccess: () => {
      toast.success('Usuario creado');
      qc.invalidateQueries({ queryKey: ['usuarios'] });
    },
    onError: (error: unknown) => {
      const err = error as { response?: { data?: { message?: string } } };
      toast.error(err.response?.data?.message || 'Error al crear usuario');
    },
  });
}

export function useUpdateUsuario() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async ({ id, data }: { id: number; data: ActualizarUsuarioRequest }) => {
      const res = await api.put(`/api/v1/usuario/${id}`, data);
      return mapUsuario(res.data);
    },
    onSuccess: () => {
      toast.success('Usuario actualizado');
      qc.invalidateQueries({ queryKey: ['usuarios'] });
    },
    onError: (error: unknown) => {
      const err = error as { response?: { data?: { message?: string } } };
      toast.error(err.response?.data?.message || 'Error al actualizar usuario');
    },
  });
}

export function useDeleteUsuario() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (id: number) => {
      await api.delete(`/api/v1/usuario/${id}`);
    },
    onSuccess: () => {
      toast.success('Usuario desactivado');
      qc.invalidateQueries({ queryKey: ['usuarios'] });
    },
    onError: (error: unknown) => {
      const err = error as { response?: { data?: { message?: string } } };
      toast.error(err.response?.data?.message || 'Error al desactivar usuario');
    },
  });
}

export function useCambiarPassword() {
  return useMutation({
    mutationFn: async (data: CambiarPasswordRequest) => {
      await api.post('/api/v1/usuario/cambiar-password', {
        passwordActual: data.passwordActual,
        passwordNuevo: data.passwordNuevo,
      });
    },
    onSuccess: () => toast.success('Contraseña actualizada — se cerrarán otras sesiones'),
    onError: (error: unknown) => {
      const err = error as { response?: { data?: { message?: string } } };
      toast.error(err.response?.data?.message || 'Error al cambiar contraseña');
    },
  });
}

export interface ActualizarPerfilRequest {
  nombre: string;
  apellido: string;
  email: string;
}

export function useUpdatePerfil() {
  return useMutation({
    mutationFn: async (data: ActualizarPerfilRequest) => {
      const res = await api.put('/api/v1/usuario/me', data);
      const raw: any = res.data;
      return {
        id: raw.id ?? raw.Id,
        nombre: raw.nombre ?? raw.Nombre ?? '',
        apellido: raw.apellido ?? raw.Apellido ?? '',
        email: raw.email ?? raw.Email ?? '',
        rol: raw.rol ?? raw.Rol ?? '',
        activo: raw.activo ?? raw.Activo ?? true,
      } as Usuario;
    },
    onSuccess: (updated) => {
      toast.success('Perfil actualizado');
      // Keep authStore in sync so header/routing reflects new name/email without reload
      try {
        const current = authStore.getState().user;
        if (current && String(current.id) === String(updated.id)) {
          authStore.getState().setUser({
            ...current,
            nombre: updated.nombre,
            apellido: updated.apellido,
            email: updated.email,
          });
        }
      } catch {
        // ignore
      }
    },
    onError: (error: unknown) => {
      const err = error as { response?: { data?: { message?: string } } };
      toast.error(err.response?.data?.message || 'Error al actualizar perfil');
    },
  });
}
