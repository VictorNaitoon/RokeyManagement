namespace API.DTO.Response.Suscripcion
{
    /// <summary>
    /// DTO con información completa de la suscripción
    /// </summary>
    public class SuscripcionResponse
    {
        public int Id { get; set; }
        public int IdNegocio { get; set; }
        public int IdPlan { get; set; }
        public string? NombrePlan { get; set; }
        public string? DescripcionPlan { get; set; }
        public Models.Enums.EstadoSuscripcion Estado { get; set; }
        public Models.Enums.TipoFacturacion TipoFacturacion { get; set; }
        public decimal Precio { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public DateTime? FechaProximoPago { get; set; }
        public DateTime? FechaCancelacion { get; set; }
        public string? MotivoCancelacion { get; set; }

        // Límites del plan
        public int MaxUsuarios { get; set; }
        public int MaxProductos { get; set; }
        public int MaxTransaccionesMes { get; set; }
        public bool SoportePrioritario { get; set; }
        public bool MultiSucursal { get; set; }
        public bool APIAccess { get; set; }
    }
}
