using API.Data;
using API.DTO.Request.Categorias;
using API.DTO.Response.Categorias;
using API.Models;
using API.Services.Common;
using Microsoft.EntityFrameworkCore;

namespace API.Services.Categorias
{
    public class CategoriaService : ICategoriaService
    {
        private readonly AppDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public CategoriaService(AppDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        /// <summary>
        /// Obtiene todas las categorías activas del negocio actual
        /// </summary>
        public async Task<CategoriaListResponse> GetAllAsync()
        {
            var categorias = await _context.Categorias
                .Where(c => c.Id_negocio == _currentUser.NegocioId && c.Activo)
                .Select(c => new CategoriaResponse
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    Descripcion = c.Descripcion,
                    Activo = c.Activo,
                    FechaAlta = DateTime.UtcNow
                })
                .ToListAsync();

            return new CategoriaListResponse
            {
                Categorias = categorias,
                Total = categorias.Count
            };
        }

        /// <summary>
        /// Obtiene una categoría por ID verificando pertenencia al negocio
        /// </summary>
        public async Task<CategoriaResponse?> GetByIdAsync(int id)
        {
            var categoria = await _context.Categorias
                .FirstOrDefaultAsync(c => c.Id == id && c.Id_negocio == _currentUser.NegocioId);

            if (categoria == null) return null;

            return new CategoriaResponse
            {
                Id = categoria.Id,
                Nombre = categoria.Nombre,
                Descripcion = categoria.Descripcion,
                Activo = categoria.Activo,
                FechaAlta = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Crea una nueva categoría para el negocio actual
        /// </summary>
        public async Task<CategoriaResponse> CreateAsync(CrearCategoriaRequest request)
        {
            // Validar que el nombre no exista en el mismo negocio
            var existingCategoria = await _context.Categorias
                .FirstOrDefaultAsync(c => c.Nombre == request.Nombre && c.Id_negocio == _currentUser.NegocioId);

            if (existingCategoria != null)
            {
                throw new InvalidOperationException("Ya existe una categoría con este nombre en este negocio");
            }

            var categoria = new Categoria
            {
                Id_negocio = _currentUser.NegocioId,
                IdUsuario = _currentUser.UserId,
                Nombre = request.Nombre,
                Descripcion = request.Descripcion,
                Activo = true
            };

            _context.Categorias.Add(categoria);
            await _context.SaveChangesAsync();

            return new CategoriaResponse
            {
                Id = categoria.Id,
                Nombre = categoria.Nombre,
                Descripcion = categoria.Descripcion,
                Activo = categoria.Activo,
                FechaAlta = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Actualiza una categoría existente verificando propiedad
        /// </summary>
        public async Task<CategoriaResponse?> UpdateAsync(int id, ActualizarCategoriaRequest request)
        {
            var categoria = await _context.Categorias
                .FirstOrDefaultAsync(c => c.Id == id && c.Id_negocio == _currentUser.NegocioId);

            if (categoria == null) return null;

            // Validar que el nuevo nombre no exista en otra categoría del mismo negocio
            var existingCategoria = await _context.Categorias
                .FirstOrDefaultAsync(c => c.Nombre == request.Nombre && c.Id_negocio == _currentUser.NegocioId && c.Id != id);

            if (existingCategoria != null)
            {
                throw new InvalidOperationException("Ya existe otra categoría con este nombre en este negocio");
            }

            categoria.Nombre = request.Nombre;
            categoria.Descripcion = request.Descripcion;
            categoria.Activo = request.Activo;

            await _context.SaveChangesAsync();

            return new CategoriaResponse
            {
                Id = categoria.Id,
                Nombre = categoria.Nombre,
                Descripcion = categoria.Descripcion,
                Activo = categoria.Activo,
                FechaAlta = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Realiza soft delete (desactiva) una categoría
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            var categoria = await _context.Categorias
                .FirstOrDefaultAsync(c => c.Id == id && c.Id_negocio == _currentUser.NegocioId);

            if (categoria == null) return false;

            // Soft delete: desactivar en lugar de eliminar
            categoria.Activo = false;
            await _context.SaveChangesAsync();

            return true;
        }
    }
}