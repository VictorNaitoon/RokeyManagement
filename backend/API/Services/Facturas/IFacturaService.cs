using API.DTO.Request.Facturas;
using API.DTO.Response.Facturas;

namespace API.Services.Facturas
{
    /// <summary>
    /// Interfaz para el servicio de gestión de facturas.
    /// </summary>
    public interface IFacturaService
    {
        /// <summary>
        /// Crea una factura proforma asociada a una venta existente.
        /// </summary>
        Task<FacturaResponse> CrearFacturaProformaAsync(CrearFacturaRequest request, CancellationToken ct);

        /// <summary>
        /// Emite una nota de crédito asociada a una venta que ya tiene factura con CAE.
        /// </summary>
        Task<FacturaResponse> EmitirNotaCreditoAsync(NotaCreditoRequest request, CancellationToken ct);

        /// <summary>
        /// Obtiene una factura por su ID.
        /// </summary>
        Task<FacturaResponse?> ObtenerFacturaPorIdAsync(int id, CancellationToken ct);

        /// <summary>
        /// Obtiene todas las facturas del negocio con filtros opcionales y paginación.
        /// </summary>
        Task<ListadoFacturaResponse> ObtenerTodasLasFacturasAsync(int page, int pageSize, DateTime? fechaDesde, DateTime? fechaHasta, CancellationToken ct);

        /// <summary>
        /// Valida que una venta no tenga una factura asociada.
        /// </summary>
        Task<bool> ValidarVentaSinFacturaAsync(int idVenta, CancellationToken ct);
    }
}
