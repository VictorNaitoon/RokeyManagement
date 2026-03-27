namespace API.DTO.Request.Public
{
    /// <summary>
    /// Request para iniciar sesión como cliente existente (sin password - autenticación por email)
    /// </summary>
    public record ClienteLoginRequest(
        string Email
    );
}