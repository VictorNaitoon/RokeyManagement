/**
 * Dashboard Error - Error state component for dashboard widgets
 * RoKey MANAGEMENT - Multi-tenant SaaS ERP/POS for locksmiths
 * 
 * Phase 5: States + Integration
 */

import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { AlertCircle, RefreshCw } from 'lucide-react';

export interface DashboardErrorProps {
  message?: string;
  onRetry?: () => void;
  className?: string;
}

export function DashboardError({
  message = 'Error al cargar datos. Intente nuevamente.',
  onRetry,
  className,
}: DashboardErrorProps) {
  return (
    <Card className={className}>
      <CardContent className="flex flex-col items-center justify-center py-12 text-center">
        <AlertCircle className="h-12 w-12 text-red-500 mb-4" />
        <h3 className="text-lg font-semibold text-foreground mb-2">
          Error de conexión
        </h3>
        <p className="text-muted-foreground mb-6 max-w-sm">{message}</p>
        <Button 
          onClick={() => {
            console.log('Retry button clicked');
            onRetry?.();
          }} 
          variant="default"
          size="lg"
        >
          <RefreshCw className="h-4 w-4 mr-2" />
          Reintentar
        </Button>
      </CardContent>
    </Card>
  );
}
