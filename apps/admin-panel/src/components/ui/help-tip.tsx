import type { ReactNode } from 'react';
import { Popover } from '@base-ui/react/popover';
import { CircleHelpIcon } from 'lucide-react';
import { cn } from '@/lib/utils';

interface HelpTipProps {
  /** Texto de ayuda que se muestra al hacer click o tocar el signo de interrogación. */
  children: ReactNode;
  /** Etiqueta accesible del botón. */
  label: string;
  className?: string;
}

/**
 * Signo de ayuda con popover. Funciona con click, tap táctil, teclado y lector
 * de pantalla: el `title` nativo no aparece en touch ni en todos los navegadores.
 */
export function HelpTip({ children, label, className }: HelpTipProps) {
  return (
    <Popover.Root>
      <Popover.Trigger
        type="button"
        aria-label={label}
        className={cn(
          'inline-flex shrink-0 cursor-help items-center justify-center rounded-full text-muted-foreground transition-colors outline-none hover:text-foreground focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2',
          className,
        )}
      >
        <CircleHelpIcon className="h-4 w-4" aria-hidden />
      </Popover.Trigger>
      <Popover.Portal>
        <Popover.Positioner side="top" align="center" sideOffset={8} className="z-50">
          <Popover.Popup className="max-w-xs rounded-lg bg-popover p-3 text-xs leading-relaxed text-popover-foreground shadow-md ring-1 ring-foreground/10 outline-none data-open:animate-in data-open:fade-in-0 data-open:zoom-in-95 data-closed:animate-out data-closed:fade-out-0 data-closed:zoom-out-95">
            <Popover.Title className="sr-only">{label}</Popover.Title>
            {children}
          </Popover.Popup>
        </Popover.Positioner>
      </Popover.Portal>
    </Popover.Root>
  );
}
