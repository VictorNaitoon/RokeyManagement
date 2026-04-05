using API.DTO.Response.Informes;

namespace API.Services.Informes
{
    /// <summary>
    /// Interface for the Informes/Dashboard service
    /// </summary>
    public interface IInformesService
    {
        /// <summary>
        /// Get sales summary (total sales, quantity, average ticket)
        /// </summary>
        Task<VentasResumenResponse> GetVentasResumenAsync(DateTime? fechaDesde, DateTime? fechaHasta, string? preset, CancellationToken ct);

        /// <summary>
        /// Get top products by revenue/quantity
        /// </summary>
        Task<ProductosTopResponse> GetProductosTopAsync(int cantidad, DateTime? fechaDesde, DateTime? fechaHasta, CancellationToken ct);

        /// <summary>
        /// Get cash flow (income vs expenses from caja)
        /// </summary>
        Task<FlujoCajaResponse> GetFlujoCajaAsync(DateTime? fechaDesde, DateTime? fechaHasta, CancellationToken ct);

        /// <summary>
        /// Get revenue vs expenses (Ventas - Compras)
        /// </summary>
        Task<IngresosGastosResponse> GetIngresosGastosAsync(DateTime? fechaDesde, DateTime? fechaHasta, CancellationToken ct);

        /// <summary>
        /// Get products below minimum stock (only products, not services)
        /// </summary>
        Task<AlertasStockResponse> GetAlertasStockAsync(CancellationToken ct);

        /// <summary>
        /// Get sales breakdown by payment method
        /// </summary>
        Task<VentasPorPagoResponse> GetVentasPorPagoAsync(DateTime? fechaDesde, DateTime? fechaHasta, CancellationToken ct);

        /// <summary>
        /// Get sales aggregated by seller
        /// </summary>
        Task<VentasPorVendedorResponse> GetVentasPorVendedorAsync(DateTime? fechaDesde, DateTime? fechaHasta, CancellationToken ct);
    }
}
