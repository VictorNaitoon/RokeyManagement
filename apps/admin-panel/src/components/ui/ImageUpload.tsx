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

interface CloudinaryWidgetResult {
  event: string;
  info: {
    secure_url: string;
  };
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

  // Open Cloudinary widget
  const openWidget = React.useCallback(() => {
    if (disabled || isUploading) return;

    // Define the callback type
    type WidgetCallback = (err: unknown, result: CloudinaryWidgetResult) => void;
    
    // Access cloudinary from window
    const windowAny = window as unknown as {
      cloudinary?: {
        createUploadWidget?: (config: unknown, callback: WidgetCallback) => {
          open: () => void;
          close: () => void;
        } | undefined;
      };
    };

    const createUploadWidget = windowAny.cloudinary?.createUploadWidget;
    
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
  }, [disabled, isUploading, onChange]);

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
        disabled={disabled || isUploading}
        className={cn('w-full h-32 flex flex-col items-center justify-center gap-2', error && 'border-destructive')}
      >
        {isUploading ? (
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