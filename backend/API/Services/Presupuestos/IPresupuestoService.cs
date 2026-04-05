using API.DTO.Request.Presupuestos;
using API.DTO.Response.Presupuestos;
using API.DTO.Response.Ventas;
using API.Models;

namespace API.Services.Presupuestos
{
    public interface IPresupuestoService
    {
        /// <summary>
        /// Crea un nuevo presupuesto
        /// </summary>
        Task<PresupuestoResponse> CreateAsync(CreatePresupuestoRequest request, CancellationToken ct);

        /// <summary>
        /// Obtiene todos los presupuestos del negocio con filtros opcionales
        /// </summary>
        Task<PresupuestoListResponse> GetAllAsync(Enums.EstadoPresupuesto? estado = null, int? idCliente = null, CancellationToken ct = default);

        /// <summary>
        /// Obtiene un presupuesto por ID
        /// </summary>
        Task<PresupuestoResponse> GetByIdAsync(int id, CancellationToken ct);

        /// <summary>
        /// Actualiza el estado de un presupuesto
        /// </summary>
        Task<PresupuestoResponse> UpdateEstadoAsync(int id, Enums.EstadoPresupuesto nuevoEstado, CancellationToken ct);

        /// <summary>
        /// Anula un presupuesto (solo si está Pendiente)
        /// </summary>
        Task AnularAsync(int id, CancellationToken ct);

        /// <summary>
        /// Convierte un presupuesto a venta
        /// </summary>
        Task<VentaResponse> ConvertirAVentaAsync(int id, CancellationToken ct);
    }
}
