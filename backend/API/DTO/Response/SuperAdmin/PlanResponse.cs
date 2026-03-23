namespace API.DTO.Response.SuperAdmin
{
    public class PlanResponse
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal PrecioMensual { get; set; }
        public decimal PrecioAnual { get; set; }
        public int MaxUsuarios { get; set; }
        public int MaxProductos { get; set; }
        public int MaxTransaccionesMes { get; set; }
        public bool SoportePrioritario { get; set; }
        public bool MultiSucursal { get; set; }
        public bool APIAccess { get; set; }
        public bool Activo { get; set; }
        public int Orden { get; set; }
    }
}
