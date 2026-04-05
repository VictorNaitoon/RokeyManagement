namespace API.DTO.Response.Suscripcion
{
    /// <summary>
    /// DTO para las métricas de uso actual del negocio
    /// </summary>
    public class MetricaUsoResponse
    {
        public int Id { get; set; }
        public int IdNegocio { get; set; }
        public int Anio { get; set; }
        public int Mes { get; set; }
        public int TotalUsuarios { get; set; }
        public int TotalProductos { get; set; }
        public int TotalTransacciones { get; set; }
        public double AlmacenamientoUsadoGB { get; set; }
        public int TotalAPICalls { get; set; }
        public DateTime UltimaActualizacion { get; set; }
    }
}
