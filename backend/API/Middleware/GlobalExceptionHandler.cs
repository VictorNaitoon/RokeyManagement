using API.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace API.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        // If response already started, cannot write ProblemDetails — let pipeline handle it
        if (httpContext.Response.HasStarted)
        {
            _logger.LogError(exception, "Unhandled exception after response started TraceId={TraceId} Path={Path}", httpContext.TraceIdentifier, httpContext.Request.Path);
            return false;
        }

        // Cancellation is not an error — just log and return 499 equivalent
        if (exception is OperationCanceledException or TaskCanceledException)
        {
            _logger.LogWarning("Request cancelled TraceId={TraceId} Path={Path}", httpContext.TraceIdentifier, httpContext.Request.Path);
            return false;
        }

        var traceId = httpContext.TraceIdentifier;

        try
        {
            if (exception is ValidationException validationEx)
        {
            _logger.LogWarning(validationEx, "Validation failed TraceId={TraceId}", traceId);

            var errors = validationEx.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            var problem = new ProblemDetails
            {
                Type = "https://httpstatuses.com/400",
                Title = "Validation Failed",
                Status = StatusCodes.Status400BadRequest,
                Detail = "One or more validation errors occurred.",
                Instance = httpContext.Request.Path
            };
            problem.Extensions["traceId"] = traceId;
            problem.Extensions["errors"] = errors;

            // Also include flat errors array for spec compatibility
            problem.Extensions["errorsFlat"] = validationEx.Errors
                .Select(e => new { propertyName = e.PropertyName, errorMessage = e.ErrorMessage })
                .ToArray();

            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            httpContext.Response.ContentType = "application/problem+json";
            await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
            return true;
        }

        if (exception is UnauthorizedAccessException)
        {
            _logger.LogWarning(exception, "Unauthorized TraceId={TraceId}", traceId);
            var problem = new ProblemDetails
            {
                Type = "https://httpstatuses.com/401",
                Title = "Unauthorized",
                Status = StatusCodes.Status401Unauthorized,
                Detail = exception.Message,
                Instance = httpContext.Request.Path
            };
            problem.Extensions["traceId"] = traceId;
            httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
            httpContext.Response.ContentType = "application/problem+json";
            await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
            return true;
        }

        if (exception is NotFoundException notFoundEx)
        {
            _logger.LogWarning(notFoundEx, "Not found TraceId={TraceId}", traceId);
            var problem = new ProblemDetails
            {
                Type = "https://httpstatuses.com/404",
                Title = "Not Found",
                Status = StatusCodes.Status404NotFound,
                Detail = notFoundEx.Message,
                Instance = httpContext.Request.Path
            };
            problem.Extensions["traceId"] = traceId;
            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            httpContext.Response.ContentType = "application/problem+json";
            await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
            return true;
        }

        if (exception is KeyNotFoundException)
        {
            _logger.LogWarning(exception, "Not found TraceId={TraceId}", traceId);
            var problem = new ProblemDetails
            {
                Type = "https://httpstatuses.com/404",
                Title = "Not Found",
                Status = StatusCodes.Status404NotFound,
                Detail = exception.Message,
                Instance = httpContext.Request.Path
            };
            problem.Extensions["traceId"] = traceId;
            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            httpContext.Response.ContentType = "application/problem+json";
            await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
            return true;
        }

        if (exception is DomainException domainEx)
        {
            _logger.LogWarning(domainEx, "Domain violation TraceId={TraceId}", traceId);
            var problem = new ProblemDetails
            {
                Type = "https://httpstatuses.com/422",
                Title = "Unprocessable Entity",
                Status = StatusCodes.Status422UnprocessableEntity,
                Detail = domainEx.Message,
                Instance = httpContext.Request.Path
            };
            problem.Extensions["traceId"] = traceId;
            problem.Extensions["errors"] = new Dictionary<string, string[]>
            {
                ["StockActual"] = new[] { domainEx.Message }
            };
            httpContext.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
            httpContext.Response.ContentType = "application/problem+json";
            await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
            return true;
        }

        _logger.LogError(exception, "Unhandled exception TraceId={TraceId}", traceId);
        var fallback = new ProblemDetails
        {
            Type = "https://httpstatuses.com/500",
            Title = "Internal Server Error",
            Status = StatusCodes.Status500InternalServerError,
            Detail = "An unexpected error occurred.",
            Instance = httpContext.Request.Path
        };
        fallback.Extensions["traceId"] = traceId;
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        httpContext.Response.ContentType = "application/problem+json";
        await httpContext.Response.WriteAsJsonAsync(fallback, cancellationToken);
        return true;
        }
        catch (Exception handlerEx)
        {
            _logger.LogError(handlerEx, "Exception handler failed TraceId={TraceId}", traceId);
            if (!httpContext.Response.HasStarted)
            {
                try
                {
                    httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    httpContext.Response.ContentType = "application/problem+json";
                    await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
                    {
                        Type = "https://httpstatuses.com/500",
                        Title = "Internal Server Error",
                        Status = StatusCodes.Status500InternalServerError,
                        Detail = "An unexpected error occurred.",
                        Instance = httpContext.Request.Path,
                        Extensions = { ["traceId"] = traceId }
                    }, cancellationToken);
                    return true;
                }
                catch { /* last resort */ }
            }
            return false;
        }
    }
}
