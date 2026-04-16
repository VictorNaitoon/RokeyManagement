/**
 * VentaAnularModal - Modal to cancel (anular) a sale
 * RoKey MANAGEMENT - Multi-tenant SaaS ERP/POS for locksmiths
 * 
 * Phase 5: UI - Anular Modal (Admin only)
 */

import * as React from 'react';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter,
} from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Textarea } from '@/components/ui/textarea';
import { Label } from '@/components/ui/label';

interface VentaAnularModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onConfirm: (motivo: string) => Promise<void>;
  isLoading: boolean;
  ventaId?: number;
}

export function VentaAnularModal({ open, onOpenChange, onConfirm, isLoading, ventaId }: VentaAnularModalProps) {
  const [motivo, setMotivo] = React.useState('');

  const handleConfirm = async () => {
    if (!motivo.trim()) return;
    await onConfirm(motivo);
    setMotivo('');
  };

  const handleOpenChange = (isOpen: boolean) => {
    if (!isOpen) {
      setMotivo('');
    }
    onOpenChange(isOpen);
  };

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle className="text-destructive">Anular Venta</DialogTitle>
          <DialogDescription>
            ¿Está seguro de que desea anular la venta #{ventaId}? Esta acción no se puede deshacer.
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-4">
          <div className="bg-destructive/10 p-4 rounded-lg">
            <p className="text-sm text-destructive font-medium">
              ⚠️ Advertencia: Al anular la venta, se revertirá el stock de los productos.
            </p>
          </div>

          <div className="space-y-2">
            <Label htmlFor="motivo">Motivo de anulación *</Label>
            <Textarea
              id="motivo"
              placeholder="Ingrese el motivo de la anulación..."
              value={motivo}
              onChange={(e) => setMotivo(e.target.value)}
              rows={4}
            />
            {motivo && motivo.length < 10 && (
              <p className="text-xs text-destructive">
                El motivo debe tener al menos 10 caracteres
              </p>
            )}
          </div>
        </div>

        <DialogFooter>
          <Button
            variant="outline"
            onClick={() => handleOpenChange(false)}
            disabled={isLoading}
          >
            Cancelar
          </Button>
          <Button
            variant="destructive"
            onClick={handleConfirm}
            disabled={isLoading || motivo.trim().length < 10}
          >
            {isLoading ? 'Anulando...' : 'Anular Venta'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
