using API.Data;
using API.DTO.Request.Proveedores;
using API.DTO.Response.Proveedores;
using API.Models;
using API.Services.Common;
using Microsoft.EntityFrameworkCore;

namespace API.Services.Proveedores
{
    public class ProveedorService : IProveedorService
    {
        private readonly AppDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public ProveedorService(AppDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        /// <summary>
        /// Obtiene todos los proveedores del negocio actual
        /// </summary>
        public async Task<ProveedorListResponse> GetAllAsync()
        {
            var proveedores = await _context.Proveedores
                .Where(p => p.Id_negocio == _currentUser.NegocioId)
                .OrderBy(p => p.Nombre)
                .Select(p => new ProveedorResponse
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Telefono = p.Telefono,
                    Email = p.Email,
                    FechaAlta = DateTime.UtcNow
                })
                .ToListAsync();

            return new ProveedorListResponse
            {
                Proveedores = proveedores,
                Total = proveedores.Count
            };
        }

        /// <summary>
        /// Obtiene un proveedor por ID verificando pertenencia al negocio
        /// </summary>
        public async Task<ProveedorResponse?> GetByIdAsync(int id)
        {
            var proveedor = await _context.Proveedores
                .FirstOrDefaultAsync(p => p.Id == id && p.Id_negocio == _currentUser.NegocioId);

            if (proveedor == null) return null;

            return new ProveedorResponse
            {
                Id = proveedor.Id,
                Nombre = proveedor.Nombre,
                Telefono = proveedor.Telefono,
                Email = proveedor.Email,
                FechaAlta = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Crea un nuevo proveedor para el negocio actual
        /// </summary>
        public async Task<ProveedorResponse> CreateAsync(CrearProveedorRequest request)
        {
            // Validar que no exista otro proveedor con el mismo nombre en el mismo negocio
            var existingProveedor = await _context.Proveedores
                .FirstOrDefaultAsync(p => p.Nombre == request.Nombre && p.Id_negocio == _currentUser.NegocioId);

            if (existingProveedor != null)
            {
                throw new InvalidOperationException("Ya existe un proveedor con este nombre en este negocio.");
            }

            var proveedor = new Proveedor
            {
                Id_negocio = _currentUser.NegocioId,
                Nombre = request.Nombre,
                Telefono = request.Telefono,
                Email = request.Email
            };

            _context.Proveedores.Add(proveedor);
            await _context.SaveChangesAsync();

            return new ProveedorResponse
            {
                Id = proveedor.Id,
                Nombre = proveedor.Nombre,
                Telefono = proveedor.Telefono,
                Email = proveedor.Email,
                FechaAlta = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Actualiza un proveedor existente verificando propiedad
        /// </summary>
        public async Task<ProveedorResponse?> UpdateAsync(int id, ActualizarProveedorRequest request)
        {
            var proveedor = await _context.Proveedores
                .FirstOrDefaultAsync(p => p.Id == id && p.Id_negocio == _currentUser.NegocioId);

            if (proveedor == null) return null;

            // Validar que no exista otro proveedor con el mismo nombre (excluyendo el actual)
            var existingProveedor = await _context.Proveedores
                .FirstOrDefaultAsync(p => p.Nombre == request.Nombre && p.Id_negocio == _currentUser.NegocioId && p.Id != id);

            if (existingProveedor != null)
            {
                throw new InvalidOperationException("Ya existe otro proveedor con este nombre en este negocio.");
            }

            proveedor.Nombre = request.Nombre;
            proveedor.Telefono = request.Telefono;
            proveedor.Email = request.Email;

            await _context.SaveChangesAsync();

            return new ProveedorResponse
            {
                Id = proveedor.Id,
                Nombre = proveedor.Nombre,
                Telefono = proveedor.Telefono,
                Email = proveedor.Email,
                FechaAlta = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Elimina un proveedor (hard delete)
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            var proveedor = await _context.Proveedores
                .FirstOrDefaultAsync(p => p.Id == id && p.Id_negocio == _currentUser.NegocioId);

            if (proveedor == null) return false;

            _context.Proveedores.Remove(proveedor);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}