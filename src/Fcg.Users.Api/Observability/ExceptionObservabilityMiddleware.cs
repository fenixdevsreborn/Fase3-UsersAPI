using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Fcg.Users.Api.Observability;

/// <summary>Catches exceptions, logs with TraceId/SpanId/CorrelationId/ExceptionType, increments exceptions.count, then rethrows.</summary>
public sealed class ExceptionObservabilityMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionObservabilityMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ILogger<ExceptionObservabilityMiddleware> logger, FcgMeters meters)
    {
        try
        {
            await _next(context).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            var traceId = ObservabilityContext.GetCurrentTraceId();
            var spanId = ObservabilityContext.GetCurrentSpanId();
            var correlationId = ObservabilityContext.GetCurrentCorrelationId();
            var exceptionType = ex.GetType().Name;
            var statusCode = 500;

            logger.LogError(ex,
                "Unhandled exception. {TraceId} {SpanId} {CorrelationId} {ExceptionType}",
                traceId, spanId, correlationId, exceptionType);

            meters.RecordException(exceptionType, statusCode);
            throw;
        }
    }
}
