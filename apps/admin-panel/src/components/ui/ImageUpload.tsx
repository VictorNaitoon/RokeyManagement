/**
 * ImageUpload - Cloudinary Upload Widget integration
 * RoKey MANAGEMENT - Multi-tenant SaaS ERP/POS for locksmiths
 * 
 * Phase 3: UI Components - Cloudinary integration for product images
 */

import * as React from 'react';
import { Upload, X, Loader2 } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { cn } from '@/lib/utils';

export interface ImageUploadProps {
  value?: string | null;
  onChange?: (url: string) => void;
  disabled?: boolean;
  className?: string;
  placeholder?: string;
}

// Cloudinary configuration
const CLOUDINARY_CLOUD_NAME = 'dt4prtxyr';
const CLOUDINARY_UPLOAD_PRESET = 'Producto';
const CLOUDINARY_SCRIPT_URL = 'https://upload-widget.cloudinary.com/global/all.js';

interface CloudinaryWidgetResult {
  event: string;
  info: {
    secure_url: string;
  };
}

// Type for window.cloudinary
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

// Load Cloudinary script dynamically
function useCloudinaryScript() {
  const [isLoaded, setIsLoaded] = React.useState(false);
  const [isLoading, setIsLoading] = React.useState(false);

  React.useEffect(() => {
    // Check if already loaded
    if (window.cloudinary?.createUploadWidget) {
      setIsLoaded(true);
      return;
    }

    // Check if script is already being loaded
    const existingScript = document.querySelector(`script[src="${CLOUDINARY_SCRIPT_URL}"]`);
    if (existingScript) {
      setIsLoading(true);
      existingScript.addEventListener('load', () => {
        setIsLoaded(true);
        setIsLoading(false);
      });
      return;
    }

    // Load the script
    setIsLoading(true);
    const script = document.createElement('script');
    script.src = CLOUDINARY_SCRIPT_URL;
    script.async = true;
    script.onload = () => {
      setIsLoaded(true);
      setIsLoading(false);
    };
    script.onerror = () => {
      setIsLoading(false);
    };
    document.body.appendChild(script);
  }, []);

  return { isLoaded, isLoading };
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
  const { isLoaded, isLoading } = useCloudinaryScript();

  // Open Cloudinary widget
  const openWidget = React.useCallback(() => {
    if (disabled || isUploading || !isLoaded) return;

    const createUploadWidget = window.cloudinary?.createUploadWidget;
    
    if (!createUploadWidget) {
      setError('Cloudinary no está disponible');
      return;
    }

    const widget = createUploadWidget(
      {
        cloudName: CLOUDINARY_CLOUD_NAME,
        uploadPreset: CLOUDINARY_UPLOAD_PRESET,
        sources: ['local', 'camera', 'url'],
        multiple: false,
        maxFiles: 1,
        maxFileSize: 5000000, // 5MB
        clientAllowedFormats: ['jpg', 'jpeg', 'png', 'webp', 'gif'],
        resourceType: 'image',
        folder: 'productos',
      },
      (err, result) => {
        if (err) {
          console.error('Cloudinary widget error:', err);
          setError('Error al subir la imagen');
          setIsUploading(false);
          return;
        }

        if (result && result.event === 'success') {
          onChange?.(result.info.secure_url);
          setIsUploading(false);
          setError(null);
        }

        if (result && result.event === 'close' && isUploading) {
          setIsUploading(false);
        }
      }
    );

    if (widget) {
      setIsUploading(true);
      widget.open();
    }
  }, [disabled, isUploading, isLoaded, onChange]);

  // Remove image
  const handleRemove = React.useCallback(() => {
    onChange?.('');
    setError(null);
  }, [onChange]);

  // Display current image
  if (value) {
    return (
      <div className={cn('relative inline-block', className)}>
        <div className="relative w-32 h-32 rounded-lg overflow-hidden border">
          <img
            src={value}
            alt="Imagen del producto"
            className="w-full h-full object-cover"
          />
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

  // Upload button
  return (
    <div className={cn('flex flex-col gap-2', className)}>
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
      {error && (
        <p className="text-xs text-destructive">{error}</p>
      )}
      <p className="text-xs text-muted-foreground">
        Formatos: JPG, PNG, WebP, GIF. Máximo 5MB.
      </p>
    </div>
  );
}

export default ImageUpload;