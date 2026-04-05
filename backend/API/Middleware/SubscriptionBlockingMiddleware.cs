using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using API.Services.Suscripcion;
using API.Services.Common;

namespace API.Middleware
{
    /// <summary>
    /// Middleware que verifica el estado de suscripción del negocio antes de permitir requests autenticados.
    /// Se ejecuta después de la autenticación y antes de la autorización.
    /// </summary>
    public class SubscriptionBlockingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SubscriptionBlockingMiddleware> _logger;

        private const string CacheKeyPrefix = "suscripcion_bloqueada_";
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

        public SubscriptionBlockingMiddleware(
            RequestDelegate next,
            ILogger<SubscriptionBlockingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(
            HttpContext context,
            IMemoryCache cache,
            IAuthService authService)
        {
            try
            {
                // 1. Skip si el endpoint permite acceso anónimo
                var endpoint = context.GetEndpoint();
                if (endpoint?.Metadata.GetMetadata<AllowAnonymousAttribute>() != null)
                {
                    await _next(context);
                    return;
                }

                // 2. Skip si el usuario es SuperAdmin
                var rolClaim = context.User.FindFirst("rol")?.Value;
                if (rolClaim == "SuperAdmin")
                {
                    _logger.LogDebug("SuperAdmin detected, skipping subscription check");
                    await _next(context);
                    return;
                }

                // 3. Obtener negocioId del claim
                var negocioIdClaim = context.User.FindFirst("negocioId")?.Value;
                if (!int.TryParse(negocioIdClaim, out var negocioId) || negocioId <= 0)
                {
                    // Usuario sin negocio asociado - no es un usuario de negocio válido
                    _logger.LogWarning("User without valid NegocioId, skipping subscription check");
                    await _next(context);
                    return;
                }

                // 4. Verificar estado en cache o DB
                var cacheKey = $"{CacheKeyPrefix}{negocioId}";

                if (!cache.TryGetValue(cacheKey, out bool puedeOperar))
                {
                    _logger.LogInformation("Cache miss for negocio {NegocioId}, querying subscription status", negocioId);
                    puedeOperar = await authService.CanNegocioOperarAsync(negocioId, context.RequestAborted);

                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(CacheDuration);

                    cache.Set(cacheKey, puedeOperar, cacheEntryOptions);
                }

                // 5. Si no puede operar, retornar 403
                if (!puedeOperar)
                {
                    var mensajeBloqueo = await authService.GetMensajeBloqueoAsync(negocioId, context.RequestAborted);

                    _logger.LogWarning("Negocio {NegocioId} blocked due to inactive subscription", negocioId);

                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    context.Response.ContentType = "application/problem+json";

                    var problemDetails = new
                    {
                        type = "https://tools.ietf.org/html/rfc9457#section-3",
                        title = "Subscription Blocked",
                        status = 403,
                        detail = mensajeBloqueo,
                        traceId = context.TraceIdentifier
                    };

                    await context.Response.WriteAsJsonAsync(problemDetails);
                    return;
                }

                // 6. Continuar al siguiente middleware
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SubscriptionBlockingMiddleware");
                // En caso de error, permitir el request para no bloquear por problemas técnicos
                await _next(context);
            }
        }
    }

    /// <summary>
    /// Extensiones para registrar el middleware en el pipeline de la aplicación
    /// </summary>
    public static class SubscriptionBlockingMiddlewareExtensions
    {
        /// <summary>
        /// Agrega el middleware de bloqueo por suscripción al pipeline de la aplicación.
        /// Debe usarse DESPUÉS de UseAuthentication() y ANTES de UseAuthorization().
        /// </summary>
        public static IApplicationBuilder UseSubscriptionBlocking(this IApplicationBuilder app)
        {
            return app.UseMiddleware<SubscriptionBlockingMiddleware>();
        }
    }
}
