/**
 * Dashboard Page - Main dashboard with KPIs, charts, and role-based access
 * RoKey MANAGEMENT - Multi-tenant SaaS ERP/POS for locksmiths
 * 
 * Phase 5: States + Integration
 */

import { useEffect, useState } from 'react';
import { format } from 'date-fns';
import { es } from 'date-fns/locale';
import { Clock, RefreshCw, WifiOff } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { useDashboardData } from '@/hooks/useDashboardData';
import {
  VentasKPI,
  IngresosGastosKPI,
  AlertasStockKPI,
  IngresosGastosChart,
  VentasPorPagoChart,
  ProductosTopChart,
  DashboardSkeleton,
} from '@/components/dashboard';

export function DashboardPage() {
  const [lastUpdated, setLastUpdated] = useState<Date | null>(null);
  const [secondsAgo, setSecondsAgo] = useState(0);
  
  const {
    isLoading,
    isFetching,
    error,
    allFailed,
    refetch,
    canViewAdminData,
  } = useDashboardData();

  // Track last update time
  useEffect(() => {
    if (!isLoading && !isFetching && !error) {
      setLastUpdated(new Date());
    }
  }, [isLoading, isFetching, error]);

  // Auto-refresh indicator - update every second
  useEffect(() => {
    if (!lastUpdated) return;

    const interval = setInterval(() => {
      const now = new Date();
      const seconds = Math.floor((now.getTime() - lastUpdated.getTime()) / 1000);
      setSecondsAgo(seconds);
    }, 1000);

    return () => clearInterval(interval);
  }, [lastUpdated]);

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

  // Debug: log state changes
  useEffect(() => {
    console.log('Dashboard state:', { isLoading, isFetching, error: error?.message, allFailed });
  }, [isLoading, isFetching, error, allFailed]);

  // Error state - show when there's an error
  if (error) {
    return (
      <div className="flex flex-col items-center justify-center min-h-[60vh] gap-6 p-8">
        <WifiOff className="h-16 w-16 text-muted-foreground" />
        <div className="text-center space-y-2">
          <h2 className="text-xl font-semibold text-foreground">
            No se pudo conectar con el servidor
          </h2>
          <p className="text-muted-foreground max-w-md">
            Verifica que el backend esté corriendo en https://localhost:7096
          </p>
        </div>
        <Button 
          onClick={() => refetch()}
          size="lg"
          className="gap-2"
        >
          <RefreshCw className={`h-4 w-4 ${isFetching ? 'animate-spin' : ''}`} />
          Reintentar
        </Button>
      </div>
    );
  }

  // Loading state
  if (isLoading || isFetching) {
    return <DashboardSkeleton />;
  }

  return (
    <div className="space-y-6">
      {/* Page Header with Title and Refresh Indicator */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold text-foreground">Dashboard</h1>
          <p className="text-muted-foreground">Bienvenido al sistema de gestión</p>
        </div>
        
        <div className="flex items-center gap-4 text-sm text-muted-foreground">
          {lastUpdated && (
            <div className="flex items-center gap-1.5">
              <Clock className="h-4 w-4" />
              <span>Actualizado hace {formatSecondsAgo(secondsAgo)}</span>
            </div>
          )}
          <Button 
            variant="outline" 
            size="sm"
            onClick={() => {
              refetch();
              setLastUpdated(new Date());
            }}
          >
            <RefreshCw className={`h-4 w-4 mr-1.5 ${isFetching ? 'animate-spin' : ''}`} />
            Actualizar
          </Button>
        </div>
      </div>

      {/* KPI Cards Row - Responsive: 1 col mobile, 2 cols tablet, 3 cols desktop */}
      <div className="grid gap-4 grid-cols-1 md:grid-cols-2 lg:grid-cols-3">
        {/* Ventas del día - All roles */}
        <VentasKPI />
        
        {/* Ingresos vs Gastos - Admin/Manager only */}
        {canViewAdminData && <IngresosGastosKPI />}
        
        {/* Alertas de Stock - All roles */}
        <AlertasStockKPI />
      </div>

      {/* Charts Row - Responsive: full-width mobile, 2 cols desktop */}
      <div className="grid gap-4 grid-cols-1 lg:grid-cols-2">
        {/* Ingresos vs Gastos Chart - Admin/Manager only */}
        {canViewAdminData && <IngresosGastosChart />}
        
        {/* Ventas por Método de Pago Chart - Admin/Manager only */}
        {canViewAdminData && <VentasPorPagoChart />}
      </div>

      {/* Top Products Chart - Full Width */}
      <ProductosTopChart />

      {/* Footer with timestamp */}
      <div className="text-xs text-muted-foreground text-center pt-4 border-t">
        <p>
          Última actualización: {lastUpdated 
            ? format(lastUpdated, "d 'de' MMMM 'de' yyyy, HH:mm", { locale: es })
            : '-'}
        </p>
      </div>
    </div>
  );
}
