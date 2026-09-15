import { useState } from 'react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import type { Preset, InformesFilter } from '@/types/informes.types';
import { PRESET } from '@/types/informes.types';

interface FilterBarProps {
  filter: InformesFilter;
  onApply: (next: InformesFilter) => void;
}

export function FilterBar({ filter, onApply }: FilterBarProps) {
  const [preset, setPreset] = useState<Preset>(filter.preset ? (filter.preset as Preset) : PRESET.MES);
  const [fechaDesde, setFechaDesde] = useState(filter.fechaDesde ?? '');
  const [fechaHasta, setFechaHasta] = useState(filter.fechaHasta ?? '');
  const [cantidad, setCantidad] = useState(String(filter.cantidad ?? 10));
  const [error, setError] = useState<string | null>(null);

  const handleApply = () => {
    setError(null);
    if (preset === PRESET.CUSTOM) {
      if (!fechaDesde || !fechaHasta) {
        setError('Selecciona fecha desde y hasta');
        return;
      }
      if (fechaDesde > fechaHasta) {
        setError('La fecha desde no puede ser posterior a la fecha hasta');
        return;
      }
      const diffDays = (new Date(fechaHasta).getTime() - new Date(fechaDesde).getTime()) / (1000 * 60 * 60 * 24);
      if (diffDays > 365) {
        setError('El rango no puede exceder 12 meses');
        return;
      }
      onApply({ fechaDesde, fechaHasta, cantidad: Number(cantidad) });
    } else {
      onApply({ preset: preset as 'hoy' | 'semana' | 'mes', cantidad: Number(cantidad) });
    }
  };

  return (
    <div className="sticky top-0 z-10 bg-background border-b p-4 flex flex-col gap-4 print:hidden">
      <div className="flex flex-wrap items-end gap-4">
        <div className="flex flex-col gap-1">
          <Label>Periodo</Label>
          <div className="flex gap-1">
            {([PRESET.HOY, PRESET.SEMANA, PRESET.MES, PRESET.CUSTOM] as const).map((p) => (
              <Button
                key={p}
                variant={preset === p ? 'default' : 'outline'}
                size="sm"
                onClick={() => setPreset(p)}
              >
                {p === PRESET.CUSTOM ? 'Personalizado' : p.charAt(0).toUpperCase() + p.slice(1)}
              </Button>
            ))}
          </div>
        </div>

        {preset === PRESET.CUSTOM && (
          <>
            <div className="flex flex-col gap-1">
              <Label htmlFor="fechaDesde">Desde</Label>
              <Input
                id="fechaDesde"
                type="date"
                value={fechaDesde}
                onChange={(e) => setFechaDesde(e.target.value)}
              />
            </div>
            <div className="flex flex-col gap-1">
              <Label htmlFor="fechaHasta">Hasta</Label>
              <Input
                id="fechaHasta"
                type="date"
                value={fechaHasta}
                onChange={(e) => setFechaHasta(e.target.value)}
              />
            </div>
          </>
        )}

        <div className="flex flex-col gap-1">
          <Label>Productos top</Label>
          <Select value={cantidad} onValueChange={(v) => setCantidad(v ?? '10')}>
            <SelectTrigger className="w-[100px]">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="10">10</SelectItem>
              <SelectItem value="25">25</SelectItem>
              <SelectItem value="50">50</SelectItem>
            </SelectContent>
          </Select>
        </div>

        <Button onClick={handleApply}>Aplicar</Button>
      </div>
      {error && <p className="text-sm text-destructive">{error}</p>}
    </div>
  );
}
