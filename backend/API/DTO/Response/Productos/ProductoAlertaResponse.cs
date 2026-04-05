namespace API.DTO.Response.Productos
{
    public class ProductoAlertaResponse
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int StockActual { get; set; }
        public int StockMinimo { get; set; }
        public int Diferencia { get; set; } // StockMinimo - StockActual
        public string? CategoriaNombre { get; set; }
    }

    public class ProductoAlertaListResponse
    {
        public List<ProductoAlertaResponse> Productos { get; set; } = new List<ProductoAlertaResponse>();
        public int Total { get; set; }
    }
}
