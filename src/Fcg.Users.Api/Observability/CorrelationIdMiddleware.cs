using Microsoft.AspNetCore.Http;

namespace Fcg.Users.Api.Observability;

/// <summary>Reads or generates X-Correlation-ID; sets it on Activity and response.</summary>
public sealed class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-ID";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var incoming = context.Request.Headers[HeaderName].FirstOrDefault();
        var correlationId = ObservabilityContext.SetCorrelationIdOrDefault(incoming, () => Guid.NewGuid().ToString("N"));
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });
        await _next(context).ConfigureAwait(false);
    }
}
