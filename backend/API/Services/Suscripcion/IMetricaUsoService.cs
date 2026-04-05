using API.DTO.Response.Suscripcion;
using API.Models;

namespace API.Services.Suscripcion
{
    /// <summary>
    /// Servicio para gestionar las métricas de uso de los negocios
    /// </summary>
    public interface IMetricaUsoService
    {
        /// <summary>
        /// Obtiene las métricas del mes actual para un negocio
        /// </summary>
        Task<MetricaUsoResponse?> GetMetricaActualAsync(int idNegocio, CancellationToken ct = default);

        /// <summary>
        /// Registra o actualiza las métricas de uso para un negocio
        /// </summary>
        Task<MetricaUso> RecordOrUpdateMetricaAsync(int idNegocio, int usuariosActivos, int productosActivos, int transacciones, long almacenamientoBytes, CancellationToken ct = default);

        /// <summary>
        /// Obtiene las métricas de un período específico (año/mes)
        /// </summary>
        Task<MetricaUsoResponse?> GetMetricaByPeriodoAsync(int idNegocio, int anio, int mes, CancellationToken ct = default);
    }
}
