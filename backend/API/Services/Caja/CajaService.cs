using API.Data;
using API.DTO.Request.Caja;
using API.DTO.Response.Caja;
using API.Models;
using API.Services.Auditoria;
using API.Services.Common;
using Microsoft.EntityFrameworkCore;

namespace API.Services.Caja
{
    public class CajaService : ICajaService
    {
        private readonly AppDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IAuditoriaService _auditoriaService;

        public CajaService(AppDbContext context, ICurrentUserService currentUser, IAuditoriaService auditoriaService)
        {
            _context = context;
            _currentUser = currentUser;
            _auditoriaService = auditoriaService;
        }

        /// <summary>
        /// Abre una nueva caja para el negocio
        /// </summary>
        public async Task<CajaResponse> AbrirCajaAsync(AperturaCajaRequest request, int userId, int negocioId, CancellationToken ct = default)
        {
            // Validar que no exista una caja abierta para este negocio
            var cajaAbierta = await _context.Cajas
                .FirstOrDefaultAsync(c => c.Id_negocio == negocioId && c.Estado == "Abierta", ct);

            if (cajaAbierta != null)
            {
                throw new InvalidOperationException("Ya existe una caja abierta para este negocio.");
            }

            // Crear la nueva caja
            var caja = new Models.Caja
            {
                Id_negocio = negocioId,
                Id_usuario_apertura = userId,
                FechaApertura = DateTime.UtcNow,
                MontoInicial = request.MontoInicial,
                Estado = "Abierta"
            };

            _context.Cajas.Add(caja);
            await _context.SaveChangesAsync(ct);

            // Registrar auditoría
            await _auditoriaService.RegistrarAsync(
                "Caja",
                caja.Id,
                "CREATE",
                null,
                new
                {
                    caja.MontoInicial,
                    caja.Estado,
                    caja.FechaApertura
                }, ct);

            return MapToCajaResponse(caja);
        }

        /// <summary>
        /// Cierra la caja abierta del negocio
        /// </summary>
        public async Task<CajaResponse> CerrarCajaAsync(CierreCajaRequest request, int userId, int negocioId, CancellationToken ct = default)
        {
            // Buscar la caja abierta
            var caja = await _context.Cajas
                .FirstOrDefaultAsync(c => c.Id_negocio == negocioId && c.Estado == "Abierta", ct);

            if (caja == null)
            {
                throw new InvalidOperationException("No hay una caja abierta para cerrar.");
            }

            // Capturar estado antes de modificar para auditoría
            var datosAnteriores = new
            {
                caja.MontoInicial,
                caja.Estado,
                caja.FechaCierre
            };

            // Cerrar la caja
            caja.Id_usuario_cierre = userId;
            caja.FechaCierre = DateTime.UtcNow;
            caja.MontoFinal = request.MontoFinal;
            caja.Estado = "Cerrada";
            caja.IdUsuarioModificador = userId;

            await _context.SaveChangesAsync(ct);

            // Registrar auditoría
            await _auditoriaService.RegistrarAsync(
                "Caja",
                caja.Id,
                "UPDATE",
                datosAnteriores,
                new
                {
                    caja.MontoFinal,
                    caja.Estado,
                    caja.FechaCierre
                }, ct);

            return MapToCajaResponse(caja);
        }

        /// <summary>
        /// Obtiene el estado de caja del negocio
        /// </summary>
        public async Task<EstadoCajaResponse> ObtenerEstadoCajaAsync(int negocioId, CancellationToken ct = default)
        {
            var caja = await _context.Cajas
                .FirstOrDefaultAsync(c => c.Id_negocio == negocioId && c.Estado == "Abierta", ct);

            return new EstadoCajaResponse
            {
                TieneCajaAbierta = caja != null,
                Caja = caja != null ? MapToCajaResponse(caja) : null
            };
        }

        /// <summary>
        /// Agrega un movimiento a la caja abierta
        /// </summary>
        public async Task<MovimientoCajaResponse> AgregarMovimientoAsync(AgregarMovimientoCajaRequest request, int userId, int negocioId, CancellationToken ct = default)
        {
            // Buscar la caja abierta
            var caja = await _context.Cajas
                .FirstOrDefaultAsync(c => c.Id_negocio == negocioId && c.Estado == "Abierta", ct);

            if (caja == null)
            {
                throw new InvalidOperationException("No hay una caja abierta para agregar movimientos.");
            }

            // Validar tipo de movimiento
            if (request.Tipo != "Ingreso" && request.Tipo != "Egreso")
            {
                throw new InvalidOperationException("El tipo de movimiento debe ser 'Ingreso' o 'Egreso'.");
            }

            // Validar monto
            if (request.Monto <= 0)
            {
                throw new InvalidOperationException("El monto debe ser mayor a cero.");
            }

            // Crear el movimiento
            var movimiento = new MovimientoCaja
            {
                Id_caja = caja.Id,
                Id_negocio = negocioId,
                Tipo = request.Tipo,
                Monto = request.Monto,
                Descripcion = request.Descripcion,
                Fecha = DateTime.UtcNow,
                Id_usuario = userId
            };

            _context.MovimientosCaja.Add(movimiento);
            await _context.SaveChangesAsync(ct);

            var usuarioNombre = await _context.Usuarios
                .Where(u => u.Id == userId)
                .Select(u => u.Nombre + " " + u.Apellido)
                .FirstOrDefaultAsync(ct);

            return MapToMovimientoCajaResponse(movimiento, usuarioNombre);
        }

