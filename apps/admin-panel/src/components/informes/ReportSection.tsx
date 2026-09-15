import type { ReactNode } from 'react';
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Skeleton } from '@/components/ui/skeleton';
import { DashboardError } from '@/components/dashboard/DashboardError';
import { useInformesExport } from '@/hooks/useInformes';
import { toast } from 'sonner';
import type { TipoInforme, InformesFilter } from '@/types/informes.types';

interface ReportSectionProps {
  title: string;
  description?: string;
  isLoading?: boolean;
  isError?: boolean;
  error?: unknown;
  onRetry?: () => void;
  isEmpty?: boolean;
  emptyMessage?: string;
  children: ReactNode;
  tipo?: TipoInforme;
  filter?: InformesFilter;
}

export function ReportSection({
  title,
  description,
  isLoading,
  isError,
  error,
  onRetry,
  isEmpty,
  emptyMessage = 'No hay datos para el periodo seleccionado',
  children,
  tipo,
  filter,
}: ReportSectionProps) {
  const exportMut = useInformesExport();

  const handleExport = (formato: 'csv' | 'pdf' | 'docx') => {
    if (!tipo || !filter) return;
    exportMut.mutate(
      { tipo, formato, filter },
      {
        onSuccess: (filename) => toast.success(`Descarga iniciada: ${filename}`),
        onError: (err: unknown) => {
          const msg =
            err instanceof Error
              ? err.message
              : typeof err === 'object' && err !== null && 'response' in err
                ? ((err as { response?: { data?: { detail?: string; errors?: unknown } } }).response?.data?.detail ??
                  'Error al exportar')
                : 'Error al exportar';
          // Try to extract Validation ProblemDetails detail
          const axiosErr = err as { response?: { data?: { detail?: string; title?: string; errors?: Record<string, string[]> } } };
          const detail = axiosErr.response?.data?.detail ?? axiosErr.response?.data?.title ?? msg;
          const errors = axiosErr.response?.data?.errors;
          const firstError = errors ? Object.values(errors).flat()[0] : null;
          toast.error((firstError as string) ?? detail ?? msg);
        },
      },
    );
  };

  return (
    <Card>
      <CardHeader>
        <CardTitle>{title}</CardTitle>
        {description && <CardDescription>{description}</CardDescription>}
      </CardHeader>
      <CardContent>
        {isLoading ? (
          <div className="space-y-2">
            <Skeleton className="h-6 w-full" />
            <Skeleton className="h-6 w-full" />
            <Skeleton className="h-6 w-3/4" />
          </div>
        ) : isError ? (
          <DashboardError
            message={error instanceof Error ? error.message : 'Error al cargar datos'}
            onRetry={onRetry}
          />
        ) : isEmpty ? (
          <div className="py-8 text-center">
            <p className="text-sm text-muted-foreground mb-4">{emptyMessage}</p>
            <Button variant="outline" size="sm" onClick={onRetry}>
              Cambiar periodo
            </Button>
          </div>
        ) : (
          children
        )}
      </CardContent>
      {tipo && filter && (
        <CardFooter className="flex gap-2 flex-wrap print:hidden">
          {(['csv', 'pdf', 'docx'] as const).map((fmt) => (
            <Button
              key={fmt}
              variant="outline"
              size="sm"
              disabled={exportMut.isPending}
              aria-label={`Export ${fmt.toUpperCase()}`}
              onClick={() => handleExport(fmt)}
            >
              {exportMut.isPending ? '...' : ''} Export {fmt.toUpperCase()}
            </Button>
          ))}
        </CardFooter>
      )}
    </Card>
  );
}
