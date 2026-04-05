namespace API.DTO.Response.SuperAdmin
{
    public class TenantResponse
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string CUIT { get; set; } = string.Empty;
        public string? Direccion { get; set; }
        public string? Telefono { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        
        // Info de suscripción
        public SuscripcionInfo? Suscripcion { get; set; }
        
        // Métricas del tenant
        public int TotalUsuarios { get; set; }
        public int TotalProductos { get; set; }
    }

    public class SuscripcionInfo
    {
        public int Id { get; set; }
        public string Plan { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string TipoFacturacion { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public DateTime? FechaProximoPago { get; set; }
        public DateTime FechaInicio { get; set; }
    }

    public class PlanRequest
    {
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
        public bool Activo { get; set; } = true;
        public int Orden { get; set; }
    }

    public class UpdateTenantEstadoRequest
    {
        public string Estado { get; set; } = "Activo";
    }
}
