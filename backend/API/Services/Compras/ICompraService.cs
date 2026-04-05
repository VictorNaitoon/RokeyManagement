using API.DTO.Request.Compras;
using API.DTO.Response.Compras;

namespace API.Services.Compras
{
    public interface ICompraService
    {
        Task<CompraListResponse> GetAllAsync();
        Task<CompraResponse?> GetByIdAsync(int id);
        Task<CompraResponse> CreateAsync(CrearCompraRequest request);
        Task<CompraResponse> AnularAsync(int id, AnularCompraRequest? request);
    }
}