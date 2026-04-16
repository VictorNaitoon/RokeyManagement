/**
 * CompraDetalleDialog - View Compra details
 * RoKey MANAGEMENT - Multi-tenant SaaS ERP/POS for locksmiths
 * 
 * Phase 6: Compras CRUD
 */

import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';

interface CompraDetalleDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  compra: { id: number } | null;
}

export function CompraDetalleDialog({
  open,
  onOpenChange,
  compra,
}: CompraDetalleDialogProps) {
  if (!compra) return null;

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle>Detalle de Compra #{compra.id}</DialogTitle>
          <DialogDescription>
            Información completa de la compra
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-4">
          <p className="text-center text-muted-foreground py-4">
            Detalles de la compra #{compra.id}
          </p>
        </div>

        <div className="flex justify-end">
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            Cerrar
          </Button>
        </div>
      </DialogContent>
    </Dialog>
  );
}
