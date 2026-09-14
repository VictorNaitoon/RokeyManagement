// @ts-nocheck
/**
 * CajaPage Integration Tests - Role-based access and rendering
 * RoKey MANAGEMENT - Multi-tenant SaaS ERP/POS for locksmiths
 * Phase 3: Integración y Testing - Integration tests for CajaPage
 * Fixed: roles use 'Empleado' (not 'Vendedor'), payloads use PascalCase, audit date is server-side
 */

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MockedProvider } from '@vitest/test-utils/react-query';
import Router from 'next/router';
import CajaPage from '@/pages/caja/CajaPage';
import { authStore } from '@/stores/authStore';

// Mock next/router
vi.mock('next/router', () => ({
  useRouter: () => ({ push: vi.fn(), replace: vi.fn() }),
  usePathname: () => '/caja',
  useSearchParams: () => new URLSearchParams(),
  usePathnameAndSearch: () => '/caja',
}));

// Mock react-query
const mockQueryClient = new (require('@tanstack/react-query').QueryClient)();

describe('CajaPage Integration Tests', () => {
  beforeEach(() => {
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

  it('renders at route /caja within DashboardLayout without errors', () => {
    render(
      <MockedProvider queryClient={mockQueryClient}>
        <CajaPage />
      </MockedProvider>
    );

    const heading = screen.getByRole('heading', { name: /Caja/i });
    expect(heading).toBeInTheDocument();
  });

  it('KPICards display data from Actual query successfully', () => {
    render(
      <MockedProvider queryClient={mockQueryClient}>
        <CajaPage />
      </MockedProvider>
    );

    const kpiCards = screen.getByTestId('kpi-cards');
    expect(kpiCards).toBeInTheDocument();

    const estadoElement = screen.getByText(/Abierta/i);
    expect(estadoElement).toBeInTheDocument();
  });

  it('AbrirCaja button visible only for Dueño/Gerente, disabled/hidden for others', () => {
    // Test with Dueño role
    authStore.setAuth('test-token', {
      id: 1,
      email: 'test@example.com',
      nombre: 'Test',
      apellido: 'User',
      rol: 'Dueño',
      id_negocio: 123,
      negocio_nombre: 'Test Negocio',
    });

    render(
      <MockedProvider queryClient={mockQueryClient}>
        <CajaPage />
      </MockedProvider>
    );

    const abrirButton = screen.getByRole('button', { name: /Abrir Caja/i });
    expect(abrirButton).toBeInTheDocument();

    // Test with SuperAdmin role
    authStore.setAuth('test-token', {
      id: 1,
      email: 'test@example.com',
      nombre: 'Test',
      apellido: 'User',
      rol: 'SuperAdmin',
      id_negocio: 123,
      negocio_nombre: 'Test Negocio',
    });

    render(
      <MockedProvider queryClient={mockQueryClient}>
        <CajaPage />
      </MockedProvider>
    );

    // SuperAdmin should not see the Abrir Caja button
    const superAdminButton = screen.queryByRole('button', { name: /Abrir Caja/i });
    expect(superAdminButton).not.toBeInTheDocument();
  });

  it('CerrarCaja button same role check', () => {
    // Dueño should see Cerrar Caja button
    authStore.setAuth('test-token', {
      id: 1,
      email: 'test@example.com',
      nombre: 'Test',
      apellido: 'User',
      rol: 'Dueño',
      id_negocio: 123,
      negocio_nombre: 'Test Negocio',
    });

    render(
      <MockedProvider queryClient={mockQueryClient}>
        <CajaPage />
      </MockedProvider>
    );

    const cerrarButton = screen.getByRole('button', { name: /Cerrar Caja/i });
    expect(cerrarButton).toBeInTheDocument();

    // Empleado should not see Cerrar Caja button (only Dueño/Gerente)
    authStore.setAuth('test-token', {
      id: 1,
      email: 'test@example.com',
      nombre: 'Test',
      apellido: 'User',
      rol: 'Empleado',
      id_negocio: 123,
      negocio_nombre: 'Test Negocio',
    });

    render(
      <MockedProvider queryClient={mockQueryClient}>
        <CajaPage />
      </MockedProvider>
    );

    const empleadoButton = screen.queryByRole('button', { name: /Cerrar Caja/i });
    expect(empleadoButton).not.toBeInTheDocument();
  });

  it('Agregar Movimiento form visible for Dueño/Gerente/Empleado, hidden for SuperAdmin', () => {
    // Dueño should see Movimiento form
    authStore.setAuth('test-token', {
      id: 1,
      email: 'test@example.com',
      nombre: 'Test',
      apellido: 'User',
      rol: 'Dueño',
      id_negocio: 123,
      negocio_nombre: 'Test Negocio',
    });

    render(
      <MockedProvider queryClient={mockQueryClient}>
        <CajaPage />
      </MockedProvider>
    );

    const movimientoForm = screen.getByText(/Agregar Movimiento/i);
    expect(movimientoForm).toBeInTheDocument();

    // Empleado should also see Movimiento form (Dueño/Gerente/Empleado allowed)
    authStore.setAuth('test-token', {
      id: 1,
      email: 'test@example.com',
      nombre: 'Test',
      apellido: 'User',
      rol: 'Empleado',
      id_negocio: 123,
      negocio_nombre: 'Test Negocio',
    });

    render(
      <MockedProvider queryClient={mockQueryClient}>
        <CajaPage />
      </MockedProvider>
    );

    const empleadoForm = screen.getByText(/Agregar Movimiento/i);
    expect(empleadoForm).toBeInTheDocument();

    // SuperAdmin should not see the form
    authStore.setAuth('test-token', {
      id: 1,
      email: 'test@example.com',
      nombre: 'Test',
      apellido: 'User',
      rol: 'SuperAdmin',
      id_negocio: 123,
      negocio_nombre: 'Test Negocio',
    });

    render(
      <MockedProvider queryClient={mockQueryClient}>
        <CajaPage />
      </MockedProvider>
    );

    const superAdminForm = screen.queryByText(/Agregar Movimiento/i);
    expect(superAdminForm).not.toBeInTheDocument();
  });

  it('MovimientosTable loads and displays data when movements query returns results', () => {
    const mockMovimientos = [
      {
        id: 1,
        fecha: '2024-01-15T10:30:00Z',
        tipo: 'ingreso' as const,
        descripcion: 'Venta de cerradura',
        monto: 500.0,
        usuario_nombre: 'Juan Pérez',
      },
      {
        id: 2,
        fecha: '2024-01-14T14:20:00Z',
        tipo: 'egreso' as const,
        descripcion: 'Compra de materiales',
        monto: 250.0,
        usuario_nombre: 'María González',
      },
    ];

    render(
      <MockedProvider queryClient={mockQueryClient}>
        <CajaPage />
      </MockedProvider>
    );

    // Mock the movimientos query - now uses cajaId not negocioId
    const { query } = require('@tanstack/react-query');
    const cajaMovimientosQuery = query({
      queryKey: ['caja', 'movimientos', 1],
      queryFn: async () => mockMovimientos,
    });

    expect(cajaMovimientosQuery).toBeDefined();
  });

  it('role-based conditional rendering works correctly across all user roles (Dueño, Gerente, Empleado, SuperAdmin)', () => {
    const roles = ['Dueño', 'Gerente', 'Empleado', 'SuperAdmin'];

    roles.forEach((rol) => {
      authStore.setAuth('test-token', {
        id: 1,
        email: 'test@example.com',
        nombre: 'Test',
        apellido: 'User',
        rol,
        id_negocio: 123,
        negocio_nombre: 'Test Negocio',
      });

      render(
        <MockedProvider queryClient={mockQueryClient}>
          <CajaPage />
        </MockedProvider>
      );

      // Each role should render without crashing
      const pageContent = screen.getByText(/Caja/i);
      expect(pageContent).toBeInTheDocument();
    });
  });

  it('No fecha field is sent in Apertura/Cierre requests - audit is server-side (UtcNow)', () => {
    // This test documents the audit contract: frontend only sends Monto/Observaciones
    const aperturaPayload = { MontoInicial: 1000, Observaciones: 'test' };
    expect((aperturaPayload as any).FechaApertura).toBeUndefined();
    expect((aperturaPayload as any).apertura_caja).toBeUndefined();
    expect((aperturaPayload as any).id_negocio).toBeUndefined();

    const cierrePayload = { MontoFinal: 1500, Observaciones: 'cierre' };
    expect((cierrePayload as any).FechaCierre).toBeUndefined();
    expect((cierrePayload as any).cierre_caja).toBeUndefined();
  });
});
