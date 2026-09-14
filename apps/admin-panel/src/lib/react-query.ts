// Deprecated: use '@/lib/api/axios' for HTTP and '@/lib/api/query-client' for QueryClient.
// This file remains for backwards compatibility and now only re-exports queryClient.
// Audit: dates for Caja are server-side (UtcNow) - client never sends FechaApertura/FechaCierre.
export { queryClient } from '@/lib/api/query-client';
