namespace API.DTO.Response.Auditoria
{
    /// <summary>
    /// Respuesta completa de un registro de auditoría
    /// </summary>
    public class AuditoriaResponse
    {
        public int Id { get; set; }
        public string Entidad { get; set; } = string.Empty;
        public int IdRegistro { get; set; }
        public string Accion { get; set; } = string.Empty;
        public int IdUsuario { get; set; }
        public string? NombreUsuario { get; set; }
        public DateTime Fecha { get; set; }
        public string? DatosAnteriores { get; set; }
        public string? DatosNuevos { get; set; }
    }
}