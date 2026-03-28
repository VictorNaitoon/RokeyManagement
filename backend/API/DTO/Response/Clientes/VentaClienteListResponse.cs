namespace API.DTO.Response.Clientes
{
    /// <summary>
    /// Respuesta paginada de ventas de cliente
    /// </summary>
    public class VentaClienteListResponse
    {
        public List<VentaResumenResponse> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
