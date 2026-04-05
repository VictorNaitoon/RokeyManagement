using API.Models;

namespace API.DTO.Response.Facturas
{
    /// <summary>
    /// Response con los detalles de una factura.
    /// </summary>
    public class FacturaResponse
    {
        public int Id { get; set; }
        public int Id_negocio { get; set; }
        public int Id_venta { get; set; }
        public string? CUIT_cliente { get; set; }
        public DateTime Fecha_realizada { get; set; }
        public Enums.TipoComprobante Tipo_comprobante { get; set; }
        public string Numero_comprobante { get; set; } = string.Empty;
        public string? CAE { get; set; }
        public DateTime? Vencimiento_CAE { get; set; }
        public string? QR { get; set; }
        public string? Condicion_venta { get; set; }
    }

    /// <summary>
    /// Item individual para el listado de facturas.
    /// </summary>
    public class FacturaListItem
    {
        public int Id { get; set; }
        public int Id_venta { get; set; }
        public string? CUIT_cliente { get; set; }
        public DateTime Fecha_realizada { get; set; }
        public Enums.TipoComprobante Tipo_comprobante { get; set; }
        public string Numero_comprobante { get; set; } = string.Empty;
        public string? CAE { get; set; }
        public string? Condicion_venta { get; set; }
    }

    /// <summary>
    /// Response paginado para el listado de facturas.
    /// </summary>
    public class ListadoFacturaResponse
    {
        public List<FacturaListItem> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
