import * as React from 'react';
import { useImportCsv } from '@/hooks/useProductos';
import { Button } from '@/components/ui/button';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription } from '@/components/ui/dialog';
import { Table, TableHeader, TableBody, TableHead, TableRow, TableCell } from '@/components/ui/table';
import type { ImportCsvResponse } from '@/types';

export interface ProductoImportDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

const MAX_BYTES = 5 * 1024 * 1024;
const MAX_ROWS = 1000;

function detectDelimiter(header: string): ',' | ';' {
  const c = (header.match(/,/g) || []).length;
  const s = (header.match(/;/g) || []).length;
  return s > c ? ';' : ',';
}

export function ProductoImportDialog({ open, onOpenChange }: ProductoImportDialogProps) {
  const [file, setFile] = React.useState<File | null>(null);
  const [preview, setPreview] = React.useState<string[][]>([]);
  const [delimiter, setDelimiter] = React.useState<',' | ';'>(',');
  const [localError, setLocalError] = React.useState<string | null>(null);
  const [result, setResult] = React.useState<ImportCsvResponse | null>(null);
  const importCsv = useImportCsv();

  React.useEffect(() => {
    if (!open) { setFile(null); setPreview([]); setLocalError(null); setResult(null); }
  }, [open]);

  const onFileChange = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const f = e.target.files?.[0] ?? null;
    setResult(null); setLocalError(null); setPreview([]);
    if (!f) { setFile(null); return; }
    if (f.size > MAX_BYTES) { setLocalError('Archivo excede 5MB'); setFile(null); return; }
    setFile(f);
    const text = await f.text();
    // handle BOM
    const clean = text.replace(/^\uFEFF/, '');
    const lines = clean.split(/\r?\n/).filter((l) => l.trim().length > 0);
    if (lines.length - 1 > MAX_ROWS) { setLocalError(`Excede 1000 filas (tiene ${lines.length - 1})`); return; }
    const delim = detectDelimiter(lines[0] ?? ',');
    setDelimiter(delim);
    const rows = lines.slice(0, 6).map((l) => l.split(delim).map((c) => c.trim()));
    setPreview(rows);
  };

  const handleImport = async () => {
    if (!file) return;
    setLocalError(null);
    try {
      const res = (await importCsv.mutateAsync(file)) as ImportCsvResponse;
      // normalize PascalCase
      const norm: ImportCsvResponse = {
        totalRows: (res as unknown as Record<string, unknown>).totalRows as number ?? (res as unknown as Record<string, unknown>).TotalRows as number ?? 0,
        created: (res as unknown as Record<string, unknown>).created as number ?? (res as unknown as Record<string, unknown>).Created as number ?? 0,
        skipped: (res as unknown as Record<string, unknown>).skipped as number ?? (res as unknown as Record<string, unknown>).Skipped as number ?? 0,
        errors: ((res as unknown as Record<string, unknown>).errors as ImportCsvResponse['errors']) ?? ((res as unknown as Record<string, unknown>).Errors as ImportCsvResponse['errors']) ?? [],
      };
      setResult(norm.totalRows !== undefined ? norm : res as ImportCsvResponse);
    } catch {
      // toast handled in hook
    }
  };

  const errors = result ? (result.errors ?? (result as unknown as Record<string, unknown>).Errors as ImportCsvResponse['errors'] ?? []) : [];
  const created = result ? ((result.created ?? (result as unknown as Record<string, unknown>).Created) as number) : 0;
  const skipped = result ? ((result.skipped ?? (result as unknown as Record<string, unknown>).Skipped) as number) : 0;
  const totalRows = result ? ((result.totalRows ?? (result as unknown as Record<string, unknown>).TotalRows) as number) : 0;

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-3xl max-h-[85vh] overflow-hidden flex flex-col">
        <DialogHeader>
          <DialogTitle>Importar productos (CSV)</DialogTitle>
          <DialogDescription>Delimiters , ; auto, UTF-8/BOM, max 5MB / 1000 filas. Columnas: Nombre*, PrecioVenta*, PrecioCompra, StockActual, StockMinimo, CodigoBusqueda, Descripcion, NombreCategoria/IdCategoria, EsServicio, Activo.</DialogDescription>
        </DialogHeader>

        <div className="space-y-3">
          <input type="file" accept=".csv,text/csv" onChange={onFileChange} className="block w-full text-sm" />
          {localError && <p className="text-sm text-destructive">{localError}</p>}
          {file && !localError && <p className="text-xs text-muted-foreground">Archivo: {file.name} — {(file.size / 1024).toFixed(1)} KB — delimitador detectado: <strong>{delimiter}</strong></p>}

          {preview.length > 0 && (
            <div className="rounded border overflow-auto max-h-40">
              <Table>
                <TableHeader><TableRow>{preview[0].map((h, i) => <TableHead key={i}>{h}</TableHead>)}</TableRow></TableHeader>
                <TableBody>{preview.slice(1).map((r, ri) => <TableRow key={ri}>{r.map((c, ci) => <TableCell key={ci} className="text-xs">{c}</TableCell>)}</TableRow>)}</TableBody>
              </Table>
            </div>
          )}

          <div className="flex gap-2">
            <Button onClick={handleImport} disabled={!file || !!localError || importCsv.isPending}>
              {importCsv.isPending ? 'Importando...' : 'Importar'}
            </Button>
            <Button variant="outline" onClick={() => onOpenChange(false)}>Cerrar</Button>
          </div>

          {result && (
            <div className="rounded border p-3 space-y-2">
              <p className="text-sm font-medium">Resultado: {totalRows} filas — {created} creados, {skipped} omitidos {skipped > 0 ? '(207 Partial)' : '(200 OK)'}</p>
              {errors.length > 0 ? (
                <div className="overflow-auto max-h-48 rounded border">
                  <Table>
                    <TableHeader><TableRow><TableHead>Fila</TableHead><TableHead>Columna</TableHead><TableHead>Mensaje</TableHead></TableRow></TableHeader>
                    <TableBody>{errors.map((e, i) => {
                      const row = (e as unknown as Record<string, unknown>).row ?? (e as unknown as Record<string, unknown>).Row ?? '';
                      const col = (e as unknown as Record<string, unknown>).column ?? (e as unknown as Record<string, unknown>).Column ?? '';
                      const msg = (e as unknown as Record<string, unknown>).message ?? (e as unknown as Record<string, unknown>).Message ?? '';
                      return <TableRow key={i}><TableCell>{String(row)}</TableCell><TableCell>{String(col)}</TableCell><TableCell className="text-xs">{String(msg)}</TableCell></TableRow>;
                    })}</TableBody>
                  </Table>
                </div>
              ) : <p className="text-sm text-muted-foreground">Sin errores.</p>}
            </div>
          )}
        </div>
      </DialogContent>
    </Dialog>
  );
}

export default ProductoImportDialog;