        /// <summary>
        /// Obtiene todos los movimientos de una caja (tenant-filtered)
        /// </summary>
        public async Task<IEnumerable<MovimientoCajaResponse>> ObtenerMovimientosAsync(int cajaId, CancellationToken ct = default)
        {
            // Legacy overload kept for compat; delegates to tenant-filtered version using current user
            return await ObtenerMovimientosAsync(cajaId, _currentUser.NegocioId, ct);
        }

        /// <summary>
        /// Obtiene todos los movimientos de una caja verificando pertenencia al negocio
        /// </summary>
        public async Task<IEnumerable<MovimientoCajaResponse>> ObtenerMovimientosAsync(int cajaId, int negocioId, CancellationToken ct = default)
        {
            // Verify caja belongs to negocio to enforce tenant isolation
            var cajaPertenece = await _context.Cajas
                .AnyAsync(c => c.Id == cajaId && c.Id_negocio == negocioId, ct);

            if (!cajaPertenece)
            {
                throw new KeyNotFoundException($"Caja con id '{cajaId}' no encontrada para este negocio.");
            }

            var movimientosConUsuario = await (
                from m in _context.MovimientosCaja
                join u in _context.Usuarios on m.Id_usuario equals u.Id into gj
                from u in gj.DefaultIfEmpty()
                where m.Id_caja == cajaId && m.Id_negocio == negocioId
                orderby m.Fecha descending
                select new
                {
                    movimiento = m,
                    usuarioNombre = u != null ? u.Nombre + " " + u.Apellido : null
                }
            ).ToListAsync(ct);

            return movimientosConUsuario.Select(x => MapToMovimientoCajaResponse(x.movimiento, x.usuarioNombre));
        }

        /// <summary>
        /// Calcula la ganancia real de una caja (tenant-filtered, agregado sin costos por producto)
        /// </summary>
        public async Task<GananciaCajaResponse> ObtenerGananciaAsync(int cajaId, int negocioId, CancellationToken ct = default)
        {
            // Verify caja belongs to negocio to enforce tenant isolation
            var cajaPertenece = await _context.Cajas
                .AnyAsync(c => c.Id == cajaId && c.Id_negocio == negocioId, ct);

            if (!cajaPertenece)
            {
                throw new KeyNotFoundException($"Caja con id '{cajaId}' no encontrada para este negocio.");
            }

            // Venta IDs are recorded in movimientos as "Venta #<id>" (VentaService)
            var descripciones = await _context.MovimientosCaja
                .Where(m => m.Id_caja == cajaId && m.Id_negocio == negocioId && m.Descripcion != null)
                .Select(m => m.Descripcion!)
                .ToListAsync(ct);

            var ventaIds = descripciones
                .Select(d =>
                {
                    const string prefix = "Venta #";
                    if (!d.StartsWith(prefix)) return (int?)null;
                    return int.TryParse(d.Substring(prefix.Length).Trim(), out var id) ? id : (int?)null;
                })
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .Distinct()
                .ToList();

            if (ventaIds.Count == 0)
            {
                return new GananciaCajaResponse { Ganancia = 0, VentasCantidad = 0 };
            }

            // Only active (non-voided) sales of this tenant count
            var ventasActivasIds = await _context.Ventas
                .Where(v => ventaIds.Contains(v.Id) && v.Id_negocio == negocioId && !v.Anulada)
                .Select(v => v.Id)
                .ToListAsync(ct);

            if (ventasActivasIds.Count == 0)
            {
                return new GananciaCajaResponse { Ganancia = 0, VentasCantidad = 0 };
            }

            var ganancia = await (
                from d in _context.DetallesVenta
                join p in _context.Productos on d.IdProducto equals p.Id
                where ventasActivasIds.Contains(d.IdVenta)
                    && p.Id_negocio == negocioId
                select (d.PrecioUnitario - p.PrecioCompra) * d.Cantidad
            ).SumAsync(ct);

            return new GananciaCajaResponse
            {
                Ganancia = ganancia,
                VentasCantidad = ventasActivasIds.Count
            };
        }

        /// <summary>
        /// Verifica si el negocio tiene una caja abierta
        /// </summary>
        public async Task<bool> TieneCajaAbiertaAsync(int negocioId, CancellationToken ct = default)
        {
            return await _context.Cajas
                .AnyAsync(c => c.Id_negocio == negocioId && c.Estado == "Abierta", ct);
        }

        private static CajaResponse MapToCajaResponse(Models.Caja caja)
        {
            return new CajaResponse
            {
                Id = caja.Id,
                Id_negocio = caja.Id_negocio,
                Id_usuario_apertura = caja.Id_usuario_apertura,
                Id_usuario_cierre = caja.Id_usuario_cierre,
                FechaApertura = caja.FechaApertura,
                FechaCierre = caja.FechaCierre,
                MontoInicial = caja.MontoInicial,
                MontoFinal = caja.MontoFinal,
                Estado = caja.Estado
            };
        }

        private static MovimientoCajaResponse MapToMovimientoCajaResponse(MovimientoCaja movimiento, string? usuarioNombre = null)
        {
            return new MovimientoCajaResponse
            {
                Id = movimiento.Id,
                Id_caja = movimiento.Id_caja,
                Id_negocio = movimiento.Id_negocio,
                Tipo = movimiento.Tipo,
                Monto = movimiento.Monto,
                Descripcion = movimiento.Descripcion,
                Fecha = movimiento.Fecha,
                Id_usuario = movimiento.Id_usuario,
                UsuarioNombre = usuarioNombre
            };
        }
    }
}
