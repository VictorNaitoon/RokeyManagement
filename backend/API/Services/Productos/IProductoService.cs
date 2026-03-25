using API.DTO.Request.Productos;
using API.DTO.Response.Productos;

namespace API.Services.Productos
{
    public interface IProductoService
    {
        Task<ProductoListResponse> GetAllAsync(string? busqueda = null);
        Task<ProductoResponse?> GetByIdAsync(int id);
        Task<ProductoResponse> CreateAsync(CrearProductoRequest request);
        Task<ProductoResponse?> UpdateAsync(int id, ActualizarProductoRequest request);
        Task<bool> DeleteAsync(int id);
        Task<bool> ActivarAsync(int id);
        Task<ProductoResponse> DuplicateAsync(int id, string nuevoNombre);
    }
}