using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.Models;

namespace API.Services.Auth
{
    public interface IAuthService
    {
        Task<(Usuario? usuario, string? error)> AuthenticateAsync(string email, string password);
        Task<(Models.SuperAdmin? superAdmin, string? error)> AuthenticateSuperAdminAsync(string email, string password);
        Task<(Usuario? usuario, string? error)> RegisterAsync(string email, string password, string nombre, string apellido, int idNegocio, Enums.RolUsuario rol);
        
        /// <summary>
        /// Genera un nuevo refresh token para el usuario.
        /// Retorna el token raw (sin hashear) para guardar en cookie.
        /// </summary>
        Task<string> GenerateRefreshTokenAsync(int userId, RefreshTokenUserType userType, string? ipAddress, string? userAgent);
        
        /// <summary>
        /// Valida un refresh token contra la base de datos
        /// </summary>
        Task<(int? userId, RefreshTokenUserType? userType, int? negocioId, string? email, string? rol)?> ValidateRefreshTokenAsync(string token);
        
        /// <summary>
        /// Revoca un refresh token específico
        /// </summary>
        Task<bool> RevokeRefreshTokenAsync(string token);
        
        /// <summary>
        /// Revoca todos los refresh tokens activos de un usuario
        /// </summary>
        Task<int> RevokeAllUserTokensAsync(int userId);
        
        /// <summary>
        /// Obtiene el email y negocioId del usuario
        /// </summary>
        Task<(string email, int negocioId)?> GetUserInfoAsync(int userId);
    }

    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;

        public AuthService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(Usuario? usuario, string? error)> AuthenticateAsync(string email, string password)
        {
            var usuario = await Task.Run(() => 
                _context.Usuarios.FirstOrDefault(u => u.Email == email && u.Activo)
            );

            if (usuario == null)
            {
                return (null, "Usuario no encontrado");
            }

            if (!VerifyPassword(password, usuario.PasswordHash))
            {
                return (null, "Contraseña incorrecta");
            }

            // Verificar que el negocio esté activo
            var negocio = _context.Negocios.FirstOrDefault(n => n.Id == usuario.Id_negocio);
            if (negocio == null || negocio.Estado != Enums.EstadoNegocio.Activo)
            {
                return (null, "El negocio asociado está inactivo o no existe");
            }

            return (usuario, null);
        }

        public async Task<(Models.SuperAdmin? superAdmin, string? error)> AuthenticateSuperAdminAsync(string email, string password)
        {
            var superAdmin = await Task.Run(() => 
                _context.SuperAdmins.FirstOrDefault(s => s.Email == email && s.Activo)
            );

            if (superAdmin == null)
            {
                return (null, "Super Admin no encontrado");
            }

            if (!VerifyPassword(password, superAdmin.PasswordHash))
            {
                return (null, "Contraseña incorrecta");
            }

            return (superAdmin, null);
        }

        public async Task<(Usuario? usuario, string? error)> RegisterAsync(
            string email, 
            string password, 
            string nombre, 
            string apellido, 
            int idNegocio, 
            Enums.RolUsuario rol)
        {
            // Verificar que el email no exista en el mismo negocio
            var existingUser = await Task.Run(() => 
                _context.Usuarios.FirstOrDefault(u => u.Email == email && u.Id_negocio == idNegocio)
            );

            if (existingUser != null)
            {
                return (null, "Ya existe un usuario con este email en este negocio");
            }

            var passwordHash = HashPassword(password);

            var usuario = new Usuario
            {
                Id_negocio = idNegocio,
                Email = email,
                PasswordHash = passwordHash,
                Nombre = nombre,
                Apellido = apellido,
                Rol = rol,
                Activo = true
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return (usuario, null);
        }

        public static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        public static bool VerifyPassword(string password, string storedHash)
        {
            var hash = HashPassword(password);
            return hash == storedHash;
        }

        /// <summary>
        /// Genera un nuevo refresh token para el usuario.
        /// Usa GUID + entropy para crear un token único y lo hashea con SHA256.
        /// Retorna el token raw (sin hashear) para guardar en cookie.
        /// </summary>
        public async Task<string> GenerateRefreshTokenAsync(int userId, RefreshTokenUserType userType, string? ipAddress, string? userAgent)
        {
            // Generar token único: GUID + entropy
            var randomBytes = new byte[32];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            
            var rawToken = $"{Guid.NewGuid()}:{Convert.ToBase64String(randomBytes)}";
            
            // Hashear con SHA256
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawToken));
            var tokenHash = Convert.ToBase64String(hashedBytes);
            
            var now = DateTime.UtcNow;
            var refreshToken = new RefreshToken
            {
                Token = tokenHash,
                UserId = userId,
                UserType = userType,
                ExpiresAt = now.AddDays(30), // 30 días de validez
                CreatedAt = now,
                IpAddress = ipAddress,
                UserAgent = userAgent
            };
            
            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();
            
