using API.Data;
using API.DTO.Request.Ventas;
using API.DTO.Response.Ventas;
using API.Models;
using API.Services.Caja;
using API.Services.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace API.Services.Ventas
{
    public class VentaService : IVentaService
    {
        private readonly AppDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly ICajaService _cajaService;
        private readonly ILogger<VentaService> _logger;

        public VentaService(AppDbContext context, ICurrentUserService currentUser, ICajaService cajaService, ILogger<VentaService> logger)
        {
            _context = context;
            _currentUser = currentUser;
            _cajaService = cajaService;
            _logger = logger;
        }

        public async Task<VentaResponse> CrearVentaAsync(CrearVentaRequest request, CancellationToken ct)
        {
            // 1. Validar que el negocio existe y está activo
            var negocio = await _context.Negocios.FindAsync(new object[] { _currentUser.NegocioId }, ct);
            if (negocio == null)
            {
                throw new InvalidOperationException("Negocio no encontrado");
            }

            if (negocio.Estado == Enums.EstadoNegocio.Inactivo)
            {
                throw new InvalidOperationException("El negocio se encuentra inactivo");
            }

            // 1.5. Validar que hay una caja abierta
            if (!await _cajaService.TieneCajaAbiertaAsync(_currentUser.NegocioId, ct))
            {
                throw new InvalidOperationException("No hay una caja abierta. Abra una caja antes de registrar ventas.");
            }

            // 2. Validar que todos los productos existen y pertenecen al mismo negocio
            var productoIds = request.Detalles.Select(d => d.IdProducto).Distinct().ToList();
            var productos = await _context.Productos
                .Where(p => productoIds.Contains(p.Id) && p.Id_negocio == _currentUser.NegocioId && p.Activo)
                .ToDictionaryAsync(p => p.Id, ct);

            if (productos.Count != productoIds.Count)
            {
                throw new InvalidOperationException("Uno o más productos no existen o no pertenecen al negocio");
            }

            // 3. Validar stock para productos que no son servicios
            foreach (var detalle in request.Detalles)
            {
                var producto = productos[detalle.IdProducto];
                if (!producto.EsServicio && producto.StockActual < detalle.Cantidad)
                {
                    throw new InvalidOperationException($"Stock insuficiente para el producto: {producto.Nombre}. Stock actual: {producto.StockActual}");
                }
            }

            // 4. Calcular total de la venta
            var totalVenta = request.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario);

            // 5. Validar suma de pagos
            var sumaPagos = request.Pagos.Sum(p => p.Monto);
            if (sumaPagos != totalVenta)
            {
                throw new InvalidOperationException("La suma de pagos debe coincidir con el total de la venta");
            }

            // 6. Si no hay cliente, usar "Consumidor Final"
            int idCliente;
            if (request.IdCliente.HasValue && request.IdCliente.Value > 0)
            {
                idCliente = request.IdCliente.Value;
            }
            else
            {
                // Buscar o crear cliente "Consumidor Final" para este negocio
                var consumidorFinal = await _context.Clientes
                    .FirstOrDefaultAsync(c => c.Id_negocio == _currentUser.NegocioId && c.Nombre == "Consumidor Final", ct);

                if (consumidorFinal == null)
                {
                    consumidorFinal = new Cliente
                    {
                        Id_negocio = _currentUser.NegocioId,
                        Nombre = "Consumidor Final",
                        Apellido = "",
                        Documento = "0",
                        CondicionIVA = "Consumidor Final",
                        Telefono = "",
                        Email = "",
                        Direccion = "",
                        FechaAlta = DateTime.UtcNow
                    };
                    _context.Clientes.Add(consumidorFinal);
                    await _context.SaveChangesAsync(ct);
                    _logger.LogInformation("Cliente 'Consumidor Final' creado para el negocio {NegocioId}", _currentUser.NegocioId);
                }
                idCliente = consumidorFinal.Id;
            }

            // 7. Usar transacción para atomicidad
            using var transaction = await _context.Database.BeginTransactionAsync(ct);
            try
            {
                // 8. Crear la venta
                var venta = new Venta
                {
                    Id_negocio = _currentUser.NegocioId,
                    IdUsuario = _currentUser.UserId,
                    IdCliente = idCliente,
                    FechaVenta = DateTime.UtcNow,
                    TotalVenta = totalVenta
                };

                _context.Ventas.Add(venta);
                await _context.SaveChangesAsync(ct);

                // 8. Crear detalles de venta
                foreach (var detalle in request.Detalles)
                {
                    var detalleVenta = new DetalleVenta
                    {
                        IdVenta = venta.Id,
                        IdProducto = detalle.IdProducto,
                        Cantidad = detalle.Cantidad,
                        PrecioUnitario = detalle.PrecioUnitario // Precio histórico
                    };

                    _context.DetallesVenta.Add(detalleVenta);

                    // 9. Deducir stock para productos que no son servicios
                    var producto = productos[detalle.IdProducto];
                    if (!producto.EsServicio)
                    {
                        var stockAnterior = producto.StockActual;
                        producto.StockActual -= detalle.Cantidad;

                        // 10. Crear MovimientoStock
                        var movimientoStock = new MovimientoStock
                        {
                            IdProducto = producto.Id,
                            IdUsuario = _currentUser.UserId,
                            IdVenta = venta.Id,
                            FechaMovimiento = DateTime.UtcNow,
                            Cantidad = detalle.Cantidad,
                            TipoMovimiento = Enums.TipoMovimiento.VentaSalida
                        };

                        _context.MovimientosStock.Add(movimientoStock);

                        // 11. Verificar stock bajo (no bloquea, solo alerta)
                        if (producto.StockActual <= producto.StockMinimo)
                        {
                            _logger.LogWarning(
                                "Stock bajo para producto {ProductoId} ({ProductoNombre}). Stock actual: {StockActual}, Mínimo: {StockMinimo}",
                                producto.Id, producto.Nombre, producto.StockActual, producto.StockMinimo);
                        }
                    }
                }

                // 12. Crear pagos
                foreach (var pago in request.Pagos)
                {
                    var nuevoPago = new Pago
                    {
                        IdVenta = venta.Id,
                        MetodoPago = pago.MetodoPago,
                        Monto = pago.Monto
                    };

                    _context.Pagos.Add(nuevoPago);
                }

                await _context.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);

                _logger.LogInformation("Venta {VentaId} creada exitosamente por usuario {UsuarioId}", venta.Id, _currentUser.UserId);

                // 13. Retornar la venta creada
                return await ObtenerVentaPorIdAsync(venta.Id, ct)
                    ?? throw new InvalidOperationException("Error al recuperar la venta creada");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                _logger.LogError(ex, "Error al crear venta");
                throw;
            }
        }

        public async Task<VentaResponse> AnularVentaAsync(int ventaId, AnularVentaRequest request, CancellationToken ct)
        {
            // 1. Verificar que el usuario es Admin
            if (!_currentUser.IsAdmin)
            {
                throw new UnauthorizedAccessException("Solo el administrador puede anular ventas");
            }

            // 2. Obtener la venta con sus detalles, pagos y factura
            var venta = await _context.Ventas
                .Where(v => v.Id == ventaId && v.Id_negocio == _currentUser.NegocioId)
                .Include(v => v.DetallesVenta)
                    .ThenInclude(d => d.Producto)
                .Include(v => v.Pagos)
                .Include(v => v.Factura)
                .FirstOrDefaultAsync(ct);

            if (venta == null)
            {
                throw new InvalidOperationException("Venta no encontrada");
            }

            // 3. Verificar si tiene factura con CAE y si tiene nota de crédito
            if (venta.Factura != null && !string.IsNullOrEmpty(venta.Factura.CAE))
            {
                var tieneNotaCredito = await _context.Facturas
                    .AnyAsync(f => f.IdVenta == venta.Id && f.TipoFactura == Enums.TipoComprobante.NotaCredito, ct);

                if (!tieneNotaCredito)
                {
                    throw new InvalidOperationException("No se puede anular una venta con factura oficial sin nota de crédito");
                }
            }

            // 4. Usar transacción
            using var transaction = await _context.Database.BeginTransactionAsync(ct);
            try
            {
                // 5. Revertir stock para productos que no son servicios
                foreach (var detalle in venta.DetallesVenta)
                {
                    if (!detalle.Producto.EsServicio)
                    {
                        detalle.Producto.StockActual += detalle.Cantidad;

                        // 6. Crear MovimientoStock de anulación
                        var movimientoStock = new MovimientoStock
                        {
                            IdProducto = detalle.Producto.Id,
                            IdUsuario = _currentUser.UserId,
                            IdVenta = venta.Id,
                            FechaMovimiento = DateTime.UtcNow,
                            Cantidad = detalle.Cantidad,
                            TipoMovimiento = Enums.TipoMovimiento.VentaAnulacion
                        };

                        _context.MovimientosStock.Add(movimientoStock);

                        _logger.LogInformation(
                            "Stock revertido para producto {ProductoId}. Cantidad: {Cantidad}",
                            detalle.Producto.Id, detalle.Cantidad);
                    }
                }

                // 7. Marcar la venta como anulada
                venta.Anulada = true;
                _context.Ventas.Update(venta);

                await _context.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);

                _logger.LogInformation("Venta {VentaId} anulada exitosamente por usuario {UsuarioId}. Motivo: {Motivo}",
                    venta.Id, _currentUser.UserId, request.Motivo ?? "Sin motivo");

                // Retornar la venta anulada
                return await ObtenerVentaPorIdAsync(ventaId, ct)
                    ?? throw new InvalidOperationException("Error al recuperar la venta anulada");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                _logger.LogError(ex, "Error al anular venta {VentaId}", ventaId);
                throw;
            }
        }

        public async Task<VentaResponse?> ObtenerVentaPorIdAsync(int ventaId, CancellationToken ct)
        {
            var venta = await _context.Ventas
                .Where(v => v.Id == ventaId && v.Id_negocio == _currentUser.NegocioId)
                .Include(v => v.DetallesVenta)
                    .ThenInclude(d => d.Producto)
                .Include(v => v.Pagos)
                .Include(v => v.Usuario)
                .Include(v => v.Cliente)
                .FirstOrDefaultAsync(ct);

            if (venta == null)
            {
                return null;
            }

            return MapToVentaResponse(venta);
        }

        public async Task<VentaListResponse> ObtenerTodasLasVentasAsync(
            int page, 
            int pageSize, 
            DateTime? fechaDesde, 
            DateTime? fechaHasta, 
            CancellationToken ct)
        {
            var query = _context.Ventas
                .Where(v => v.Id_negocio == _currentUser.NegocioId)
                .Include(v => v.Usuario)
                .Include(v => v.Cliente)
                .AsQueryable();

            // Filtrar por rango de fechas
            if (fechaDesde.HasValue)
            {
                query = query.Where(v => v.FechaVenta >= fechaDesde.Value);
            }

            if (fechaHasta.HasValue)
            {
                query = query.Where(v => v.FechaVenta <= fechaHasta.Value);
            }

            // Contar total
            var totalCount = await query.CountAsync(ct);

            // Paginar
            var ventas = await query
                .OrderByDescending(v => v.FechaVenta)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(v => new VentaListItem
                {
                    Id = v.Id,
                    Fecha = v.FechaVenta,
                    TotalVenta = v.TotalVenta,
                    NombreUsuario = v.Usuario != null ? v.Usuario.Nombre + " " + v.Usuario.Apellido : null,
                    NombreCliente = v.Cliente != null ? v.Cliente.Nombre : null,
                    Estado = v.Anulada ? "Anulada" : "Activa"
                })
                .ToListAsync(ct);

            return new VentaListResponse
            {
                Items = ventas,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<List<DetalleVentaResponse>> ObtenerDetallesVentaAsync(int ventaId, CancellationToken ct)
        {
            // Primero verificar que la venta pertenece al negocio
            var venta = await _context.Ventas
                .Where(v => v.Id == ventaId && v.Id_negocio == _currentUser.NegocioId)
                .FirstOrDefaultAsync(ct);

            if (venta == null)
            {
                throw new InvalidOperationException("Venta no encontrada");
            }

            var detalles = await _context.DetallesVenta
                .Where(d => d.IdVenta == ventaId)
                .Include(d => d.Producto)
                .Select(d => new DetalleVentaResponse
                {
                    Id = d.Id,
                    IdProducto = d.IdProducto,
                    NombreProducto = d.Producto != null ? d.Producto.Nombre : null,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario,
                    Subtotal = d.Cantidad * d.PrecioUnitario
                })
                .ToListAsync(ct);

            return detalles;
        }

        public async Task<List<PagoResponse>> ObtenerPagosVentaAsync(int ventaId, CancellationToken ct)
        {
            // Primero verificar que la venta pertenece al negocio
            var venta = await _context.Ventas
                .Where(v => v.Id == ventaId && v.Id_negocio == _currentUser.NegocioId)
                .FirstOrDefaultAsync(ct);

            if (venta == null)
            {
                throw new InvalidOperationException("Venta no encontrada");
            }

            var pagos = await _context.Pagos
                .Where(p => p.IdVenta == ventaId)
                .Select(p => new PagoResponse
                {
                    Id = p.Id,
                    MetodoPago = p.MetodoPago,
                    MetodoPagoNombre = p.MetodoPago.ToString(),
                    Monto = p.Monto
                })
                .ToListAsync(ct);

            return pagos;
        }

        private VentaResponse MapToVentaResponse(Venta venta)
        {
            var detalles = venta.DetallesVenta?.Select(d => new DetalleVentaResponse
            {
                Id = d.Id,
                IdProducto = d.IdProducto,
                NombreProducto = d.Producto?.Nombre,
                Cantidad = d.Cantidad,
                PrecioUnitario = d.PrecioUnitario,
                Subtotal = d.Cantidad * d.PrecioUnitario
            }).ToList() ?? new List<DetalleVentaResponse>();

            var pagos = venta.Pagos?.Select(p => new PagoResponse
            {
                Id = p.Id,
                MetodoPago = p.MetodoPago,
                MetodoPagoNombre = p.MetodoPago.ToString(),
                Monto = p.Monto
            }).ToList() ?? new List<PagoResponse>();

            return new VentaResponse
            {
                Id = venta.Id,
                Fecha = venta.FechaVenta,
                TotalVenta = venta.TotalVenta,
                IdUsuario = venta.IdUsuario,
                NombreUsuario = venta.Usuario != null ? venta.Usuario.Nombre + " " + venta.Usuario.Apellido : null,
                IdCliente = venta.IdCliente > 0 ? venta.IdCliente : null,
                NombreCliente = venta.Cliente?.Nombre,
                Estado = venta.Anulada ? "Anulada" : "Activa",
                Detalles = detalles,
                Pagos = pagos
            };
        }
    }
}
