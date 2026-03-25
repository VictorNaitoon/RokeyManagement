using API.DTO.Request.Ventas;
using API.DTO.Response.Ventas;

namespace API.Services.Ventas
{
    public interface IVentaService
    {
        Task<VentaResponse> CrearVentaAsync(CrearVentaRequest request, CancellationToken ct);
        Task<VentaResponse> AnularVentaAsync(int ventaId, AnularVentaRequest request, CancellationToken ct);
        Task<VentaResponse?> ObtenerVentaPorIdAsync(int ventaId, CancellationToken ct);
        Task<VentaListResponse> ObtenerTodasLasVentasAsync(int page, int pageSize, DateTime? fechaDesde, DateTime? fechaHasta, CancellationToken ct);
        Task<List<DetalleVentaResponse>> ObtenerDetallesVentaAsync(int ventaId, CancellationToken ct);
        Task<List<PagoResponse>> ObtenerPagosVentaAsync(int ventaId, CancellationToken ct);
    }
}
