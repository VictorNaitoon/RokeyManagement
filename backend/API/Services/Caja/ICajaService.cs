using API.DTO.Request.Caja;
using API.DTO.Response.Caja;

namespace API.Services.Caja
{
    public interface ICajaService
    {
        /// <summary>
        /// Abre una nueva caja para el negocio especificado
        /// </summary>
        /// <param name="request">Request con el monto inicial</param>
        /// <param name="userId">ID del usuario que abre la caja</param>
        /// <param name="negocioId">ID del negocio</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>La caja creada</returns>
        Task<CajaResponse> AbrirCajaAsync(AperturaCajaRequest request, int userId, int negocioId, CancellationToken ct = default);

        /// <summary>
        /// Cierra la caja abierta del negocio especificado
        /// </summary>
        /// <param name="request">Request con el monto final</param>
        /// <param name="userId">ID del usuario que cierra la caja</param>
        /// <param name="negocioId">ID del negocio</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>La caja cerrada</returns>
        Task<CajaResponse> CerrarCajaAsync(CierreCajaRequest request, int userId, int negocioId, CancellationToken ct = default);

        /// <summary>
        /// Obtiene el estado de caja del negocio (abierta o cerrada)
        /// </summary>
        /// <param name="negocioId">ID del negocio</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Estado de caja con la caja abierta si existe</returns>
        Task<EstadoCajaResponse> ObtenerEstadoCajaAsync(int negocioId, CancellationToken ct = default);

        /// <summary>
        /// Agrega un movimiento (ingreso o egreso) a la caja abierta
        /// </summary>
        /// <param name="request">Request con los datos del movimiento</param>
        /// <param name="userId">ID del usuario que registra el movimiento</param>
        /// <param name="negocioId">ID del negocio</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>El movimiento creado</returns>
        Task<MovimientoCajaResponse> AgregarMovimientoAsync(AgregarMovimientoCajaRequest request, int userId, int negocioId, CancellationToken ct = default);

        /// <summary>
        /// Obtiene todos los movimientos de una caja específica
        /// </summary>
        /// <param name="cajaId">ID de la caja</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Lista de movimientos</returns>
        Task<IEnumerable<MovimientoCajaResponse>> ObtenerMovimientosAsync(int cajaId, CancellationToken ct = default);

        /// <summary>
        /// Verifica si el negocio tiene una caja abierta
        /// </summary>
        /// <param name="negocioId">ID del negocio</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>True si hay una caja abierta</returns>
        Task<bool> TieneCajaAbiertaAsync(int negocioId, CancellationToken ct = default);
    }
}
