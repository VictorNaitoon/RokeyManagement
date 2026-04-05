namespace API.DTO.Response.Presupuestos
{
    public class PresupuestoListItem
    {
        public int Id { get; set; }
        public string? NombreCliente { get; set; }
        public DateTime FechaEmision { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public string Estado { get; set; } = "Pendiente";
        public decimal Total { get; set; }
    }

    public class PresupuestoListResponse
    {
        public List<PresupuestoListItem> Items { get; set; } = new();
        public int TotalCount { get; set; }
    }
}
