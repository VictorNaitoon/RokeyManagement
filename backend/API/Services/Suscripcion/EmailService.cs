using Microsoft.Extensions.Logging;

namespace API.Services.Suscripcion
{
    /// <summary>
    /// Implementación del servicio de email para notificaciones de suscripción.
    /// Por ahora es un stub que escribe a la consola/log.
    /// 
    /// Para implementar con SMTP real:
    /// 1. Agregar Microsoft.Extensions.Mail en el proyecto
    /// 2. Crear SmtpEmailService que implemente IEmailService
    /// 3. Registrar en DI en Program.cs
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
        }

        public Task SendSubscriptionExpiredAsync(string emailDestino, string nombreNegocio, string nombrePlan, CancellationToken ct = default)
        {
            // Stub: log en lugar de enviar email real
            _logger.LogInformation(
                "[EMAIL STUB] Suscripción vencida: To={Email}, Negocio={Negocio}, Plan={Plan}",
                emailDestino, nombreNegocio, nombrePlan);

            // TODO: Implementar envío de email real cuando se configure SMTP
            // var message = new MimeMessage();
            // message.From.Add(new MailboxAddress("RoKey", "noreply@rokeystream.com"));
            // message.To.Add(new MailboxAddress(nombreNegocio, emailDestino));
            // message.Subject = "Tu suscripción ha vencido";
            // message.Body = new TextPart("html") { Text = GenerateExpiredEmailHtml(nombreNegocio, nombrePlan) };
            // await _smtpClient.SendAsync(message, ct);

            return Task.CompletedTask;
        }

        public Task SendSubscriptionWarningAsync(string emailDestino, string nombreNegocio, DateTime fechaVencimiento, CancellationToken ct = default)
        {
            // Stub: log en lugar de enviar email real
            _logger.LogInformation(
                "[EMAIL STUB] Advertencia suscripción: To={Email}, Negocio={Negocio}, Vence={Fecha}",
                emailDestino, nombreNegocio, fechaVencimiento.ToString("yyyy-MM-dd"));

            // TODO: Implementar envío de email real cuando se configure SMTP
            // var message = new MimeMessage();
            // message.From.Add(new MailboxAddress("RoKey", "noreply@rokeystream.com"));
            // message.To.Add(new MailboxAddress(nombreNegocio, emailDestino));
            // message.Subject = "Tu suscripción vence pronto";
            // message.Body = new TextPart("html") { Text = GenerateWarningEmailHtml(nombreNegocio, fechaVencimiento) };
            // await _smtpClient.SendAsync(message, ct);

            return Task.CompletedTask;
        }
    }
}
