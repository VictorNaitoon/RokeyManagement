using API.Data;
using API.DTO.Request.Productos;
using API.DTO.Response.Productos;
using API.Models;
using API.Services.Common;
using Microsoft.EntityFrameworkCore;

namespace API.Services.Productos
{
    public class ProductoService : IProductoService
    {
        private readonly AppDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public ProductoService(AppDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<ProductoListResponse> GetAllAsync(string? busqueda = null)
        {
            var query = _context.Productos
                .Where(p => p.Id_negocio == _currentUser.NegocioId)
                .Include(p => p.Categoria)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                query = query.Where(p => 
                    p.Nombre.Contains(busqueda) || 
                    p.CodigoBusqueda.Contains(busqueda));
            }

            var productos = await query
                .Select(p => new ProductoResponse
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    CodigoBusqueda = p.CodigoBusqueda,
                    Descripcion = p.Descripcion,
                    PrecioCompra = p.PrecioCompra,
                    PrecioVenta = p.PrecioVenta,
                    StockActual = p.StockActual,
                    StockMinimo = p.StockMinimo,
                    ImagenURL = p.ImagenURL,
                    EsServicio = p.EsServicio,
                    Activo = p.Activo,
                    IdCategoria = p.IdCategoria,
                    NombreCategoria = p.Categoria != null ? p.Categoria.Nombre : null
                })
                .ToListAsync();

            return new ProductoListResponse
            {
                Productos = productos,
                Total = productos.Count
            };
        }

        public async Task<ProductoResponse?> GetByIdAsync(int id)
        {
            var producto = await _context.Productos
                .Where(p => p.Id == id && p.Id_negocio == _currentUser.NegocioId)
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync();

            if (producto == null) return null;

            return new ProductoResponse
            {
                Id = producto.Id,
                Nombre = producto.Nombre,
                CodigoBusqueda = producto.CodigoBusqueda,
                Descripcion = producto.Descripcion,
                PrecioCompra = _currentUser.IsAdmin ? producto.PrecioCompra : null,
                PrecioVenta = producto.PrecioVenta,
                StockActual = producto.StockActual,
                StockMinimo = producto.StockMinimo,
                ImagenURL = producto.ImagenURL,
                EsServicio = producto.EsServicio,
                Activo = producto.Activo,
                IdCategoria = producto.IdCategoria,
                NombreCategoria = producto.Categoria != null ? producto.Categoria.Nombre : null
            };
        }

        public async Task<ProductoResponse> CreateAsync(CrearProductoRequest request)
        {
            // Validar que la categoría exista y esté activa si se especifica
            if (request.IdCategoria.HasValue)
            {
                var categoria = await _context.Categorias
                    .FirstOrDefaultAsync(c => c.Id == request.IdCategoria.Value && 
                                            c.Id_negocio == _currentUser.NegocioId && 
                                            c.Activo);

                if (categoria == null)
                {
                    throw new InvalidOperationException("La categoría especificada no existe o está inactiva");
                }
            }

            // Validar unicidad de CodigoBusqueda si se especifica
            if (!string.IsNullOrWhiteSpace(request.CodigoBusqueda))
            {
                var existeCodigo = await _context.Productos
                    .AnyAsync(p => p.Id_negocio == _currentUser.NegocioId && 
                                   p.CodigoBusqueda == request.CodigoBusqueda);

                if (existeCodigo)
                {
                    throw new InvalidOperationException("Ya existe un producto con este código de búsqueda");
                }
            }

            var producto = new Producto
            {
                Id_negocio = _currentUser.NegocioId,
                IdUsuarioCreador = _currentUser.UserId,
                Nombre = request.Nombre,
                CodigoBusqueda = request.CodigoBusqueda,
                Descripcion = request.Descripcion,
                PrecioCompra = request.PrecioCompra,
                PrecioVenta = request.PrecioVenta,
                StockActual = request.StockActual,
                StockMinimo = request.StockMinimo,
                ImagenURL = request.ImagenURL,
                EsServicio = request.EsServicio,
                IdCategoria = request.IdCategoria,
                Activo = true
            };

            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(producto.Id);
        }

