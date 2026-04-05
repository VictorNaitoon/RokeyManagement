namespace API.DTO.Response.Presupuestos
{
    public class PresupuestoResponse
    {
        public int Id { get; set; }
        public int IdUsuario { get; set; }
        public string? NombreUsuario { get; set; }
        public int? IdCliente { get; set; }
        public string? NombreCliente { get; set; }
        public DateTime FechaEmision { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public string Estado { get; set; } = "Pendiente";
        public decimal Total { get; set; }
        public List<DetallePresupuestoResponse> Detalles { get; set; } = new();
    }
}
