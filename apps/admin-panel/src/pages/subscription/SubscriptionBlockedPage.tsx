import { AlertTriangle, ArrowLeft, CreditCard } from 'lucide-react';
import { useSubscriptionStore } from '@/stores/subscriptionStore';
import { authStore } from '@/stores/authStore';
import { useNavigate } from 'react-router-dom';

export function SubscriptionBlockedPage() {
  const navigate = useNavigate();
  const { message, clearBlocked } = useSubscriptionStore();
  const { logout } = authStore;

  const handleLogout = async () => {
    clearBlocked();
    await logout();
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-background p-4">
      <div className="w-full max-w-md">
        {/* Card */}
        <div className="bg-surface rounded-lg shadow-lg border border-border p-8 text-center">
          {/* Icon */}
          <div className="mx-auto w-16 h-16 rounded-full bg-amber-100 flex items-center justify-center mb-6">
            <AlertTriangle className="size-8 text-amber-600" />
          </div>

          {/* Title */}
          <h1 className="text-2xl font-bold text-text-primary mb-2">
            Suscripción pendiente de pago
          </h1>

          {/* Message */}
          <p className="text-text-secondary mb-6">
            {message || 'Su suscripción está pendiente de pago. Por favor, complete el pago para continuar usando el sistema.'}
          </p>

          {/* Info box */}
          <div className="bg-amber-50 border border-amber-200 rounded-lg p-4 mb-6 text-left">
            <div className="flex items-start gap-3">
              <CreditCard className="size-5 text-amber-600 mt-0.5 flex-shrink-0" />
              <div>
                <p className="text-sm text-amber-800 font-medium">¿Qué significa esto?</p>
                <p className="text-sm text-amber-700 mt-1">
                  Su negocio tiene una suscripción activa pero el pago está pendiente. 
                  Mientras tanto, no podrá acceder a las funciones del sistema.
                </p>
              </div>
            </div>
          </div>

          {/* Actions */}
          <div className="space-y-3">
            <button
              onClick={handleLogout}
              className="w-full py-2.5 px-4 rounded-lg font-medium transition-colors flex items-center justify-center gap-2 bg-primary hover:bg-primary-hover text-white"
            >
              <ArrowLeft className="size-4" />
              Cerrar sesión
            </button>
          </div>
        </div>

        {/* Footer */}
        <p className="text-center text-xs text-text-muted mt-4">
          Si cree que esto es un error, contacte al administrador del sistema.
        </p>
      </div>
    </div>
  );
}
