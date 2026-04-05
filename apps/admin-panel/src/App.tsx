import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { QueryClientProvider } from '@tanstack/react-query';
import { Toaster } from '@/components/ui/sonner';
import { queryClient } from '@/lib/api/query-client';
import { authStore } from '@/stores/authStore';
import { ProtectedRoute } from '@/components/layout/ProtectedRoute';
import { LoginPage } from '@/pages/auth/LoginPage';
import { DashboardPage } from '@/pages/dashboard/DashboardPage';
import { SuperAdminDashboardPage } from '@/pages/superadmin/SuperAdminDashboardPage';
import { DashboardLayout } from '@/components/layout/DashboardLayout';

function App() {
  const { isAuthenticated, user } = authStore();

  // Redirect authenticated users based on role
  const defaultRedirect = user?.rol === 'SuperAdmin' ? '/admin' : '/dashboard';

  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <Routes>
          {/* Public routes */}
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

          {/* Protected routes with DashboardLayout (business users) */}
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
          </Route>

          {/* SuperAdmin routes */}
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

          {/* Catch all */}
          <Route path="*" element={<Navigate to={isAuthenticated ? defaultRedirect : '/login'} replace />} />
        </Routes>
      </BrowserRouter>
      <Toaster />
    </QueryClientProvider>
  );
}

export default App;
