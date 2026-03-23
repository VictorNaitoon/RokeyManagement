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
    }
}
