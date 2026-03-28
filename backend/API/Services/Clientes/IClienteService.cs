using API.DTO.Request.Clientes;
using API.DTO.Response.Clientes;

namespace API.Services.Clientes
{
    /// <summary>
    /// Servicio para gestión de clientes (Admin + Vendedor read-only)
    /// </summary>
    public interface IClienteService
    {
        /// <summary>
        /// Lista clientes activos del negocio
        /// </summary>
        /// <param name="idNegocio">ID del negocio</param>
        /// <param name="incluirConsumidorFinal">Indica si incluir el cliente "Consumidor Final"</param>
        /// <param name="ct">CancellationToken</param>
        Task<List<AccountResponse>> GetAllAsync(int idNegocio, bool incluirConsumidorFinal = false, CancellationToken ct = default);
        
        /// <summary>
        /// Obtiene un cliente por ID
        /// </summary>
        Task<AccountResponse?> GetByIdAsync(int clienteId, int idNegocio, CancellationToken ct = default);
        
        /// <summary>
        /// Crea un nuevo cliente
        /// </summary>
        Task<AccountResponse> CreateAsync(CrearClienteRequest request, int idNegocio, CancellationToken ct = default);
        
        /// <summary>
        /// Actualiza un cliente existente
        /// </summary>
        Task<AccountResponse?> UpdateAsync(int clienteId, ActualizarClienteRequest request, int idNegocio, CancellationToken ct = default);
        
        /// <summary>
        /// Elimina (soft delete) un cliente
        /// </summary>
        Task<bool> DeleteAsync(int clienteId, int idNegocio, CancellationToken ct = default);
        
        /// <summary>
        /// Obtiene el historial de ventas de un cliente (paginado)
        /// </summary>
        Task<VentaClienteListResponse> GetVentasAsync(int clienteId, int idNegocio, int page, int pageSize, CancellationToken ct = default);
        
        /// <summary>
        /// Obtiene el saldo pendiente de un cliente (stub para Phase 3)
        /// </summary>
        Task<SaldoClienteResponse> GetSaldoAsync(int clienteId, int idNegocio, CancellationToken ct = default);
        
        /// <summary>
        /// Obtiene los pagos de un cliente (stub para Phase 3)
        /// </summary>
        Task<List<PagoClienteResponse>> GetPagosAsync(int clienteId, int idNegocio, CancellationToken ct = default);
    }
}
