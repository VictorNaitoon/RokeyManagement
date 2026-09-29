/**
 * Dashboard Page - Inicio: quick actions, today's metrics, stock alerts and latest sales
 * RoKey MANAGEMENT - Multi-tenant SaaS ERP/POS for locksmiths
 */

import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { format } from 'date-fns';
import { es } from 'date-fns/locale';
import {
  Clock,
  FileText,
  Package,
  RefreshCw,
  ShoppingCart,
  Truck,
  WifiOff,
} from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { authStore } from '@/stores/authStore';
import { useDashboardData } from '@/hooks/useDashboardData';
import {
  AlertasStockPanel,
  DashboardSkeleton,
  ResumenHoy,
  UltimasVentas,
} from '@/components/dashboard';

type QuickAction = {
  key: 'venta' | 'producto' | 'compra' | 'presupuesto';
  title: string;
  titleEmpleado?: string;
  href: string;
  icon: typeof ShoppingCart;
  description: string;
  descriptionEmpleado?: string;
  roles: readonly string[];
};

const quickActionsBase: readonly QuickAction[] = [
  {
    key: 'venta',
    title: 'Nueva Venta',
    href: '/ventas/nueva',
    icon: ShoppingCart,
    description: 'Registrar una venta',
    roles: ['Dueño', 'Gerente', 'Empleado'],
  },
  {
    key: 'producto',
    title: 'Agregar Producto',
    titleEmpleado: 'Listar Productos',
    href: '/productos',
    icon: Package,
    description: 'Crear nuevo producto',
    descriptionEmpleado: 'Ver productos',
    roles: ['Dueño', 'Gerente', 'Empleado'],
  },
  {
    key: 'compra',
    title: 'Registrar Compra',
    href: '/compras',
    icon: Truck,
    description: 'Cargar compra a proveedor',
    roles: ['Dueño', 'Gerente'],
  },
  {
    key: 'presupuesto',
    title: 'Crear Presupuesto',
    href: '/presupuestos',
    icon: FileText,
    description: 'Generar presupuesto',
    roles: ['Dueño', 'Gerente', 'Empleado'],
  },
];

export function DashboardPage() {
  const [secondsAgo, setSecondsAgo] = useState(0);
  const user = authStore((s) => s.user);
  const rol = user?.rol ?? 'Empleado';
  const canManageProductos = rol === 'Dueño';
  const filteredActions = quickActionsBase.filter((a) => a.roles.includes(rol));

  const { isLoading, isFetching, error, refetch, canViewAdminData, dataUpdatedAt } =
    useDashboardData();

  const lastUpdated = dataUpdatedAt > 0 ? new Date(dataUpdatedAt) : null;

  // Auto-refresh indicator: React Query already tells us when data landed,
  // so the timestamp is derived instead of stored (no setState inside an effect).
  useEffect(() => {
    if (!dataUpdatedAt) return;

    const update = () => setSecondsAgo(Math.floor((Date.now() - dataUpdatedAt) / 1000));
    update();
    const interval = setInterval(update, 1000);

    return () => clearInterval(interval);
  }, [dataUpdatedAt]);

  // Format relative time in Spanish
  const formatSecondsAgo = (seconds: number): string => {
    if (seconds < 60) {
      return seconds === 1 ? '1 segundo' : `${seconds} segundos`;
    }
    const minutes = Math.floor(seconds / 60);
    if (minutes < 60) {
      return minutes === 1 ? '1 minuto' : `${minutes} minutos`;
    }
    const hours = Math.floor(minutes / 60);
    return hours === 1 ? '1 hora' : `${hours} horas`;
  };

  // First load only: background refetches must NOT blank the page
  if (isLoading) {
    return <DashboardSkeleton />;
  }

  return (
    <div className="space-y-6">
      {/* Page Header - Inicio */}
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-2xl font-bold text-foreground">Inicio</h1>
          <p className="text-muted-foreground">
            {user?.nombre ? `Bienvenido, ${user.nombre}` : 'Bienvenido al sistema de gestión'}
            {user?.nombre ? ' — sistema de gestión' : ''}
          </p>
        </div>

        <div className="flex items-center gap-4 text-sm text-muted-foreground">
          {lastUpdated && (
            <div className="flex items-center gap-1.5">
              <Clock className="h-4 w-4" />
              <span>Actualizado hace {formatSecondsAgo(secondsAgo)}</span>
            </div>
          )}
          <Button variant="outline" size="sm" onClick={() => refetch()}>
            <RefreshCw className={`h-4 w-4 mr-1.5 ${isFetching ? 'animate-spin' : ''}`} />
            Actualizar
          </Button>
        </div>
      </div>

      {/* Connection warning - the page stays usable when a single query fails */}
      {error && (
        <div className="flex flex-col gap-3 rounded-xl border border-amber-500/40 bg-amber-500/10 px-4 py-3 text-sm sm:flex-row sm:items-center sm:justify-between">
          <div className="flex items-start gap-2 text-amber-700 dark:text-amber-400">
            <WifiOff className="mt-0.5 h-4 w-4 shrink-0" aria-hidden />
            <p>
              Algunos datos no se pudieron cargar. Verificá que el backend esté corriendo en
              http://localhost:5147
            </p>
          </div>
          <Button variant="outline" size="sm" onClick={() => refetch()} className="shrink-0">
            <RefreshCw className={`h-4 w-4 mr-1.5 ${isFetching ? 'animate-spin' : ''}`} />
            Reintentar
          </Button>
        </div>
      )}

      {/* Quick Actions - role filtered */}
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
        {filteredActions.map((action) => {
          const isProducto = action.key === 'producto';
          const useEmpleadoCopy = isProducto && !canManageProductos;
          const title = (useEmpleadoCopy && action.titleEmpleado) || action.title;
          const description =
            (useEmpleadoCopy && action.descriptionEmpleado) || action.description;
          const Icon = action.icon;
          return (
            <Link key={action.href} to={action.href} className="group">
              <Card className="h-full gap-0 py-0 transition-all hover:border-primary hover:shadow-md group-hover:border-primary">
                <CardContent className="flex items-center gap-4 p-5">
                  <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-primary text-primary-foreground">
                    <Icon className="h-5 w-5" />
                  </div>
                  <div className="min-w-0">
                    <p className="text-sm font-semibold text-foreground transition-colors group-hover:text-primary">
                      {title}
                    </p>
                    <p className="text-xs text-muted-foreground">{description}</p>
                  </div>
                </CardContent>
              </Card>
            </Link>
          );
        })}
      </div>

      {/* Today's metrics - Dueño/Gerente only (/informes is restricted) */}
      {canViewAdminData && <ResumenHoy />}

      {/* Stock alerts - Dueño/Gerente only (backend restriction) */}
      {canViewAdminData && <AlertasStockPanel />}

      {/* Latest sales - every role can read /api/v1/ventas */}
      <UltimasVentas />

      {/* Footer with timestamp */}
      <div className="border-t pt-4 text-center text-xs text-muted-foreground">
        <p>
          Última actualización:{' '}
          {lastUpdated
            ? format(lastUpdated, "d 'de' MMMM 'de' yyyy, HH:mm", { locale: es })
            : '-'}
        </p>
      </div>
    </div>
  );
}
