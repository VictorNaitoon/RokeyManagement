using API.Data;
using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Jobs
{
    /// <summary>
    /// Background job que limpia carritos internos abandonados cada día a las 3 AM.
    /// Elimina carritos que no han sido convertidos y tienen más de 30 días sin actividad.
    /// </summary>
    public class CarritoInternoCleanupJob : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<CarritoInternoCleanupJob> _logger;

        public CarritoInternoCleanupJob(
            IServiceScopeFactory scopeFactory,
            ILogger<CarritoInternoCleanupJob> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            _logger.LogInformation("CarritoInternoCleanupJob iniciado. Próxima ejecución: 3:00 AM");

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    // Calcular tiempo hasta las 3 AM
                    var now = DateTime.UtcNow;
                    var nextRun = new DateTime(now.Year, now.Month, now.Day, 3, 0, 0, DateTimeKind.Utc);
                    
                    if (now > nextRun)
                    {
                        // Ya pasaron las 3 AM hoy, programar para mañana
                        nextRun = nextRun.AddDays(1);
                    }

                    var delay = nextRun - now;
                    _logger.LogInformation("Esperando {Horas} horas hasta la próxima limpieza", delay.TotalHours);

                    await Task.Delay(delay, ct);

                    // Ejecutar la limpieza
                    await LimpiarCarritosAbandonadosAsync(ct);
                }
                catch (OperationCanceledException)
                {
                    // Cancellation requested
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en CarritoInternoCleanupJob");
                    
                    // Esperar 1 hora antes de reintentar en caso de error
                    await Task.Delay(TimeSpan.FromHours(1), ct);
                }
            }

            _logger.LogInformation("CarritoInternoCleanupJob detenido");
        }

        private async Task LimpiarCarritosAbandonadosAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            _logger.LogInformation("Iniciando limpieza de carritos abandonados...");

            // Carritos sin actividad por más de 30 días que no están convertidos
            var cutoffDate = DateTime.UtcNow.AddDays(-30);
            
            var carritosAbandonados = await context.CarritosInternos
                .Where(c => c.Estado != Enums.EstadoCarritoInterno.Convertido
                    && c.FechaActualizacion < cutoffDate)
                .ToListAsync(ct);

            if (carritosAbandonados.Any())
            {
                context.CarritosInternos.RemoveRange(carritosAbandonados);
                await context.SaveChangesAsync(ct);

                _logger.LogInformation("Se eliminaron {Cantidad} carritos abandonados", carritosAbandonados.Count);
            }
            else
            {
                _logger.LogInformation("No se encontraron carritos abandonados para eliminar");
            }
        }
    }
}
