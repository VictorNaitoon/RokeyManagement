/**
 * ImageUpload - Cloudinary Upload Widget integration + direct fallback
 * RoKey MANAGEMENT - Multi-tenant SaaS ERP/POS for locksmiths
 * 
 * Phase 3: UI Components - Cloudinary integration for product images
 * Fix: adblock can block upload-widget/all.js or its inner rollbar.min.js.
 * We add script.onerror + direct fetch fallback to api.cloudinary.com.
 */

import * as React from 'react';
import { Upload, X, Loader2, Image as ImageIcon } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { cn } from '@/lib/utils';

export interface ImageUploadProps {
  value?: string | null;
  onChange?: (url: string) => void;
  disabled?: boolean;
  className?: string;
  placeholder?: string;
}

// Cloudinary configuration — DO NOT CHANGE (preset/folder/cloudName locked)
const CLOUDINARY_CLOUD_NAME = 'dt4prtxyr';
const CLOUDINARY_UPLOAD_PRESET = 'Producto';
const CLOUDINARY_FOLDER = 'productos';
const CLOUDINARY_SCRIPT_URL = 'https://upload-widget.cloudinary.com/global/all.js';
const CLOUDINARY_DIRECT_UPLOAD_URL = `https://api.cloudinary.com/v1_1/${CLOUDINARY_CLOUD_NAME}/image/upload`;

const MAX_FILE_SIZE = 5_000_000; // 5MB
const ALLOWED_FORMATS = ['jpg', 'jpeg', 'png', 'webp', 'gif'];
const ALLOWED_MIME = ['image/jpeg', 'image/png', 'image/webp', 'image/gif'];

const BLOCKED_MESSAGE =
  'El bloqueador de anuncios está bloqueando Cloudinary. Desactívalo para este sitio o usa la subida alternativa.';

interface CloudinaryWidgetResult {
  event: string;
  info: {
    secure_url: string;
  };
}

interface CloudinaryWidgetInstance {
  open: () => void;
  close: () => void;
}

type CloudinaryCreateWidget = (
  config: unknown,
  callback: (error: unknown, result: CloudinaryWidgetResult) => void
) => CloudinaryWidgetInstance | undefined;

declare global {
  interface Window {
    cloudinary?: {
      createUploadWidget?: CloudinaryCreateWidget;
    };
  }
}

// Load Cloudinary script dynamically — now with onerror + blocked message
function useCloudinaryScript() {
  const [isLoaded, setIsLoaded] = React.useState(false);
  const [isLoading, setIsLoading] = React.useState(false);
  const [scriptError, setScriptError] = React.useState<string | null>(null);

  React.useEffect(() => {
    if (window.cloudinary?.createUploadWidget) {
      setIsLoaded(true);
      return;
    }

    const existingScript = document.querySelector(`script[src="${CLOUDINARY_SCRIPT_URL}"]`) as HTMLScriptElement | null;
    if (existingScript) {
      // If already in DOM, wait for load/error
      if (window.cloudinary?.createUploadWidget) {
        setIsLoaded(true);
        return;
      }
      setIsLoading(true);
      const onLoad = () => {
        setIsLoaded(true);
        setIsLoading(false);
        setScriptError(null);
      };
      const onError = () => {
        setIsLoading(false);
        setScriptError(BLOCKED_MESSAGE);
      };
      existingScript.addEventListener('load', onLoad);
      existingScript.addEventListener('error', onError);
      return () => {
        existingScript.removeEventListener('load', onLoad);
        existingScript.removeEventListener('error', onError);
      };
    }

    setIsLoading(true);
    setScriptError(null);
    const script = document.createElement('script');
    script.src = CLOUDINARY_SCRIPT_URL;
    script.async = true;
    script.onload = () => {
      setIsLoaded(true);
      setIsLoading(false);
      setScriptError(null);
    };
    script.onerror = () => {
      setIsLoading(false);
      setScriptError(BLOCKED_MESSAGE);
    };
    document.body.appendChild(script);
  }, []);

  return { isLoaded, isLoading, scriptError };
}

