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
        
        // Stock Alerts
        Task<List<ProductoAlertaResponse>> GetProductosConStockBajoAsync(int idNegocio, CancellationToken ct = default);
        Task<int> GetContadorStockBajoAsync(int idNegocio, CancellationToken ct = default);
        Task<MovimientoStockListResponse> GetMovimientosStockAsync(int productoId, int idNegocio, int page, int pageSize, CancellationToken ct = default);

        // Ajuste de stock con auditoría (slice stock-mvp-hardening)
        Task<ProductoResponse?> AjustarStockAsync(int id, AjusteStockRequest request, CancellationToken ct = default);

        // CSV Import streaming (CI-01/02/03)
        Task<ImportCsvResponse> ImportarCsvAsync(IFormFile file, CancellationToken ct = default);

        // Actualización Masiva de Precios (CU-010)
        Task<ActualizacionMasivaPreciosResponse> ActualizarPreciosPorCategoriaAsync(ActualizacionMasivaPreciosCategoriaRequest request);
        Task<ActualizacionMasivaPreciosResponse> ActualizarPreciosPorProductosAsync(ActualizacionMasivaPreciosProductosRequest request);
    }
}