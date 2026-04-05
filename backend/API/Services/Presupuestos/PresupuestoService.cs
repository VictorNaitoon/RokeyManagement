using API.Data;
using API.DTO.Request.Presupuestos;
using API.DTO.Response.Presupuestos;
using API.DTO.Response.Ventas;
using API.Models;
using API.Services.Auditoria;
using API.Services.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace API.Services.Presupuestos
{
    public class PresupuestoService : IPresupuestoService
    {
        private readonly AppDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IAuditoriaService _auditoriaService;
        private readonly ILogger<PresupuestoService> _logger;

        public PresupuestoService(AppDbContext context, ICurrentUserService currentUser, IAuditoriaService auditoriaService, ILogger<PresupuestoService> logger)
        {
            _context = context;
            _currentUser = currentUser;
            _auditoriaService = auditoriaService;
            _logger = logger;
        }

        public async Task<PresupuestoResponse> CreateAsync(CreatePresupuestoRequest request, CancellationToken ct)
        {
            // 1. Validar que el negocio existe y está activo
            var negocio = await _context.Negocios.FindAsync(new object[] { _currentUser.NegocioId }, ct);
            if (negocio == null)
            {
                throw new InvalidOperationException("Negocio no encontrado");
            }

            // Nota: La verificación de Estado == Inactivo ahora se maneja centralmente en SubscriptionBlockingMiddleware

            // 2. Validar que todos los productos existen y pertenecen al mismo negocio
            var productoIds = request.Detalles.Select(d => d.IdProducto).Distinct().ToList();
            var productos = await _context.Productos
                .Where(p => productoIds.Contains(p.Id) && p.Id_negocio == _currentUser.NegocioId && p.Activo)
                .ToDictionaryAsync(p => p.Id, ct);

            if (productos.Count != productoIds.Count)
            {
                throw new InvalidOperationException("Uno o más productos no existen o no pertenecen al negocio");
            }

            // 3. Calcular total del presupuesto
            var totalPresupuesto = request.Detalles.Sum(d => d.Cantidad * d.PrecioPactado);

            // 4. Si no hay cliente, usar "Consumidor Final"
            int idCliente;
            if (request.IdCliente.HasValue && request.IdCliente.Value > 0)
            {
                // Verificar que el cliente pertenece al negocio
                var cliente = await _context.Clientes
                    .FirstOrDefaultAsync(c => c.Id == request.IdCliente.Value && c.Id_negocio == _currentUser.NegocioId, ct);
                
                if (cliente == null)
                {
                    throw new InvalidOperationException("El cliente no existe o no pertenece al negocio");
                }
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

            // 5. Fecha de vencimiento: usar la especificada o 30 días por defecto
            var fechaVencimiento = request.FechaVencimiento ?? DateTime.UtcNow.AddDays(30);

            // 6. Crear el presupuesto
            var presupuesto = new Presupuesto
            {
                Id_negocio = _currentUser.NegocioId,
                IdUsuario = _currentUser.UserId,
                IdCliente = idCliente,
                FechaEmision = DateTime.UtcNow,
                FechaVencimiento = fechaVencimiento,
                Estado = Enums.EstadoPresupuesto.Pendiente,
                TotalPresupuesto = totalPresupuesto
            };

            _context.Presupuestos.Add(presupuesto);
            await _context.SaveChangesAsync(ct);

            // 7. Crear los detalles del presupuesto
            foreach (var detalle in request.Detalles)
            {
                var detallePresupuesto = new DetallePresupuesto
                {
                    IdPresupuesto = presupuesto.Id,
                    IdProducto = detalle.IdProducto,
                    Cantidad = detalle.Cantidad,
                    PrecioPactado = detalle.PrecioPactado
                };

                _context.DetallesPresupuesto.Add(detallePresupuesto);
            }

            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Presupuesto {PresupuestoId} creado exitosamente por usuario {UsuarioId}", presupuesto.Id, _currentUser.UserId);

            // Registrar auditoría
            await _auditoriaService.RegistrarAsync(
                "Presupuesto",
                presupuesto.Id,
                "CREATE",
                null,
                new
                {
                    presupuesto.TotalPresupuesto,
                    presupuesto.IdCliente,
                    presupuesto.Estado,
                    presupuesto.FechaVencimiento
                }, ct);

            // 8. Retornar el presupuesto creado
            return await GetByIdAsync(presupuesto.Id, ct);
        }

        public async Task<PresupuestoListResponse> GetAllAsync(Enums.EstadoPresupuesto? estado = null, int? idCliente = null, CancellationToken ct = default)
        {
            var query = _context.Presupuestos
                .Where(p => p.Id_negocio == _currentUser.NegocioId)
                .Include(p => p.Cliente)
                .AsQueryable();

            // Filtrar por estado
            if (estado.HasValue)
            {
                query = query.Where(p => p.Estado == estado.Value);
            }

            // Filtrar por cliente
            if (idCliente.HasValue)
            {
                query = query.Where(p => p.IdCliente == idCliente.Value);
            }

            // Obtener todos y aplicar auto-evaluación de Vencido en memoria
            var presupuestos = await query
                .OrderByDescending(p => p.FechaEmision)
                .ToListAsync(ct);

            var items = presupuestos.Select(p =>
            {
                // Auto-evaluar estado Vencido
                var estadoMostrar = p.Estado;
                if (p.Estado == Enums.EstadoPresupuesto.Pendiente && p.FechaVencimiento < DateTime.UtcNow.Date)
                {
                    estadoMostrar = Enums.EstadoPresupuesto.Vencido;
                }

                return new PresupuestoListItem
                {
                    Id = p.Id,
                    NombreCliente = p.Cliente?.Nombre,
                    FechaEmision = p.FechaEmision,
                    FechaVencimiento = p.FechaVencimiento,
                    Estado = estadoMostrar.ToString(),
                    Total = p.TotalPresupuesto
                };
            }).ToList();

            return new PresupuestoListResponse
            {
                Items = items,
                TotalCount = items.Count
            };
        }

        public async Task<PresupuestoResponse> GetByIdAsync(int id, CancellationToken ct)
        {
            var presupuesto = await _context.Presupuestos
                .Where(p => p.Id == id && p.Id_negocio == _currentUser.NegocioId)
                .Include(p => p.Cliente)
                .Include(p => p.Usuario)
                .Include(p => p.DetallesPresupuesto)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(ct);

            if (presupuesto == null)
            {
                throw new InvalidOperationException("Presupuesto no encontrado");
            }

            // Auto-evaluar estado Vencido
            var estadoMostrar = presupuesto.Estado;
            if (presupuesto.Estado == Enums.EstadoPresupuesto.Pendiente && presupuesto.FechaVencimiento < DateTime.UtcNow.Date)
            {
                estadoMostrar = Enums.EstadoPresupuesto.Vencido;
            }

            var detalles = presupuesto.DetallesPresupuesto.Select(d => new DetallePresupuestoResponse
            {
                Id = d.Id,
                IdProducto = d.IdProducto,
                NombreProducto = d.Producto?.Nombre,
                Cantidad = d.Cantidad,
                PrecioPactado = d.PrecioPactado
            }).ToList();

            return new PresupuestoResponse
            {
                Id = presupuesto.Id,
                IdUsuario = presupuesto.IdUsuario,
                NombreUsuario = presupuesto.Usuario != null ? presupuesto.Usuario.Nombre + " " + presupuesto.Usuario.Apellido : null,
                IdCliente = presupuesto.IdCliente,
                NombreCliente = presupuesto.Cliente?.Nombre,
                FechaEmision = presupuesto.FechaEmision,
                FechaVencimiento = presupuesto.FechaVencimiento,
                Estado = estadoMostrar.ToString(),
                Total = presupuesto.TotalPresupuesto,
                Detalles = detalles
            };
        }

        public async Task<PresupuestoResponse> UpdateEstadoAsync(int id, Enums.EstadoPresupuesto nuevoEstado, CancellationToken ct)
        {
            var presupuesto = await _context.Presupuestos
                .Where(p => p.Id == id && p.Id_negocio == _currentUser.NegocioId)
                .FirstOrDefaultAsync(ct);

            if (presupuesto == null)
            {
                throw new InvalidOperationException("Presupuesto no encontrado");
            }

            // Validar que no se puede modificar si está Aceptado o Rechazado
            if (presupuesto.Estado == Enums.EstadoPresupuesto.Aceptado || presupuesto.Estado == Enums.EstadoPresupuesto.Rechazado)
            {
                throw new InvalidOperationException("No se puede modificar un presupuesto que ya ha sido aceptado o rechazado");
            }

            // Validar que el nuevo estado sea válido para la transición
            // No permitimos cambiar directamente a Vencido (eso es automático)
            if (nuevoEstado == Enums.EstadoPresupuesto.Vencido)
            {
                throw new InvalidOperationException("El estado Vencido se evalúa automáticamente, no se puede establecer manualmente");
            }

            // Si el nuevo estado es Aceptado, convertir a venta automáticamente
            if (nuevoEstado == Enums.EstadoPresupuesto.Aceptado)
            {
                // Llamar a ConvertirAVentaAsync que hace todo el proceso
                await ConvertirAVentaAsync(id, ct);
                
                // Retornar el presupuesto actualizado (ahora en estado Aceptado)
                return await GetByIdAsync(id, ct);
            }

            // Para otros estados (Pendiente, Anulado, Rechazado), solo actualizar el estado
            var datosAnteriores = new
            {
                presupuesto.Estado
            };

            presupuesto.Estado = nuevoEstado;
            presupuesto.IdUsuarioModificador = _currentUser.UserId;
            await _context.SaveChangesAsync(ct);

            // Registrar auditoría
            await _auditoriaService.RegistrarAsync(
                "Presupuesto",
                presupuesto.Id,
                "UPDATE",
                datosAnteriores,
                new { presupuesto.Estado }, ct);

            _logger.LogInformation("Presupuesto {PresupuestoId} actualizado a estado {Estado} por usuario {UsuarioId}", 
                id, nuevoEstado, _currentUser.UserId);

            return await GetByIdAsync(id, ct);
        }

        public async Task AnularAsync(int id, CancellationToken ct)
        {
            var presupuesto = await _context.Presupuestos
                .Where(p => p.Id == id && p.Id_negocio == _currentUser.NegocioId)
                .FirstOrDefaultAsync(ct);

            if (presupuesto == null)
            {
                throw new InvalidOperationException("Presupuesto no encontrado");
            }

            // Solo se puede anular si está Pendiente
            if (presupuesto.Estado != Enums.EstadoPresupuesto.Pendiente)
            {
                throw new InvalidOperationException("Solo se pueden anular presupuestos en estado Pendiente");
            }

            // Capturar estado antes de modificar para auditoría
            var datosAnteriores = new
            {
                presupuesto.Estado
            };

            presupuesto.Estado = Enums.EstadoPresupuesto.Anulado;
            presupuesto.IdUsuarioModificador = _currentUser.UserId;
            await _context.SaveChangesAsync(ct);

            // Registrar auditoría
            await _auditoriaService.RegistrarAsync(
                "Presupuesto",
                presupuesto.Id,
                "UPDATE",
                datosAnteriores,
                new { presupuesto.Estado }, ct);

            _logger.LogInformation("Presupuesto {PresupuestoId} anulado por usuario {UsuarioId}", id, _currentUser.UserId);
        }

        public async Task<VentaResponse> ConvertirAVentaAsync(int id, CancellationToken ct)
        {
            // 1. Obtener el presupuesto con todos los datos necesarios
            var presupuesto = await _context.Presupuestos
                .Where(p => p.Id == id && p.Id_negocio == _currentUser.NegocioId)
                .Include(p => p.DetallesPresupuesto)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(ct);

            if (presupuesto == null)
            {
                throw new InvalidOperationException("Presupuesto no encontrado");
            }

            // 2. Validar que el presupuesto está Pendiente
            if (presupuesto.Estado != Enums.EstadoPresupuesto.Pendiente)
            {
                throw new InvalidOperationException("Solo se pueden convertir presupuestos en estado Pendiente");
            }

            // 3. Validar que los productos aún existen y están activos
            foreach (var detalle in presupuesto.DetallesPresupuesto)
            {
                if (detalle.Producto == null || !detalle.Producto.Activo)
                {
                    throw new InvalidOperationException($"El producto con ID {detalle.IdProducto} ya no está disponible");
                }
            }

            // 4. Validar stock disponible
            foreach (var detalle in presupuesto.DetallesPresupuesto)
            {
                if (!detalle.Producto.EsServicio && detalle.Producto.StockActual < detalle.Cantidad)
                {
                    throw new InvalidOperationException($"Stock insuficiente para el producto: {detalle.Producto.Nombre}. Stock actual: {detalle.Producto.StockActual}");
                }
            }

            // 5. Usar transacción para atomicidad
            using var transaction = await _context.Database.BeginTransactionAsync(ct);
            try
            {
                // 6. Crear la venta
                var venta = new Venta
                {
                    Id_negocio = _currentUser.NegocioId,
                    IdUsuario = _currentUser.UserId,
                    IdCliente = presupuesto.IdCliente ?? 0,
                    FechaVenta = DateTime.UtcNow,
                    TotalVenta = presupuesto.TotalPresupuesto
                };

                _context.Ventas.Add(venta);
                await _context.SaveChangesAsync(ct);

                // 7. Crear detalles de venta y deducir stock
                foreach (var detallePresupuesto in presupuesto.DetallesPresupuesto)
                {
                    var detalleVenta = new DetalleVenta
                    {
                        IdVenta = venta.Id,
                        IdProducto = detallePresupuesto.IdProducto,
                        Cantidad = detallePresupuesto.Cantidad,
                        PrecioUnitario = detallePresupuesto.PrecioPactado // Precio histórico
                    };

                    _context.DetallesVenta.Add(detalleVenta);

                    // Deducir stock si no es servicio
                    if (!detallePresupuesto.Producto.EsServicio)
                    {
                        var producto = detallePresupuesto.Producto;
                        var stockAnterior = producto.StockActual;
                        producto.StockActual -= detallePresupuesto.Cantidad;

                        // Crear MovimientoStock
                        var movimientoStock = new MovimientoStock
                        {
                            Id_negocio = _currentUser.NegocioId,
                            IdProducto = producto.Id,
                            IdUsuario = _currentUser.UserId,
                            IdVenta = venta.Id,
                            FechaMovimiento = DateTime.UtcNow,
                            Cantidad = detallePresupuesto.Cantidad,
                            TipoMovimiento = Enums.TipoMovimiento.VentaSalida,
                            StockAnterior = stockAnterior,
                            StockNuevo = producto.StockActual,
                            Motivo = $"Presupuesto #{presupuesto.Id} → Venta #{venta.Id}"
                        };

                        _context.MovimientosStock.Add(movimientoStock);

                        // Verificar stock bajo
                        if (producto.StockActual <= producto.StockMinimo)
                        {
                            _logger.LogWarning(
                                "Stock bajo para producto {ProductoId} ({ProductoNombre}). Stock actual: {StockActual}, Mínimo: {StockMinimo}",
                                producto.Id, producto.Nombre, producto.StockActual, producto.StockMinimo);
                        }
                    }
                }

                // 8. Crear pago en efectivo por defecto (el total del presupuesto)
                var pago = new Pago
                {
                    IdVenta = venta.Id,
                    MetodoPago = Enums.MetodoPago.Efectivo,
                    Monto = presupuesto.TotalPresupuesto
                };

                _context.Pagos.Add(pago);

                // 9. Actualizar estado del presupuesto a Aceptado
                presupuesto.Estado = Enums.EstadoPresupuesto.Aceptado;
                presupuesto.IdUsuarioModificador = _currentUser.UserId;

                await _context.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);

                // Registrar auditoría del presupuesto actualizado
                await _auditoriaService.RegistrarAsync(
                    "Presupuesto",
                    presupuesto.Id,
                    "UPDATE",
                    new { Estado = Enums.EstadoPresupuesto.Pendiente },
                    new { presupuesto.Estado }, ct);

                // Registrar auditoría de la venta creada
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
                    }, ct);

                _logger.LogInformation("Presupuesto {PresupuestoId} convertido a venta {VentaId} por usuario {UsuarioId}", 
                    id, venta.Id, _currentUser.UserId);

                // 10. Retornar la venta creada
                return await ObtenerVentaPorIdAsync(venta.Id, ct)
                    ?? throw new InvalidOperationException("Error al recuperar la venta creada");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                _logger.LogError(ex, "Error al convertir presupuesto {PresupuestoId} a venta", id);
                throw;
            }
        }

        private async Task<VentaResponse?> ObtenerVentaPorIdAsync(int ventaId, CancellationToken ct)
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
