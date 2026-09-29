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
