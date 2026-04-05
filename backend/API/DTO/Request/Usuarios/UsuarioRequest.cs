namespace API.DTO.Request.Usuarios
{
    public class CrearUsuarioRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public int Rol { get; set; } // 0 = Admin, 1 = Manager, 2 = Vendedor
    }

    public class ActualizarUsuarioRequest
    {
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public int Rol { get; set; }
        public bool Activo { get; set; }
    }

    public class CambiarPasswordRequest
    {
        public string PasswordActual { get; set; } = string.Empty;
        public string PasswordNuevo { get; set; } = string.Empty;
    }
}