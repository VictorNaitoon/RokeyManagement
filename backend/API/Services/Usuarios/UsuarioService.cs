using API.Data;
using API.DTO.Request.Usuarios;
using API.DTO.Response.Usuarios;
using API.Models;
using API.Services.Common;
using Microsoft.EntityFrameworkCore;

namespace API.Services.Usuarios
{
    public interface IUsuarioService
    {
        Task<UsuarioListResponse> GetAllAsync();
        Task<UsuarioResponse?> GetByIdAsync(int id);
        Task<UsuarioResponse> CreateAsync(CrearUsuarioRequest request);
        Task<UsuarioResponse?> UpdateAsync(int id, ActualizarUsuarioRequest request);
        Task<bool> DeleteAsync(int id);
        Task<bool> CambiarPasswordAsync(CambiarPasswordRequest request);
    }

    public class UsuarioService : IUsuarioService
    {
        private readonly AppDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public UsuarioService(AppDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<UsuarioListResponse> GetAllAsync()
        {
            List<UsuarioResponse> usuarios;

            // Si es SuperAdmin (NegocioId = 0), mostrar TODOS los usuarios de todos los negocios
            if (_currentUser.NegocioId == 0 && _currentUser.Rol == "SuperAdmin")
            {
                usuarios = await _context.Usuarios
                    .Where(u => u.Activo)
                    .Select(u => new UsuarioResponse
                    {
                        Id = u.Id,
                        Email = u.Email,
                        Nombre = u.Nombre,
                        Apellido = u.Apellido,
                        Rol = u.Rol.ToString(),
                        Activo = u.Activo,
                        FechaAlta = DateTime.UtcNow
                    })
                    .ToListAsync();
            }
            else
            {
                // Usuario normal: solo los de su negocio
                usuarios = await _context.Usuarios
                    .Where(u => u.Id_negocio == _currentUser.NegocioId && u.Activo)
                    .Select(u => new UsuarioResponse
                    {
                        Id = u.Id,
                        Email = u.Email,
                        Nombre = u.Nombre,
                        Apellido = u.Apellido,
                        Rol = u.Rol.ToString(),
                        Activo = u.Activo,
                        FechaAlta = DateTime.UtcNow
                    })
                    .ToListAsync();
            }

            return new UsuarioListResponse
            {
                Usuarios = usuarios,
                Total = usuarios.Count
            };
        }

        public async Task<UsuarioResponse?> GetByIdAsync(int id)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == id);

            if (usuario == null) return null;

            // Si es SuperAdmin, puede ver cualquier usuario
            // Si es usuario normal, solo puede ver usuarios de su negocio
            if (_currentUser.NegocioId != 0 && usuario.Id_negocio != _currentUser.NegocioId)
            {
                return null;
            }

            return new UsuarioResponse
            {
                Id = usuario.Id,
                Email = usuario.Email,
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                Rol = usuario.Rol.ToString(),
                Activo = usuario.Activo,
                FechaAlta = DateTime.Now // Simplified - could be from creation date
            };
        }

        public async Task<UsuarioResponse> CreateAsync(CrearUsuarioRequest request)
        {
            // El Super Admin no puede crear usuarios porque no pertenece a un negocio específico
            if (_currentUser.NegocioId == 0 && _currentUser.Rol == "SuperAdmin")
            {
                throw new InvalidOperationException("El Super Admin no puede crear usuarios. Contacte al administrador del negocio.");
            }

            // Verificar que el email no exista en el mismo negocio
            var existingUser = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == request.Email && u.Id_negocio == _currentUser.NegocioId);

            if (existingUser != null)
            {
                throw new InvalidOperationException("Ya existe un usuario con este email en este negocio");
            }

            var passwordHash = Auth.AuthService.HashPassword(request.Password);

            var usuario = new Usuario
            {
                Id_negocio = _currentUser.NegocioId,
                Email = request.Email,
                PasswordHash = passwordHash,
                Nombre = request.Nombre,
                Apellido = request.Apellido,
                Rol = (Enums.RolUsuario)request.Rol,
                Activo = true
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return new UsuarioResponse
            {
                Id = usuario.Id,
                Email = usuario.Email,
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                Rol = usuario.Rol.ToString(),
                Activo = usuario.Activo,
                FechaAlta = DateTime.Now
            };
        }

        public async Task<UsuarioResponse?> UpdateAsync(int id, ActualizarUsuarioRequest request)
        {
            // El Super Admin no puede actualizar usuarios porque no pertenece a un negocio específico
            if (_currentUser.NegocioId == 0 && _currentUser.Rol == "SuperAdmin")
            {
                throw new InvalidOperationException("El Super Admin no puede modificar usuarios. Contacte al administrador del negocio.");
            }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == id && u.Id_negocio == _currentUser.NegocioId);

            if (usuario == null) return null;

            usuario.Nombre = request.Nombre;
            usuario.Apellido = request.Apellido;
            usuario.Rol = (Enums.RolUsuario)request.Rol;
            usuario.Activo = request.Activo;

            await _context.SaveChangesAsync();

            return new UsuarioResponse
            {
                Id = usuario.Id,
                Email = usuario.Email,
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                Rol = usuario.Rol.ToString(),
                Activo = usuario.Activo,
                FechaAlta = DateTime.Now
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            // El Super Admin no puede eliminar usuarios porque no pertenece a un negocio específico
            if (_currentUser.NegocioId == 0 && _currentUser.Rol == "SuperAdmin")
            {
                throw new InvalidOperationException("El Super Admin no puede eliminar usuarios. Contacte al administrador del negocio.");
            }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == id && u.Id_negocio == _currentUser.NegocioId);

            if (usuario == null) return false;

            // No eliminar, solo desactivar (soft delete)
            usuario.Activo = false;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> CambiarPasswordAsync(CambiarPasswordRequest request)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == _currentUser.UserId && u.Id_negocio == _currentUser.NegocioId);

            if (usuario == null)
            {
                throw new InvalidOperationException("Usuario no encontrado");
            }

            // Verificar password actual
            if (!Auth.AuthService.VerifyPassword(request.PasswordActual, usuario.PasswordHash))
            {
                throw new InvalidOperationException("La contraseña actual es incorrecta");
            }

            // Actualizar password
            usuario.PasswordHash = Auth.AuthService.HashPassword(request.PasswordNuevo);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}