namespace API.DTO.Response.Compras
{
    public class CompraResponse
    {
        public int Id { get; set; }
        public string NumeroComprobante { get; set; } = string.Empty;
        public int IdProveedor { get; set; }
        public string? NombreProveedor { get; set; }
        public DateTime FechaCompra { get; set; }
        public decimal TotalGasto { get; set; }
        public bool Anulada { get; set; }
        public string? MotivoAnulacion { get; set; }
        public List<DetalleCompraResponse> Detalles { get; set; } = new List<DetalleCompraResponse>();
    }

    public class DetalleCompraResponse
    {
        public int Id { get; set; }
        public int IdProducto { get; set; }
        public string? NombreProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
    }

    public class CompraListResponse
    {
        public List<CompraResponse> Compras { get; set; } = new List<CompraResponse>();
        public int Total { get; set; }
    }
}