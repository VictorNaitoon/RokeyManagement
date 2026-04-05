using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace API.Controllers.Webhooks
{
    [ApiController]
    [Route("api/v1/webhooks")]
    public class WebhooksController : ControllerBase
    {
        private readonly ILogger<WebhooksController> _logger;

        public WebhooksController(ILogger<WebhooksController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Endpoint stub para webhooks de MercadoPago
        /// </summary>
        [HttpPost("mercadopago")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ReceiveMercadoPagoWebhook()
        {
            try
            {
                // Leer el body del request
                using var reader = new StreamReader(Request.Body);
                var body = await reader.ReadToEndAsync();

                // Loguear el webhook recibido (en producción, validar签名)
                _logger.LogInformation("MercadoPago webhook recibido: {Body}", body);

                // Parsear para obtener información básica (opcional)
                if (!string.IsNullOrEmpty(body))
                {
                    try
                    {
                        var payload = JsonSerializer.Deserialize<JsonElement>(body);
                        var topic = payload.TryGetProperty("topic", out var t) ? t.GetString() : null;
                        var resourceId = payload.TryGetProperty("resource", out var r) ? r.GetString() : null;

                        _logger.LogInformation("MercadoPago topic: {Topic}, resource: {ResourceId}", topic, resourceId);
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning(ex, "No se pudo parsear el payload de MercadoPago");
                    }
                }

                // Stub: siempre retornar 200 OK
                return Ok(new { status = "received" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar webhook de MercadoPago");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
    }
}
