using API.Data;
using API.Models;
using API.Services.Suscripcion;
using static API.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace API.Jobs
{
    /// <summary>
    /// Background job que verifica diariamente las suscripciones vencidas.
    /// Corre a las 3:00 AM UTC (ajustable) y:
    /// 1. Marca como Vencidas las suscripciones cuya fecha de próximo pago ya pasó
    /// 2. Envía email de notificación al Dueño del negocio
    /// 3. Envía warning a los que vencen en 3 días
    /// </summary>
    public class SubscriptionExpirationService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<SubscriptionExpirationService> _logger;

        /// <summary>
        /// Hora del día en que corre el job (default: 3 AM UTC)
        /// </summary>
        public int HourToRun { get; set; } = 3;

        public SubscriptionExpirationService(
            IServiceScopeFactory scopeFactory,
            ILogger<SubscriptionExpirationService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            _logger.LogInformation("SubscriptionExpirationService iniciado. Próxima ejecución: {Hour}:00 UTC", HourToRun);

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    // Calcular tiempo hasta la próxima ejecución a la hora configurada
                    var now = DateTime.UtcNow;
                    var nextRun = new DateTime(now.Year, now.Month, now.Day, HourToRun, 0, 0, DateTimeKind.Utc);

                    if (now > nextRun)
                    {
                        // Ya pasó la hora hoy, programar para mañana
                        nextRun = nextRun.AddDays(1);
                    }

                    var delay = nextRun - now;
                    _logger.LogInformation("Esperando {Horas} horas hasta la próxima verificación de suscripciones", delay.TotalHours);

                    await Task.Delay(delay, ct);

                    // Ejecutar la verificación de suscripciones vencidas
                    await CheckAndProcessExpiredSubscriptionsAsync(ct);
                }
                catch (OperationCanceledException)
                {
                    // Cancellation requested
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en SubscriptionExpirationService");

                    // Esperar 1 hora antes de reintentar en caso de error
                    await Task.Delay(TimeSpan.FromHours(1), ct);
                }
            }

            _logger.LogInformation("SubscriptionExpirationService detenido");
        }

        private async Task CheckAndProcessExpiredSubscriptionsAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            _logger.LogInformation("Iniciando verificación de suscripciones vencidas...");

            var now = DateTime.UtcNow;

            // 1. Procesar suscripciones vencidas (FechaProximoPago < now AND Estado == Activa)
            var suscripcionesVencidas = await context.Suscripciones
                .Where(s => s.FechaProximoPago < now && s.Estado == EstadoSuscripcion.Activa)
                .Include(s => s.Plan)
                .Include(s => s.Negocio)
                .ToListAsync(ct);

            foreach (var suscripcion in suscripcionesVencidas)
            {
                try
                {
                    // Cambiar estado a Vencida
                    suscripcion.Estado = EstadoSuscripcion.Vencida;
                    _logger.LogInformation("Suscripción {Id} del negocio {Negocio} marcada como Vencida",
                        suscripcion.Id, suscripcion.Negocio?.Nombre);

                    // Enviar email al Dueño del negocio
                    await NotificarSuscripcionVencidaAsync(context, emailService, suscripcion, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al procesar suscripción vencida {Id}", suscripcion.Id);
                }
            }

            // 2. Procesar suscripciones que vencen en 3 días (warning)
            var fechaWarning = now.AddDays(3);
            var suscripcionesPorVencer = await context.Suscripciones
                .Where(s => s.FechaProximoPago >= now && s.FechaProximoPago <= fechaWarning && s.Estado == EstadoSuscripcion.Activa)
                .Include(s => s.Plan)
                .Include(s => s.Negocio)
                .ToListAsync(ct);

            foreach (var suscripcion in suscripcionesPorVencer)
            {
                try
                {
                    // Enviar email de advertencia
                    await NotificarSuscripcionPorVencerAsync(context, emailService, suscripcion, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al enviar advertencia de suscripción {Id}", suscripcion.Id);
                }
            }

            // Guardar todos los cambios
            await context.SaveChangesAsync(ct);

            _logger.LogInformation("Verificación completada. Vencidas: {Vencidas}, Por vencer: {PorVencer}",
                suscripcionesVencidas.Count, suscripcionesPorVencer.Count);
        }

        private async Task NotificarSuscripcionVencidaAsync(AppDbContext context, IEmailService emailService, Suscripcion suscripcion, CancellationToken ct)
        {
            if (suscripcion.Negocio == null) return;

            var negocio = suscripcion.Negocio;

            // Obtener el email del Dueño del negocio (usuario con rol Admin)
            var emailDueño = await context.Usuarios
                .Where(u => u.Id_negocio == negocio.Id && u.Rol == RolUsuario.Dueño && u.Activo)
                .Select(u => u.Email)
                .FirstOrDefaultAsync(ct);

            if (string.IsNullOrEmpty(emailDueño))
            {
                _logger.LogWarning("No se encontró usuario Administrador para el negocio {Negocio}", negocio.Nombre);
                return;
            }

            var nombrePlan = suscripcion.Plan?.Nombre ?? "Plan desconocido";

            // Enviar email al Dueño
            await emailService.SendSubscriptionExpiredAsync(emailDueño, negocio.Nombre, nombrePlan, ct);

            // Si el email del negocio es diferente, también enviar ahí
            if (!string.IsNullOrEmpty(negocio.Email) && !negocio.Email.Equals(emailDueño, StringComparison.OrdinalIgnoreCase))
            {
                await emailService.SendSubscriptionExpiredAsync(negocio.Email, negocio.Nombre, nombrePlan, ct);
            }

            _logger.LogInformation("Notificación de suscripción vencida enviada para negocio {Negocio}", negocio.Nombre);
        }

        private async Task NotificarSuscripcionPorVencerAsync(AppDbContext context, IEmailService emailService, Suscripcion suscripcion, CancellationToken ct)
        {
            if (suscripcion.Negocio == null || !suscripcion.FechaProximoPago.HasValue) return;

            var negocio = suscripcion.Negocio;

            // Obtener el email del Dueño del negocio (usuario con rol Admin)
            var emailDueño = await context.Usuarios
                .Where(u => u.Id_negocio == negocio.Id && u.Rol == RolUsuario.Dueño && u.Activo)
                .Select(u => u.Email)
                .FirstOrDefaultAsync(ct);

            if (string.IsNullOrEmpty(emailDueño))
            {
                _logger.LogWarning("No se encontró usuario Administrador para el negocio {Negocio}", negocio.Nombre);
                return;
            }

            var fechaVencimiento = suscripcion.FechaProximoPago.Value;

            // Enviar email al Dueño
            await emailService.SendSubscriptionWarningAsync(emailDueño, negocio.Nombre, fechaVencimiento, ct);

            // Si el email del negocio es diferente, también enviar ahí
            if (!string.IsNullOrEmpty(negocio.Email) && !negocio.Email.Equals(emailDueño, StringComparison.OrdinalIgnoreCase))
            {
                await emailService.SendSubscriptionWarningAsync(negocio.Email, negocio.Nombre, fechaVencimiento, ct);
            }

            _logger.LogInformation("Notificación de suscripción por vencer enviada para negocio {Negocio}, vence el {Fecha}",
                negocio.Nombre, fechaVencimiento.ToString("yyyy-MM-dd"));
        }
    }
}