export function ImageUpload({
  value,
  onChange,
  disabled = false,
  className,
  placeholder = 'Subir imagen',
}: ImageUploadProps) {
  const [isUploading, setIsUploading] = React.useState(false);
  const [error, setError] = React.useState<string | null>(null);
  const { isLoaded, isLoading, scriptError } = useCloudinaryScript();
  const fileInputRef = React.useRef<HTMLInputElement>(null);

  // Show script blocked error in UI
  React.useEffect(() => {
    if (scriptError) setError(scriptError);
  }, [scriptError]);

  const validateFile = React.useCallback((file: File): string | null => {
    if (file.size > MAX_FILE_SIZE) {
      return 'La imagen no puede superar 5MB.';
    }
    const ext = file.name.split('.').pop()?.toLowerCase() ?? '';
    const mimeOk = ALLOWED_MIME.includes(file.type);
    const extOk = ALLOWED_FORMATS.includes(ext);
    if (!mimeOk && !extOk) {
      return 'Formato no permitido. Usa JPG, PNG, WebP o GIF.';
    }
    return null;
  }, []);

  const uploadDirect = React.useCallback(
    async (file: File) => {
      const validation = validateFile(file);
      if (validation) {
        setError(validation);
        return;
      }
      setError(null);
      setIsUploading(true);
      try {
        const form = new FormData();
        form.append('file', file);
        form.append('upload_preset', CLOUDINARY_UPLOAD_PRESET);
        form.append('folder', CLOUDINARY_FOLDER);

        const res = await fetch(CLOUDINARY_DIRECT_UPLOAD_URL, {
          method: 'POST',
          body: form,
        });

        if (!res.ok) {
          let detail = '';
          try {
            const j = (await res.json()) as { error?: { message?: string } };
            detail = j?.error?.message ?? '';
          } catch {
            // ignore json parse
          }
          throw new Error(detail || `Error ${res.status} al subir la imagen`);
        }

        const data = (await res.json()) as { secure_url?: string; url?: string };
        const secureUrl = data.secure_url ?? data.url;
        if (!secureUrl) throw new Error('Cloudinary no devolvió URL');
        onChange?.(secureUrl);
        setError(null);
      } catch (e) {
        const msg = e instanceof Error ? e.message : 'Error al subir la imagen';
        // Map network/blocked to friendly message
        if (msg.includes('Failed to fetch') || msg.includes('NetworkError')) {
          setError('Error de red al subir. Verifica tu conexión o desactiva el bloqueador.');
        } else {
          setError(msg);
        }
      } finally {
        setIsUploading(false);
        // reset input so same file can be re-selected
        if (fileInputRef.current) fileInputRef.current.value = '';
      }
    },
    [onChange, validateFile]
  );

  const triggerFilePicker = React.useCallback(() => {
    if (disabled || isUploading) return;
    fileInputRef.current?.click();
  }, [disabled, isUploading]);

  const handleFileChange = React.useCallback(
    (e: React.ChangeEvent<HTMLInputElement>) => {
      const file = e.target.files?.[0];
      if (file) void uploadDirect(file);
    },
    [uploadDirect]
  );

  // Open Cloudinary widget — wrapped in try/catch; fallback to file picker if blocked/undefined
  // Timeout de seguridad: si all.js cargó pero rollbar interno falla y widget queda roto sin abrir/callback, no dejar colgado
  const openWidget = React.useCallback(() => {
    if (disabled || isUploading) return;

    // If script failed or widget unavailable -> fallback directly
    if (scriptError || !isLoaded || !window.cloudinary?.createUploadWidget) {
      triggerFilePicker();
      if (!scriptError) setError(null);
      return;
    }

    try {
      const createUploadWidget = window.cloudinary.createUploadWidget;
      if (!createUploadWidget) {
        triggerFilePicker();
        return;
      }

      let callbackFired = false;
      let timeoutId: number | undefined;

      const widget = createUploadWidget(
        {
          cloudName: CLOUDINARY_CLOUD_NAME,
          uploadPreset: CLOUDINARY_UPLOAD_PRESET,
          sources: ['local', 'camera', 'url'],
          multiple: false,
          maxFiles: 1,
          maxFileSize: MAX_FILE_SIZE,
          clientAllowedFormats: ALLOWED_FORMATS,
          resourceType: 'image',
          folder: CLOUDINARY_FOLDER,
        },
        (err, result) => {
          callbackFired = true;
          if (timeoutId !== undefined) window.clearTimeout(timeoutId);
          if (err) {
            console.error('Cloudinary widget error:', err);
            setError('Error al subir la imagen. Probá la subida alternativa.');
            setIsUploading(false);
            return;
          }

          if (result && result.event === 'success') {
            onChange?.(result.info.secure_url);
            setIsUploading(false);
            setError(null);
          }

          if (result && result.event === 'close') {
            setIsUploading(false);
          }
        }
      );

      if (widget) {
        setIsUploading(true);
        setError(null);
        widget.open();
        // Seguridad: si widget no abre ni dispara callback en 800ms (rollbar bloqueado), liberar UI y sugerir fallback
        timeoutId = window.setTimeout(() => {
          if (!callbackFired) {
            setIsUploading(false);
            setError('El widget no respondió. Usá la subida alternativa de abajo.');
          }
        }, 800);
      } else {
        // createUploadWidget returned undefined (blocked) -> fallback
        triggerFilePicker();
      }
    } catch (err) {
      console.error('Cloudinary createUploadWidget threw:', err);
      triggerFilePicker();
      setError('No se pudo abrir el widget. Usando subida alternativa.');
    }
  }, [disabled, isUploading, isLoaded, scriptError, onChange, triggerFilePicker]);

  const handleRemove = React.useCallback(() => {
    onChange?.('');
    setError(null);
  }, [onChange]);

  // Display current image
  if (value) {
    return (
      <div className={cn('relative inline-block', className)}>
        <div className="relative w-32 h-32 rounded-lg overflow-hidden border">
          <img src={value} alt="Imagen del producto" className="w-full h-full object-cover" />
          {!disabled && (
            <button
              onClick={handleRemove}
              className="absolute top-1 right-1 bg-destructive text-white rounded-full p-1 hover:bg-destructive/90 transition-colors"
              type="button"
            >
              <X className="h-3 w-3" />
            </button>
          )}
        </div>
      </div>
    );
  }

  // Fallback solo visible si el widget está bloqueado — no impacta el flujo normal (Edge/Prod)
  const showFallbackHint = !!scriptError || (!isLoading && !isLoaded);

  // Upload button(s)
  return (
    <div className={cn('flex flex-col gap-2', className)}>
      {/* Hidden fallback input — existe siempre pero solo se usa si el widget falla */}
      <input
        ref={fileInputRef}
        type="file"
        accept={ALLOWED_MIME.join(',') + ',.jpg,.jpeg,.png,.webp,.gif'}
        className="hidden"
        onChange={handleFileChange}
        disabled={disabled || isUploading}
      />

      <Button
        type="button"
        variant="outline"
        onClick={openWidget}
        disabled={disabled || isUploading || isLoading}
        className={cn('w-full h-32 flex flex-col items-center justify-center gap-2', error && 'border-destructive')}
      >
        {isLoading ? (
          <>
            <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
            <span className="text-sm text-muted-foreground">Cargando...</span>
          </>
        ) : isUploading ? (
          <>
            <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
            <span className="text-sm text-muted-foreground">Subiendo...</span>
          </>
        ) : (
          <>
            <Upload className="h-8 w-8 text-muted-foreground" />
            <span className="text-sm text-muted-foreground">{placeholder}</span>
          </>
        )}
      </Button>

      {/* Subida alternativa — solo si el widget está bloqueado (no molesta en desarrollo normal) */}
      {showFallbackHint && !isUploading && (
        <Button
          type="button"
          variant="secondary"
          onClick={triggerFilePicker}
          disabled={disabled || isUploading}
          className="w-full"
        >
          <ImageIcon className="h-4 w-4 mr-2" />
          Subida alternativa (sin bloqueador)
        </Button>
      )}

      {error && <p className="text-xs text-destructive">{error}</p>}
      <p className="text-xs text-muted-foreground">Formatos: JPG, PNG, WebP, GIF. Máximo 5MB.</p>
    </div>
  );
}

export default ImageUpload;
