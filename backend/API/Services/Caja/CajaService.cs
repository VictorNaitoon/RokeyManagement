using API.Data;
using API.DTO.Request.Caja;
using API.DTO.Response.Caja;
using API.Models;
using API.Services.Common;
using Microsoft.EntityFrameworkCore;

namespace API.Services.Caja
{
    public class CajaService : ICajaService
    {
        private readonly AppDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public CajaService(AppDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
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

            // Cerrar la caja
            caja.Id_usuario_cierre = userId;
            caja.FechaCierre = DateTime.UtcNow;
            caja.MontoFinal = request.MontoFinal;
            caja.Estado = "Cerrada";

            await _context.SaveChangesAsync(ct);

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

            return MapToMovimientoCajaResponse(movimiento);
        }

        /// <summary>
        /// Obtiene todos los movimientos de una caja
        /// </summary>
        public async Task<IEnumerable<MovimientoCajaResponse>> ObtenerMovimientosAsync(int cajaId, CancellationToken ct = default)
        {
            var movimientos = await _context.MovimientosCaja
                .Where(m => m.Id_caja == cajaId)
                .OrderByDescending(m => m.Fecha)
                .ToListAsync(ct);

            return movimientos.Select(MapToMovimientoCajaResponse);
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

        private static MovimientoCajaResponse MapToMovimientoCajaResponse(MovimientoCaja movimiento)
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
                Id_usuario = movimiento.Id_usuario
            };
        }
    }
}
