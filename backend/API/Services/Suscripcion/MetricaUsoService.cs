using API.Data;
using API.DTO.Response.Suscripcion;
using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Services.Suscripcion
{
    /// <summary>
    /// Implementación del servicio de métricas de uso
    /// </summary>
    public class MetricaUsoService : IMetricaUsoService
    {
        private readonly AppDbContext _context;

        public MetricaUsoService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<MetricaUsoResponse?> GetMetricaActualAsync(int idNegocio, CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;
            return await GetMetricaByPeriodoAsync(idNegocio, now.Year, now.Month, ct);
        }

        public async Task<MetricaUso> RecordOrUpdateMetricaAsync(int idNegocio, int usuariosActivos, int productosActivos, int transacciones, long almacenamientoBytes, CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;
            var anio = now.Year;
            var mes = now.Month;

            // Buscar métrica existente para el mes actual
            var metrica = await _context.MetricasUso
                .FirstOrDefaultAsync(m => m.Id_negocio == idNegocio && m.Anio == anio && m.Mes == mes, ct);

            if (metrica == null)
            {
                // Crear nueva métrica
                metrica = new MetricaUso
                {
                    Id_negocio = idNegocio,
                    Anio = anio,
                    Mes = mes,
                    TotalUsuarios = usuariosActivos,
                    TotalProductos = productosActivos,
                    TotalTransacciones = transacciones,
                    AlmacenamientoBytes = almacenamientoBytes,
                    TotalAPICalls = 0,
                    UltimaActualizacion = now
                };

                _context.MetricasUso.Add(metrica);
            }
            else
            {
                // Actualizar métrica existente
                metrica.TotalUsuarios = usuariosActivos;
                metrica.TotalProductos = productosActivos;
                metrica.TotalTransacciones = transacciones;
                metrica.AlmacenamientoBytes = almacenamientoBytes;
                metrica.UltimaActualizacion = now;
            }

            await _context.SaveChangesAsync(ct);
            return metrica;
        }

        public async Task<MetricaUsoResponse?> GetMetricaByPeriodoAsync(int idNegocio, int anio, int mes, CancellationToken ct = default)
        {
            var metrica = await _context.MetricasUso
                .FirstOrDefaultAsync(m => m.Id_negocio == idNegocio && m.Anio == anio && m.Mes == mes, ct);

            if (metrica == null)
                return null;

            return new MetricaUsoResponse
            {
                Id = metrica.Id,
                IdNegocio = metrica.Id_negocio,
                Anio = metrica.Anio,
                Mes = metrica.Mes,
                TotalUsuarios = metrica.TotalUsuarios,
                TotalProductos = metrica.TotalProductos,
                TotalTransacciones = metrica.TotalTransacciones,
                AlmacenamientoUsadoGB = Math.Round(metrica.AlmacenamientoBytes / (1024.0 * 1024.0 * 1024.0), 2),
                TotalAPICalls = metrica.TotalAPICalls,
                UltimaActualizacion = metrica.UltimaActualizacion
            };
        }
    }
}
