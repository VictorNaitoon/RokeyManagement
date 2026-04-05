import { useState } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { useMutation } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { toast } from 'sonner';
import { Loader2 } from 'lucide-react';
import { api } from '@/lib/api/axios';
import { authStore, type User, type UserRole } from '@/stores/authStore';
import { loginSchema, type LoginFormData } from '@/lib/schemas/login.schema';
import { cn } from '@/lib/utils';

interface LoginResponse {
  token: string;
  usuario: User;
}

interface SuperAdminLoginResponse {
  token: string;
  superAdmin: {
    id: number;
    email: string;
    rol: string;
  };
}

export function LoginPage() {
  const navigate = useNavigate();
  const location = useLocation();
  const [isSuperAdmin, setIsSuperAdmin] = useState(false);
  const [error, setError] = useState('');

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginFormData>({
    resolver: zodResolver(loginSchema),
  });

  const from = (location.state as { from?: Location })?.from?.pathname || '/dashboard';

  const loginMutation = useMutation({
    mutationFn: async (data: { email: string; password: string }) => {
      if (isSuperAdmin) {
        const response = await api.post<SuperAdminLoginResponse>('/api/v1/auth/super-admin/login', data);
        return { type: 'superadmin' as const, data: response.data };
      }
      const response = await api.post<LoginResponse>('/api/v1/auth/login', data);
      return { type: 'user' as const, data: response.data };
    },
    onSuccess: (result) => {
      toast.success('Bienvenido');
      if (result.type === 'superadmin') {
        const sa = result.data.superAdmin;
        authStore.getState().setAuth(result.data.token, {
          id: sa.id,
          email: sa.email,
          nombre: 'Super',
          apellido: 'Admin',
          rol: 'SuperAdmin' as UserRole,
        });
        navigate('/admin', { replace: true });
      } else {
        authStore.getState().setAuth(result.data.token, result.data.usuario);
        navigate(from, { replace: true });
      }
    },
    onError: (error: unknown) => {
      // Clear local error state - we're using sonner now
      setError('');
      
      // Determine error type from axios response
      const axiosError = error as { response?: { status?: number }; message?: string };
      const status = axiosError.response?.status;
      
      if (status === 401) {
        toast.error('Credenciales inválidas');
      } else if (status >= 500) {
        toast.error('Error del servidor. Intenta más tarde');
      } else if (status === 0 || axiosError.message?.includes('Network Error')) {
        toast.error('Error de conexión. Verifica tu internet');
      } else {
        toast.error('Error de conexión');
      }
    },
  });

  const onSubmit = (data: LoginFormData) => {
    setError('');
    loginMutation.mutate(data);
  };

  const handleSuperAdminToggle = () => {
    setIsSuperAdmin(!isSuperAdmin);
    setError('');
  };

  return (
    <div className="space-y-6">
      {/* Logo & Brand */}
      <div className="text-center">
        <h1 className="text-2xl font-bold text-primary">RoKey</h1>
        <p className="text-sm text-text-secondary mt-1">Management</p>
      </div>

      {/* Login Card */}
      <div className="bg-surface rounded-lg shadow-md p-6 border border-border">
        <h2 className="text-lg font-semibold text-center mb-6">
          {isSuperAdmin ? 'Super Admin' : 'Iniciar Sesión'}
        </h2>
        
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          {error && (
            <div className="p-3 bg-error/10 border border-error/20 rounded-lg text-sm text-error">
              {error}
            </div>
          )}

          <div className="space-y-2">
            <label htmlFor="email" className="block text-sm font-medium">
              Correo electrónico
            </label>
            <input
              id="email"
              type="email"
              {...register('email')}
              disabled={loginMutation.isPending}
              className={cn(
                'w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-primary',
                'border-border bg-surface',
                errors.email && 'border-error focus:ring-error'
              )}
              placeholder={isSuperAdmin ? 'admin@rokeystore.com' : 'correo@ejemplo.com'}
            />
            {errors.email && (
              <p className="text-sm text-error">{errors.email.message}</p>
            )}
          </div>

          <div className="space-y-2">
            <label htmlFor="password" className="block text-sm font-medium">
              Contraseña
            </label>
            <input
              id="password"
              type="password"
              {...register('password')}
              disabled={loginMutation.isPending}
              className={cn(
                'w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-primary',
                'border-border bg-surface',
                errors.password && 'border-error focus:ring-error'
              )}
              placeholder="••••••••"
            />
            {errors.password && (
              <p className="text-sm text-error">{errors.password.message}</p>
            )}
          </div>

          <button
            type="submit"
            disabled={loginMutation.isPending}
            className={cn(
              'w-full py-2 px-4 rounded-lg font-medium transition-colors flex items-center justify-center gap-2',
              'bg-primary hover:bg-primary-hover text-white',
              'disabled:opacity-50 disabled:cursor-not-allowed'
            )}
          >
            {loginMutation.isPending && <Loader2 className="size-4 animate-spin" />}
            {loginMutation.isPending ? 'Ingresando...' : 'Iniciar Sesión'}
          </button>
        </form>
      </div>

      {/* SuperAdmin Toggle */}
      <div className="text-center">
        <button
          type="button"
          onClick={handleSuperAdminToggle}
          className="text-sm text-primary hover:underline"
        >
          {isSuperAdmin ? '← Volver al login normal' : 'Ingresar como Super Admin'}
        </button>
      </div>
    </div>
  );
}
