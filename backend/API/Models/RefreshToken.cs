using API.Models;

namespace API.Models
{
    /// <summary>
    /// Tipo de usuario asociado al refresh token
    /// </summary>
    public enum RefreshTokenUserType
    {
        Usuario = 1,
        SuperAdmin = 2,
        Cliente = 3
    }

    /// <summary>
    /// Entidad de refresh token para sesiones persistentes.
    /// Usa soft-revoke (RevokedAt) para auditoría.
    /// </summary>
    public class RefreshToken
    {
        public int Id { get; set; }
        
        /// <summary>
        /// Token hasheado con SHA256
        /// </summary>
        public string Token { get; set; } = string.Empty;
        
        public int UserId { get; set; }
        
        public RefreshTokenUserType UserType { get; set; }
        
        /// <summary>
        /// Fecha de expiración (CreatedAt + 30 días)
        /// </summary>
        public DateTime ExpiresAt { get; set; }
        
        /// <summary>
        /// Fecha de revocación (soft-revoke). Null = activo.
        /// </summary>
        public DateTime? RevokedAt { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// IP del cliente que solicitó el token (auditoría)
        /// </summary>
        public string? IpAddress { get; set; }
        
        /// <summary>
        /// User-Agent del cliente (auditoría)
        /// </summary>
        public string? UserAgent { get; set; }
    }
}
