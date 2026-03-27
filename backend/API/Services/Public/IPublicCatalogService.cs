using API.DTO.Response.Public;

namespace API.Services.Publicos
{
    /// <summary>
    /// Servicio para el catálogo público de productos (sin autenticación)
    /// </summary>
    public interface ICatalogoPublicoService
    {
        /// <summary>
        /// Obtiene todos los productos visibles del catálogo público para un negocio
        /// </summary>
        Task<ProductoPublicResponse[]> GetCatalogoAsync(int idNegocio);
        
        /// <summary>
        /// Obtiene un producto específico del catálogo público
        /// </summary>
        Task<ProductoPublicResponse?> GetProductoAsync(int idNegocio, int productoId);
    }
}