using API.Data;
using API.DTO.Request.Facturas;
using API.DTO.Response.Facturas;
using API.Models;
using API.Services.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace API.Services.Facturas
{
    /// <summary>
    /// Servicio para la gestión de facturas y notas de crédito.
    /// </summary>
    public class FacturaService : IFacturaService
    {
        private readonly AppDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<FacturaService> _logger;

        public FacturaService(AppDbContext context, ICurrentUserService currentUser, ILogger<FacturaService> logger)
        {
            _context = context;
            _currentUser = currentUser;
            _logger = logger;
        }

        public async Task<FacturaResponse> CrearFacturaProformaAsync(CrearFacturaRequest request, CancellationToken ct)
        {
            // 1. Validar que la venta existe y pertenece al negocio del usuario
            var venta = await _context.Ventas
                .Include(v => v.Factura)
                .FirstOrDefaultAsync(v => v.Id == request.IdVenta && v.Id_negocio == _currentUser.NegocioId, ct);

            if (venta == null)
            {
                throw new InvalidOperationException("Venta no encontrada");
            }

            // 2. Validar que la venta no está anulada
            if (venta.Anulada)
            {
                throw new InvalidOperationException("No se puede crear una factura para una venta anulada");
            }

            // 3. Validar que la venta no tiene una factura asociada (relación 1:1)
            if (venta.Factura != null)
            {
                throw new InvalidOperationException("La venta ya posee una factura asociada");
            }

            // 4. Obtener el número de comprobante secuencial
            var numeroComprobante = await GenerarNumeroComprobanteAsync(ct);

            // 5. Crear la factura proforma (CAE = null)
            var factura = new Factura
            {
                Id_negocio = _currentUser.NegocioId,
                IdVenta = request.IdVenta,
                CuitCliente = request.CUIT_cliente,
                FechaRealizada = DateTime.UtcNow,
                TipoFactura = request.TipoComprobante,
                NumeroComprobante = numeroComprobante,
                CAE = null, // Proforma - sin CAE
                VencimientoCAE = null,
                QR = null,
                CondicionVenta = request.CondicionVenta
            };

            _context.Facturas.Add(factura);
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Factura proforma {FacturaId} creada para venta {VentaId} por usuario {UsuarioId}",
                factura.Id, request.IdVenta, _currentUser.UserId);

            return MapToFacturaResponse(factura);
        }

        public async Task<FacturaResponse> EmitirNotaCreditoAsync(NotaCreditoRequest request, CancellationToken ct)
        {
            // 1. Validar que la venta existe y pertenece al negocio del usuario
            var venta = await _context.Ventas
                .Include(v => v.Factura)
                .FirstOrDefaultAsync(v => v.Id == request.IdVenta && v.Id_negocio == _currentUser.NegocioId, ct);

            if (venta == null)
            {
                throw new InvalidOperationException("Venta no encontrada");
            }

            // 2. Validar que la venta tiene una factura asociada con CAE
            if (venta.Factura == null || string.IsNullOrEmpty(venta.Factura.CAE))
            {
                throw new InvalidOperationException("La venta no tiene una factura oficial asociada (con CAE)");
            }

            // 3. Obtener el número de comprobante secuencial para la nota de crédito
            var numeroComprobante = await GenerarNumeroComprobanteAsync(ct);

            // 4. Crear la nota de crédito
            var notaCredito = new Factura
            {
                Id_negocio = _currentUser.NegocioId,
                IdVenta = request.IdVenta,
                CuitCliente = venta.Factura.CuitCliente,
                FechaRealizada = DateTime.UtcNow,
                TipoFactura = Enums.TipoComprobante.NotaCredito,
                NumeroComprobante = numeroComprobante,
                CAE = null, // Las notas de crédito también son proforma inicialmente
                VencimientoCAE = null,
                QR = null,
                CondicionVenta = $"Nota de Crédito: {request.Motivo}"
            };

            _context.Facturas.Add(notaCredito);
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Nota de crédito {NotaCreditoId} creada para venta {VentaId} por usuario {UsuarioId}. Motivo: {Motivo}",
                notaCredito.Id, request.IdVenta, _currentUser.UserId, request.Motivo);

            return MapToFacturaResponse(notaCredito);
        }

        public async Task<FacturaResponse?> ObtenerFacturaPorIdAsync(int id, CancellationToken ct)
        {
            var factura = await _context.Facturas
                .Where(f => f.Id == id && f.Id_negocio == _currentUser.NegocioId)
                .FirstOrDefaultAsync(ct);

            if (factura == null)
            {
                return null;
            }

            return MapToFacturaResponse(factura);
        }

        public async Task<ListadoFacturaResponse> ObtenerTodasLasFacturasAsync(
            int page,
            int pageSize,
            DateTime? fechaDesde,
            DateTime? fechaHasta,
            CancellationToken ct)
        {
            var query = _context.Facturas
                .Where(f => f.Id_negocio == _currentUser.NegocioId)
                .AsQueryable();

            // Filtrar por rango de fechas
            if (fechaDesde.HasValue)
            {
                query = query.Where(f => f.FechaRealizada >= fechaDesde.Value);
            }

            if (fechaHasta.HasValue)
            {
                query = query.Where(f => f.FechaRealizada <= fechaHasta.Value);
            }

            // Contar total
            var totalCount = await query.CountAsync(ct);

            // Paginar
            var facturas = await query
                .OrderByDescending(f => f.FechaRealizada)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(f => new FacturaListItem
                {
                    Id = f.Id,
                    Id_venta = f.IdVenta,
                    CUIT_cliente = f.CuitCliente,
                    Fecha_realizada = f.FechaRealizada,
                    Tipo_comprobante = f.TipoFactura,
                    Numero_comprobante = f.NumeroComprobante,
                    CAE = f.CAE,
                    Condicion_venta = f.CondicionVenta
                })
                .ToListAsync(ct);

            return new ListadoFacturaResponse
            {
                Items = facturas,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<bool> ValidarVentaSinFacturaAsync(int idVenta, CancellationToken ct)
        {
            var venta = await _context.Ventas
                .Include(v => v.Factura)
                .FirstOrDefaultAsync(v => v.Id == idVenta && v.Id_negocio == _currentUser.NegocioId, ct);

            if (venta == null)
            {
                throw new InvalidOperationException("Venta no encontrada");
            }

            // Retorna true si NO tiene factura asociada
            return venta.Factura == null;
        }

        /// <summary>
        /// Genera el número de comprobante en formato "0001-00000001" usando PuntoDeVenta del negocio.
        /// </summary>
        private async Task<string> GenerarNumeroComprobanteAsync(CancellationToken ct)
        {
            // Obtener el negocio para el PuntoDeVenta
            var negocio = await _context.Negocios.FindAsync(new object[] { _currentUser.NegocioId }, ct);
            
            // Obtener el último número de comprobante para este negocio
            var ultimaFactura = await _context.Facturas
                .Where(f => f.Id_negocio == _currentUser.NegocioId)
                .OrderByDescending(f => f.Id)
                .FirstOrDefaultAsync(ct);

            int numeroSecuencial = 1;
            if (ultimaFactura != null)
            {
                // Extraer el número secuencial del último comprobante
                var partes = ultimaFactura.NumeroComprobante.Split('-');
                if (partes.Length == 2 && int.TryParse(partes[1], out int ultimoNumero))
                {
                    numeroSecuencial = ultimoNumero + 1;
                }
            }

            // Obtener el punto de venta (default 1 si no existe)
            var puntoVenta = negocio?.PuntoDeVenta ?? "1";
            
            // Formato: 0001-00000001
            return $"{puntoVenta.PadLeft(4, '0')}-{numeroSecuencial.ToString().PadLeft(8, '0')}";
        }

        private static FacturaResponse MapToFacturaResponse(Factura factura)
        {
            return new FacturaResponse
            {
                Id = factura.Id,
                Id_negocio = factura.Id_negocio,
                Id_venta = factura.IdVenta,
                CUIT_cliente = factura.CuitCliente,
                Fecha_realizada = factura.FechaRealizada,
                Tipo_comprobante = factura.TipoFactura,
                Numero_comprobante = factura.NumeroComprobante,
                CAE = factura.CAE,
                Vencimiento_CAE = factura.VencimientoCAE,
                QR = factura.QR,
                Condicion_venta = factura.CondicionVenta
            };
        }
    }
}
