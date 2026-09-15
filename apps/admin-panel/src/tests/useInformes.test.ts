// @ts-nocheck
/**
 * useInformes unit tests - buildParams validation (R07)
 * Minimal covering tests for sdd-verify UNTESTED gate (1-2 per layer)
 */

import { describe, it, expect } from 'vitest';
import { buildParams } from '@/hooks/useInformes';
import type { InformesFilter } from '@/types/informes.types';

describe('useInformes buildParams (R07 - Fixed Param Mapping)', () => {
  it('preset hoy sends preset and never fecha/limite', () => {
    const filter: InformesFilter = { preset: 'hoy' };
    const params = buildParams(filter);

    expect(params).toEqual({ preset: 'hoy' });
    expect((params as any).fecha).toBeUndefined();
    expect((params as any).limite).toBeUndefined();
  });

  it('preset mes with cantidad sends preset and cantidad', () => {
    const filter: InformesFilter = { preset: 'mes', cantidad: 25 };
    const params = buildParams(filter);

    expect(params.preset).toBe('mes');
    expect(params.cantidad).toBe(25);
    expect((params as any).fecha).toBeUndefined();
    expect((params as any).limite).toBeUndefined();
  });

  it('custom range sends fechaDesde/fechaHasta without preset', () => {
    const filter: InformesFilter = {
      fechaDesde: '2026-01-01',
      fechaHasta: '2026-01-31',
    };
    const params = buildParams(filter);

    expect(params.fechaDesde).toBe('2026-01-01');
    expect(params.fechaHasta).toBe('2026-01-31');
    expect((params as any).preset).toBeUndefined();
  });

  it('custom range ignores preset when both present (R10)', () => {
    const filter: InformesFilter = {
      preset: 'semana',
      fechaDesde: '2026-01-01',
      fechaHasta: '2026-01-31',
    };
    const params = buildParams(filter);

    expect(params.fechaDesde).toBe('2026-01-01');
    expect(params.fechaHasta).toBe('2026-01-31');
    expect((params as any).preset).toBeUndefined();
  });

  it('cantidad OOR 99 is preserved as param (backend will 400) and never clamps', () => {
    const filter: InformesFilter = { preset: 'mes', cantidad: 99 };
    const params = buildParams(filter);

    expect(params.cantidad).toBe(99);
    // ensure not silently clamped to 50 by frontend
    expect(params.cantidad).not.toBe(50);
  });

  it('default productos-top cantidad=10 is handled via buildProductosTopParams equivalent', () => {
    // buildParams without cantidad leaves it undefined; useInformes will default to 10 for productos-top
    // Here we test that explicit 10 is sent correctly
    const filter: InformesFilter = { preset: 'mes', cantidad: 10 };
    const params = buildParams(filter);
    expect(params.cantidad).toBe(10);
  });

  it('never emits legacy fecha or limite keys for any filter shape', () => {
    const cases: InformesFilter[] = [
      { preset: 'hoy' },
      { preset: 'semana', cantidad: 50 },
      { fechaDesde: '2026-02-01', fechaHasta: '2026-02-28' },
      { preset: 'mes', fechaDesde: '2026-01-01', fechaHasta: '2026-01-31', cantidad: 25 },
    ];

    for (const f of cases) {
      const p = buildParams(f);
      expect((p as any).fecha).toBeUndefined();
      expect((p as any).limite).toBeUndefined();
    }
  });
});
