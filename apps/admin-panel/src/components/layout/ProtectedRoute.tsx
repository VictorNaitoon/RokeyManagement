import { Navigate, useLocation } from 'react-router-dom';
import { authStore, type UserRole } from '@/stores/authStore';

interface ProtectedRouteProps {
  children: React.ReactNode;
  roles?: UserRole[];
}

export function ProtectedRoute({ children, roles }: ProtectedRouteProps) {
  const location = useLocation();
  const { isAuthenticated, user } = authStore();

  if (!isAuthenticated) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  if (roles && user && !roles.includes(user.rol)) {
    // User doesn't have required role - redirect to dashboard
    return <Navigate to="/dashboard" replace />;
  }

  return <>{children}</>;
}