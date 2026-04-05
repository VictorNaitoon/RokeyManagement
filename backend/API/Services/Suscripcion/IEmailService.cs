namespace API.Services.Suscripcion
{
    /// <summary>
    /// Interfaz para el servicio de email de suscripciones
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Envía email de notificación de suscripción vencida
        /// </summary>
        Task SendSubscriptionExpiredAsync(string emailDestino, string nombreNegocio, string nombrePlan, CancellationToken ct = default);

        /// <summary>
        /// Envía email de advertencia de suscripción por vencer
        /// </summary>
        Task SendSubscriptionWarningAsync(string emailDestino, string nombreNegocio, DateTime fechaVencimiento, CancellationToken ct = default);
    }
}
