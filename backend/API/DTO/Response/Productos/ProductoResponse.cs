namespace API.DTO.Response.Productos
{
    public class ProductoResponse
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? CodigoBusqueda { get; set; }
        public string? Descripcion { get; set; }
        public decimal PrecioVenta { get; set; }
        
        // Solo visible para Admin - null para Vendedor
        public decimal? PrecioCompra { get; set; }
        
        public int StockActual { get; set; }
        public int StockMinimo { get; set; }
        public bool StockBajo => StockActual <= StockMinimo;
        public string? ImagenURL { get; set; }
        public bool EsServicio { get; set; }
        public bool Activo { get; set; }
        public int? IdCategoria { get; set; }
        public string? NombreCategoria { get; set; }
    }

    public class ProductoListResponse
    {
        public List<ProductoResponse> Productos { get; set; } = new List<ProductoResponse>();
        public int Total { get; set; }
    }
}