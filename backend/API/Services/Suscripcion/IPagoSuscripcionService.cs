using API.DTO.Response.Suscripcion;
using static API.Models.Enums;

namespace API.Services.Suscripcion
{
    /// <summary>
    /// Servicio para gestionar los pagos de suscripciones
    /// </summary>
    public interface IPagoSuscripcionService
    {
        /// <summary>
        /// Obtiene el historial de pagos de un negocio
        /// </summary>
        Task<List<PagoSuscripcionResponse>> GetPagosByNegocioAsync(int idNegocio, CancellationToken ct = default);

        /// <summary>
        /// Obtiene un pago específico por su ID
        /// </summary>
        Task<PagoSuscripcionResponse?> GetPagoByIdAsync(int id, int idNegocio, CancellationToken ct = default);

        /// <summary>
        /// Registra un nuevo pago para una suscripción
        /// </summary>
        Task<PagoSuscripcionResponse> CreatePagoAsync(int idSuscripcion, decimal monto, MetodoPagoSuscripcion metodo, string? referencia, CancellationToken ct = default);
    }
}
