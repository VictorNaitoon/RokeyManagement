import type { User, UserRole } from '@/stores/authStore';

/**
 * Decodes a JWT without verification (client-side only).
 * Returns null if token is malformed.
 */
export function decodeJwtPayload(token: string): Record<string, unknown> | null {
  try {
    const base64Url = token.split('.')[1];
    if (!base64Url) return null;
    let base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    // Pad base64 to multiple of 4
    base64 = base64.padEnd(base64.length + ((4 - (base64.length % 4)) % 4), '=');
    const binary = atob(base64);
    // UTF-8 safe decode: binary -> Uint8Array -> TextDecoder
    const bytes = Uint8Array.from(binary, (c) => c.charCodeAt(0));
    const json = new TextDecoder().decode(bytes);
    return JSON.parse(json) as Record<string, unknown>;
  } catch {
    return null;
  }
}

/**
 * Builds a minimal User from JWT claims.
 * Claims expected: sub (id), email, rol, negocioId, nombre (optional).
 * nota: SuperAdmin no tiene negocioId y no persiste cookie -> bootstrap no lo restaura (esperado).
 */
export function userFromJwtPayload(payload: Record<string, unknown>): User | null {
  const sub = payload['sub'] as string | undefined;
  const email = payload['email'] as string | undefined;
  // rol puede venir como 'rol', 'role' o claim largo de Microsoft
  const rol =
    (payload['rol'] as string | undefined) ??
    (payload['role'] as string | undefined) ??
    (payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] as string | undefined);
  const negocioIdRaw =
    (payload['negocioId'] as string | number | undefined) ??
    (payload['negocio_id'] as string | number | undefined) ??
    (payload['id_negocio'] as string | number | undefined);

  if (!sub || !email || !rol) return null;

  const id = Number(sub);
  if (Number.isNaN(id)) return null;

  const negocioId = negocioIdRaw != null ? Number(negocioIdRaw) : undefined;

  return {
    id,
    email,
    nombre: (payload['nombre'] as string | undefined) ?? '',
    apellido: (payload['apellido'] as string | undefined) ?? '',
    rol: rol as UserRole,
    id_negocio: negocioId != null && !Number.isNaN(negocioId) ? negocioId : undefined,
  };
}

/**
 * Decodes token and returns User or null.
 */
export function decodeUserFromToken(token: string): User | null {
  const payload = decodeJwtPayload(token);
  if (!payload) return null;
  return userFromJwtPayload(payload);
}
