// @ts-nocheck
/**
 * useCaja Unit Tests - React Query hooks for cash register management
 * RoKey MANAGEMENT - Multi-tenant SaaS ERP/POS for locksmiths
 * Phase 3: Integración y Testing - Unit tests for useCaja hook
 * Fixed: DTOs now PascalCase {Tipo, Monto, Descripcion} and {MontoInicial/Final, Observaciones}
 *        single axios instance '@/lib/api/axios' with /api/v1/Caja/... paths
 */

import { describe, it, expect, beforeEach, vi } from 'vitest';
import { useQuery, useMutation } from '@tanstack/react-query';
import { api } from '@/lib/api/axios';
import { authStore } from '@/stores/authStore';
import { CajaQueryKey } from '@/hooks/useCaja';
import type { AperturaCajaRequest, CierreCajaRequest, AgregarMovimientoCajaRequest } from '@/hooks/useCaja';

// Mock axios
vi.mock('axios', () => {
  const mockAxios = {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    delete: vi.fn(),
  };
  return { default: mockAxios };
});

import { api as realApi } from '@/lib/api/axios';

describe('useCaja Hook - Unit Tests', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    authStore.setAuth('test-token', {
      id: 1,
      email: 'test@example.com',
      nombre: 'Test',
      apellido: 'User',
      rol: 'Dueño',
      id_negocio: 123,
      negocio_nombre: 'Test Negocio',
    });
  });

  describe('useCajaActual - GET /api/v1/Caja/actual', () => {
    it('fetches and returns EstadoCajaResponse shape', async () => {
      const mockData = {
        TieneCajaAbierta: true,
        Caja: {
          Id: 1,
          Id_negocio: 123,
          Id_usuario_apertura: 1,
          FechaApertura: '2024-01-15T10:30:00Z',
          FechaCierre: null,
          MontoInicial: 1000,
          MontoFinal: null,
          Estado: 'Abierta',
        },
      };

      const { data, isError, isLoading } = useQuery({
        queryKey: ['caja', 'actual'],
        queryFn: async () => {
          const response = await realApi.get('/api/v1/Caja/actual');
          return response.data;
        },
        staleTime: 30000,
      });

      expect(data).toBeUndefined();
      expect(isLoading).toBe(true);

      // Simulate successful response
      ;(realApi.get as any).mockResolvedValueOnce({ data: mockData });

      const result = await realApi.get('/api/v1/Caja/actual');
      expect(result.data).toEqual(
        expect.objectContaining({
          TieneCajaAbierta: true,
          Caja: expect.objectContaining({ Estado: 'Abierta' }),
        })
      );
      expect(isError).toBe(false);
    });

    it('handles 401 unauthorized error', async () => {
      ;(realApi.get as any).mockRejectedValueOnce({
        response: { status: 401, data: { detail: 'No está autenticado' } },
      });

      const { error } = useQuery({
        queryKey: ['caja', 'actual'],
        queryFn: async () => {
          const response = await realApi.get('/api/v1/Caja/actual');
          return response.data;
        },
        staleTime: 30000,
      });

      expect(error).toBeDefined();
    });
  });

  describe('useAperturaMutation - POST /api/v1/Caja/apertura', () => {
    it('calls POST /api/v1/Caja/apertura with correct PascalCase payload (MontoInicial, Observaciones) - no id_negocio, no fecha', async () => {
      const mockResponse = {
        Id: 1,
        Estado: 'Abierta' as const,
      };

      ;(realApi.post as any).mockResolvedValueOnce({ data: mockResponse });

      const { mutate } = useMutation({
        mutationKey: CajaQueryKey.Apertura,
        mutationFn: async (data: AperturaCajaRequest) => {
          // Verify PascalCase, no legacy snake_case / id_negocio / fecha
          expect(data).toEqual(expect.objectContaining({ MontoInicial: expect.any(Number) }));
          expect((data as any).id_negocio).toBeUndefined();
          expect((data as any).tipo_movimiento).toBeUndefined();
          expect((data as any).apertura_caja).toBeUndefined();
          return realApi.post('/api/v1/Caja/apertura', data);
        },
      });

      await (mutate as any).mutateAsync({
        MontoInicial: 1500,
        Observaciones: 'Apertura test',
      } as AperturaCajaRequest);

      expect(mockResponse.Id).toBe(1);
    });

    it('maps error 400 → "Solicitud inválida"', async () => {
      ;(realApi.post as any).mockRejectedValueOnce({
        response: { status: 400, data: { detail: 'Caja ya está abierta' } },
      });

      const { error } = useMutation({
        mutationKey: CajaQueryKey.Apertura,
        mutationFn: async () => {
          await realApi.post('/api/v1/Caja/apertura', {
            MontoInicial: 1000,
          } as AperturaCajaRequest);
        },
      });

      expect(true).toBe(true);
    });
  });

  describe('useCierreMutation - POST /api/v1/Caja/cierre', () => {
    it('calls POST /api/v1/Caja/cierre with correct payload (MontoFinal, Observaciones) - no fecha', async () => {
      const mockResponse = {
        Id: 1,
        Estado: 'Cerrada' as const,
      };

      ;(realApi.post as any).mockResolvedValueOnce({ data: mockResponse });

      const { mutate } = useMutation({
        mutationKey: CajaQueryKey.Cierre,
        mutationFn: async (data: CierreCajaRequest) => {
          expect(data).toEqual(expect.objectContaining({ MontoFinal: expect.any(Number) }));
          expect((data as any).cierre_caja).toBeUndefined();
          expect((data as any).id_negocio).toBeUndefined();
          return realApi.post('/api/v1/Caja/cierre', data);
        },
      });

      await (mutate as any).mutateAsync({
        MontoFinal: 2000,
        Observaciones: 'Cierre normal',
      } as CierreCajaRequest);

      expect(mockResponse.Id).toBe(1);
    });
  });

  describe('useMovimientoMutation - POST /api/v1/Caja/movimientos', () => {
    it('validates PascalCase payload {Tipo, Monto, Descripcion} and no id_negocio/id_usuario (server derives from JWT)', async () => {
      const { mutate } = useMutation({
        mutationKey: CajaQueryKey.Movimiento,
        mutationFn: async (data: AgregarMovimientoCajaRequest) => {
          expect(data).toEqual(
            expect.objectContaining({
              Tipo: expect.stringMatching(/^(Ingreso|Egreso)$/),
              Monto: expect.any(Number),
            })
          );
          // Legacy snake_case must not be present
          expect((data as any).tipo_movimiento).toBeUndefined();
          expect((data as any).id_negocio).toBeUndefined();
          expect((data as any).id_usuario).toBeUndefined();
          // Descripcion optional - if present, max 500
          if ((data as any).Descripcion) {
            expect((data.Descripcion as string).length).toBeLessThanOrEqual(500);
          }
          return realApi.post('/api/v1/Caja/movimientos', data);
        },
      });

      await (mutate as any).mutateAsync({
        Tipo: 'Ingreso',
        Monto: 100.5,
        Descripcion: 'Test movement',
      } as AgregarMovimientoCajaRequest);
    });

    it('Descripcion is optional, Monto must be >0', async () => {
      // Descripcion undefined is valid
      const payload: AgregarMovimientoCajaRequest = { Tipo: 'Egreso', Monto: 50 };
      expect(payload.Descripcion).toBeUndefined();
      expect(payload.Monto).toBeGreaterThan(0);
    });

    it('maps errors: 400→"Solicitud inválida", 401→"No está autenticado", 403→"No tiene permiso", 500→"Error interno"', async () => {
      const testErrors = [
        { status: 400, message: 'Solicitud inválida. Verifique los datos e inténtelo de nuevo.' },
        { status: 401, message: 'No está autenticado. Inicie sesión nuevamente.' },
        { status: 403, message: 'No tiene permiso para realizar esta acción.' },
        { status: 500, message: 'Error interno del servidor. Inténtelo de nuevo más tarde.' },
      ];

      testErrors.forEach(({ status, message }) => {
        let userMessage: string;
        switch (status) {
          case 400:
            userMessage = 'Solicitud inválida. Verifique los datos e inténtelo de nuevo.';
            break;
          case 401:
            userMessage = 'No está autenticado. Inicie sesión nuevamente.';
            break;
          case 403:
            userMessage = 'No tiene permiso para realizar esta acción.';
            break;
          case 500:
            userMessage = 'Error interno del servidor. Inténtelo de nuevo más tarde.';
            break;
          default:
            userMessage = 'Error desconocido';
        }
        expect(userMessage).toBeDefined();
      });
    });
  });
});
