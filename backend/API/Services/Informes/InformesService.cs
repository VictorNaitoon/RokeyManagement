using API.Data;
using API.DTO.Response.Informes;
using API.Models;
using API.Services.Common;
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

            // If explicit dates provided, use them (ensure UTC)
            if (fechaDesde.HasValue && fechaHasta.HasValue)
            {
                desde = fechaDesde.Value.ToUniversalTime();
                hasta = fechaHasta.Value.ToUniversalTime();
            }
            else
            {
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

            // Validate date range doesn't exceed 12 months
            if ((hasta - desde).TotalDays > 365)
            {
                throw new InvalidOperationException("El rango de fechas no puede exceder 12 meses.");
            }

            return (desde, hasta);
        }

        public async Task<VentasResumenResponse> GetVentasResumenAsync(DateTime? fechaDesde, DateTime? fechaHasta, string? preset, CancellationToken ct)
        {
            var (desde, hasta) = ResolveDateRange(fechaDesde, fechaHasta, preset);
            var negocioId = _currentUser.NegocioId;

            var ventas = await _context.Ventas
                .Where(v => v.Id_negocio == negocioId && v.FechaVenta >= desde && v.FechaVenta <= hasta)
                .ToListAsync(ct);

            var ventasActivas = ventas.Where(v => !v.Anulada).ToList();
            var ventasAnuladas = ventas.Where(v => v.Anulada).ToList();

            var totalVentas = ventasActivas.Sum(v => v.TotalVenta);
            var cantidadVentas = ventasActivas.Count;
            var ticketPromedio = cantidadVentas > 0 ? totalVentas / cantidadVentas : 0;

            return new VentasResumenResponse(
                TotalVentas: totalVentas,
                CantidadVentas: cantidadVentas,
                TicketPromedio: ticketPromedio,
                VentasAnuladas: ventasAnuladas.Count,
                Periodo: $"{desde:yyyy-MM-dd} al {hasta:yyyy-MM-dd}"
            );
        }

        public async Task<ProductosTopResponse> GetProductosTopAsync(int cantidad, DateTime? fechaDesde, DateTime? fechaHasta, CancellationToken ct)
        {
            // Default to 10 if not provided
            var limite = cantidad > 0 ? Math.Min(cantidad, 50) : 10;

            var (desde, hasta) = ResolveDateRange(fechaDesde, fechaHasta, null);
            var negocioId = _currentUser.NegocioId;

            // Get top products by revenue, excluding services
            // Materialize first to avoid EF Core GroupBy + Include translation issue
            var detalles = await _context.DetallesVenta
                .Include(d => d.Venta)
                .Include(d => d.Producto)
                .Where(d => d.Venta.Id_negocio == negocioId
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

        public async Task<FlujoCajaResponse> GetFlujoCajaAsync(DateTime? fechaDesde, DateTime? fechaHasta, CancellationToken ct)
        {
            var (desde, hasta) = ResolveDateRange(fechaDesde, fechaHasta, null);
            var negocioId = _currentUser.NegocioId;

            // Get movimientos de caja within date range, only closed caja
            var movimientos = await _context.MovimientosCaja
                .Include(m => m.Caja)
                .Where(m => m.Id_negocio == negocioId
                            && m.Fecha >= desde
                            && m.Fecha <= hasta
                            && m.Caja.Estado == "Cerrada")  // Only closed caja
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

        public async Task<IngresosGastosResponse> GetIngresosGastosAsync(DateTime? fechaDesde, DateTime? fechaHasta, CancellationToken ct)
        {
            var (desde, hasta) = ResolveDateRange(fechaDesde, fechaHasta, null);
            var negocioId = _currentUser.NegocioId;

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

            var gananciaBruta = ventasTotales - comprasTotales;
            var margenPorcentaje = ventasTotales > 0
                ? Math.Round((gananciaBruta / ventasTotales) * 100, 2)
                : 0;

            return new IngresosGastosResponse(
                VentasTotales: ventasTotales,
                ComprasTotales: comprasTotales,
                GananciaBruta: gananciaBruta,
                MargenPorcentaje: margenPorcentaje
            );
        }

        public async Task<AlertasStockResponse> GetAlertasStockAsync(CancellationToken ct)
        {
            var negocioId = _currentUser.NegocioId;

            // Get products below minimum stock, only products (not services), only active
            var productos = await _context.Productos
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

        public async Task<VentasPorPagoResponse> GetVentasPorPagoAsync(DateTime? fechaDesde, DateTime? fechaHasta, CancellationToken ct)
        {
            var (desde, hasta) = ResolveDateRange(fechaDesde, fechaHasta, null);
            var negocioId = _currentUser.NegocioId;

            // Get all pagos for non-cancelled ventas within date range
            var pagos = await _context.Pagos
                .Include(p => p.Venta)
                .Where(p => p.Venta.Id_negocio == negocioId
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

        public async Task<VentasPorVendedorResponse> GetVentasPorVendedorAsync(DateTime? fechaDesde, DateTime? fechaHasta, CancellationToken ct)
        {
            var (desde, hasta) = ResolveDateRange(fechaDesde, fechaHasta, null);
            var negocioId = _currentUser.NegocioId;

            // Get ventas grouped by user
            // Materialize first to avoid EF Core GroupBy + Include translation issue
            var ventas = await _context.Ventas
                .Include(v => v.Usuario)
                .Where(v => v.Id_negocio == negocioId
                            && v.FechaVenta >= desde
                            && v.FechaVenta <= hasta
                            && !v.Anulada)
                .ToListAsync(ct);

            var ventasPorVendedor = ventas
                .GroupBy(v => new { v.IdUsuario, v.Usuario.Nombre, v.Usuario.Apellido })
                .Select(g => new VendedorResponse(
                    g.Key.IdUsuario,
                    g.Key.Nombre + " " + g.Key.Apellido,
                    g.Count(),
                    g.Sum(v => v.TotalVenta)
                ))
                .OrderByDescending(v => v.MontoTotal)
                .ToList();

            return new VentasPorVendedorResponse(Vendedores: ventasPorVendedor);
        }
    }
}
