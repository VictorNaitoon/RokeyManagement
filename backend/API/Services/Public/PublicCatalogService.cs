using API.Data;
using API.DTO.Response.Public;
using Microsoft.EntityFrameworkCore;

namespace API.Services.Publicos
{
    /// <summary>
    /// Implementación del servicio de catálogo público de productos
    /// </summary>
    public class CatalogoPublicoService : ICatalogoPublicoService
    {
        private readonly AppDbContext _context;

        public CatalogoPublicoService(AppDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc/>
        public async Task<ProductoPublicResponse[]> GetCatalogoAsync(int idNegocio)
        {
            // Solo productos visibles (Activo = true) y con stock
            var productos = await _context.Productos
                .Where(p => p.Id_negocio == idNegocio 
                    && p.Activo == true 
                    && p.StockActual > 0)
                .Select(p => new ProductoPublicResponse(
                    p.Id,
                    p.Nombre,
                    p.Descripcion,
                    p.PrecioVenta,
                    p.ImagenURL,
                    p.StockActual
                ))
                .ToArrayAsync();

            return productos;
        }

        /// <inheritdoc/>
        public async Task<ProductoPublicResponse?> GetProductoAsync(int idNegocio, int productoId)
        {
            var producto = await _context.Productos
                .Where(p => p.Id == productoId 
                    && p.Id_negocio == idNegocio 
                    && p.Activo == true 
                    && p.StockActual > 0)
                .Select(p => new ProductoPublicResponse(
                    p.Id,
                    p.Nombre,
                    p.Descripcion,
                    p.PrecioVenta,
                    p.ImagenURL,
                    p.StockActual
                ))
                .FirstOrDefaultAsync();

            return producto;
        }
    }
}