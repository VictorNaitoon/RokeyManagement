import type { User, UserRole } from '@/stores/authStore';

/**
 * Decodes a JWT without verification (client-side only).
 * Returns null if token is malformed.
 */
export function decodeJwtPayload(token: string): Record<string, unknown> | null {
  try {
    const base64Url = token.split('.')[1];
    if (!base64Url) return null;
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    const json = atob(base64);
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
  const rol = (payload['rol'] as string | undefined) ?? (payload['role'] as string | undefined);
  const negocioIdRaw = payload['negocioId'] as string | number | undefined;

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
