/**
 * ClienteCtaCteDialog - Dialog for viewing customer account current
 * RoKey MANAGEMENT - Multi-tenant SaaS ERP/POS for locksmiths
 * 
 * Phase 6: Cliente Cuenta Corriente
 */

import { CreditCard, Calendar, DollarSign } from 'lucide-react';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { useClienteSaldo, useClienteVentas } from '@/hooks';
import type { Cliente } from '@/types';

export interface ClienteCtaCteDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  cliente: Cliente | null;
}

export function ClienteCtaCteDialog({
  open,
  onOpenChange,
  cliente,
}: ClienteCtaCteDialogProps) {
  // Queries
  const { data: saldo, isLoading: loadingSaldo } = useClienteSaldo(cliente?.id ?? null);
  const { data: ventasData, isLoading: loadingVentas } = useClienteVentas(cliente?.id ?? null);

  const isLoading = loadingSaldo || loadingVentas;

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>Cuenta Corriente - {cliente?.nombre} {cliente?.apellido}</DialogTitle>
          <DialogDescription>
            Resumen de cuenta y movimientos del cliente
          </DialogDescription>
        </DialogHeader>

        {isLoading ? (
          <div className="flex items-center justify-center py-8">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
          </div>
        ) : (
          <div className="space-y-6">
            {/* Saldo Summary */}
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <Card>
                <CardHeader className="pb-2">
                  <CardTitle className="text-sm font-medium text-muted-foreground">
                    Saldo Pendiente
                  </CardTitle>
                </CardHeader>
                <CardContent>
                  <div className="flex items-center gap-2">
                    <DollarSign className="h-5 w-5 text-muted-foreground" />
                    <span className="text-2xl font-bold">
                      ${saldo?.saldoPendiente?.toFixed(2) ?? '0.00'}
                    </span>
                  </div>
                </CardContent>
              </Card>

              <Card>
                <CardHeader className="pb-2">
                  <CardTitle className="text-sm font-medium text-muted-foreground">
                    Total Compras
                  </CardTitle>
                </CardHeader>
                <CardContent>
                  <div className="flex items-center gap-2">
                    <CreditCard className="h-5 w-5 text-muted-foreground" />
                    <span className="text-2xl font-bold">
                      ${saldo?.totalPurchases?.toFixed(2) ?? '0.00'}
                    </span>
                  </div>
                </CardContent>
              </Card>

              <Card>
                <CardHeader className="pb-2">
                  <CardTitle className="text-sm font-medium text-muted-foreground">
                    Cantidad de Ventas
                  </CardTitle>
                </CardHeader>
                <CardContent>
                  <div className="flex items-center gap-2">
                    <Calendar className="h-5 w-5 text-muted-foreground" />
                    <span className="text-2xl font-bold">
                      {ventasData?.total ?? 0}
                    </span>
                  </div>
                </CardContent>
              </Card>
            </div>

            {/* Ventas List */}
            <div>
              <h3 className="text-lg font-semibold mb-4">Últimas Ventas</h3>
              {ventasData?.ventas && ventasData.ventas.length > 0 ? (
                <div className="space-y-2">
                  {ventasData.ventas.map((venta) => (
                    <div
                      key={venta.id}
                      className="flex items-center justify-between p-3 bg-muted/50 rounded-lg"
                    >
                      <div className="flex items-center gap-3">
                        <div>
                          <div className="font-medium">Venta #{venta.id}</div>
                          <div className="text-sm text-muted-foreground">
                            {new Date(venta.fecha).toLocaleDateString('es-AR')}
                          </div>
                        </div>
                      </div>
                      <div className="text-right">
                        <div className="font-medium">${venta.total.toFixed(2)}</div>
                        <Badge variant={venta.estado === 'Activa' ? 'default' : 'secondary'}>
                          {venta.estado}
                        </Badge>
                      </div>
                    </div>
                  ))}
                </div>
              ) : (
                <div className="text-center py-8 text-muted-foreground">
                  No hay ventas registradas para este cliente
                </div>
              )}
            </div>
          </div>
        )}
      </DialogContent>
    </Dialog>
  );
}

export default ClienteCtaCteDialog;