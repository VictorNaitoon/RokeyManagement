using static API.Models.Enums;

namespace API.Services.Suscripcion
{
    /// <summary>
    /// Interfaz para verificar el estado de acceso de un negocio basado en su suscripción
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Verifica si el negocio puede operar (acceso completo al sistema)
        /// Retorna false si la suscripción está vencida hace más de 7 días
        /// </summary>
        Task<bool> CanNegocioOperarAsync(int idNegocio, CancellationToken ct = default);

        /// <summary>
        /// Retorna un mensaje legible para el usuario sobre el estado de su suscripción
        /// </summary>
        Task<string> GetMensajeBloqueoAsync(int idNegocio, CancellationToken ct = default);
    }
}
