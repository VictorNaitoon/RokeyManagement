using API.Data;
using API.DTO.Request.Ventas;
using API.DTO.Response.Ventas;
using API.Models;
using API.Services.Auditoria;
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
        private readonly IAuditoriaService _auditoriaService;
        private readonly ILogger<VentaService> _logger;

        public VentaService(AppDbContext context, ICurrentUserService currentUser, ICajaService cajaService, IAuditoriaService auditoriaService, ILogger<VentaService> logger)
        {
            _context = context;
            _currentUser = currentUser;
            _cajaService = cajaService;
            _auditoriaService = auditoriaService;
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

            // Nota: La verificación de Estado == Inactivo ahora se maneja centralmente en SubscriptionBlockingMiddleware

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

            // 5. Validar límite de crédito si el cliente tiene fiado habilitado
            if (request.IdCliente.HasValue && request.IdCliente.Value > 0)
            {
                await ValidarCreditoClienteAsync(request.IdCliente.Value, request.Pagos, totalVenta, ct);
            }

            // 6. Validar suma de pagos
            var sumaPagos = request.Pagos.Sum(p => p.Monto);
            if (sumaPagos != totalVenta)
            {
                throw new InvalidOperationException("La suma de pagos debe coincidir con el total de la venta");
            }

            // 7. Si no hay cliente, usar "Consumidor Final"
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

            // 8. Usar transacción para atomicidad
            using var transaction = await _context.Database.BeginTransactionAsync(ct);
            try
            {
                // 9. Crear la venta
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

                // 10. Crear detalles de venta
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

                    // 11. Deducir stock para productos que no son servicios
                    var producto = productos[detalle.IdProducto];
                    if (!producto.EsServicio)
                    {
                        var stockAnterior = producto.StockActual;
                        producto.StockActual -= detalle.Cantidad;

                        // 12. Crear MovimientoStock
                        var movimientoStock = new MovimientoStock
                        {
                            Id_negocio = _currentUser.NegocioId,
                            IdProducto = producto.Id,
                            IdUsuario = _currentUser.UserId,
                            IdVenta = venta.Id,
                            FechaMovimiento = DateTime.UtcNow,
                            Cantidad = detalle.Cantidad,
                            TipoMovimiento = Enums.TipoMovimiento.VentaSalida,
                            StockAnterior = stockAnterior,
                            StockNuevo = producto.StockActual,
                            Motivo = $"Venta #{venta.Id}"
                        };

                        _context.MovimientosStock.Add(movimientoStock);

                        // 13. Verificar stock bajo (no bloquea, solo alerta)
                        if (producto.StockActual <= producto.StockMinimo)
                        {
                            _logger.LogWarning(
                                "Stock bajo para producto {ProductoId} ({ProductoNombre}). Stock actual: {StockActual}, Mínimo: {StockMinimo}",
                                producto.Id, producto.Nombre, producto.StockActual, producto.StockMinimo);
                        }
                    }
                }

                // 14. Crear pagos
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

                // 15. Registrar movimiento de caja automático (Ingreso)
                try
                {
                    await _cajaService.AgregarMovimientoAsync(
                        new DTO.Request.Caja.AgregarMovimientoCajaRequest
                        {
                            Tipo = "Ingreso",
                            Monto = totalVenta,
                            Descripcion = $"Venta #{venta.Id}"
                        },
                        _currentUser.UserId,
                        _currentUser.NegocioId,
                        ct);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "No se pudo registrar movimiento de caja para la venta {VentaId}", venta.Id);
                }

                _logger.LogInformation("Venta {VentaId} creada exitosamente por usuario {UsuarioId}", venta.Id, _currentUser.UserId);

                // 16. Registrar auditoría
                await _auditoriaService.RegistrarAsync(
                    "Venta",
                    venta.Id,
                    "CREATE",
                    null,
                    new
                    {
                        venta.TotalVenta,
                        venta.IdCliente,
                        Estado = "Activa"
                    });

                // 17. Retornar la venta creada
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
                        var stockAnterior = detalle.Producto.StockActual;
                        detalle.Producto.StockActual += detalle.Cantidad;

                        // 6. Crear MovimientoStock de anulación
                        var movimientoStock = new MovimientoStock
                        {
                            Id_negocio = _currentUser.NegocioId,
                            IdProducto = detalle.Producto.Id,
                            IdUsuario = _currentUser.UserId,
                            IdVenta = venta.Id,
                            FechaMovimiento = DateTime.UtcNow,
                            Cantidad = detalle.Cantidad,
                            TipoMovimiento = Enums.TipoMovimiento.VentaAnulacion,
                            StockAnterior = stockAnterior,
                            StockNuevo = detalle.Producto.StockActual,
                            Motivo = $"Anulación Venta #{venta.Id}"
                        };

                        _context.MovimientosStock.Add(movimientoStock);

                        _logger.LogInformation(
                            "Stock revertido para producto {ProductoId}. Cantidad: {Cantidad}",
                            detalle.Producto.Id, detalle.Cantidad);
                    }
                }

                // 7. Capturar estado antes de modificar para auditoría
                var datosAnteriores = new
                {
                    venta.TotalVenta,
                    venta.IdCliente,
                    Estado = "Activa"
                };

                // 8. Marcar la venta como anulada
                venta.Anulada = true;
                venta.IdUsuarioModificador = _currentUser.UserId;
                _context.Ventas.Update(venta);

                await _context.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);

                // 9. Registrar auditoría
                await _auditoriaService.RegistrarAsync(
                    "Venta",
                    venta.Id,
                    "UPDATE",
                    datosAnteriores,
                    new
                    {
                        venta.TotalVenta,
                        venta.IdCliente,
                        Estado = "Anulada"
                    }, ct);

                // 10. Registrar movimiento de caja automático (Egreso por anulación)
                try
                {
                    await _cajaService.AgregarMovimientoAsync(
                        new DTO.Request.Caja.AgregarMovimientoCajaRequest
                        {
                            Tipo = "Egreso",
                            Monto = venta.TotalVenta,
                            Descripcion = $"Anulación de Venta #{venta.Id}"
                        },
                        _currentUser.UserId,
                        _currentUser.NegocioId,
                        ct);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "No se pudo registrar movimiento de caja por anulación de venta {VentaId}", venta.Id);
                }

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

        /// <summary>
        /// Valida el límite de crédito del cliente antes de crear una venta.
        /// Se salta la validación si:
        /// - El cliente no tiene permitido el fiado (PermiteFiado = false)
        /// - El límite de crédito es 0 o null
        /// - La venta es pagada completamente en efectivo
        /// </summary>
        private async Task ValidarCreditoClienteAsync(
            int clienteId,
            List<DTO.Request.Ventas.PagoRequest> pagos,
            decimal montoVenta,
            CancellationToken ct)
        {
            // 1. Obtener el cliente
            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.Id == clienteId && c.Id_negocio == _currentUser.NegocioId, ct);

            if (cliente == null)
            {
                // Cliente no encontrado, no validamos crédito (podría ser consumidor final)
                return;
            }

            // 2. Si el cliente no tiene permitido el fiado, omitir validación
            if (!cliente.PermiteFiado)
            {
                _logger.LogDebug("Cliente {ClienteId} no tiene permitido el fiado. Se omite validación de crédito.", clienteId);
                return;
            }

            // 3. Si el límite de crédito es 0 o null con fiado habilitado, no se permite crédito
            if (cliente.LimiteCredito == null || cliente.LimiteCredito <= 0)
            {
                throw new InvalidOperationException(
                    $"El cliente no tiene límite de crédito configured. Configure un límite de crédito mayor a 0 para permitir ventas a fiado.");
            }

            // 4. Determinar si la venta es pagada en efectivo (pago inmediato)
            // Una venta es "pagada en efectivo" si TODOS los pagos son en efectivo
            bool esPagoInmediato = pagos.All(p => p.MetodoPago == Enums.MetodoPago.Efectivo);
            if (esPagoInmediato)
            {
                _logger.LogDebug("Venta pagada en efectivo. Se omite validación de crédito para cliente {ClienteId}.", clienteId);
                return;
            }

            // 5. Calcular el saldo pendiente actual del cliente (ventas fiadas no pagadas)
            var ventasFiado = await _context.Ventas
                .Where(v => v.IdCliente == clienteId 
                            && v.Id_negocio == _currentUser.NegocioId 
                            && !v.Anulada)
                .ToListAsync(ct);

            decimal totalFiado = 0;
            foreach (var venta in ventasFiado)
            {
                // Calcular cuanto se ha pagado en esta venta
                var pagosVenta = await _context.Pagos
                    .Where(p => p.IdVenta == venta.Id)
                    .SumAsync(p => (decimal?)p.Monto, ct) ?? 0;

                // Si el pago es menor al total, hay saldo pendiente
                if (pagosVenta < venta.TotalVenta)
                {
                    totalFiado += (venta.TotalVenta - pagosVenta);
                }
            }

            // 6. Verificar si agregar esta venta excede el límite
            decimal creditoDisponible = cliente.LimiteCredito.Value - totalFiado;
            if (montoVenta > creditoDisponible)
            {
                _logger.LogWarning(
                    "Venta rechazada por límite de crédito. Cliente {ClienteId}, Límite: {LimiteCredito}, " +
                    "Saldo pendiente: {SaldoPendiente}, Venta actual: {MontoVenta}, Crédito disponible: {CreditoDisponible}",
                    clienteId, cliente.LimiteCredito, totalFiado, montoVenta, creditoDisponible);

                throw new InvalidOperationException(
                    $"La venta excede el límite de crédito del cliente. " +
                    $"Límite: ${cliente.LimiteCredito:N2}, " +
                    $"Saldo pendiente: ${totalFiado:N2}, " +
                    $"Venta actual: ${montoVenta:N2}, " +
                    $"Crédito disponible: ${creditoDisponible:N2}");
            }

            _logger.LogDebug(
                "Validación de crédito passed. Cliente {ClienteId}, Saldo pendiente: {SaldoPendiente}, " +
                "Venta actual: {MontoVenta}, Crédito disponible después: {CreditoDisponible}",
                clienteId, totalFiado, montoVenta, creditoDisponible - montoVenta);
        }
    }
}
