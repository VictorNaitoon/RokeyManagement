/**
 * Mobile device detection shared by UI components and their tests.
 * Kept in a plain module (not a component file) so react-refresh stays valid.
 */

const MOBILE_UA_PATTERN = /android|iphone|ipad|ipod|mobile|windows phone/i;

/** Detecta dispositivos móviles por user-agent. */
export function isMobileUserAgent(userAgent: string): boolean {
  return MOBILE_UA_PATTERN.test(userAgent);
}