        public async Task<ProductoResponse?> UpdateAsync(int id, ActualizarProductoRequest request)
        {
            var producto = await _context.Productos
                .Where(p => p.Id == id && p.Id_negocio == _currentUser.NegocioId)
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync();

            if (producto == null) return null;

            // Validar que la categoría exista y esté activa si se especifica
            if (request.IdCategoria.HasValue)
            {
                var categoria = await _context.Categorias
                    .FirstOrDefaultAsync(c => c.Id == request.IdCategoria.Value && 
                                            c.Id_negocio == _currentUser.NegocioId && 
                                            c.Activo);

                if (categoria == null)
                {
                    throw new InvalidOperationException("La categoría especificada no existe o está inactiva");
                }
            }

            // Validar unicidad de CodigoBusqueda si se especifica y cambió
            if (!string.IsNullOrWhiteSpace(request.CodigoBusqueda) && 
                request.CodigoBusqueda != producto.CodigoBusqueda)
            {
                var existeCodigo = await _context.Productos
                    .AnyAsync(p => p.Id_negocio == _currentUser.NegocioId && 
                                   p.Id != id && 
                                   p.CodigoBusqueda == request.CodigoBusqueda);

                if (existeCodigo)
                {
                    throw new InvalidOperationException("Ya existe un producto con este código de búsqueda");
                }
            }

            producto.Nombre = request.Nombre;
            producto.CodigoBusqueda = request.CodigoBusqueda;
            producto.Descripcion = request.Descripcion;
            
            // Solo Admin puede modificar PrecioCompra
            if (_currentUser.IsAdmin && request.PrecioCompra.HasValue)
            {
                producto.PrecioCompra = request.PrecioCompra.Value;
            }
            
            producto.PrecioVenta = request.PrecioVenta;
            producto.StockActual = request.StockActual;
            producto.StockMinimo = request.StockMinimo;
            producto.ImagenURL = request.ImagenURL;
            producto.EsServicio = request.EsServicio;
            producto.Activo = request.Activo;
            producto.IdCategoria = request.IdCategoria;

            await _context.SaveChangesAsync();

            return await GetByIdAsync(producto.Id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var producto = await _context.Productos
                .FirstOrDefaultAsync(p => p.Id == id && p.Id_negocio == _currentUser.NegocioId);

            if (producto == null) return false;

            // Soft delete: desactivar en lugar de eliminar
            producto.Activo = false;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ActivarAsync(int id)
        {
            var producto = await _context.Productos
                .FirstOrDefaultAsync(p => p.Id == id && p.Id_negocio == _currentUser.NegocioId);

            if (producto == null) return false;

            // Reactivar el producto
            producto.Activo = true;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<ProductoResponse> DuplicateAsync(int id, string nuevoNombre)
        {
            var productoOriginal = await _context.Productos
                .Where(p => p.Id == id && p.Id_negocio == _currentUser.NegocioId)
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync();

            if (productoOriginal == null)
            {
                throw new InvalidOperationException("Producto no encontrado o no pertenece al negocio");
            }

            // Validar que el nuevo nombre no esté vacío
            if (string.IsNullOrWhiteSpace(nuevoNombre))
            {
                throw new InvalidOperationException("El nombre del producto duplicado no puede estar vacío");
            }

            var productoDuplicado = new Producto
            {
                Id_negocio = _currentUser.NegocioId,
                IdUsuarioCreador = _currentUser.UserId,
                Nombre = nuevoNombre.Trim(),
                CodigoBusqueda = null, // Se deja nulo para que el usuario lo asigne manualmente si lo desea
                Descripcion = productoOriginal.Descripcion,
                PrecioCompra = productoOriginal.PrecioCompra,
                PrecioVenta = productoOriginal.PrecioVenta,
                StockActual = productoOriginal.StockActual,
                StockMinimo = productoOriginal.StockMinimo,
                ImagenURL = productoOriginal.ImagenURL,
                EsServicio = productoOriginal.EsServicio,
                IdCategoria = productoOriginal.IdCategoria,
                Activo = true
            };

            _context.Productos.Add(productoDuplicado);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(productoDuplicado.Id);
        }
    }
}