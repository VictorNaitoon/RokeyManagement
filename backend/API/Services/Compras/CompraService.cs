using API.Data;
using API.DTO.Request.Compras;
using API.DTO.Response.Compras;
using API.Models;
using API.Services.Auditoria;
using API.Services.Common;
using API.Services.Caja;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace API.Services.Compras
{
    public class CompraService : ICompraService
    {
        private readonly AppDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly ICajaService _cajaService;
        private readonly IAuditoriaService _auditoriaService;
        private readonly ILogger<CompraService> _logger;

        public CompraService(AppDbContext context, ICurrentUserService currentUser, ICajaService cajaService, IAuditoriaService auditoriaService, ILogger<CompraService> logger)
        {
            _context = context;
            _currentUser = currentUser;
            _cajaService = cajaService;
            _auditoriaService = auditoriaService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todas las compras del negocio actual
        /// </summary>
        public async Task<CompraListResponse> GetAllAsync()
        {
            var compras = await _context.Compras
                .Where(c => c.Id_negocio == _currentUser.NegocioId)
                .Include(c => c.Proveedor)
                .Include(c => c.DetallesCompra)
                    .ThenInclude(d => d.Producto)
                .OrderByDescending(c => c.FechaCompra)
                .Select(c => new CompraResponse
                {
                    Id = c.Id,
                    NumeroComprobante = c.NumeroComprobante ?? string.Empty,
                    IdProveedor = c.IdProveedor,
                    NombreProveedor = c.Proveedor != null ? c.Proveedor.Nombre : null,
                    FechaCompra = c.FechaCompra,
                    TotalGasto = c.TotalGasto,
                    Anulada = c.Anulada,
                    MotivoAnulacion = c.MotivoAnulacion,
                    Detalles = c.DetallesCompra.Select(d => new DetalleCompraResponse
                    {
                        Id = d.Id,
                        IdProducto = d.IdProducto,
                        NombreProducto = d.Producto != null ? d.Producto.Nombre : null,
                        Cantidad = d.Cantidad,
                        PrecioUnitario = d.PrecioUnitario
                    }).ToList()
                })
                .ToListAsync();

            return new CompraListResponse
            {
                Compras = compras,
                Total = compras.Count
            };
        }

        /// <summary>
        /// Obtiene una compra por ID verificando pertenencia al negocio
        /// </summary>
        public async Task<CompraResponse?> GetByIdAsync(int id)
        {
            var compra = await _context.Compras
                .Include(c => c.Proveedor)
                .Include(c => c.DetallesCompra)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(c => c.Id == id && c.Id_negocio == _currentUser.NegocioId);

            if (compra == null) return null;

            return new CompraResponse
            {
                Id = compra.Id,
                NumeroComprobante = compra.NumeroComprobante ?? string.Empty,
                IdProveedor = compra.IdProveedor,
                NombreProveedor = compra.Proveedor?.Nombre,
                FechaCompra = compra.FechaCompra,
                TotalGasto = compra.TotalGasto,
                Anulada = compra.Anulada,
                MotivoAnulacion = compra.MotivoAnulacion,
                Detalles = compra.DetallesCompra.Select(d => new DetalleCompraResponse
                {
                    Id = d.Id,
                    IdProducto = d.IdProducto,
                    NombreProducto = d.Producto?.Nombre,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario
                }).ToList()
            };
        }

        /// <summary>
        /// Crea una nueva compra y actualiza el stock de los productos
        /// </summary>
        public async Task<CompraResponse> CreateAsync(CrearCompraRequest request)
        {
            // 1. Validar que el negocio existe y está activo
            var negocio = await _context.Negocios.FindAsync(new object[] { _currentUser.NegocioId });
            if (negocio == null)
            {
                throw new InvalidOperationException("Negocio no encontrado");
            }

            // Nota: La verificación de Estado == Inactivo ahora se maneja centralmente en SubscriptionBlockingMiddleware

            // 2. Validar que el proveedor existe y pertenece al negocio
            var proveedor = await _context.Proveedores
                .FirstOrDefaultAsync(p => p.Id == request.IdProveedor && p.Id_negocio == _currentUser.NegocioId);

            if (proveedor == null)
            {
                throw new InvalidOperationException("El proveedor no existe o no pertenece a este negocio");
            }

            // 3. Validar que todos los productos existen y pertenecen al mismo negocio
            var productoIds = request.Detalles.Select(d => d.IdProducto).Distinct().ToList();
            var productos = await _context.Productos
                .Where(p => productoIds.Contains(p.Id) && p.Id_negocio == _currentUser.NegocioId && p.Activo)
                .ToDictionaryAsync(p => p.Id);

            if (productos.Count != productoIds.Count)
            {
                throw new InvalidOperationException("Uno o más productos no existen, no pertenecen al negocio o están inactivos");
            }

            // 4. Calcular total de la compra
            var totalGasto = request.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario);

            // 5. Usar transacción para atomicidad
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 6. Crear la compra
                var compra = new Compra
                {
                    Id_negocio = _currentUser.NegocioId,
                    IdProveedor = request.IdProveedor,
                    IdUsuario = _currentUser.UserId,
                    FechaCompra = DateTime.UtcNow,
                    TotalGasto = totalGasto,
                    NumeroComprobante = request.NumeroComprobante,
                    Anulada = false
                };

                _context.Compras.Add(compra);
                await _context.SaveChangesAsync();

                // 7. Crear detalles de compra y actualizar stock
                foreach (var detalle in request.Detalles)
                {
                    var detalleCompra = new DetalleCompra
                    {
                        IdCompra = compra.Id,
                        IdProducto = detalle.IdProducto,
                        Cantidad = detalle.Cantidad,
                        PrecioUnitario = detalle.PrecioUnitario
                    };

                    _context.DetallesCompra.Add(detalleCompra);

                    // 8. Actualizar stock del producto (incrementar)
                    var producto = productos[detalle.IdProducto];
                    var stockAnterior = producto.StockActual;
                    producto.StockActual += detalle.Cantidad;

                    // 9. Crear MovimientoStock tipo CompraEntrada
                    var movimientoStock = new MovimientoStock
                    {
                        Id_negocio = _currentUser.NegocioId,
                        IdProducto = producto.Id,
                        IdUsuario = _currentUser.UserId,
                        IdCompra = compra.Id,
                        FechaMovimiento = DateTime.UtcNow,
                        Cantidad = detalle.Cantidad,
                        TipoMovimiento = Enums.TipoMovimiento.CompraEntrada,
                        StockAnterior = stockAnterior,
                        StockNuevo = producto.StockActual,
                        Motivo = $"Compra #{compra.Id}"
                    };

                    _context.MovimientosStock.Add(movimientoStock);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // 11. Registrar movimiento de caja automático (Egreso)
                try
                {
                    await _cajaService.AgregarMovimientoAsync(
                        new API.DTO.Request.Caja.AgregarMovimientoCajaRequest
                        {
                            Tipo = "Egreso",
                            Monto = totalGasto,
                            Descripcion = $"Compra #{compra.Id}"
                        },
                        _currentUser.UserId,
                        _currentUser.NegocioId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "No se pudo registrar movimiento de caja para la compra {CompraId}", compra.Id);
                }

                _logger.LogInformation("Compra {CompraId} creada por usuario {UserId} en negocio {NegocioId}", 
                    compra.Id, _currentUser.UserId, _currentUser.NegocioId);

                // 12. Registrar auditoría
                await _auditoriaService.RegistrarAsync(
                    "Compra",
                    compra.Id,
                    "CREATE",
                    null,
                    new
                    {
                        compra.TotalGasto,
                        compra.IdProveedor,
                        Estado = "Activa"
                    });

                // 13. Retornar la compra creada
                return await GetByIdAsync(compra.Id) ?? throw new InvalidOperationException("Error al obtener la compra creada");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al crear compra");
                throw;
            }
        }

        /// <summary>
        /// Anula una compra y revierte el stock de los productos
        /// </summary>
        public async Task<CompraResponse> AnularAsync(int id, AnularCompraRequest? request)
        {
            // 1. Validar que la compra existe y pertenece al negocio
            var compra = await _context.Compras
                .Include(c => c.DetallesCompra)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(c => c.Id == id && c.Id_negocio == _currentUser.NegocioId);

            if (compra == null)
            {
                throw new InvalidOperationException("Compra no encontrada");
            }

            // 2. Validar que la compra no está anulada
            if (compra.Anulada)
            {
                throw new InvalidOperationException("La compra ya está anulada");
            }

            // 3. Capturar estado antes de modificar para auditoría
            var datosAnteriores = new
            {
                compra.TotalGasto,
                compra.IdProveedor,
                Estado = "Activa"
            };

            // 4. Usar transacción para atomicidad
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 5. Revertir stock de cada detalle (decrementar)
                foreach (var detalle in compra.DetallesCompra)
                {
                    if (detalle.Producto != null)
                    {
                        var stockAnterior = detalle.Producto.StockActual;
                        detalle.Producto.StockActual -= detalle.Cantidad;

                        // 5. Crear MovimientoStock tipo CompraAnulacion
                        var movimientoStock = new MovimientoStock
                        {
                            Id_negocio = _currentUser.NegocioId,
                            IdProducto = detalle.Producto.Id,
                            IdUsuario = _currentUser.UserId,
                            IdCompra = compra.Id,
                            FechaMovimiento = DateTime.UtcNow,
                            Cantidad = -detalle.Cantidad, // Negativo para indicar reversión
                            TipoMovimiento = Enums.TipoMovimiento.CompraAnulacion,
                            StockAnterior = stockAnterior,
                            StockNuevo = detalle.Producto.StockActual,
                            Motivo = $"Anulación Compra #{compra.Id}"
                        };

                        _context.MovimientosStock.Add(movimientoStock);
                    }
                }

                // 7. Marcar compra como anulada
                compra.Anulada = true;
                compra.MotivoAnulacion = request?.Motivo;
                compra.IdUsuarioModificador = _currentUser.UserId;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // 9. Registrar auditoría
                await _auditoriaService.RegistrarAsync(
                    "Compra",
                    compra.Id,
                    "UPDATE",
                    datosAnteriores,
                    new
                    {
                        compra.TotalGasto,
                        compra.IdProveedor,
                        Estado = "Anulada"
                    });

                // 10. Registrar movimiento de caja automático (Ingreso por anulación)
                try
                {
                    await _cajaService.AgregarMovimientoAsync(
                        new API.DTO.Request.Caja.AgregarMovimientoCajaRequest
                        {
                            Tipo = "Ingreso",
                            Monto = compra.TotalGasto,
                            Descripcion = $"Anulación de Compra #{compra.Id}"
                        },
                        _currentUser.UserId,
                        _currentUser.NegocioId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "No se pudo registrar movimiento de caja por anulación de compra {CompraId}", compra.Id);
                }

                _logger.LogInformation("Compra {CompraId} anulada por usuario {UserId} en negocio {NegocioId}", 
                    id, _currentUser.UserId, _currentUser.NegocioId);

                // 7. Retornar la compra anulada
                return await GetByIdAsync(id) ?? throw new InvalidOperationException("Error al obtener la compra anulada");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al anular compra {CompraId}", id);
                throw;
            }
        }
    }
}