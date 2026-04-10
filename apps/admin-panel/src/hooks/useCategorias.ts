/**
 * Categorías Hooks - React Query hooks for category management
 * RoKey MANAGEMENT - Multi-tenant SaaS ERP/POS for locksmiths
 * 
 * Phase 2: Data Layer - CRUD operations for categories
 * Only Dueño and Gerente can manage categories
 */

import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { api } from '@/lib/api/axios';
import { authStore } from '@/stores/authStore';
import type {
  Categoria,
  CategoriaListResponse,
  CrearCategoriaRequest,
  ActualizarCategoriaRequest,
  CategoriaFilters,
} from '@/types';
import { toast } from 'sonner';

// ============================================
// Configuration Constants
// ============================================

const CATEGORIAS_QUERY_CONFIG = {
  staleTime: 30000,    // 30 seconds
  retry: 2,
  refetchOnWindowFocus: false,
};

// ============================================
// Helper Functions
// ============================================

/**
 * Get current user role from auth store
 */
function getUserRole() {
  const user = authStore.getState().user;
  if (!user) return null;
  return user.rol;
}

/**
 * Check if user can manage categories (Dueño or Gerente only)
 */
function canManageCategorias(): boolean {
  const role = getUserRole();
  return role === 'Dueño' || role === 'Gerente';
}

// ============================================
// Query Hooks
// ============================================

/**
 * Hook for fetching paginated list of categories
 * GET /api/v1/categorias
 */
export interface UseCategoriasOptions {
  filters?: CategoriaFilters;
  pagina?: number;
  tamanoPagina?: number;
  ordenarPor?: string;
  orden?: 'asc' | 'desc';
}

export function useCategorias(options: UseCategoriasOptions = {}) {
  const { filters = {}, pagina = 1, tamanoPagina = 50, ordenarPor = 'nombre', orden = 'asc' } = options;

  return useQuery({
    queryKey: ['categorias', filters, pagina, tamanoPagina, ordenarPor, orden],
    queryFn: async () => {
      const response = await api.get<CategoriaListResponse>('/api/v1/categorias', {
        params: {
          ...filters,
          pagina,
          tamanoPagina,
          ordenarPor,
          orden,
        },
      });
      return response.data;
    },
    ...CATEGORIAS_QUERY_CONFIG,
  });
}

/**
 * Hook for fetching a single category by ID
 * GET /api/v1/categorias/{id}
 */
export function useCategoria(id: number | null) {
  return useQuery({
    queryKey: ['categoria', id],
    queryFn: async () => {
      if (!id) return null;
      const response = await api.get<Categoria>(`/api/v1/categorias/${id}`);
      return response.data;
    },
    enabled: !!id,
    ...CATEGORIAS_QUERY_CONFIG,
  });
}

// ============================================
// Mutation Hooks
// ============================================

/**
 * Hook for creating a new category
 * POST /api/v1/categorias
 * Only Dueño or Gerente can create categories
 */
export function useCreateCategoria() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (data: CrearCategoriaRequest) => {
      const response = await api.post<Categoria>('/api/v1/categorias', data);
      return response.data;
    },
    onSuccess: () => {
      toast.success('Categoría creada exitosamente');
      queryClient.invalidateQueries({ queryKey: ['categorias'] });
    },
    onError: (error: unknown) => {
      const err = error as { response?: { data?: { detail?: string } } };
      toast.error(err.response?.data?.detail || 'Error al crear la categoría');
    },
  });
}

/**
 * Hook for updating an existing category
 * PUT /api/v1/categorias/{id}
 * Only Dueño or Gerente can update categories
 */
export function useUpdateCategoria() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ id, data }: { id: number; data: ActualizarCategoriaRequest }) => {
      const response = await api.put<Categoria>(`/api/v1/categorias/${id}`, data);
      return response.data;
    },
    onSuccess: () => {
      toast.success('Categoría actualizada exitosamente');
      queryClient.invalidateQueries({ queryKey: ['categorias'] });
      queryClient.invalidateQueries({ queryKey: ['categoria'] });
    },
    onError: (error: unknown) => {
      const err = error as { response?: { data?: { detail?: string } } };
      toast.error(err.response?.data?.detail || 'Error al actualizar la categoría');
    },
  });
}

/**
 * Hook for deleting (deactivating) a category
 * DELETE /api/v1/categorias/{id}
 * Only Dueño or Gerente can delete categories
 */
export function useDeleteCategoria() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (id: number) => {
      await api.delete(`/api/v1/categorias/${id}`);
    },
    onSuccess: () => {
      toast.success('Categoría eliminada exitosamente');
      queryClient.invalidateQueries({ queryKey: ['categorias'] });
    },
    onError: (error: unknown) => {
      const err = error as { response?: { data?: { detail?: string } } };
      toast.error(err.response?.data?.detail || 'Error al eliminar la categoría');
    },
  });
}

// ============================================
// Utility Exports
// ============================================

export { canManageCategorias };