using API.Data;
using API.DTO.Response.Suscripcion;
using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Services.Suscripcion
{
    /// <summary>
    /// Implementación del servicio de consulta de planes
    /// </summary>
    public class PlanService : IPlanService
    {
        private readonly AppDbContext _context;

        public PlanService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<PlanSuscResponse>> GetAllPlanesAsync(CancellationToken ct = default)
        {
            var planes = await _context.Planes
                .Where(p => p.Activo)
                .OrderBy(p => p.Orden)
                .Select(p => new PlanSuscResponse
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    PrecioMensual = p.PrecioMensual,
                    PrecioAnual = p.PrecioAnual,
                    MaxUsuarios = p.MaxUsuarios,
                    MaxProductos = p.MaxProductos,
                    MaxTransaccionesMes = p.MaxTransaccionesMes,
                    SoportePrioritario = p.SoportePrioritario,
                    MultiSucursal = p.MultiSucursal,
                    APIAccess = p.APIAccess,
                    Activo = p.Activo,
                    Orden = p.Orden
                })
                .ToListAsync(ct);

            return planes;
        }

        public async Task<PlanSuscResponse?> GetPlanByIdAsync(int id, CancellationToken ct = default)
        {
            var plan = await _context.Planes
                .Where(p => p.Id == id && p.Activo)
                .Select(p => new PlanSuscResponse
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    PrecioMensual = p.PrecioMensual,
                    PrecioAnual = p.PrecioAnual,
                    MaxUsuarios = p.MaxUsuarios,
                    MaxProductos = p.MaxProductos,
                    MaxTransaccionesMes = p.MaxTransaccionesMes,
                    SoportePrioritario = p.SoportePrioritario,
                    MultiSucursal = p.MultiSucursal,
                    APIAccess = p.APIAccess,
                    Activo = p.Activo,
                    Orden = p.Orden
                })
                .FirstOrDefaultAsync(ct);

            return plan;
        }
    }
}
