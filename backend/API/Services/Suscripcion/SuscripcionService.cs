using API.Data;
using API.DTO.Request.Suscripcion;
using API.DTO.Response.Suscripcion;
using API.Models;
using static API.Models.Enums;
using Microsoft.EntityFrameworkCore;
using SuscripcionModel = API.Models.Suscripcion;

namespace API.Services.Suscripcion
{
    /// <summary>
    /// Implementación del servicio de gestión de suscripciones
    /// </summary>
    public class SuscripcionService : ISuscripcionService
    {
        private readonly AppDbContext _context;
        private readonly IMetricaUsoService _metricaUsoService;

        public SuscripcionService(AppDbContext context, IMetricaUsoService metricaUsoService)
        {
            _context = context;
            _metricaUsoService = metricaUsoService;
        }

        public async Task<SuscripcionResponse?> GetSuscripcionByNegocioAsync(int idNegocio, CancellationToken ct = default)
        {
            var suscripcion = await _context.Suscripciones
                .Where(s => s.Id_negocio == idNegocio)
                .Include(s => s.Plan)
                .OrderByDescending(s => s.FechaInicio)
                .FirstOrDefaultAsync(ct);

            if (suscripcion == null)
                return null;

            return MapToResponse(suscripcion);
        }

        public async Task<SuscripcionResponse> CreateSuscripcionAsync(int idNegocio, SuscripcionRequest request, CancellationToken ct = default)
        {
            // Validar que el plan existe y está activo
            var plan = await _context.Planes
                .FirstOrDefaultAsync(p => p.Id == request.IdPlan && p.Activo, ct);

            if (plan == null)
            {
                throw new InvalidOperationException($"El plan con ID {request.IdPlan} no existe o no está activo");
            }

            // Verificar si ya existe una suscripción activa
            var suscripcionExistente = await _context.Suscripciones
                .Where(s => s.Id_negocio == idNegocio && s.Estado == EstadoSuscripcion.Activa)
                .FirstOrDefaultAsync(ct);

            if (suscripcionExistente != null)
            {
                throw new InvalidOperationException("El negocio ya tiene una suscripción activa");
            }

            // Calcular fecha de inicio y fin según el tipo de facturación
            var fechaInicio = DateTime.UtcNow;
            var fechaFin = request.TipoFacturacion == TipoFacturacion.Anual
                ? fechaInicio.AddYears(1)
                : fechaInicio.AddMonths(1);

            var fechaProximoPago = request.TipoFacturacion == TipoFacturacion.Anual
                ? fechaInicio.AddYears(1)
                : fechaInicio.AddMonths(1);

            var suscripcion = new SuscripcionModel
            {
                Id_negocio = idNegocio,
                IdPlan = request.IdPlan,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                FechaProximoPago = fechaProximoPago,
                Estado = EstadoSuscripcion.Activa,
                TipoFacturacion = request.TipoFacturacion
            };

            _context.Suscripciones.Add(suscripcion);
            await _context.SaveChangesAsync(ct);

            // Recargar con el plan para devolver la respuesta completa
            suscripcion = await _context.Suscripciones
                .Where(s => s.Id == suscripcion.Id)
                .Include(s => s.Plan)
                .FirstAsync(ct);

            return MapToResponse(suscripcion);
        }

        public async Task<SuscripcionResponse> UpdateSuscripcionAsync(int idNegocio, SuscripcionRequest request, CancellationToken ct = default)
        {
            var suscripcion = await _context.Suscripciones
                .Where(s => s.Id_negocio == idNegocio)
                .Include(s => s.Plan)
                .FirstOrDefaultAsync(ct);

            if (suscripcion == null)
            {
                throw new InvalidOperationException("No se encontró una suscripción para este negocio");
            }

            // Validar estado para permitir actualización
            if (suscripcion.Estado != EstadoSuscripcion.Activa && suscripcion.Estado != EstadoSuscripcion.PendientePago)
            {
                throw new InvalidOperationException($"No se puede actualizar una suscripción en estado {suscripcion.Estado}");
            }

            // Validar que el nuevo plan existe y está activo
            var nuevoPlan = await _context.Planes
                .FirstOrDefaultAsync(p => p.Id == request.IdPlan && p.Activo, ct);

            if (nuevoPlan == null)
            {
                throw new InvalidOperationException($"El plan con ID {request.IdPlan} no existe o no está activo");
            }

            // Si cambia el tipo de facturación, recalcular fechas
            if (suscripcion.TipoFacturacion != request.TipoFacturacion)
            {
                var fechaActual = DateTime.UtcNow;
                suscripcion.FechaFin = request.TipoFacturacion == TipoFacturacion.Anual
                    ? fechaActual.AddYears(1)
                    : fechaActual.AddMonths(1);

                suscripcion.FechaProximoPago = request.TipoFacturacion == TipoFacturacion.Anual
                    ? fechaActual.AddYears(1)
                    : fechaActual.AddMonths(1);
            }

            suscripcion.IdPlan = request.IdPlan;
            suscripcion.TipoFacturacion = request.TipoFacturacion;

            // Si estaba pendiente de pago, activarla al actualizarla
            if (suscripcion.Estado == EstadoSuscripcion.PendientePago)
            {
                suscripcion.Estado = EstadoSuscripcion.Activa;
            }

            await _context.SaveChangesAsync(ct);

            // Recargar con el nuevo plan
            suscripcion = await _context.Suscripciones
                .Where(s => s.Id == suscripcion.Id)
                .Include(s => s.Plan)
                .FirstAsync(ct);

            return MapToResponse(suscripcion);
        }

