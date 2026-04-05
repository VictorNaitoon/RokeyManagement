namespace API.DTO.Response.Auditoria
{
    /// <summary>
    /// Respuesta resumida para listados de auditoría (sin JSON diff)
    /// </summary>
    public class AuditoriaListResponse
    {
        public int Id { get; set; }
        public string Entidad { get; set; } = string.Empty;
        public int IdRegistro { get; set; }
        public string Accion { get; set; } = string.Empty;
        public int IdUsuario { get; set; }
        public string? NombreUsuario { get; set; }
        public DateTime Fecha { get; set; }
    }
}