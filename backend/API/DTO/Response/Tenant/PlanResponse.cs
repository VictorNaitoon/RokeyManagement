namespace API.DTO.Response.Tenant
{
    public class TenantPlanResponse
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal PrecioMensual { get; set; }
        public decimal PrecioAnual { get; set; }
    }
}
