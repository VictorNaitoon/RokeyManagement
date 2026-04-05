namespace API.DTO.Response.Usuarios
{
    public class UsuarioResponse
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public DateTime? FechaAlta { get; set; }
    }

    public class UsuarioListResponse
    {
        public List<UsuarioResponse> Usuarios { get; set; } = new();
        public int Total { get; set; }
    }
}