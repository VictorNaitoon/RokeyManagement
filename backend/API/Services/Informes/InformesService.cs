using API.Data;
using API.DTO.Response.Informes;
using API.Models;
using API.Services.Common;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;

namespace API.Services.Informes
{
    public class InformesService : IInformesService
    {
        private readonly AppDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public InformesService(AppDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        /// <summary>
        /// Resolves date range from query params and presets
        /// Presets: "hoy", "semana", "mes" (default "mes")
        /// </summary>
        private (DateTime Desde, DateTime Hasta) ResolveDateRange(DateTime? fechaDesde, DateTime? fechaHasta, string? preset)
        {
            var ahora = DateTime.UtcNow;
            DateTime desde;
            DateTime hasta;

            // If explicit dates provided, use them (normalize to UTC 00:00 / 23:59:59.999)
            if (fechaDesde.HasValue && fechaHasta.HasValue)
            {
                var desdeDate = fechaDesde.Value.Date;
                var hastaDate = fechaHasta.Value.Date;
                desde = new DateTime(desdeDate.Year, desdeDate.Month, desdeDate.Day, 0, 0, 0, DateTimeKind.Utc);
                hasta = new DateTime(hastaDate.Year, hastaDate.Month, hastaDate.Day, 23, 59, 59, 999, DateTimeKind.Utc);
            }
            else
            {
                // Validate preset before defaulting — invalid preset must return 400, not silent default
                if (preset != null && preset.ToLowerInvariant() is not ("hoy" or "semana" or "mes"))
                {
                    throw new ValidationException(new[]
                    {
                        new ValidationFailure("preset", "Preset debe ser hoy, semana o mes.")
                    });
                }

                // Use preset or default to "mes"
                var presetNormalizado = (preset ?? "mes").ToLowerInvariant();

                switch (presetNormalizado)
                {
                    case "hoy":
                        // Start of today in UTC
                        var hoyUtc = ahora.Date;
                        desde = new DateTime(hoyUtc.Year, hoyUtc.Month, hoyUtc.Day, 0, 0, 0, DateTimeKind.Utc);
                        hasta = ahora;
                        break;
                    case "semana":
                        // Start of current week (Monday) in UTC
                        var diff = ((int)ahora.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
                        var inicioSemanaUtc = ahora.Date.AddDays(-diff);
                        desde = new DateTime(inicioSemanaUtc.Year, inicioSemanaUtc.Month, inicioSemanaUtc.Day, 0, 0, 0, DateTimeKind.Utc);
                        hasta = ahora;
                        break;
                    case "mes":
                    default:
                        // Start of current month in UTC
                        var inicioMesUtc = new DateTime(ahora.Year, ahora.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                        desde = inicioMesUtc;
                        hasta = ahora;
                        break;
                }
            }

            // Validate inverted range (from custom dates) — must be 400 not silent
            if (desde > hasta)
            {
                throw new ValidationException(new[]
                {
                    new ValidationFailure("fechaDesde", "fechaDesde no puede ser posterior a fechaHasta.")
                });
            }

            // Validate date range doesn't exceed 12 months -> 400
            if ((hasta - desde).TotalDays > 365)
            {
                throw new ValidationException(new[]
                {
                    new ValidationFailure("fechaHasta", "El rango no puede exceder 12 meses.")
                });
            }

            return (desde, hasta);
        }

        /// <summary>
        /// Margen por producto vendido: (precio historico de venta - costo actual) * cantidad.
        /// Formula compartida por el resumen de ventas, ingresos-gastos y la caja.
        /// </summary>
        private async Task<decimal> CalcularGananciaMargenAsync(
            IReadOnlyList<int> ventasActivasIds, int negocioId, CancellationToken ct)
        {
            if (ventasActivasIds.Count == 0) return 0;

            return await (
                from d in _context.DetallesVenta.AsNoTracking()
                join p in _context.Productos on d.IdProducto equals p.Id
                where ventasActivasIds.Contains(d.IdVenta)
                    && p.Id_negocio == negocioId
                select (d.PrecioUnitario - p.PrecioCompra) * d.Cantidad
            ).SumAsync(ct);
        }

        /// <summary>
        /// Costo de la mercaderia vendida: (costo actual del producto) * cantidad.
        /// Misma base de datos y mismo filtro que la ganancia por margen.
        /// </summary>
        private async Task<decimal> CalcularCostoVendidoAsync(
            IReadOnlyList<int> ventasActivasIds, int negocioId, CancellationToken ct)
        {
            if (ventasActivasIds.Count == 0) return 0;

            return await (
                from d in _context.DetallesVenta.AsNoTracking()
                join p in _context.Productos on d.IdProducto equals p.Id
                where ventasActivasIds.Contains(d.IdVenta)
                    && p.Id_negocio == negocioId
                select p.PrecioCompra * d.Cantidad
            ).SumAsync(ct);
        }

        public async Task<VentasResumenResponse> GetVentasResumenAsync(DateTime? fechaDesde, DateTime? fechaHasta, string? preset, CancellationToken ct)
        {
            var (desde, hasta) = ResolveDateRange(fechaDesde, fechaHasta, preset);
            var negocioId = _currentUser.NegocioId;

            var ventas = await _context.Ventas
                .AsNoTracking()
                .Where(v => v.Id_negocio == negocioId && v.FechaVenta >= desde && v.FechaVenta <= hasta)
                .ToListAsync(ct);

            var ventasActivas = ventas.Where(v => !v.Anulada).ToList();
            var ventasAnuladas = ventas.Where(v => v.Anulada).ToList();

            var totalVentas = ventasActivas.Sum(v => v.TotalVenta);
            var cantidadVentas = ventasActivas.Count;
            var ticketPromedio = cantidadVentas > 0 ? totalVentas / cantidadVentas : 0;

            // Margen real por producto, igual que CajaService.ObtenerGananciaAsync.
            var ganancia = await CalcularGananciaMargenAsync(
                ventasActivas.Select(v => v.Id).ToList(), negocioId, ct);
            var costoVendido = await CalcularCostoVendidoAsync(
                ventasActivas.Select(v => v.Id).ToList(), negocioId, ct);

            return new VentasResumenResponse(
                TotalVentas: totalVentas,
                CantidadVentas: cantidadVentas,
                TicketPromedio: ticketPromedio,
                VentasAnuladas: ventasAnuladas.Count,
                Periodo: $"{desde:yyyy-MM-dd} al {hasta:yyyy-MM-dd}",
                Ganancia: ganancia,
                CostoVendido: costoVendido
            );
        }

        public async Task<ProductosTopResponse> GetProductosTopAsync(int cantidad, DateTime? fechaDesde, DateTime? fechaHasta, string? preset, CancellationToken ct)
        {
            // Validate cantidad 1..50 — must be 400 for OOR, not silent clamp
            if (cantidad < 1 || cantidad > 50)
            {
                throw new ValidationException(new[]
                {
                    new ValidationFailure("cantidad", "cantidad debe estar entre 1 y 50.")
                });
            }

            var limite = cantidad;

            var (desde, hasta) = ResolveDateRange(fechaDesde, fechaHasta, preset);
            var negocioId = _currentUser.NegocioId;

            // Get top products by revenue, excluding services
            // Materialize first to avoid EF Core GroupBy + Include translation issue
            var detalles = await _context.DetallesVenta
                .AsNoTracking()
                .Include(d => d.Venta)
                .Include(d => d.Producto)
                .Where(d => d.Venta != null && d.Producto != null
                            && d.Venta.Id_negocio == negocioId
                            && d.Venta.FechaVenta >= desde
                            && d.Venta.FechaVenta <= hasta
                            && !d.Venta.Anulada
                            && !d.Producto.EsServicio)
                .ToListAsync(ct);

            var productosTop = detalles
                .GroupBy(d => new { d.IdProducto, d.Producto.Nombre })
                .Select(g => new TopProductoResponse(
                    g.Key.IdProducto,
                    g.Key.Nombre,
                    g.Sum(d => d.Cantidad),
                    g.Sum(d => d.Cantidad * d.PrecioUnitario)
                ))
                .OrderByDescending(p => p.MontoTotal)
                .Take(limite)
                .ToList();

            return new ProductosTopResponse(Productos: productosTop);
        }

        public async Task<FlujoCajaResponse> GetFlujoCajaAsync(DateTime? fechaDesde, DateTime? fechaHasta, string? preset, CancellationToken ct)
        {
            var (desde, hasta) = ResolveDateRange(fechaDesde, fechaHasta, preset);
            var negocioId = _currentUser.NegocioId;

            // Get movimientos de caja within date range, only closed caja
            var movimientos = await _context.MovimientosCaja
                .AsNoTracking()
                .Include(m => m.Caja)
                .Where(m => m.Id_negocio == negocioId
                            && m.Fecha >= desde
                            && m.Fecha <= hasta
                            && m.Caja != null && m.Caja.Estado == "Cerrada")  // Only closed caja, null-safe
                .ToListAsync(ct);

            var ingresos = movimientos.Where(m => m.Tipo == "Ingreso").ToList();
            var egresos = movimientos.Where(m => m.Tipo == "Egreso").ToList();

            return new FlujoCajaResponse(
                Ingresos: ingresos.Sum(m => m.Monto),
                Egresos: egresos.Sum(m => m.Monto),
                Balance: ingresos.Sum(m => m.Monto) - egresos.Sum(m => m.Monto),
                MovimientosIngreso: ingresos.Count,
                MovimientosEgreso: egresos.Count
            );
        }

        public async Task<IngresosGastosResponse> GetIngresosGastosAsync(DateTime? fechaDesde, DateTime? fechaHasta, string? preset, CancellationToken ct)
        {
            var (desde, hasta) = ResolveDateRange(fechaDesde, fechaHasta, preset);
            var negocioId = _currentUser.NegocioId;

            var ventasActivasIds = await _context.Ventas
                .AsNoTracking()
                .Where(v => v.Id_negocio == negocioId
                            && v.FechaVenta >= desde
                            && v.FechaVenta <= hasta
                            && !v.Anulada)
                .Select(v => v.Id)
                .ToListAsync(ct);

            // Get total ventas (excluding cancelled)
            var ventasTotales = await _context.Ventas
                .Where(v => v.Id_negocio == negocioId
                            && v.FechaVenta >= desde
                            && v.FechaVenta <= hasta
                            && !v.Anulada)
                .SumAsync(v => v.TotalVenta, ct);

            // Get total compras (excluding cancelled)
            var comprasTotales = await _context.Compras
                .Where(c => c.Id_negocio == negocioId
                            && c.FechaCompra >= desde
                            && c.FechaCompra <= hasta
                            && !c.Anulada)
                .SumAsync(c => c.TotalGasto, ct);

            // Ganancia = margen de lo vendido (no se hunde con compras de reposición).
            // FlujoNeto = ventas - compras: inversión en stock, no una pérdida.
            var ganancia = await CalcularGananciaMargenAsync(ventasActivasIds, negocioId, ct);
            var flujoNeto = ventasTotales - comprasTotales;
            var margenPorcentaje = ventasTotales > 0
                ? Math.Round((ganancia / ventasTotales) * 100, 2)
                : 0;

            // Enriched detail: itemised ventas and compras for export/reporting
            // Keep aggregates intact; detail is additive and capped at 5000 rows to protect export.
            // Minimal query: no Include, no string.Format/interpolation inside SQL; fallback client-side.
            const int detailCap = 5000;
            var detalleVentasRaw = await _context.DetallesVenta
                .AsNoTracking()
                .Where(d => d.Venta.Id_negocio == negocioId
                            && d.Venta.FechaVenta >= desde
                            && d.Venta.FechaVenta <= hasta
                            && !d.Venta.Anulada)
                .Select(d => new
                {
                    Nombre = d.Producto.Nombre,
                    d.Cantidad,
                    d.PrecioUnitario
                })
                .ToListAsync(ct);

            var detalleVentas = detalleVentasRaw
                .OrderBy(x => x.Nombre ?? "Producto eliminado")
                .Take(detailCap)
                .Select(x => new DetalleVentaInforme(
                    x.Nombre ?? "Producto eliminado",
                    x.Cantidad,
                    x.PrecioUnitario,
                    x.Cantidad * x.PrecioUnitario))
                .ToList();

            var detalleComprasRaw = await _context.DetallesCompra
                .AsNoTracking()
                .Where(d => d.Compra.Id_negocio == negocioId
                            && d.Compra.FechaCompra >= desde
                            && d.Compra.FechaCompra <= hasta
                            && !d.Compra.Anulada)
                .Select(d => new
                {
                    Nombre = d.Producto.Nombre,
                    d.Cantidad,
                    d.PrecioUnitario
                })
                .ToListAsync(ct);

            var detalleCompras = detalleComprasRaw
                .OrderBy(x => x.Nombre ?? "Producto eliminado")
                .Take(detailCap)
                .Select(x => new DetalleCompraInforme(
                    x.Nombre ?? "Producto eliminado",
                    x.Cantidad,
                    x.PrecioUnitario,
                    x.Cantidad * x.PrecioUnitario))
                .ToList();

            return new IngresosGastosResponse(
                VentasTotales: ventasTotales,
                ComprasTotales: comprasTotales,
                GananciaBruta: flujoNeto,
                MargenPorcentaje: margenPorcentaje,
                DetalleVentas: detalleVentas,
                DetalleCompras: detalleCompras,
                Ganancia: ganancia,
                FlujoNeto: flujoNeto
            );
        }

        public async Task<ComprasReposicionResponse> GetComprasReposicionAsync(DateTime? fechaDesde, DateTime? fechaHasta, string? preset, CancellationToken ct)
        {
            var (desde, hasta) = ResolveDateRange(fechaDesde, fechaHasta, preset);
            var negocioId = _currentUser.NegocioId;

            // Compras activas del negocio en el periodo, con su detalle y proveedor.
            var raw = await (
                from d in _context.DetallesCompra.AsNoTracking()
                join c in _context.Compras on d.IdCompra equals c.Id
                join p in _context.Productos on d.IdProducto equals p.Id
                join pr in _context.Proveedores on c.IdProveedor equals pr.Id into proveedores
                from pr in proveedores.DefaultIfEmpty()
                where c.Id_negocio == negocioId
                    && c.FechaCompra >= desde && c.FechaCompra <= hasta
                    && !c.Anulada
                select new
                {
                    d.IdCompra,
                    d.IdProducto,
                    Producto = p.Nombre ?? "Producto eliminado",
                    d.Cantidad,
                    d.PrecioUnitario,
                    Subtotal = d.Cantidad * d.PrecioUnitario,
                    Proveedor = pr != null ? pr.Nombre : "Sin proveedor",
                    c.FechaCompra
                }
            ).ToListAsync(ct);

            // Los totales se calculan sobre todas las filas; solo la tabla se limita para export.
            const int detailCap = 5000;
            var lineas = raw
                .OrderByDescending(x => x.FechaCompra)
                .Take(detailCap)
                .Select(x => new CompraReposicionItem(
                    x.Producto, x.Cantidad, x.PrecioUnitario, x.Subtotal, x.Proveedor, x.FechaCompra))
                .ToList();

            return new ComprasReposicionResponse(
                TotalInvertido: raw.Sum(x => x.Subtotal),
                CantidadCompras: raw.Select(x => x.IdCompra).Distinct().Count(),
                ProductosDistintos: raw.Select(x => x.IdProducto).Distinct().Count(),
                Lineas: lineas
            );
        }

        public async Task<AlertasStockResponse> GetAlertasStockAsync(CancellationToken ct)
        {
            var negocioId = _currentUser.NegocioId;

            // Get products below minimum stock, only products (not services), only active
            var productos = await _context.Productos
                .AsNoTracking()
                .Where(p => p.Id_negocio == negocioId
                            && p.Activo
                            && !p.EsServicio  // Only products, not services
                            && p.StockActual < p.StockMinimo)
                .OrderBy(p => p.StockActual - p.StockMinimo)  // Most critical first — order by expression, NOT by DTO property
                .Select(p => new StockAlertResponse(
                    p.Id,
                    p.Nombre,
                    p.StockActual,
                    p.StockMinimo,
                    p.StockActual - p.StockMinimo
                ))
                .ToListAsync(ct);

            return new AlertasStockResponse(Productos: productos);
        }

        public async Task<VentasPorPagoResponse> GetVentasPorPagoAsync(DateTime? fechaDesde, DateTime? fechaHasta, string? preset, CancellationToken ct)
        {
            var (desde, hasta) = ResolveDateRange(fechaDesde, fechaHasta, preset);
            var negocioId = _currentUser.NegocioId;

            // Get all pagos for non-cancelled ventas within date range
            var pagos = await _context.Pagos
                .AsNoTracking()
                .Include(p => p.Venta)
                .Where(p => p.Venta != null
                            && p.Venta.Id_negocio == negocioId
                            && p.Venta.FechaVenta >= desde
                            && p.Venta.FechaVenta <= hasta
                            && !p.Venta.Anulada)
                .ToListAsync(ct);

            var totalMonto = pagos.Sum(p => p.Monto);

            var grouped = pagos
                .GroupBy(p => p.MetodoPago.ToString())
                .Select(g => new MetodoPagoResponse(
                    Metodo: g.Key,
                    Cantidad: g.Count(),
                    Monto: g.Sum(p => p.Monto),
                    Porcentaje: totalMonto > 0 ? Math.Round((g.Sum(p => p.Monto) / totalMonto) * 100, 2) : 0
                ))
                .OrderByDescending(m => m.Monto)
                .ToList();

            return new VentasPorPagoResponse(Metodos: grouped);
        }

        public async Task<VentasPorVendedorResponse> GetVentasPorVendedorAsync(DateTime? fechaDesde, DateTime? fechaHasta, string? preset, CancellationToken ct)
        {
            var (desde, hasta) = ResolveDateRange(fechaDesde, fechaHasta, preset);
            var negocioId = _currentUser.NegocioId;

            // Get ventas grouped by user
            // Materialize first to avoid EF Core GroupBy + Include translation issue
            var ventas = await _context.Ventas
                .AsNoTracking()
                .Include(v => v.Usuario)
                .Where(v => v.Id_negocio == negocioId
                            && v.FechaVenta >= desde
                            && v.FechaVenta <= hasta
                            && !v.Anulada)
                .ToListAsync(ct);

            var ventasPorVendedor = ventas
                .Where(v => v.Usuario != null)
                .GroupBy(v => new { v.IdUsuario, Nombre = v.Usuario!.Nombre ?? $"Usuario #{v.IdUsuario}", Apellido = v.Usuario!.Apellido ?? "" })
                .Select(g => new VendedorResponse(
                    g.Key.IdUsuario,
                    (g.Key.Nombre + " " + g.Key.Apellido).Trim(),
                    g.Count(),
                    g.Sum(v => v.TotalVenta)
                ))
                .OrderByDescending(v => v.MontoTotal)
                .ToList();

            return new VentasPorVendedorResponse(Vendedores: ventasPorVendedor);
        }
    }
}
