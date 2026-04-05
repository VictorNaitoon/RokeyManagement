using API.DTO.Request.Suscripcion;
using API.DTO.Response.Suscripcion;
using static API.Models.Enums;

namespace API.Services.Suscripcion
{
    /// <summary>
    /// Servicio para gestionar suscripciones de negocios
    /// </summary>
    public interface ISuscripcionService
    {
        /// <summary>
        /// Obtiene la suscripción activa de un negocio
        /// </summary>
        Task<SuscripcionResponse?> GetSuscripcionByNegocioAsync(int idNegocio, CancellationToken ct = default);

        /// <summary>
        /// Crea una nueva suscripción para un negocio
        /// </summary>
        Task<SuscripcionResponse> CreateSuscripcionAsync(int idNegocio, SuscripcionRequest request, CancellationToken ct = default);

        /// <summary>
        /// Actualiza la suscripción (upgrade/downgrade de plan)
        /// </summary>
        Task<SuscripcionResponse> UpdateSuscripcionAsync(int idNegocio, SuscripcionRequest request, CancellationToken ct = default);

        /// <summary>
        /// Cancela la suscripción de un negocio
        /// </summary>
        Task<SuscripcionResponse> CancelarSuscripcionAsync(int idNegocio, CancelarSuscripcionRequest request, CancellationToken ct = default);

        /// <summary>
        /// Verifica si el negocio está dentro de los límites de su plan
        /// </summary>
        Task<CheckLimitesResponse> CheckLimitesAsync(int idNegocio, CancellationToken ct = default);

        /// <summary>
        /// Obtiene el estado actual de la suscripción
        /// </summary>
        Task<EstadoSuscripcion?> GetEstadoSuscripcionAsync(int idNegocio, CancellationToken ct = default);
    }
}
