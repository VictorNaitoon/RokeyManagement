namespace API.DTO.Response.Productos
{
    public class MovimientoStockResponse
    {
        public int Id { get; set; }
        public DateTime FechaMovimiento { get; set; }
        public string TipoMovimiento { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public int StockAnterior { get; set; }
        public int StockNuevo { get; set; }
        public string? Motivo { get; set; }
        public int IdUsuario { get; set; }
    }

    public class MovimientoStockListResponse
    {
        public List<MovimientoStockResponse> Movimientos { get; set; } = new List<MovimientoStockResponse>();
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
