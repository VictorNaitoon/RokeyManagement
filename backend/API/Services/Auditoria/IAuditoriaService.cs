using API.DTO.Request.Auditoria;
using API.DTO.Response.Auditoria;

namespace API.Services.Auditoria
{
    /// <summary>
    /// Interfaz del servicio de auditoría
    /// </summary>
    public interface IAuditoriaService
    {
        /// <summary>
        /// Registra una acción de auditoría
        /// </summary>
        /// <param name="entidad">Nombre de la entidad (Venta, Compra, Producto, etc.)</param>
        /// <param name="idRegistro">ID del registro modificado</param>
        /// <param name="accion">Tipo de acción: CREATE, UPDATE, SOFT_DELETE</param>
        /// <param name="datosAnteriores">Objeto con el estado anterior (null en Create)</param>
        /// <param name="datosNuevos">Objeto con el estado nuevo (null en Delete)</param>
        /// <param name="ct">Cancellation token</param>
        Task RegistrarAsync(
            string entidad,
            int idRegistro,
            string accion,
            object? datosAnteriores = null,
            object? datosNuevos = null,
            CancellationToken ct = default);

        /// <summary>
        /// Lista registros de auditoría con filtros y paginación
        /// </summary>
        Task<(List<AuditoriaListResponse> Items, int Total)> ListarAsync(
            DTO.Request.Auditoria.FiltroAuditoriaRequest filtro,
            CancellationToken ct = default);

        /// <summary>
        /// Obtiene un registro de auditoría por ID
        /// </summary>
        /// <param name="id">ID del registro de auditoría</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>El registro de auditoría o null si no existe</returns>
        Task<DTO.Response.Auditoria.AuditoriaResponse?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
    }
}