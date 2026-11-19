/**
 * Format Helpers - Shared currency/number formatting for the admin panel
 * RoKey MANAGEMENT - Multi-tenant SaaS ERP/POS for locksmiths
 */

const currencyFormatter = new Intl.NumberFormat('es-AR', {
  style: 'currency',
  currency: 'ARS',
  minimumFractionDigits: 0,
  maximumFractionDigits: 0,
});

const numberFormatter = new Intl.NumberFormat('es-AR');

/**
 * Format an ARS amount without decimals (matches the dashboard voice)
 */
export function formatCurrency(value: number): string {
  return currencyFormatter.format(value);
}

/**
 * Format an integer with es-AR thousands separators
 */
export function formatNumber(value: number): string {
  return numberFormatter.format(value);
}

/**
 * Format a sale timestamp as HH:mm
 */
export function formatHora(iso: string): string {
  const date = new Date(iso);
  if (Number.isNaN(date.getTime())) return '--:--';
  return new Intl.DateTimeFormat('es-AR', {
    hour: '2-digit',
    minute: '2-digit',
    hour12: false,
  }).format(date);
}

/**
 * Format an ISO date or DateTime as dd/MM/yyyy using only its date part.
 * The backend serializes DateTime with a time component (and sometimes a Z
 * suffix), so parsing the whole value as local time could shift the day in
 * UTC-3 or throw "Invalid time value". A midday local date avoids both.
 */
export function formatFecha(iso: string | null | undefined): string {
  if (!iso) return '—';
  const datePart = iso.slice(0, 10);
  if (!/^\d{4}-\d{2}-\d{2}$/.test(datePart)) return '—';
  const date = new Date(`${datePart}T12:00:00`);
  if (Number.isNaN(date.getTime())) return '—';
  return new Intl.DateTimeFormat('es-AR', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
  }).format(date);
}