        public async Task<SuscripcionResponse> CancelarSuscripcionAsync(int idNegocio, CancelarSuscripcionRequest request, CancellationToken ct = default)
        {
            var suscripcion = await _context.Suscripciones
                .Where(s => s.Id_negocio == idNegocio)
                .Include(s => s.Plan)
                .FirstOrDefaultAsync(ct);

            if (suscripcion == null)
            {
                throw new InvalidOperationException("No se encontró una suscripción para este negocio");
            }

            // Validar estado para permitir cancelación
            if (suscripcion.Estado != EstadoSuscripcion.Activa && suscripcion.Estado != EstadoSuscripcion.PendientePago)
            {
                throw new InvalidOperationException($"No se puede cancelar una suscripción en estado {suscripcion.Estado}");
            }

            // Marcar como cancelada - la fecha de fin se mantiene como estaba
            suscripcion.Estado = EstadoSuscripcion.Cancelada;
            suscripcion.FechaCancelacion = DateTime.UtcNow;
            suscripcion.MotivoCancelacion = request.MotivoCancelacion;

            await _context.SaveChangesAsync(ct);

            return MapToResponse(suscripcion);
        }

        public async Task<CheckLimitesResponse> CheckLimitesAsync(int idNegocio, CancellationToken ct = default)
        {
            var respuesta = new CheckLimitesResponse
            {
                DentroLimites = true,
                LimitesExcedidos = new List<CheckLimitesResponse.LimiteExcedido>()
            };

            // Obtener la suscripción más reciente (activa o pendiente de pago)
            var suscripcion = await _context.Suscripciones
                .Where(s => s.Id_negocio == idNegocio 
                    && (s.Estado == EstadoSuscripcion.Activa || s.Estado == EstadoSuscripcion.PendientePago))
                .Include(s => s.Plan)
                .OrderByDescending(s => s.FechaInicio)
                .FirstOrDefaultAsync(ct);

            if (suscripcion == null)
            {
                throw new InvalidOperationException("El negocio no tiene una suscripción activa");
            }

            // Obtener métricas actuales de uso (puede ser null si no hay registros)
            var metricas = await _metricaUsoService.GetMetricaActualAsync(idNegocio, ct);

            // Comparar con límites del plan
            var plan = suscripcion.Plan;

            // Si no hay métricas, el negocio está dentro de límites (0 uso)
            var usuariosActuales = metricas?.TotalUsuarios ?? 0;
            var productosActuales = metricas?.TotalProductos ?? 0;
            var transaccionesActuales = metricas?.TotalTransacciones ?? 0;

            if (usuariosActuales > plan.MaxUsuarios)
            {
                respuesta.DentroLimites = false;
                respuesta.LimitesExcedidos.Add(new CheckLimitesResponse.LimiteExcedido
                {
                    Recurso = "Usuarios",
                    Actual = usuariosActuales,
                    Limite = plan.MaxUsuarios
                });
            }

            if (productosActuales > plan.MaxProductos)
            {
                respuesta.DentroLimites = false;
                respuesta.LimitesExcedidos.Add(new CheckLimitesResponse.LimiteExcedido
                {
                    Recurso = "Productos",
                    Actual = productosActuales,
                    Limite = plan.MaxProductos
                });
            }

            if (transaccionesActuales > plan.MaxTransaccionesMes)
            {
                respuesta.DentroLimites = false;
                respuesta.LimitesExcedidos.Add(new CheckLimitesResponse.LimiteExcedido
                {
                    Recurso = "Transacciones Mensuales",
                    Actual = transaccionesActuales,
                    Limite = plan.MaxTransaccionesMes
                });
            }

            return respuesta;
        }

        public async Task<EstadoSuscripcion?> GetEstadoSuscripcionAsync(int idNegocio, CancellationToken ct = default)
        {
            var suscripcion = await _context.Suscripciones
                .Where(s => s.Id_negocio == idNegocio)
                .OrderByDescending(s => s.FechaInicio)
                .FirstOrDefaultAsync(ct);

            return suscripcion?.Estado;
        }

        private static SuscripcionResponse MapToResponse(SuscripcionModel suscripcion)
        {
            var precio = suscripcion.TipoFacturacion == TipoFacturacion.Mensual
                ? suscripcion.Plan?.PrecioMensual ?? 0
                : suscripcion.Plan?.PrecioAnual ?? 0;

            return new SuscripcionResponse
            {
                Id = suscripcion.Id,
                IdNegocio = suscripcion.Id_negocio,
                IdPlan = suscripcion.IdPlan,
                NombrePlan = suscripcion.Plan?.Nombre,
                DescripcionPlan = suscripcion.Plan?.Descripcion,
                Estado = suscripcion.Estado,
                TipoFacturacion = suscripcion.TipoFacturacion,
                Precio = precio,
                FechaInicio = suscripcion.FechaInicio,
                FechaFin = suscripcion.FechaFin,
                FechaProximoPago = suscripcion.FechaProximoPago,
                FechaCancelacion = suscripcion.FechaCancelacion,
                MotivoCancelacion = suscripcion.MotivoCancelacion,
                MaxUsuarios = suscripcion.Plan?.MaxUsuarios ?? 0,
                MaxProductos = suscripcion.Plan?.MaxProductos ?? 0,
                MaxTransaccionesMes = suscripcion.Plan?.MaxTransaccionesMes ?? 0,
                SoportePrioritario = suscripcion.Plan?.SoportePrioritario ?? false,
                MultiSucursal = suscripcion.Plan?.MultiSucursal ?? false,
                APIAccess = suscripcion.Plan?.APIAccess ?? false
            };
        }
    }
}