            return rawToken; // Retornar el token raw para guardar en cookie
        }

        /// <summary>
        /// Valida un refresh token contra la base de datos.
        /// Retorna la información del usuario si el token es válido.
        /// </summary>
        public async Task<(int? userId, RefreshTokenUserType? userType, int? negocioId, string? email, string? rol)?> ValidateRefreshTokenAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
                return null;
            
            // Hashear el token para buscar en la DB
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
            var tokenHash = Convert.ToBase64String(hashedBytes);
            
            var refreshToken = await Task.Run(() => 
                _context.RefreshTokens.FirstOrDefault(rt => rt.Token == tokenHash)
            );
            
            if (refreshToken == null)
                return null;
            
            // Validar: no está revocado y no ha expirado
            if (refreshToken.RevokedAt != null)
                return null;
            
            if (refreshToken.ExpiresAt < DateTime.UtcNow)
                return null;
            
            // Obtener información del usuario (incluyendo el rol real)
            var userInfo = await GetUserInfoWithRoleAsync(refreshToken.UserId);
            if (userInfo == null)
                return null;
            
            return (refreshToken.UserId, refreshToken.UserType, userInfo.Value.negocioId, userInfo.Value.email, userInfo.Value.rol);
        }

        /// <summary>
        /// Revoca un refresh token específico estableciendo RevokedAt.
        /// </summary>
        public async Task<bool> RevokeRefreshTokenAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
                return false;
            
            // Hashear el token para buscar en la DB
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
            var tokenHash = Convert.ToBase64String(hashedBytes);
            
            var refreshToken = await Task.Run(() => 
                _context.RefreshTokens.FirstOrDefault(rt => rt.Token == tokenHash)
            );
            
            if (refreshToken == null || refreshToken.RevokedAt != null)
                return false;
            
            refreshToken.RevokedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            
            return true;
        }

        /// <summary>
        /// Revoca todos los refresh tokens activos de un usuario.
        /// Usado al cambiar contraseña para forzar logout de todas las sesiones.
        /// </summary>
        public async Task<int> RevokeAllUserTokensAsync(int userId)
        {
            var activeTokens = await Task.Run(() => 
                _context.RefreshTokens
                    .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
                    .ToList()
            );
            
            var now = DateTime.UtcNow;
            foreach (var token in activeTokens)
            {
                token.RevokedAt = now;
            }
            
            await _context.SaveChangesAsync();
            
            return activeTokens.Count;
        }

        /// <summary>
        /// Obtiene el email e id_negocio del usuario.
        /// </summary>
        public async Task<(string email, int negocioId)?> GetUserInfoAsync(int userId)
        {
            var usuario = await Task.Run(() => 
                _context.Usuarios.FirstOrDefault(u => u.Id == userId)
            );
            
            if (usuario != null)
                return (usuario.Email, usuario.Id_negocio);
            
            // Buscar en SuperAdmin
            var superAdmin = await Task.Run(() => 
                _context.SuperAdmins.FirstOrDefault(s => s.Id == userId)
            );
            
            if (superAdmin != null)
                return (superAdmin.Email, 0); // negocioId = 0 para SuperAdmin
            
            // Buscar en Cliente
            var cliente = await Task.Run(() => 
                _context.Clientes.FirstOrDefault(c => c.Id == userId)
            );
            
            if (cliente != null)
                return (cliente.Email ?? "", cliente.Id_negocio);
            
            return null;
        }

        /// <summary>
        /// Obtiene el email, id_negocio y rol real del usuario.
        /// Usado para generar access tokens desde refresh tokens.
        /// </summary>
        private async Task<(string email, int negocioId, string rol)?> GetUserInfoWithRoleAsync(int userId)
        {
            var usuario = await Task.Run(() => 
                _context.Usuarios.FirstOrDefault(u => u.Id == userId)
            );
            
            if (usuario != null)
            {
                // Mapear RolUsuario enum al string que espera el JWT
                var rol = usuario.Rol switch
                {
                    Enums.RolUsuario.Dueño => "Admin",
                    Enums.RolUsuario.Gerente => "Gerente",
                    Enums.RolUsuario.Empleado => "Vendedor",
                    _ => "Vendedor"
                };
                return (usuario.Email, usuario.Id_negocio, rol);
            }
            
            // Buscar en SuperAdmin
            var superAdmin = await Task.Run(() => 
                _context.SuperAdmins.FirstOrDefault(s => s.Id == userId)
            );
            
            if (superAdmin != null)
                return (superAdmin.Email, 0, "SuperAdmin");
            
            // Buscar en Cliente
            var cliente = await Task.Run(() => 
                _context.Clientes.FirstOrDefault(c => c.Id == userId)
            );
            
            if (cliente != null)
                return (cliente.Email ?? "", cliente.Id_negocio, "Cliente");
            
            return null;
        }
    }
}
