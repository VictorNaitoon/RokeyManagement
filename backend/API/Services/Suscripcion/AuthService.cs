using API.Data;
using static API.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace API.Services.Suscripcion
{
    /// <summary>
    /// Implementación del servicio de autenticación para verificación de suscripción
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AuthService> _logger;

        public AuthService(AppDbContext context, ILogger<AuthService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> CanNegocioOperarAsync(int idNegocio, CancellationToken ct = default)
        {
            // Obtener la suscripción más reciente del negocio
            var suscripcion = await _context.Suscripciones
                .Where(s => s.Id_negocio == idNegocio)
                .OrderByDescending(s => s.FechaInicio)
                .FirstOrDefaultAsync(ct);

            // Si no hay suscripción, denegar acceso
            if (suscripcion == null)
            {
                _logger.LogWarning("Negocio {IdNegocio} no tiene suscripción", idNegocio);
                return false;
            }

            // Según la decisión de negocio: permitir lectura por 1 semana post-vencimiento
            // Si está vencida hace más de 7 días → bloquear total
            if (suscripcion.Estado == EstadoSuscripcion.Vencida && suscripcion.FechaFin.HasValue)
            {
                var diasTranscurridos = (DateTime.UtcNow - suscripcion.FechaFin.Value).TotalDays;
                
                if (diasTranscurridos > 7)
                {
                    _logger.LogWarning(
                        "Negocio {IdNegocio} tiene suscripción vencida hace más de 7 días. Bloqueando acceso.",
                        idNegocio);
                    return false;
                }

                // Está dentro del período de gracia de 7 días - permitir acceso con warning
                _logger.LogInformation(
                    "Negocio {IdNegocio} en período de gracia (vencida hace {Dias} días). Permitiendo acceso.",
                    idNegocio, diasTranscurridos);
                return true;
            }

            // Suscripción activa → permitir
            if (suscripcion.Estado == EstadoSuscripcion.Activa)
            {
                return true;
            }

            // Otros estados: PendientePago, Cancelada, Suspendida
            // Por defecto denegamos hasta que se regularice
            _logger.LogWarning(
                "Negocio {IdNegocio} tiene suscripción en estado {Estado}. Bloqueando acceso.",
                idNegocio, suscripcion.Estado);
            return false;
        }

        public async Task<string> GetMensajeBloqueoAsync(int idNegocio, CancellationToken ct = default)
        {
            var suscripcion = await _context.Suscripciones
                .Where(s => s.Id_negocio == idNegocio)
                .OrderByDescending(s => s.FechaInicio)
                .FirstOrDefaultAsync(ct);

            if (suscripcion == null)
            {
                return "Su negocio no tiene una suscripción activa. Por favor, contacte al administrador.";
            }

            return suscripcion.Estado switch
            {
                EstadoSuscripcion.Activa => "Su suscripción está activa.",
                
                EstadoSuscripcion.Vencida when suscripcion.FechaFin.HasValue =>
                    ObtenerMensajeVencida(suscripcion.FechaFin.Value),
                
                EstadoSuscripcion.PendientePago => 
                    "Su suscripción está pendiente de pago. Por favor, complete el pago para continuar usando el sistema.",
                
                EstadoSuscripcion.Cancelada => 
                    "Su suscripción ha sido cancelada. Por favor, contacte al administrador para más información.",
                
                EstadoSuscripcion.Suspendida => 
                    "Su suscripción está suspendida. Por favor, contacte al administrador para más información.",
                
                _ => "Su suscripción no está activa. Por favor, contacte al administrador."
            };
        }

        private static string ObtenerMensajeVencida(DateTime fechaFin)
        {
            var diasTranscurridos = (DateTime.UtcNow - fechaFin).TotalDays;

            if (diasTranscurridos > 7)
            {
                return "Su suscripción ha vencido y el período de gracia de 7 días ha expirado. " +
                       "El acceso al sistema ha sido bloqueado. Por favor, contacte al administrador para renovar su suscripción.";
            }

            var diasRestantes = 7 - (int)diasTranscurridos;
            return $"Su suscripción ha vencido. Tiene {diasRestantes} día(s) de acceso de lectura restante. " +
                   "Por favor, renueve su suscripción para continuar usando el sistema sin interrupciones.";
        }
    }
}
