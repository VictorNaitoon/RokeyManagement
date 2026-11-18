/**
 * ImageUpload — cámara nativa en móvil, widget de Cloudinary en escritorio
 * RoKey MANAGEMENT - Multi-tenant SaaS ERP/POS for locksmiths
 *
 * Regresión: en celular la fuente `camera` del widget de Cloudinary se cuelga
 * y dispara "El widget no respondió". En móvil se abre la cámara nativa con
 * `capture="environment"`; en escritorio se conserva el widget actual.
 */

import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { act } from 'react';
import { createRoot, type Root } from 'react-dom/client';
import { ImageUpload } from '@/components/ui/ImageUpload';
import { isMobileUserAgent } from '@/lib/mobile-device';

const DESKTOP_UA =
  'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36';
const ANDROID_UA =
  'Mozilla/5.0 (Linux; Android 13; Pixel 7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Mobile Safari/537.36';
const IPHONE_UA =
  'Mozilla/5.0 (iPhone; CPU iPhone OS 17_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.0 Mobile/15E148 Safari/604.1';

describe('ImageUpload — cámara en móvil', () => {
  let container: HTMLDivElement;
  let root: Root | undefined;
  let originalUserAgent: string;
  let createUploadWidget: ReturnType<typeof vi.fn>;

  beforeEach(() => {
    (globalThis as { IS_REACT_ACT_ENVIRONMENT?: boolean }).IS_REACT_ACT_ENVIRONMENT = true;
    vi.useFakeTimers();
    originalUserAgent = window.navigator.userAgent;
    container = document.createElement('div');
    document.body.appendChild(container);
    createUploadWidget = vi.fn(() => ({ open: vi.fn(), close: vi.fn() }));
    (window as unknown as { cloudinary: unknown }).cloudinary = {
      createUploadWidget,
    };
  });

  afterEach(() => {
    act(() => root?.unmount());
    container.remove();
    Object.defineProperty(window.navigator, 'userAgent', {
      value: originalUserAgent,
      configurable: true,
    });
    delete (window as unknown as { cloudinary?: unknown }).cloudinary;
    vi.useRealTimers();
    vi.restoreAllMocks();
  });

  function setUserAgent(value: string) {
    Object.defineProperty(window.navigator, 'userAgent', { value, configurable: true });
  }

  async function render() {
    await act(async () => {
      root = createRoot(container);
      root.render(<ImageUpload onChange={vi.fn()} />);
    });
  }

  it('detecta móvil (Android/iPhone) y escritorio', () => {
    expect(isMobileUserAgent(ANDROID_UA)).toBe(true);
    expect(isMobileUserAgent(IPHONE_UA)).toBe(true);
    expect(isMobileUserAgent(DESKTOP_UA)).toBe(false);
  });

  it('en móvil abre la cámara nativa y NO el widget de Cloudinary', async () => {
    setUserAgent(ANDROID_UA);
    await render();

    const cameraInput = container.querySelector(
      'input[capture="environment"]'
    ) as HTMLInputElement | null;
    expect(cameraInput).not.toBeNull();

    const cameraClick = vi.spyOn(cameraInput as HTMLInputElement, 'click').mockImplementation(() => {});
    const galleryInput = container.querySelector(
      'input[type="file"]:not([capture])'
    ) as HTMLInputElement;
    const galleryClick = vi.spyOn(galleryInput, 'click').mockImplementation(() => {});

    await act(async () => {
      (container.querySelector('button') as HTMLButtonElement).click();
    });

    expect(cameraClick).toHaveBeenCalledTimes(1);
    expect(galleryClick).not.toHaveBeenCalled();
    expect(createUploadWidget).not.toHaveBeenCalled();
    expect(container.textContent).toContain('Elegir de la galería');
    expect(container.textContent).not.toContain('El widget no respondió');
  });

  it('en escritorio conserva el widget con la fuente camera', async () => {
    setUserAgent(DESKTOP_UA);
    await render();

    await act(async () => {
      (container.querySelector('button') as HTMLButtonElement).click();
    });

    expect(createUploadWidget).toHaveBeenCalledTimes(1);
    const config = createUploadWidget.mock.calls[0][0] as { sources: string[] };
    expect(config.sources).toEqual(['local', 'camera', 'url']);
    expect(container.querySelector('input[capture="environment"]')).toBeNull();
  });
});
