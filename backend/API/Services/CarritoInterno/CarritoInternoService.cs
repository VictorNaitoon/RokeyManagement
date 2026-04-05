using API.Data;
using API.DTO.Request.CarritoInterno;
using API.DTO.Response.CarritoInterno;
using API.DTO.Response.Ventas;
using API.Models;
using API.Services.Common;
using API.Services.Ventas;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace API.Services.CarritoInterno
{
    public class CarritoInternoService : ICarritoInternoService
    {
        private readonly AppDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IVentaService _ventaService;
        private readonly ILogger<CarritoInternoService> _logger;

        private const int DefaultMaxActiveCarts = 5;

        public CarritoInternoService(
            AppDbContext context,
            ICurrentUserService currentUser,
            IVentaService ventaService,
            ILogger<CarritoInternoService> logger)
        {
            _context = context;
            _currentUser = currentUser;
            _ventaService = ventaService;
            _logger = logger;
        }

        public async Task<CarritoInternoResponse> CrearAsync(API.DTO.Request.CarritoInterno.CreateCarritoInternoRequest request, CancellationToken ct)
        {
            // 1. Validar negocio
            var negocio = await _context.Negocios.FindAsync(new object[] { _currentUser.NegocioId }, ct);
            if (negocio == null)
            {
                throw new InvalidOperationException("Negocio no encontrado");
            }

            // Nota: La verificación de Estado == Inactivo ahora se maneja centralmente en SubscriptionBlockingMiddleware

            // 2. Verificar límite de carritos activos
            var limiteCarritos = negocio.LimiteCarritosActivos ?? DefaultMaxActiveCarts;
            var carritosActivos = await _context.CarritosInternos
                .CountAsync(c => c.IdNegocio == _currentUser.NegocioId
                    && c.IdUsuario == _currentUser.UserId
                    && c.Estado == Enums.EstadoCarritoInterno.Activo, ct);

            if (carritosActivos >= limiteCarritos)
            {
                throw new InvalidOperationException(
                    $"Límite de carritos activos alcanzado. Máximo permitido: {limiteCarritos}");
            }

            // 3. Crear el carrito
            var carrito = new API.Models.CarritoInterno
            {
                IdNegocio = _currentUser.NegocioId,
                IdUsuario = _currentUser.UserId,
                Estado = Enums.EstadoCarritoInterno.Activo,
                Nombre = request.Nombre,
                FechaCreacion = DateTime.UtcNow,
                FechaActualizacion = DateTime.UtcNow
            };

            _context.CarritosInternos.Add(carrito);
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Carrito interno {CarritoId} creado por usuario {UsuarioId}",
                carrito.Id, _currentUser.UserId);

            return MapToResponse(carrito, new List<API.Models.CarritoInternoItem>());
        }

        public async Task<CarritoInternoResponse?> ObtenerPorIdAsync(int id, CancellationToken ct)
        {
            var carrito = await _context.CarritosInternos
                .Where(c => c.Id == id && c.IdNegocio == _currentUser.NegocioId)
                .Include(c => c.Items)
                    .ThenInclude(i => i.Producto)
                .FirstOrDefaultAsync(ct);

            if (carrito == null)
            {
                return null;
            }

            return MapToResponse(carrito, carrito.Items.ToList());
        }

        public async Task<List<CarritoInternoResponse>> ListarActivosAsync(CancellationToken ct)
        {
            var carritos = await _context.CarritosInternos
                .Where(c => c.IdNegocio == _currentUser.NegocioId
                    && c.IdUsuario == _currentUser.UserId
                    && c.Estado == Enums.EstadoCarritoInterno.Activo)
                .Include(c => c.Items)
                    .ThenInclude(i => i.Producto)
                .OrderByDescending(c => c.FechaActualizacion)
                .ToListAsync(ct);

            return carritos.Select(c => MapToResponse(c, c.Items.ToList())).ToList();
        }

        public async Task<CarritoInternoItemResponse> AgregarItemAsync(int carritoId, API.DTO.Request.CarritoInterno.AgregarItemRequest request, CancellationToken ct)
        {
            // 1. Obtener el carrito
            var carrito = await _context.CarritosInternos
                .Where(c => c.Id == carritoId && c.IdNegocio == _currentUser.NegocioId)
                .Include(c => c.Items)
                .FirstOrDefaultAsync(ct);

            if (carrito == null)
            {
                throw new InvalidOperationException("Carrito no encontrado");
            }

            if (carrito.Estado != Enums.EstadoCarritoInterno.Activo)
            {
                throw new InvalidOperationException("Solo se pueden agregar items a carritos activos");
            }

            // 2. Validar que el producto existe y pertenece al negocio
            var producto = await _context.Productos
                .FirstOrDefaultAsync(p => p.Id == request.IdProducto && p.Id_negocio == _currentUser.NegocioId && p.Activo, ct);

            if (producto == null)
            {
                throw new InvalidOperationException("Producto no encontrado o inactivo");
            }

            // 3. Validar stock (solo para productos que no son servicios)
            if (!producto.EsServicio && producto.StockActual < request.Cantidad)
            {
                throw new InvalidOperationException(
                    $"Stock insuficiente para el producto: {producto.Nombre}. Stock actual: {producto.StockActual}");
            }

            // 4. Verificar si el producto ya está en el carrito (actualizar cantidad)
            var itemExistente = carrito.Items.FirstOrDefault(i => i.IdProducto == request.IdProducto);
            if (itemExistente != null)
            {
                // Sumar la cantidad
                var nuevaCantidad = itemExistente.Cantidad + request.Cantidad;
                
                if (!producto.EsServicio && producto.StockActual < nuevaCantidad)
                {
                    throw new InvalidOperationException(
                        $"Stock insuficiente para el producto: {producto.Nombre}. Stock actual: {producto.StockActual}");
                }

                itemExistente.Cantidad = nuevaCantidad;
                itemExistente.PrecioUnitario = producto.PrecioVenta;
                itemExistente.Notas = request.Notas;
            }
            else
            {
                // 5. Crear nuevo item
                var item = new CarritoInternoItem
                {
                    CarritoInternoId = carritoId,
                    IdProducto = request.IdProducto,
                    Cantidad = request.Cantidad,
                    PrecioUnitario = producto.PrecioVenta, // Guardar precio histórico
                    Notas = request.Notas
                };

                _context.CarritosInternosItems.Add(item);
            }

            // 6. Actualizar fecha de modificación
            carrito.FechaActualizacion = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Item agregado al carrito {CarritoId}. Producto: {ProductoId}, Cantidad: {Cantidad}",
                carritoId, request.IdProducto, request.Cantidad);

            // Retornar el item creado o actualizado
            var itemActualizado = itemExistente ?? await _context.CarritosInternosItems
                .FirstOrDefaultAsync(i => i.CarritoInternoId == carritoId && i.IdProducto == request.IdProducto, ct);

            return MapItemToResponse(itemActualizado!, producto);
        }

        public async Task<CarritoInternoItemResponse> ActualizarItemAsync(int carritoId, int itemId, API.DTO.Request.CarritoInterno.UpdateItemRequest request, CancellationToken ct)
        {
            // 1. Obtener el carrito
            var carrito = await _context.CarritosInternos
                .Where(c => c.Id == carritoId && c.IdNegocio == _currentUser.NegocioId)
                .Include(c => c.Items)
                .FirstOrDefaultAsync(ct);

            if (carrito == null)
            {
                throw new InvalidOperationException("Carrito no encontrado");
            }

            if (carrito.Estado != Enums.EstadoCarritoInterno.Activo)
            {
                throw new InvalidOperationException("Solo se pueden modificar items de carritos activos");
            }

            // 2. Obtener el item
            var item = await _context.CarritosInternosItems
                .Include(i => i.Producto)
                .FirstOrDefaultAsync(i => i.Id == itemId && i.CarritoInternoId == carritoId, ct);

            if (item == null)
            {
                throw new InvalidOperationException("Item no encontrado");
            }

            // 3. Si cantidad es 0, eliminar el item
            if (request.Cantidad == 0)
            {
                _context.CarritosInternosItems.Remove(item);
            }
            else
            {
                // 4. Validar stock
                if (!item.Producto!.EsServicio && item.Producto.StockActual < request.Cantidad)
                {
                    throw new InvalidOperationException(
                        $"Stock insuficiente para el producto: {item.Producto.Nombre}. Stock actual: {item.Producto.StockActual}");
                }

                item.Cantidad = request.Cantidad;
            }

            // 5. Actualizar fecha de modificación
            carrito.FechaActualizacion = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Item {ItemId} actualizado en carrito {CarritoId}. Nueva cantidad: {Cantidad}",
                itemId, carritoId, request.Cantidad);

            // Retornar el item (o el primero si se eliminó)
            if (request.Cantidad == 0)
            {
                var primerItem = await _context.CarritosInternosItems
                    .Include(i => i.Producto)
                    .FirstOrDefaultAsync(i => i.CarritoInternoId == carritoId, ct);
                
                if (primerItem != null)
                {
                    return MapItemToResponse(primerItem, primerItem.Producto!);
                }
                
                // Si no hay más items, retornar respuesta vacía
                return new CarritoInternoItemResponse { Id = itemId };
            }

            // Recargar el producto para la respuesta
            await _context.Entry(item).Reference(i => i.Producto).LoadAsync(ct);
            return MapItemToResponse(item, item.Producto!);
        }

        public async Task EliminarItemAsync(int carritoId, int itemId, CancellationToken ct)
        {
            // 1. Obtener el carrito
            var carrito = await _context.CarritosInternos
                .Where(c => c.Id == carritoId && c.IdNegocio == _currentUser.NegocioId)
                .FirstOrDefaultAsync(ct);

            if (carrito == null)
            {
                throw new InvalidOperationException("Carrito no encontrado");
            }

            if (carrito.Estado != Enums.EstadoCarritoInterno.Activo)
            {
                throw new InvalidOperationException("Solo se pueden eliminar items de carritos activos");
            }

            // 2. Obtener el item
            var item = await _context.CarritosInternosItems
                .FirstOrDefaultAsync(i => i.Id == itemId && i.CarritoInternoId == carritoId, ct);

            if (item == null)
            {
                throw new InvalidOperationException("Item no encontrado");
            }

            // 3. Eliminar el item
            _context.CarritosInternosItems.Remove(item);

            // 4. Actualizar fecha de modificación
            carrito.FechaActualizacion = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Item {ItemId} eliminado del carrito {CarritoId}", itemId, carritoId);
        }

        public async Task EliminarAsync(int carritoId, CancellationToken ct)
        {
            // 1. Obtener el carrito
            var carrito = await _context.CarritosInternos
                .Where(c => c.Id == carritoId && c.IdNegocio == _currentUser.NegocioId)
                .FirstOrDefaultAsync(ct);

            if (carrito == null)
            {
                throw new InvalidOperationException("Carrito no encontrado");
            }

            if (carrito.Estado == Enums.EstadoCarritoInterno.Convertido)
            {
                throw new InvalidOperationException("No se puede eliminar un carrito convertido");
            }

            // 2. Eliminar el carrito (cascade elimina los items)
            _context.CarritosInternos.Remove(carrito);
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Carrito {CarritoId} eliminado por usuario {UsuarioId}", carritoId, _currentUser.UserId);
        }

        public async Task<VentaResponse> ConvertirAsync(int carritoId, API.DTO.Request.CarritoInterno.ConvertirCarritoRequest request, CancellationToken ct)
        {
            // 1. Obtener el carrito con todos sus items
            var carrito = await _context.CarritosInternos
                .Where(c => c.Id == carritoId && c.IdNegocio == _currentUser.NegocioId)
                .Include(c => c.Items)
                    .ThenInclude(i => i.Producto)
                .FirstOrDefaultAsync(ct);

            if (carrito == null)
            {
                throw new InvalidOperationException("Carrito no encontrado");
            }

            if (carrito.Estado != Enums.EstadoCarritoInterno.Activo)
            {
                throw new InvalidOperationException("Solo se pueden convertir carritos activos");
            }

            if (!carrito.Items.Any())
            {
                throw new InvalidOperationException("No se puede convertir un carrito vacío");
            }

            // 2. Re-validar stock para todos los productos
            foreach (var item in carrito.Items)
            {
                if (!item.Producto!.EsServicio && item.Producto.StockActual < item.Cantidad)
                {
                    throw new InvalidOperationException(
                        $"Stock insuficiente para el producto: {item.Producto.Nombre}. Stock actual: {item.Producto.StockActual}");
                }
            }

            // 3. Mapear a CrearVentaRequest
            var detalles = carrito.Items.Select(i => new DTO.Request.Ventas.DetalleVentaRequest
            {
                IdProducto = i.IdProducto,
                Cantidad = i.Cantidad,
                PrecioUnitario = i.PrecioUnitario
            }).ToList();

            var total = detalles.Sum(d => d.Cantidad * d.PrecioUnitario);

            var ventaRequest = new DTO.Request.Ventas.CrearVentaRequest
            {
                IdCliente = request.IdCliente,
                Detalles = detalles,
                Pagos = new List<DTO.Request.Ventas.PagoRequest>
                {
                    new DTO.Request.Ventas.PagoRequest
                    {
                        MetodoPago = request.FormaPago,
                        Monto = total
                    }
                }
            };

            // 4. Crear la venta usando VentaService
            var venta = await _ventaService.CrearVentaAsync(ventaRequest, ct);

            // 5. Marcar el carrito como convertido
            carrito.Estado = Enums.EstadoCarritoInterno.Convertido;
            carrito.FechaActualizacion = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Carrito {CarritoId} convertido a venta {VentaId}", carritoId, venta.Id);

            return venta;
        }

        private static API.DTO.Response.CarritoInterno.CarritoInternoResponse MapToResponse(API.Models.CarritoInterno carrito, List<API.Models.CarritoInternoItem> items)
        {
            return new API.DTO.Response.CarritoInterno.CarritoInternoResponse
            {
                Id = carrito.Id,
                IdNegocio = carrito.IdNegocio,
                IdUsuario = carrito.IdUsuario,
                Nombre = carrito.Nombre,
                Estado = carrito.Estado,
                FechaCreacion = carrito.FechaCreacion,
                FechaActualizacion = carrito.FechaActualizacion,
                Items = items.Select(i => MapItemToResponse(i, i.Producto!)).ToList()
            };
        }

        private static API.DTO.Response.CarritoInterno.CarritoInternoItemResponse MapItemToResponse(API.Models.CarritoInternoItem item, API.Models.Producto producto)
        {
            return new API.DTO.Response.CarritoInterno.CarritoInternoItemResponse
            {
                Id = item.Id,
                CarritoInternoId = item.CarritoInternoId,
                IdProducto = item.IdProducto,
                NombreProducto = producto.Nombre,
                Cantidad = item.Cantidad,
                PrecioUnitario = item.PrecioUnitario,
                Notas = item.Notas
            };
        }
    }
}
