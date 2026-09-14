import { useForm } from 'react-hook-form';
import { toast } from 'sonner';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import { Skeleton } from '@/components/ui/skeleton';
import { useMovimientoMutation } from '@/hooks/useCaja';
import type { AgregarMovimientoCajaRequest } from '@/hooks/useCaja';

interface MovimientoFormValues {
  Tipo: 'Ingreso' | 'Egreso';
  Monto: number;
  Descripcion?: string;
}

export interface MovimientoFormProps {
  onAgregar?: (data: AgregarMovimientoCajaRequest) => void;
  disabled?: boolean;
  skeleton?: boolean;
}

export function MovimientoForm({ onAgregar, disabled = false, skeleton = false }: MovimientoFormProps) {
  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
  } = useForm<MovimientoFormValues>({
    defaultValues: {
      Tipo: 'Ingreso',
      Descripcion: '',
      Monto: 0,
    },
  });

  const movimientoMutation = useMovimientoMutation();

  if (skeleton) {
    return (
      <div className="space-y-4">
        <Skeleton className="h-10 w-full" />
        <Skeleton className="h-10 w-full" />
        <Skeleton className="h-10 w-full" />
      </div>
    );
  }

  const onSubmit = handleSubmit((values: MovimientoFormValues) => {
    const payload: AgregarMovimientoCajaRequest = {
      Tipo: values.Tipo,
      Monto: Number(values.Monto),
      Descripcion: values.Descripcion?.trim() ? values.Descripcion.trim() : undefined,
    };

    if (onAgregar) {
      // Do not reset optimistically; only clear on success to avoid losing data on error
      onAgregar(payload);
      // Parent is expected to handle success feedback; if parent uses its own mutation,
      // prefer removing onAgregar prop so this form's own mutation handles reset via onSuccess below.
      return;
    }

    movimientoMutation.mutate(payload, {
      onSuccess: () => {
        reset();
      },
      onError: (err: unknown) => {
        const e = err as { response?: { data?: { detail?: string; message?: string } }; message?: string };
        const message = e.response?.data?.detail ?? e.response?.data?.message ?? e.message ?? 'Error al agregar movimiento';
        toast.error(message);
      },
    });
  });

  const isPending = movimientoMutation.isPending || disabled;

  return (
    <form onSubmit={onSubmit} className="space-y-4">
      <div className="grid grid-cols-2 gap-4">
        <div>
          <label className="block text-sm font-medium mb-1">Tipo de Movimiento</label>
          <select
            {...register('Tipo', { required: 'Tipo requerido' })}
            disabled={isPending}
            className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm shadow-sm"
            defaultValue="Ingreso"
          >
            <option value="Ingreso">Ingreso</option>
            <option value="Egreso">Egreso</option>
          </select>
          {errors.Tipo && (
            <p className="mt-1 text-xs text-destructive">{errors.Tipo.message}</p>
          )}
        </div>

        <div>
          <label className="block text-sm font-medium mb-1">Monto</label>
          <Input
            type="number"
            step="0.01"
            {...register('Monto', { required: 'Monto requerido', valueAsNumber: true, min: { value: 0.01, message: 'Monto debe ser mayor a 0' }, validate: (v) => Number(v) > 0 || 'Monto debe ser mayor a 0' })}
            placeholder="0.00"
            disabled={isPending}
          />
          {errors.Monto && (
            <p className="mt-1 text-xs text-destructive">{errors.Monto.message}</p>
          )}
        </div>
      </div>

      <div>
        <label className="block text-sm font-medium mb-1">Descripción</label>
        <Input
          {...register('Descripcion', { maxLength: { value: 500, message: 'Máximo 500 caracteres' } })}
          placeholder="Descripción del movimiento (opcional)"
          disabled={isPending}
        />
        {errors.Descripcion && (
          <p className="mt-1 text-xs text-destructive">{errors.Descripcion.message}</p>
        )}
      </div>

      <Button
        type="submit"
        disabled={isPending}
        className="w-full py-3 px-4 bg-green-600 text-white rounded-md font-medium hover:bg-green-700 disabled:opacity-50 disabled:pointer-events-none"
      >
        {movimientoMutation.isPending ? 'Guardando...' : 'Agregar Movimiento'}
      </Button>
    </form>
  );
}
