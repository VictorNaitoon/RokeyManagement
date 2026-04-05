using API.DTO.Request.Proveedores;
using API.DTO.Response.Proveedores;

namespace API.Services.Proveedores
{
    public interface IProveedorService
    {
        Task<ProveedorListResponse> GetAllAsync();
        Task<ProveedorResponse?> GetByIdAsync(int id);
        Task<ProveedorResponse> CreateAsync(CrearProveedorRequest request);
        Task<ProveedorResponse?> UpdateAsync(int id, ActualizarProveedorRequest request);
        Task<bool> DeleteAsync(int id);
    }
}