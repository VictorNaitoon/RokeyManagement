namespace API.DTO.Response
{
    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public UsuarioDto Usuario { get; set; } = null!;
    }

    public class UsuarioDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public int IdNegocio { get; set; }
        public string? NombreNegocio { get; set; }
    }

    public class SuperAdminAuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public SuperAdminDto SuperAdmin { get; set; } = null!;
    }

    public class SuperAdminDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class ErrorResponse
    {
        public string Error { get; set; } = string.Empty;
    }

    /// <summary>
    /// Respuesta para refresh token endpoint
    /// </summary>
    public class RefreshResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public int ExpiresIn { get; set; }
        public string TokenType { get; set; } = "Bearer";
    }

    /// <summary>
    /// Respuesta de revocación de token
    /// </summary>
    public class RevokeResponse
    {
        public string Message { get; set; } = string.Empty;
    }
}
