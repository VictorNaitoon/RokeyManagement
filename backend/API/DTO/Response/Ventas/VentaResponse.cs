namespace API.DTO.Response.Ventas
{
    public class VentaResponse
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public decimal TotalVenta { get; set; }
        public int IdUsuario { get; set; }
        public string? NombreUsuario { get; set; }
        public int? IdCliente { get; set; }
        public string? NombreCliente { get; set; }
        public string Estado { get; set; } = "Activa";
        public List<DetalleVentaResponse> Detalles { get; set; } = new();
        public List<PagoResponse> Pagos { get; set; } = new();
    }

    public class VentaListItem
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public decimal TotalVenta { get; set; }
        public string? NombreUsuario { get; set; }
        public string? NombreCliente { get; set; }
        public string Estado { get; set; } = "Activa";
    }

    public class VentaListResponse
    {
        public List<VentaListItem> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
