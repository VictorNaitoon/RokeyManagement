using API.DTO.Request.Public;
using API.DTO.Response.Public;

namespace API.Services.Publicos
{
    /// <summary>
    /// Servicio para gestión de clientes (solo Admin)
    /// </summary>
    public interface ICuentaPublicaService
    {
        /// <summary>
        /// Lista clientes del negocio (para Admin)
        /// </summary>
        Task<List<AccountResponse>> GetClientesAsync(int idNegocio);
        
        /// <summary>
        /// Crea un nuevo cliente
        /// </summary>
        Task<AccountResponse> CrearClienteAsync(CrearClienteRequest request, int idNegocio);
        
        /// <summary>
        /// Actualiza un cliente existente
        /// </summary>
        Task<AccountResponse?> ActualizarClienteAsync(int clienteId, ActualizarClienteRequest request, int idNegocio);
        
        /// <summary>
        /// Elimina (desactiva) un cliente
        /// </summary>
        Task<bool> EliminarClienteAsync(int clienteId, int idNegocio);
    }
}