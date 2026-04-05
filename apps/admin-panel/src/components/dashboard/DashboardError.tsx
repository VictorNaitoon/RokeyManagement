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
      <CardContent className="flex flex-col items-center justify-center py-8 text-center">
        <AlertCircle className="h-10 w-10 text-destructive mb-3" />
        <p className="text-muted-foreground mb-4">{message}</p>
        {onRetry && (
          <Button onClick={onRetry} variant="outline" size="sm">
            <RefreshCw className="h-4 w-4 mr-2" />
            Reintentar
          </Button>
        )}
      </CardContent>
    </Card>
  );
}
