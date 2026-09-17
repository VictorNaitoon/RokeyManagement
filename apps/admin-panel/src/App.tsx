import { BrowserRouter, Routes, Route, Navigate, useNavigate } from 'react-router-dom';
import { useEffect } from 'react';
import { QueryClientProvider } from '@tanstack/react-query';
import { Toaster } from '@/components/ui/sonner';
import { queryClient } from '@/lib/api/query-client';
import { authStore } from '@/stores/authStore';
import { useSubscriptionStore } from '@/stores/subscriptionStore';
import { ProtectedRoute } from '@/components/layout/ProtectedRoute';
import { LoginPage } from '@/pages/auth/LoginPage';
import { DashboardPage } from '@/pages/dashboard/DashboardPage';
import { SuperAdminDashboardPage } from '@/pages/superadmin/SuperAdminDashboardPage';
import { SubscriptionBlockedPage } from '@/pages/subscription/SubscriptionBlockedPage';
import { ProductosPage } from '@/pages/productos/ProductosPage';
import { AlertasStockPage } from '@/pages/productos/AlertasStockPage';
import { CategoriasPage } from '@/pages/categorias/CategoriasPage';
import { VentasPage } from '@/pages/ventas/VentasPage';
import { CrearVentaPage } from '@/pages/ventas/CrearVentaPage';
import { ClientesPage } from '@/pages/clientes/ClientesPage';
import { ProveedoresPage } from '@/pages/proveedores/ProveedoresPage';
import { PresupuestosPage } from '@/pages/presupuestos/PresupuestosPage';
import { ComprasPage } from '@/pages/compras/ComprasPage';
import { CajaPage } from '@/pages/caja/CajaPage';
import { InformesPage } from '@/pages/informes/InformesPage';
import { DashboardLayout } from '@/components/layout/DashboardLayout';

function SubscriptionRedirect() {
  const navigate = useNavigate();
  const isBlocked = useSubscriptionStore((state) => state.isBlocked);

  useEffect(() => {
    if (isBlocked) {
      navigate('/suscripcion-bloqueada', { replace: true });
    }
  }, [isBlocked, navigate]);

  return null;
}

function App() {
  const { isAuthenticated, isLoading, user, initializeAuth } = authStore();
  const defaultRedirect = user?.rol === 'SuperAdmin' ? '/admin' : '/dashboard';

  useEffect(() => {
    initializeAuth();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="size-8 animate-spin rounded-full border-4 border-primary border-t-transparent" />
      </div>
    );
  }

  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <SubscriptionRedirect />
        <Routes>
          <Route
            path="/suscripcion-bloqueada"
            element={<SubscriptionBlockedPage />}
          />
          <Route
            path="/login"
            element={
              isAuthenticated ? (
                <Navigate to={defaultRedirect} replace />
              ) : (
                <div className="min-h-screen flex items-center justify-center bg-background p-4">
                  <div className="w-full max-w-[400px]">
                    <LoginPage />
                  </div>
                </div>
              )
            }
          />
          <Route
            path="/"
            element={
              <ProtectedRoute>
                <DashboardLayout />
              </ProtectedRoute>
            }
          >
            <Route index element={<Navigate to="/dashboard" replace />} />
            <Route path="dashboard" element={<DashboardPage />} />
            <Route path="productos" element={<ProductosPage />} />
            <Route path="productos/alertas" element={<AlertasStockPage />} />
            <Route path="categorias" element={<CategoriasPage />} />
            <Route path="ventas" element={<VentasPage />} />
            <Route path="ventas/nueva" element={<CrearVentaPage />} />
            <Route path="clientes" element={<ClientesPage />} />
            <Route path="proveedores" element={<ProveedoresPage />} />
            <Route path="presupuestos" element={<PresupuestosPage />} />
            <Route path="compras" element={<ComprasPage />} />
            <Route path="caja" element={<CajaPage />} />
            <Route
              path="informes"
              element={
                <ProtectedRoute roles={['Dueño', 'Gerente']}>
                  <InformesPage />
                </ProtectedRoute>
              }
            />
          </Route>
          <Route
            path="/admin"
            element={
              <ProtectedRoute roles={['SuperAdmin']}>
                <DashboardLayout />
              </ProtectedRoute>
            }
          >
            <Route index element={<SuperAdminDashboardPage />} />
          </Route>
          <Route path="*" element={<Navigate to={isAuthenticated ? defaultRedirect : '/login'} replace />} />
        </Routes>
      </BrowserRouter>
      <Toaster />
    </QueryClientProvider>
  );
}

export default App;
